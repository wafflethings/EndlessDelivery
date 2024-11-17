using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BuildPipeline.Editor.Building;
using EndlessDelivery.Cosmetics.Skins;
using EndlessDelivery.Gameplay.EnemyGeneration;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class GenerateDataFile
{
    [MenuItem("EndlessDelivery/Generate Data File")]
    private static void CreateDataFile()
    {
        Dictionary<string, List<string>> dataInfo = new Dictionary<string, List<string>>();
        AddData<Scene>(dataInfo, "Assets/Delivery/Scenes");
        AddData<EnemyGroup>(dataInfo, "Assets/Delivery/ScriptableObjects/EnemyGroups");
        AddData<BaseSkin>(dataInfo, "Assets/Delivery/ScriptableObjects/Skins");

        using (StreamWriter writer = new StreamWriter(File.OpenWrite(Path.Combine("Built Bundles", "data.json"))))
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(new JsonTextWriter(writer) { Formatting = Formatting.Indented }, dataInfo);
        }
    }

    private static void AddData<T>(Dictionary<string, List<string>> dataInfo, string assetsPath)
    {
        List<string> value = Directory.GetFiles(assetsPath).Where((path) => !path.EndsWith(".meta")).Select((path) => path.Replace('\\', '/')).ToList();
        dataInfo.Add(typeof(T).FullName, value);
    }
}
