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
public static class EnemyHack
{
    private static bool s_hasAdded = false;

    public static void AddToPools(params EnemyGroup[] groups)
    {
        if (s_hasAdded)
        {
            return;
        }

        s_hasAdded = true;

        PrefabDatabase database = Addressables.LoadAssetAsync<PrefabDatabase>("Assets/Data/Cyber Grind Patterns/Data/Prefab Database.asset").WaitForCompletion();
        foreach (EnemyGroup group in groups)
        {
            AddEnemies(group, database);
        }
    }

    private static void AddEnemies(EnemyGroup group, PrefabDatabase database)
    {
        List<EndlessEnemy> enemies = group.Enemies.ToList();
        enemies.Clear();

        switch (group.Class)
        {
            case DeliveryEnemyClass.Projectile:
                foreach (EndlessEnemy enemy in database.projectileEnemies)
                {
                    Plugin.Log.LogInfo($"Added {enemy.enemyType} to {group.name}!");
                    enemies.Add(enemy);
                }
                break;

            case DeliveryEnemyClass.Uncommon:
                foreach (EndlessEnemy enemy in database.uncommonEnemies)
                {
                    if (enemy.enemyType != EnemyType.Idol)
                    {
                        Plugin.Log.LogInfo($"Added {enemy.enemyType} to {group.name}!");
                        enemies.Add(enemy);
                    }
                }
                break;

            case DeliveryEnemyClass.Melee:
                foreach (EndlessEnemy enemy in database.meleeEnemies)
                {
                    Plugin.Log.LogInfo($"Added {enemy.enemyType} to {group.name}!");
                    enemies.Add(enemy);
                }
                break;

            case DeliveryEnemyClass.Special:
                foreach (EndlessEnemy enemy in database.specialEnemies)
                {
                    if (enemy.enemyType != EnemyType.Sisyphus)
                    {
                        Plugin.Log.LogInfo($"Added {enemy.enemyType} to {group.name}!");
                        enemies.Add(enemy);
                    }
                }
                break;
        }

        group.Enemies = enemies.ToArray();
    }
}
