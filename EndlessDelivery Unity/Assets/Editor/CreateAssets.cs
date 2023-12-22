using System.IO;
using System.Reflection;
using EndlessDelivery.Gameplay;
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
    
        [MenuItem("Assets/Create/EndlessDelivery/RoomData")]
        public static void CreateRoomData()
        {
            if (TryGetActiveFolderPath(out string path))
            {
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<RoomData>(), Path.Combine(path, "New Room Data.asset"));
            }
        }
    
        [MenuItem("Assets/Create/EndlessDelivery/RoomPool")]
        public static void CreateRoomPool()
        {
            if (TryGetActiveFolderPath(out string path))
            {
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<RoomPool>(), Path.Combine(path, "New Room Pool.asset"));
            }
        }
}