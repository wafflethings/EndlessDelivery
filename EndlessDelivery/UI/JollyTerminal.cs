using System;
using EndlessDelivery.Common;
using EndlessDelivery.ScoreManagement;
using EndlessDelivery.Utils;
using TMPro;
using UnityEngine;

namespace EndlessDelivery.UI;

public class JollyTerminal : MonoBehaviour
{
    public GameObject[] DisableIfNotOnHighscoreDifficulty;

    private void OnEnable()
    {
        bool targetState = PrefsManager.Instance.GetInt("difficulty") >= 3;
        foreach (GameObject gameObject in DisableIfNotOnHighscoreDifficulty)
        {
            gameObject.SetActive(targetState);
        }
    }
}
