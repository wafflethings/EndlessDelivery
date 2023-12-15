using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EndlessDelivery.Assets
{
    public static class AddressableManager
    {
        public static string AssetPathLocation => "{" + $"{typeof(AddressableManager).FullName}.{nameof(AssetPath)}" + "}"; //should eval to "{EndlessDelivery.Assets.AdressableManager.AssetPath}"
        public static string MonoScriptBundleName => "monoscript_endlessdelivery_monoscripts.bundle";
        public static string AssetPath => Path.Combine(ModFolder, "Assets");
        public static string CatalogPath => Path.Combine(AssetPath, "catalog.json");
        public static string ModFolder => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        
        public static void Setup()
        {
            Addressables.LoadContentCatalogAsync(CatalogPath, true).WaitForCompletion();
            Debug.Log("Added catalog i think");
        }
    }
}