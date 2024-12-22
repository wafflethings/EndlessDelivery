using System.Collections.Generic;
using UnityEngine;

namespace EndlessDelivery.Gameplay.SpecialWaves;

public class Radiant : SpecialWave
{
    private static Dictionary<EnemyType, float> s_typeCost = new()
    {
        { EnemyType.Cerberus, 0.4f },
        { EnemyType.Ferryman, 1f },
        { EnemyType.Gutterman, 1.2f },
        { EnemyType.Guttertank, 1.5f },
        { EnemyType.Idol, 0 },
        { EnemyType.Stalker, 0},
        { EnemyType.MaliciousFace, 0.4f },
        { EnemyType.Mindflayer, 1.5f },
        { EnemyType.Swordsmachine, 0.4f },

        { EnemyType.Virtue, 0.6f },
        { EnemyType.Turret, 0.6f },
        { EnemyType.Filth, 0.1f },
        { EnemyType.Stray, 0.2f },
        { EnemyType.Schism, 0.2f },
    };
    private static Dictionary<EnemyType, int> s_typeCaps = new()
    {
        { EnemyType.Ferryman, 2 },
        { EnemyType.Gutterman, 2 },
        { EnemyType.Guttertank, 2 },
        { EnemyType.Mindflayer, 2 },
        { EnemyType.Swordsmachine, 3 },
    };

    private Dictionary<EnemyType, int> _currentAmounts = new();
    private const float StartCost = 4.5f;
    private float _remainingCost;

    public override string Name => "RADIANCE";
    public override int Cost => 15;

    public override void Start()
    {
        _remainingCost = StartCost;
        _currentAmounts.Clear();
        foreach (EnemyType type in s_typeCaps.Keys)
        {
            _currentAmounts.Add(type, 0);
        }
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

        if (!s_typeCost.TryGetValue(enemy.enemyType, out float cost))
        {
            return;
        }

        if (_remainingCost - cost < 0)
        {
            return;
        }

        if (s_typeCaps.ContainsKey(enemy.enemyType) && _currentAmounts[enemy.enemyType] >= s_typeCaps[enemy.enemyType])
        {
            return;
        }

        s_typeCaps[enemy.enemyType]++;

        Plugin.Log.LogMessage($"Spawned {enemy.enemyType}, remaining {_remainingCost}");
        _remainingCost -= cost;

        enemy.HealthBuff();
        enemy.SpeedBuff();
    }
}
