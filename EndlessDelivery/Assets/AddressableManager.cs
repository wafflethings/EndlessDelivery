using System;
using System.IO;
using System.Reflection;
using EndlessDelivery.Utils;
using HarmonyLib;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EndlessDelivery.Assets
{
    [PatchThis($"{Plugin.GUID}.AddressableManager")]
    public static class AddressableManager
    {
        public static string AssetPathLocation => "{" + $"{typeof(AddressableManager).FullName}.{nameof(AssetPath)}" + "}"; //should eval to "{EndlessDelivery.Assets.AdressableManager.AssetPath}"
        public static string MonoScriptBundleName => "monoscript_endlessdelivery_monoscripts.bundle";
        public static string AssetPath => Path.Combine(ModFolder, "Assets");
        public static string CatalogPath => Path.Combine(AssetPath, "catalog.json");
        public static string ModFolder => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        
        private static bool _dontSanitizeScenes;
        
        public static void Setup()
        {
            Addressables.LoadContentCatalogAsync(CatalogPath, true).WaitForCompletion();
            Debug.Log("Added catalog i think");
        }

        public static T Load<T>(string path)
        {
            return Addressables.LoadAssetAsync<T>(path).WaitForCompletion();
        }

        public static void LoadScene(string path)
        {
            _dontSanitizeScenes = true;
            
            try
            {
                SceneHelper.LoadScene(path);
            }
            catch (Exception ex)
            {
                // i hate using trycatch but if this isnt set back to false, every unmodded scene load will fail
                Debug.LogError(ex.ToString());
            }
            
            _dontSanitizeScenes = false;
        }

        [HarmonyPatch(typeof(SceneHelper), nameof(SceneHelper.SanitizeLevelPath)), HarmonyPrefix]
        private static bool PreventSanitization(string scene, ref string __result)
        {
            if (_dontSanitizeScenes)
            {
                __result = scene;
                return false;
            }

            return true;
        }
    }
}