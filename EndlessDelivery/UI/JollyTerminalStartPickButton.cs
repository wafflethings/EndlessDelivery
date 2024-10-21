using EndlessDelivery.Config;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessDelivery.UI;

public class JollyTerminalStartPickButton
{
    [SerializeField] private Button _button;
    [SerializeField] private Text _text;

    public void Setup(int startWave)
    {
        StartTimes.StartTime currentTime = StartTimes.Instance.Data.CurrentTimes;
        _text.text = startWave.ToString();

        if (!currentTime.UnlockedStartTimes.Contains(startWave))
        {
            _button.interactable = false;
            return;
        }

        _button.onClick.AddListener(() => currentTime.SelectedWave = startWave);
    }
}
