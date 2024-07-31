using System;
using EndlessDelivery.Common;
using EndlessDelivery.Config;
using EndlessDelivery.ScoreManagement;
using EndlessDelivery.Utils;
using TMPro;
using UnityEngine;

namespace EndlessDelivery.UI;

public class JollyTerminal : MonoBehaviour
{
    public TMP_Text ScoreText;
    public AudioSource Music;
    public GameObject[] DisableIfNotOnHighscoreDifficulty;

    private void OnEnable()
    {
        AssignScoreText();

        bool targetState = PrefsManager.Instance.GetInt("difficulty") == 3;
        foreach (GameObject gameObject in DisableIfNotOnHighscoreDifficulty)
        {
            gameObject.SetActive(targetState);
        }
    }

    public void AssignScoreText()
    {
        Score score = ScoreManager.CurrentDifficultyHighscore;
        ScoreText.text = $"{score.Rooms}\n{score.Kills}\n{score.Deliveries}\n{TimeSpan.FromSeconds(score.Time).Formatted()}";
    }
}
