using BepInEx;
using BepInEx.Logging;
using EndlessDelivery.Assets;
using EndlessDelivery.Config;
using EndlessDelivery.Cosmetics;
using EndlessDelivery.Online;
using EndlessDelivery.ScoreManagement;
using HarmonyLib;
using UnityEngine;

namespace EndlessDelivery;

[BepInDependency(AtlasLib.Plugin.Guid)]
[BepInPlugin(Guid, Name, Version)]
public class Plugin : BaseUnityPlugin
{
    public static ManualLogSource Log;
    public const string Name = "Divine Delivery";
    public const string Version = "2.0.0";
    public const string Guid = "waffle.ultrakill.christmasdelivery";

    private void Start()
    {
        Log = Logger;
        Log.LogInfo($"{Name} has started !!");
        AssetManager.LoadCatalog();
        AssetManager.LoadDataFile();
        OnlineFunctionality.Init();
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
            ScoreManager.SubmitScore(new(1,1,1,1), 0);
            Log.LogInfo($"Ticket! [{OnlineFunctionality.GetTicket()}]");
        }
    }
#endif
}
