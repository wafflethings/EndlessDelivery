using System;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessDelivery.UI;

public class LoadoutHudVariantButton : MonoBehaviour
{
    public int VariationIndex;
    [SerializeField] private LoadoutHud _parentHud;
    [SerializeField] private ColorBlindGet[] _colourGetters;
    [SerializeField] private Button _button;
    [SerializeField] private GameObject _check;

    private void OnEnable()
    {
        foreach (ColorBlindGet colourGetter in _colourGetters)
        {
            colourGetter.variationNumber = VariationIndex;
            colourGetter.UpdateColor();
        }
    }

    private void Awake()
    {
        _button.onClick.AddListener(() => _parentHud.EquipForVariation(VariationIndex));
    }

    public void SetState(bool on)
    {
        _check.SetActive(on);
    }

    public void SetInteractable(bool interactable) => _button.interactable = interactable;
}
