using System.Collections;
using System.Collections.Generic;
using System.IO;
using EndlessDelivery.Cosmetics.Skins;
using EndlessDelivery.Gameplay.EnemyGeneration;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class BuildRoomNavmesh
{
    private const string RoomsFolder = "Assets/Delivery/Prefabs/Rooms";
    private const string NavmeshFolder = RoomsFolder + "/Navmeshes/";

    [MenuItem("EndlessDelivery/Build Room Navmeshes")]
    private static void BuildRoomNavmeshes()
    {
        foreach (string fileName in Directory.GetFiles(RoomsFolder))
        {
            if (!fileName.EndsWith(".prefab"))
            {
                continue;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(fileName)) as GameObject;

            try
            {
                if (!instance.TryGetComponent(out NavMeshSurface surface))
                {
                    Debug.LogWarning($"Room {instance.name} has no NavMeshSurface");
                    Object.DestroyImmediate(instance, false);
                    continue;
                }

                string navmeshDataAssetPath = NavmeshFolder + instance.name + ".asset";

                if (File.Exists(navmeshDataAssetPath))
                {
                    File.Delete(navmeshDataAssetPath);
                }

                surface.BuildNavMesh();

                if (!Directory.Exists(NavmeshFolder))
                {
                    Directory.CreateDirectory(NavmeshFolder);
                }

                AssetDatabase.CreateAsset(surface.navMeshData, navmeshDataAssetPath);
                surface.navMeshData = AssetDatabase.LoadAssetAtPath<NavMeshData>(navmeshDataAssetPath);
                PrefabUtility.ApplyPrefabInstance(instance, InteractionMode.AutomatedAction);
                Object.DestroyImmediate(instance, false);
            }
            catch
            {
                Object.DestroyImmediate(instance, false);
            }
        }
    }
}
