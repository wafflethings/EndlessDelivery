using System;
using System.Collections;
using EndlessDelivery.Assets;
using EndlessDelivery.Gameplay;
using EndlessDelivery.Scores;
using EndlessDelivery.Utils;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessDelivery.UI
{
    [PatchThis($"{Plugin.GUID}.EndScreen")]
    public class EndScreen : MonoSingleton<EndScreen>
    {
        public GameObject[] ToAppear;
        public GameObject AudioObject;
        public Image HighscoreFlash;
        public float Delay = 0.25f;
        public float SkippingDelay = 0.05f;
        [Space(10)]
        public ReachValueText Rooms;
        public ReachValueText Kills;
        public ReachValueText Deliveries;
        public ReachValueText TimeElapsed;
        [Space(10)]
        public Text BestRooms;
        public Text BestKills;
        public Text BestDeliveries;
        public Text BestTimeElapsed;
        [Space(10)] 
        public ReachValueText MoneyGainText;
        
        [HideInInspector] public bool NewBest;
        [HideInInspector] public ReachValueText CurrentText;
        private int _currentIndex;
        private float _timeSinceComplete;
        private bool _appearedSelf;
        private bool _complete;

        public bool Skipping => InputManager.Instance.InputSource.Fire1.WasPerformedThisFrame || InputManager.Instance.InputSource.Jump.WasPerformedThisFrame;

        private void Awake()
        {
            foreach (GameObject toAppearObject in ToAppear)
            {
                toAppearObject.SetActive(false);
            }
        }
        
        public void Appear()
        {
            SetPanelValues(GameManager.Instance.CurrentScore, Rooms, Kills, Deliveries, TimeElapsed);
            SetPanelValues(NewBest ? Score.PreviousHighscore : Score.Highscore, BestRooms, BestKills, BestDeliveries, BestTimeElapsed);
            MoneyGainText.Target = GameManager.Instance.CurrentScore.MoneyGain;
            
            StartCoroutine(AppearCoroutine());
        }

        //stolen from FinalCyberRank:Update
        private IEnumerator AppearCoroutine()
        {
            TimeController timeController = TimeController.Instance;
            timeController.controlTimeScale = false;

            while (timeController.timeScale > 0f)
            {
                timeController.timeScale = Mathf.MoveTowards(timeController.timeScale, 0f, Time.unscaledDeltaTime * (timeController.timeScale + 0.01f));
                Time.timeScale = timeController.timeScale * timeController.timeScaleModifier;
                AudioMixerController.Instance.allSound.SetFloat("allPitch", timeController.timeScale);
                AudioMixerController.Instance.allSound.SetFloat("allVolume", 0.5f + timeController.timeScale / 2f);
                AudioMixerController.Instance.musicSound.SetFloat("allPitch", timeController.timeScale);
                MusicManager.Instance.volume = 0.5f + timeController.timeScale / 2f;
                yield return null;
            }

            ShowNext();
            _appearedSelf = true;
            MusicManager.Instance.forcedOff = true;
            MusicManager.Instance.StopMusic();
            AudioObject.SetActive(true);
        }
        
        private void SetPanelValues(Score score, ReachValueText rooms, ReachValueText kills, ReachValueText deliveries, ReachValueText time)
        {
            rooms.Target = score.Rooms;
            kills.Target = score.Kills;
            deliveries.Target = score.Deliveries;
            time.Target = score.Time;
        }
        
        private void SetPanelValues(Score score, Text rooms, Text kills, Text deliveries, Text time)
        {
            rooms.text = score.Rooms.ToString();
            kills.text = score.Kills.ToString();
            deliveries.text = score.Deliveries.ToString();
            time.text = TimeSpan.FromSeconds(score.Time).Formatted();
        }
        
        private void Update()
        {
            if (!_appearedSelf)
            {
                return;
            }
            
            if (CurrentText?.Done ?? true)
            {
                _timeSinceComplete += Time.unscaledDeltaTime;
            }

            if (_timeSinceComplete >= (!Skipping ? Delay : SkippingDelay))
            {
                ShowNext();
                _timeSinceComplete = 0;
            }
        }
        
        private void ShowNext()
        {
            if (_currentIndex == ToAppear.Length)
            {
                NewMovement.Instance.hp = 0; // needs to be at 0 to respawn!! - StatsManager.Update 
                _complete = true;
                return;
            }

            ToAppear[_currentIndex++].SetActive(true);
        }

        public void DoFlashIfApplicable()
        {
            if (NewBest)
            {
                SetPanelValues(Score.Highscore, BestRooms, BestKills, BestDeliveries, BestTimeElapsed);
                StartCoroutine(DoHighscoreFlash());
            }
        }

        private IEnumerator DoHighscoreFlash()
        {
            HighscoreFlash.color = new Color(1, 1, 1, 1);
            HighscoreFlash.GetComponent<AudioSource>().Play();

            while (HighscoreFlash.color.a != 0)
            {
                HighscoreFlash.color = new Color(1, 1, 1, Mathf.MoveTowards(HighscoreFlash.color.a, 0, Time.unscaledDeltaTime));
                yield return null;
            }
        }

        [HarmonyPatch(typeof(CanvasController), nameof(CanvasController.Awake)), HarmonyPostfix]
        private static void CreateSelf()
        {
            Instantiate(AddressableManager.Load<GameObject>("Assets/Delivery/Prefabs/HUD/Delivery End Screen.prefab"));
        }
    }
}