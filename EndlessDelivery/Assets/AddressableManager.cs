using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AtlasLib.Utils;
using EndlessDelivery.Gameplay.EnemyGeneration;
using EndlessDelivery.Utils;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace EndlessDelivery.Assets
{
    [PatchThis($"{Plugin.GUID}.AddressableManager")]
    public static class AddressableManager
    {
        public static string AssetPathLocation => "{" + $"{typeof(AddressableManager).FullName}.{nameof(AssetPath)}" + "}"; //should eval to "{EndlessDelivery.Assets.AdressableManager.AssetPath}"
        public static string MonoScriptBundleName => "monoscript_endlessdelivery_monoscripts.bundle";
        public static string AssetPath => Path.Combine(PathUtils.ModPath(), "Assets");
        public static string CatalogPath => Path.Combine(AssetPath, "catalog.json");
        public static string ModDataPath => Path.Combine(AssetPath, "data.json");
        public static bool InSceneFromThisMod => _scenesFromThisMod.Contains(SceneHelper.CurrentScene);
        private static List<string> _scenesFromThisMod; 
        private static bool _dontSanitizeScenes;
        
        public static void Setup()
        {
            Addressables.LoadContentCatalogAsync(CatalogPath, true).WaitForCompletion();
            LoadDataFile();
        }
        
        private static void LoadDataFile()
        {
            // i stole this from teamdoodz - ultracrypt v1 ModAssets:LoadDataFile;
            
            Dictionary<string, List<string>> data = null;
            using (StreamReader reader = new(File.OpenRead(ModDataPath)))
            {
                JsonSerializer serializer = new();
                data = serializer.Deserialize<Dictionary<string, List<string>>>(new JsonTextReader(reader));
            }

            if (data.ContainsKey(typeof(EnemyGroup).FullName))
            {
                EnemyGroup.SetGroups(GetDataOfType<EnemyGroup>(data));
            }
            
            if (data.ContainsKey(typeof(Scene).FullName))
            {
                _scenesFromThisMod = data[typeof(Scene).FullName];
            }
        }
        
        private static IEnumerable<T> GetDataOfType<T>(Dictionary<string, List<string>> data) where T : UnityEngine.Object
        {
            if (!data.ContainsKey(typeof(T).FullName))
                return Array.Empty<T>(); //Prevent index out of range tbh

            return data[typeof(T).FullName].Select(Load<T>);
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