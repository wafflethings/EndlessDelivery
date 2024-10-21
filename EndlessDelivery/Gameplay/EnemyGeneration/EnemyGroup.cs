using System.Collections.Generic;
using UnityEngine;

namespace EndlessDelivery.Gameplay.EnemyGeneration;

public class EnemyGroup : ScriptableObject
{
    public static Dictionary<DeliveryEnemyClass, EnemyGroup> Groups = new();

    public DeliveryEnemyClass Class;
    [Space(10)] public EndlessEnemy[] Enemies;

    public static void SetGroups(IEnumerable<EnemyGroup> groups)
    {
        foreach (EnemyGroup group in groups)
        {
            if (group == null)
            {
                Debug.LogError(group + " group was null!");
                continue;
            }
            Groups.Add(group.Class, group);
        }
    }
}
