using System;
using BepInEx;
using EndlessDelivery.Anticheat;
using EndlessDelivery.Assets;
using EndlessDelivery.Cheats;
using EndlessDelivery.Config;
using EndlessDelivery.Online;
using EndlessDelivery.Online.Requests;
using EndlessDelivery.Saving;
using HarmonyLib;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EndlessDelivery;

[BepInDependency(AtlasLib.Plugin.Guid)]
[BepInPlugin(Guid, Name, Version)]
public class Plugin : BaseUnityPlugin
{
    public const string Name = "Divine Delivery";
    public const string Version = "2.0.0";
    public const string Guid = "waffle.ultrakill.christmasdelivery";


    private void Start()
    {
        Debug.Log($"{Name} has started !!");
        AssetManager.LoadCatalog();
        AssetManager.LoadDataFile();
        new Harmony(Guid).PatchAll();
    }

#if DEBUG
    private void Update()
    {
        if (InputManager.Instance?.InputSource == null)
        {
            return;
        }

        if (InputManager.Instance.InputSource.Dodge.IsPressed && InputManager.Instance.InputSource.Hook.IsPressed && InputManager.Instance.InputSource.Slot6.WasPerformedThisFrame)
        {
            Debug.Log($"Ticket! [{Authentication.GetTicket()}]");
        }

        if (InputManager.Instance.InputSource.Dodge.IsPressed && InputManager.Instance.InputSource.Hook.IsPressed && InputManager.Instance.InputSource.Slot4.WasPerformedThisFrame)
        {
            OnlineFunctionality.UseLocalUrl = true;
        }
    }
#endif

    private void OnDestroy()
    {
        SaveFile.SaveAll();
    }
}
