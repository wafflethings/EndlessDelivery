using System;
using EndlessDelivery.Common.ContentFile;
using EndlessDelivery.Online;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessDelivery.UI;

public class AdventCalendarButton : MonoBehaviour
{
    [SerializeField] private TMP_Text _numberText;
    [SerializeField] private Button _button;
    [SerializeField] private Image _image;
    [SerializeField] private Color _todayColour;
    [SerializeField] private Color _beforeTodayColour;
    [SerializeField] private Color _afterTodayColour;

    public void SetUp(AdventCalendar parent, CalendarReward reward)
    {
        _numberText.text = reward.Date.Day.ToString();
        _button.onClick.AddListener(() => parent.EnableClaimUi(reward.Id));

        Color colour = _todayColour;

        if (reward.Date.Date < DateTime.UtcNow.Date)
        {
            colour = _beforeTodayColour;
        }

        if (reward.Date.Date > DateTime.UtcNow.Date)
        {
            _button.interactable = false;
            colour = _afterTodayColour;
        }

        _image.color = colour;
    }
}
