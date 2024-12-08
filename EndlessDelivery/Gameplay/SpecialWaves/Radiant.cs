using System.Collections.Generic;
using UnityEngine;

namespace EndlessDelivery.Gameplay.SpecialWaves;

public class Radiant : SpecialWave
{
    public override string Name => "RADIANT";
    public override int Cost => 10;


    private static Dictionary<EnemyType, float> s_typeOdds = new()
    {
        { EnemyType.Cerberus, 0.8f },
        { EnemyType.Ferryman, 0.4f },
        { EnemyType.Gutterman, 0.6f },
        { EnemyType.Guttertank, 0.4f },
        { EnemyType.Idol, 1 },
        { EnemyType.Stalker, 1},
        { EnemyType.MaliciousFace, 0.8f },
        { EnemyType.Mindflayer, 0.6f },
        { EnemyType.Swordsmachine, 0.8f },

        { EnemyType.Virtue, 0.6f },
        { EnemyType.Turret, 0.6f },
        { EnemyType.Filth, 0.5f },
        { EnemyType.Stray, 0.5f },
        { EnemyType.Schism, 0.5f },
    };

    public override void Start()
    {
        GameManager.Instance.EnemySpawned += OnEnemySpawned;
    }

    public override void End()
    {
        GameManager.Instance.EnemySpawned -= OnEnemySpawned;
    }

    private void OnEnemySpawned(EnemyIdentifier enemy)
    {
        if (enemy == null)
        {
            return;
        }

        if (!s_typeOdds.TryGetValue(enemy.enemyType, out float odds) || Random.value <= odds)
        {
            return;
        }

        enemy.HealthBuff();
        enemy.SpeedBuff();
    }
}
