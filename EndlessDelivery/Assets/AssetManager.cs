using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using EndlessDelivery.Gameplay.EnemyGeneration;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace EndlessDelivery.Assets;

[HarmonyPatch]
[HarmonyPatch]
public static class AssetManager
{
    private static bool s_dontSanitizeScenes;

    public static string ModFolder => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    public static string AssetPath => Path.Combine(ModFolder, "Assets");
    public static string CatalogPath => Path.Combine(AssetPath, "catalog_wbp.json");
    public static string ModDataPath => Path.Combine(AssetPath, "data.json");

    public static bool InSceneFromThisMod => _scenesFromThisMod.Contains(SceneHelper.CurrentScene);
    private static List<string> _scenesFromThisMod;
    private static bool _dontSanitizeScenes;

    public static void LoadCatalog()
    {
        Addressables.LoadContentCatalogAsync(CatalogPath, true).WaitForCompletion();
    }

    public static void LoadDataFile()
    {
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

        return data[typeof(T).FullName].Select(name => Addressables.LoadAssetAsync<T>(name).WaitForCompletion());
    }

    public static void LoadSceneUnsanitzed(string path)
    {
        s_dontSanitizeScenes = true;

        try
        {
            SceneHelper.LoadScene(path);
        }
        catch (Exception ex)
        {
            // i hate using trycatch but if this isnt set back to false, every unmodded scene load will fail
            Plugin.Log.LogError(ex.ToString());
        }

        s_dontSanitizeScenes = false;
    }

    [HarmonyPatch(typeof(SceneHelper), nameof(SceneHelper.SanitizeLevelPath)), HarmonyPrefix]
    private static bool PreventSanitization(string scene, ref string __result)
    {
        if (s_dontSanitizeScenes)
        {
            __result = scene;
            return false;
        }

        return true;
    }
}
