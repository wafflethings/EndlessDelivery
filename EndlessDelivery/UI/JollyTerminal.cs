using System;
using EndlessDelivery.Scores;
using EndlessDelivery.Utils;
using TMPro;
using UnityEngine;

namespace EndlessDelivery.UI
{
    public class JollyTerminal : MonoBehaviour
    {
        public TMP_Text ScoreText;

        private void OnEnable()
        {
            // more should go here when i add leaderboards
            AssignScoreText();
        }

        private void AssignScoreText()
        {
            Score score = Score.Highscore;
            ScoreText.text = $"{score.Rooms}\n{score.Kills}\n{score.Deliveries}\n{TimeSpan.FromSeconds(score.Time).Formatted()}";
        }
    }
}