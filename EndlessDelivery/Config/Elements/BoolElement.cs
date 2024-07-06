using System;
using EndlessDelivery.Scores;
using EndlessDelivery.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace EndlessDelivery.Config.Elements;

public class BoolElement : MonoBehaviour
{
    public TMP_Text EnabledText;
    public string Id;
    public UltrakillEvent OnToggled;
    private Option<bool> _option;
        
    private void Start()
    {
            _option = (Option<bool>)Option.AllOptions[Id];
            UpdateText();
        }

    public void Toggle()
    {
            _option.Value = !_option.Value;
            UpdateText();
        }
        
    private void UpdateText()
    {
            EnabledText.text = _option.Value ? "ENABLED" : "DISABLED";

            if (_option.Value)
            {
                OnToggled.Invoke();
            }
            else
            {
                OnToggled.Revert();
            }
        }
}