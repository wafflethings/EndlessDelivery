using System;
using EndlessDelivery.Config;
using UnityEngine;

namespace EndlessDelivery.UI;

public class JollyTerminalStartPicker : MonoBehaviour
{
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
    }

    private void Populate()
    {
        if (_buttonPrefab == null || _buttonHolder == null)
        {
            throw new Exception("StartPicker Button prefab or button holder is null");
        }

        foreach (int startableRoom in StartTimes.StartTime.StartableWaves)
        {
            Instantiate(_buttonPrefab, _buttonHolder).GetComponent<JollyTerminalStartPickButton>().Setup(startableRoom);
        }
    }
}
