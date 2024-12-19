using System;
using System.Collections.Generic;
using EndlessDelivery.Config;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessDelivery.UI;

public class PresentColourPicker : MonoBehaviour
{
    [SerializeField] private PresentColourUi _parent;
    [SerializeField] private int _colourIndex;
    [SerializeField] private Slider[] _sliders;
    [SerializeField] private Image _image;

    private void Awake()
    {
        for (int i = 0; i < _sliders.Length; i++)
        {
            int currentIndex = i;

            _sliders[i].onValueChanged.AddListener((value) =>
            {
                _parent.SliderChanged(_colourIndex, currentIndex, value);
                UpdateSelf();
            });
        }

        UpdateSelf();
    }

    public void UpdateSelf()
    {
        Color color = ConfigFile.Instance.Data.GetColour(_colourIndex);

        for (int i = 0; i < _sliders.Length; i++)
        {
            _sliders[i].SetValueWithoutNotify(color[i]);
        }

        _image.color = color;
    }
}
