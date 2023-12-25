using EndlessDelivery.Gameplay;
using EndlessDelivery.Gameplay.EnemyGeneration;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(Room))]
public class AddChildEnemies : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Add All Enemy Spawn Points"))
        {
            List<EnemySpawnPoint> points = ((Room)target).SpawnPoints.ToList();
            points.AddRange(((Room)target).GetComponentsInChildren<EnemySpawnPoint>());
            ((Room)target).SpawnPoints = points.ToArray();
        }
    }
}