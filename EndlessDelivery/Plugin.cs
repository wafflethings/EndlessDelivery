using System;
using BepInEx;
using EndlessDelivery.Anticheat;
using EndlessDelivery.Assets;
using EndlessDelivery.Cheats;
using EndlessDelivery.Config;
using EndlessDelivery.Saving;
using EndlessDelivery.Scores;
using EndlessDelivery.Scores.Server;
using EndlessDelivery.Server;
using EndlessDelivery.Server.ContentFile;
using EndlessDelivery.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EndlessDelivery;

[BepInPlugin(Guid, Name, Version)]
public class Plugin : BaseUnityPlugin
{
    public const string Name = "Divine Delivery";
    public const string Version = "2.0.0";
    public const string Guid = "waffle.ultrakill.christmasdelivery";



    private void Start()
    {
        Debug.Log($"{Name} has started !!");

        AddressableManager.Setup();
        SaveData.LoadAll();
        Option.Load();
    }

#if DEBUG
    private void Update()
    {
        if (InputManager.Instance.InputSource.Dodge.IsPressed && InputManager.Instance.InputSource.Hook.IsPressed && InputManager.Instance.InputSource.Slot6.WasPerformedThisFrame)
        {
            Debug.Log($"Ticket! [{SteamAuth.GetTicket()}]");
        }

        if (InputManager.Instance.InputSource.Dodge.IsPressed && InputManager.Instance.InputSource.Hook.IsPressed && InputManager.Instance.InputSource.Slot4.WasPerformedThisFrame)
        {
            OnlineFunctionality.UseLocalUrl = true;
        }

        if (InputManager.Instance.InputSource.Dodge.IsPressed && InputManager.Instance.InputSource.Hook.IsPressed && InputManager.Instance.InputSource.Slot5.WasPerformedThisFrame)
        {
            DoThing();
        }
    }

    private async void DoThing()
    {
        Debug.Log((await ContentDownloader.GetContent()).Banners.Keys);
    }
#endif

    private void OnDestroy()
    {
        SaveData.SaveAll();
        Option.Save();
    }
}
