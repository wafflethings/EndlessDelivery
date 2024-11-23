using System.Collections;
using System.Collections.Generic;
using System.IO;
using EndlessDelivery.Cosmetics.Skins;
using EndlessDelivery.Gameplay.EnemyGeneration;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class BuildRoomNavmesh
{
    [MenuItem("EndlessDelivery/Build Room Navmeshes")]
    private static void BuildRoomNavmeshes()
    {
        foreach (string fileName in Directory.GetFiles("Assets/Delivery/Prefabs/Rooms"))
        {
            if (!fileName.EndsWith(".prefab"))
            {
                continue;
            }

            GameObject prefab = PrefabUtility.LoadPrefabContents(fileName);
            GameObject instance = Object.Instantiate(prefab);
            Debug.Log(instance.scene + "instance scene");

            if (!instance.TryGetComponent(out NavMeshSurface surface))
            {
                Debug.LogWarning($"Room {prefab.name} has no NavMeshSurface");
                continue;
            }

            surface.BuildNavMesh();
            prefab.GetComponent<NavMeshSurface>().navMeshData = surface.navMeshData;
            PrefabUtility.SaveAsPrefabAsset(prefab, fileName);
            Object.Destroy(instance);
        }
    }
}
