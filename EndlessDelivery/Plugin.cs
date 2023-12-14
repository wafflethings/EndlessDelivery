﻿using System;
using BepInEx;
using EndlessDelivery.Assets;
using UnityEngine;

namespace EndlessDelivery
{
    [BepInPlugin(GUID, Name, Version)]
    public class Plugin : BaseUnityPlugin
    {
        public const string Name = "Endless Delivery";
        public const string Version = "1.0.0";
        public const string GUID = "waffle.ultrakill.christmasdelivery";

        public void Start()
        {
            Debug.Log($"{Name} has started !!");
            
            AddressableManager.Setup();
        }
    }
}