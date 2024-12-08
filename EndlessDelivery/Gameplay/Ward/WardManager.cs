using System.Collections.Generic;
using EndlessDelivery.Components;
using UnityEngine;

namespace EndlessDelivery.Gameplay.Ward;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class WardManager : MonoSingleton<WardManager>
{
    [HideInInspector] public List<Ward> AllWards = new();
    [SerializeField] private GameObject _ward;

    public void CreateWard(Present present)
    {
        Ward ward = Instantiate(_ward).GetComponent<Ward>();
        AllWards.Add(ward);
        ward.Present = present;
    }
}
