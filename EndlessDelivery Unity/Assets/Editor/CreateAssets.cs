using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using EndlessDelivery.Gameplay.EnemyGeneration;

public class CreateAssets
{
    public static bool TryGetActiveFolderPath(out string path)
    {
        MethodInfo tryGetActiveFolderPath = typeof(ProjectWindowUtil).GetMethod("TryGetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);

        object[] args = { null };
        bool found = (bool)tryGetActiveFolderPath.Invoke(null, args);
        path = (string)args[0];

        return found;
    }

    [MenuItem("Assets/Create/EndlessDelivery/EnemyGroup")]
    public static void CreateEnemyGroup()
    {
        if (TryGetActiveFolderPath(out string path))
        {
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<EnemyGroup>(), Path.Combine(path, "New Enemy Group.asset"));
        }
    }
}