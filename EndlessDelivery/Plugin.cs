using System;
using BepInEx;
using EndlessDelivery.Assets;
using EndlessDelivery.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;

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
            PatchThis.AddPatches();
        }

        public void Update()
        {
            if (InputManager.instance != null && InputManager.Instance.InputSource.Fire2.IsPressed && InputManager.Instance.InputSource.Fire1.WasPerformedThisFrame)
            {
                AddressableManager.LoadScene("Assets/Scenes/Test Scene.unity");
            }
        }
    }
}