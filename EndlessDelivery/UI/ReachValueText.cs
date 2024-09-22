using System;
using EndlessDelivery.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessDelivery.UI;

public class ReachValueText : MonoBehaviour
{
    public IntOrFloat Type;
    public Text Text;
    public float Target;
    public string Format;
    public string StartValue;
    public float ChangePerSecond;
    public bool IsTimestamp;

    private float _timeElapsed;
    private AudioSource _audio;
    private float _currentValue;

    [HideInInspector] public bool Done;

    public enum IntOrFloat
    {
        Int,
        Float
    }

    private void OnEnable()
    {
        EndScreen.Instance.CurrentText = this;
        Text.text = StartValue;
        _audio = GetComponent<AudioSource>();
        _audio?.Play();
    }

    private void Update()
    {
        if (EndScreen.Instance.Skipping)
        {
            _currentValue = Target;
            SetText();
        }

        if (Target == _currentValue)
        {
            SetText();
            Done = true;
            enabled = false;
            _audio?.Stop();
            return;
        }

        _timeElapsed += Time.unscaledDeltaTime;
        _currentValue = Mathf.MoveTowards(_currentValue, Target, ChangePerSecond * Time.unscaledDeltaTime);

        SetText();
    }

    private void SetText()
    {
        float value = Type == IntOrFloat.Int ? Mathf.Floor(_currentValue) : _currentValue;
        Text.text = !IsTimestamp ? string.Format(Format, value) : TimeSpan.FromSeconds(value).Formatted();
    }
}
