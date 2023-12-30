using System;
using BepInEx;
using EndlessDelivery.Anticheat;
using EndlessDelivery.Assets;
using EndlessDelivery.Cheats;
using EndlessDelivery.Config;
using EndlessDelivery.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EndlessDelivery
{
    [BepInPlugin(GUID, Name, Version)]
    public class Plugin : BaseUnityPlugin
    {
        public const string Name = "Endless Delivery";
        public const string Version = "1.2.0";
        public const string GUID = "waffle.ultrakill.christmasdelivery";

        private void Start()
        {
            Debug.Log($"{Name} has started !!");

            AddressableManager.Setup();
            PatchThis.AddPatches();
            Option.Load();
        }

        private void OnDestroy()
        {
            Option.Save();
        }
    }
}