using System.Collections;
using BepInEx;
using BepInEx.Logging;
using EndlessDelivery.Assets;
using EndlessDelivery.Online;
using EndlessDelivery.ScoreManagement;
using HarmonyLib;
using Steamworks;

namespace EndlessDelivery;

[BepInDependency(AtlasLib.Plugin.Guid)]
[BepInPlugin(Guid, Name, Version)]
public class Plugin : BaseUnityPlugin
{
    public static ManualLogSource Log;
    public const string Name = "Divine Delivery";
    public const string Version = "2.0.5";
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
}
