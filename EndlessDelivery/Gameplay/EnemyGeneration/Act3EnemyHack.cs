using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EndlessDelivery.Gameplay.EnemyGeneration;

/// <summary>
/// This WILL be deleted.
/// I can't reference act 3 enemy EndlessEnemy SOs as my Tundra version doesn't have them (yet).
/// I have to add the official ones.
/// </summary>
public static class Act3EnemyHack
{
    private static bool _hasAdded = false;

    public static void AddToPools(params EnemyGroup[] groups)
    {
        if (_hasAdded)
        {
            return;
        }

        _hasAdded = true;

        PrefabDatabase database = Addressables.LoadAssetAsync<PrefabDatabase>("Assets/Data/Cyber Grind Patterns/Data/Prefab Database.asset").WaitForCompletion();
        foreach (EnemyGroup group in groups)
        {
            AddEnemies(group, database);
        }
    }

    private static void AddEnemies(EnemyGroup group, PrefabDatabase database)
    {
        List<EndlessEnemy> enemies = group.Enemies.ToList();

        switch (group.Class)
        {
            case DeliveryEnemyClass.Projectile:
                foreach (EndlessEnemy enemy in database.projectileEnemies)
                {
                    if (enemy.enemyType is EnemyType.Gutterman or EnemyType.Mannequin)
                    {
                        Debug.Log($"Added {enemy.enemyType} to {group.name}!");
                        enemies.Add(enemy);
                    }
                }

                break;

            case DeliveryEnemyClass.Uncommon:
                foreach (EndlessEnemy enemy in database.uncommonEnemies)
                {
                    if (enemy.enemyType == EnemyType.Guttertank)
                    {
                        Debug.Log($"Added {enemy.enemyType} to {group.name}!");
                        enemies.Add(enemy);
                    }
                }

                break;
        }

        group.Enemies = enemies.ToArray();
    }
}
