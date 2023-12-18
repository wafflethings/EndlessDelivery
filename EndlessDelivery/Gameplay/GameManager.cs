using System.Collections;
using EndlessDelivery.Hud;
using UnityEngine;

namespace EndlessDelivery.Gameplay
{
    public class GameManager : MonoSingleton<GameManager>
    {
        public const float StartTime = 45;
        public const float TimeAddLength = 0.5f;

        public AudioSource TimeAddSound;
        
        public bool GameStarted { get; private set; }
        public float TimeLeft { get; private set; }
        public float TimeElapsed { get; private set; }
        public float DeliveredPresents { get; private set; }
        public Room CurrentRoom { get; private set; }
        public bool TimerActive { get; private set; }
        
        private Coroutine _pauseCoroutine;

        public void Update()
        {
            if (GameStarted && GunControl.Instance.activated)
            {
                TimeLeft = Mathf.MoveTowards(TimeLeft, 0, Time.deltaTime);
                TimeElapsed += Time.deltaTime;

                if (TimeLeft == 0)
                {
                    EndGame();
                }
            }
        }

        public void AddTime(float seconds)
        {
            TimeAddSound?.Play();
            TimerActive = false;
            
            if (_pauseCoroutine != null)
            {
                StopCoroutine(_pauseCoroutine);
            }
            _pauseCoroutine = StartCoroutine(UnpauseTimer());
            
            TimeLeft += seconds;
        }

        private IEnumerator UnpauseTimer()
        {
            yield return new WaitForSeconds(TimeAddLength);
            TimerActive = true;
        }

        public void SetRoom(Room room)
        {
            CurrentRoom = room;
            if (!GameStarted)
            {
                StartGame();
            }
        }

        public void StartGame()
        {
            PresentTimeHud.Instance.gameObject.SetActive(true);
            TimeLeft = StartTime;
            TimerActive = true;
            GameStarted = true;
        }

        public void EndGame()
        {
            NewMovement.Instance.GetHurt(1000, false);
            GameStarted = false;
        }
    }
}