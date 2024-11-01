using EndlessDelivery.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessDelivery.UI;

public class JollyTerminalStartPickButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _text;

    public void Setup(JollyTerminalStartPicker picker, int startWave)
    {
        StartTimes.StartTime currentTime = StartTimes.Instance.Data.CurrentTimes;
        _text.text = startWave.ToString();

        if (!currentTime.UnlockedStartTimes.Contains(startWave))
        {
            _button.interactable = false;
            return;
        }

        _button.onClick.AddListener(() =>
        {
            picker.SetWave(startWave);
        });
    }
}
