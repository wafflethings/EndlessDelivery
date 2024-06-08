﻿using System;
using BepInEx;
using EndlessDelivery.Anticheat;
using EndlessDelivery.Assets;
using EndlessDelivery.Cheats;
using EndlessDelivery.Config;
using EndlessDelivery.Scores;
using EndlessDelivery.Scores.Server;
using EndlessDelivery.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EndlessDelivery
{
    [BepInPlugin(GUID, Name, Version)]
    public class Plugin : BaseUnityPlugin
    {
        public const string Name = "Divine Delivery";
        public const string Version = "2.0.0";
        public const string GUID = "waffle.ultrakill.christmasdelivery";

        private void Start()
        {
            Debug.Log($"{Name} has started !!");

            AddressableManager.Setup();
            PatchThis.AddPatches();
            Option.Load();
        }
        
#if DEBUG
        private void Update()
        {
            if (InputManager.Instance.InputSource.Dodge.IsPressed && InputManager.Instance.InputSource.Hook.IsPressed && InputManager.Instance.InputSource.Slot6.WasPerformedThisFrame)
            {
                Debug.Log($"Ticket! [{SteamAuth.GetTicket()}]");
            }
        }
#endif

        private void OnDestroy()
        {
            Option.Save();
        }
    }
}