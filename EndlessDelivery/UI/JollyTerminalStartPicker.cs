using System;
using System.Globalization;
using EndlessDelivery.Config;
using EndlessDelivery.Gameplay;
using TMPro;
using UnityEngine;

namespace EndlessDelivery.UI;

public class JollyTerminalStartPicker : MonoBehaviour
{
    [SerializeField] private TMP_Text? _timeDisplayText;
    [SerializeField] private GameObject? _buttonPrefab;
    [SerializeField] private Transform? _buttonHolder;
    private bool _hasPopulated;

    private void OnEnable()
    {
        if (_hasPopulated)
        {
            return;
        }

        Populate();
        SetWave(StartTimes.Instance.Data.CurrentTimes.SelectedWave);
    }

    private void Populate()
    {
        if (_buttonPrefab == null || _buttonHolder == null)
        {
            throw new Exception("StartPicker Button prefab or button holder is null");
        }

        foreach (int startableWave in StartTimes.StartTime.StartableWaves)
        {
            Instantiate(_buttonPrefab, _buttonHolder).GetComponent<JollyTerminalStartPickButton>().Setup(this, startableWave);
        }

        _hasPopulated = true;
    }

    public void SetWave(int wave)
    {
        StartTimes.StartTime currentTime = StartTimes.Instance.Data.CurrentTimes;
        currentTime.SelectedWave = wave;

        if (_timeDisplayText == null)
        {
            Plugin.Log.LogWarning("Time display text on start picker is null");
            return;
        }

        string time = TimeSpan.FromSeconds(currentTime.GetRoomTime(wave)).ToString(@"mm\:ss\.fff", new CultureInfo("en-US"));
        _timeDisplayText.text = $"<size=20><color=orange>ROOM {wave}\n<size=15><color=#999999>TIME: {time}";
    }
}
