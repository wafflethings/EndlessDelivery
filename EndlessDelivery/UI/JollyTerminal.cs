using System;
using EndlessDelivery.Config;
using EndlessDelivery.Scores;
using EndlessDelivery.Utils;
using TMPro;
using UnityEngine;

namespace EndlessDelivery.UI
{
    public class JollyTerminal : MonoBehaviour
    {
        public TMP_Text ScoreText;
        public AudioSource Music;
        public GameObject[] DisableIfNotOnHighscoreDifficulty;

        private void OnEnable()
        {
            AssignScoreText();
            SetMusicStatus();

            bool targetState = PrefsManager.Instance.GetInt("difficulty") == 3;
            foreach (GameObject gameObject in DisableIfNotOnHighscoreDifficulty)
            {
                gameObject.SetActive(targetState);
            }
        }

        public void AssignScoreText()
        {
            Score score = Score.Highscore;
            ScoreText.text = $"{score.Rooms}\n{score.Kills}\n{score.Deliveries}\n{TimeSpan.FromSeconds(score.Time).Formatted()}";
        }

        public void SetMusicStatus()
        {
            if (Option.GetValue<bool>("disable_copyrighted_music"))
            {
                Music.Pause();
            }
            else
            {
                Music.UnPause();
            }
        }
    }
}