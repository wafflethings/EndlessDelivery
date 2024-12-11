using System;
using System.Collections.Generic;
using System.Linq;
using AtlasLib.Utils;
using EndlessDelivery.Components;
using EndlessDelivery.Gameplay.EnemyGeneration;
using EndlessDelivery.Gameplay.SpecialWaves;
using EndlessDelivery.Utils;
using HarmonyLib;
using UnityEngine;

namespace EndlessDelivery.Gameplay;

public class Room : MonoBehaviour
{
    public GameObject SpawnPoint;
    public ActivateArena Arena;
    public EnemySpawnPoint[] SpawnPoints;
    public bool RoomHasGameplay = true;
    [HideInInspector] public bool RoomAlreadyVisited;

    [Space(15)] [Header("Make sure that you have 4 chimneys, one for each colour.")]
    public List<Chimney> Chimneys = new();

    [Space(15)] [Header("Make sure that you have 10 presents (10 is the maximum, random ones will be chosen)")]
    public List<Present> Presents = new();

    [HideInInspector] public int[] PresentColourAmounts = { 0, 0, 0, 0 };
    [HideInInspector] public List<Collider> EnvColliders = new();
    [HideInInspector] public List<EnemyIdentifier> Enemies = new();
    [HideInInspector] public bool RoomCleared;
    [HideInInspector] public bool RoomActivated;

    public Dictionary<WeaponVariant, Chimney> AllChimneys = new() { { WeaponVariant.BlueVariant, null }, { WeaponVariant.GreenVariant, null }, { WeaponVariant.RedVariant, null }, { WeaponVariant.GoldVariant, null } };
    public Dictionary<WeaponVariant, int> AmountDelivered = new() { { WeaponVariant.BlueVariant, 0 }, { WeaponVariant.GreenVariant, 0 }, { WeaponVariant.RedVariant, 0 }, { WeaponVariant.GoldVariant, 0 } };

    private Dictionary<EnemyType, int> _amountSpawned = new();
    private int _pointsLeft;
    private int _meleeSpawnsUsed;
    private int _projectileSpawnsUsed;
    private int _wavesSinceSpecial; // this is special ENEMIES
    private SpecialWave? _specialWave;

    public bool AllEnemiesSpawned => Arena.currentEnemy == Arena.enemies.Length;
    public bool ChimneysDone => AmountDelivered.All(kvp => PresentColourAmounts[(int)kvp.Key] <= kvp.Value);

    public bool Done(WeaponVariant colour)
    {
        return AmountDelivered[colour] == PresentColourAmounts[(int)colour];
    }

    public void Deliver(WeaponVariant colour)
    {
        GameManager.Instance.DeliveredPresents++;
        AmountDelivered[colour]++;

        if (ChimneysDone)
        {
            CompleteRoom();
        }
    }

    private void OnDestroy()
    {
        if (_specialWave != null)
        {
            EndSpecialWave();
        }
    }

    private void CompleteRoom()
    {
        if (_specialWave != null)
        {
            EndSpecialWave();
        }

        foreach (Chimney chimney in AllChimneys.Values)
        {
            chimney?.JumpPad.gameObject.SetActive(false);
        }
    }

    private void EndSpecialWave()
    {
        _specialWave?.End();
        _specialWave = null;
    }

    public void Initialize()
    {
        if (RoomHasGameplay)
        {
            PresentColourAmounts = GenerationEquations.DistributeBetween(4, GenerationEquations.PresentAmount(GameManager.Instance.RoomsComplete));
        }

        _pointsLeft = GameManager.Instance.PointsPerWave;

        DecideChimneyColours();
        DecidePresentColours();
        DecideSpecialWave();
        DecideSpawnPointEnemies();

        for (int i = 0; i < PresentColourAmounts.Length; i++)
        {
            Chimney chimney = AllChimneys[(WeaponVariant)i];

            if (chimney != null)
            {
                chimney.AmountToDeliver = PresentColourAmounts[i];
            }
        }

        if (ChimneysDone)
        {
            CompleteRoom();
        }

        foreach (Collider col in GetComponentsInChildren<Collider>(true))
        {
            if (LayerMaskDefaults.Get(LMD.Environment).Contains(col.gameObject.layer))
            {
                EnvColliders.Add(col);
            }
        }
    }

    private void DecideSpecialWave()
    {
        Plugin.Log.LogMessage($"Deciding: {GameManager.Instance.RoomsComplete >= GameManager.SpecialWaveStart} && {GameManager.Instance.RoomsComplete % GameManager.SpecialWaveInterval} == 0");
        if (GameManager.Instance.RoomsComplete < GameManager.SpecialWaveStart || GameManager.Instance.RoomsComplete % GameManager.SpecialWaveInterval != 0)
        {
            return;
        }

        _specialWave = GameManager.Instance.PickSpecialWave();
        _specialWave.Start();
        _pointsLeft -= _specialWave.Cost;

        ResetNamePopup();
        LevelNamePopup popup = LevelNamePopup.Instance;
        popup.nameString = _specialWave.Name;
        popup.layerString = "-- SPECIAL WAVE --";
        popup.NameAppear();
    }

    private void ResetNamePopup()
    {
        LevelNamePopup popup = LevelNamePopup.Instance;
        popup.layerText.color = Color.white;
        popup.nameText.color = Color.white;
        popup.layerText.text = string.Empty;
        popup.nameText.text = string.Empty;
        popup.activated = false;
    }

    private void DecideChimneyColours()
    {
        WeaponVariant colour = 0;

        foreach (Chimney chimney in Chimneys.Shuffle())
        {
            AllChimneys[colour] = chimney;
            chimney.SetColour(colour);
            chimney.Room = this;
            colour++;
        }
    }

    private void DecidePresentColours()
    {
        Dictionary<WeaponVariant, int> amounts = new();
        for (int i = 0; i < PresentColourAmounts.Length; i++)
        {
            if (PresentColourAmounts[i] != 0)
            {
                amounts.Add((WeaponVariant)i, PresentColourAmounts[i]);
            }
        }

        foreach (Present present in Presents.Shuffle())
        {
            if (amounts.Count != 0)
            {
                KeyValuePair<WeaponVariant, int> pair = amounts.ToList().Pick();
                present.SetColour(pair.Key);
                amounts[pair.Key]--;

                if (amounts[pair.Key] == 0)
                {
                    amounts.Remove(pair.Key);
                }
            }
            else
            {
                present.gameObject.SetActive(false);
            }
        }
    }

    private void DecideSpawnPointEnemies()
    {
        List<EnemySpawnPoint> projectileSpawns = SpawnPoints.Where(sp => sp.Class == DeliveryEnemyClass.Projectile).ShuffleAndToList();
        List<EnemySpawnPoint> meleeSpawns = SpawnPoints.Where(sp => sp.Class == DeliveryEnemyClass.Melee).ShuffleAndToList();

        if (GameManager.Instance.RoomsComplete > 10)
        {
            int maxUncommonsAndSpecials = (GameManager.Instance.RoomsComplete / 10) + 1;
            DecideUncommons(meleeSpawns, ref maxUncommonsAndSpecials);

            if (GameManager.Instance.RoomsComplete >= 15)
            {
                DecideSpecials(meleeSpawns, ref maxUncommonsAndSpecials);
            }
        }

        DecideNormalMeleeAndProjectile(projectileSpawns, meleeSpawns);
    }

    private void DecideUncommons(List<EnemySpawnPoint> spawns, ref int max)
    {
        int uncommonAmount = UnityEngine.Random.Range(1, max + 1);
        max -= uncommonAmount;

        List<EndlessEnemy> uncommons = GetPotentialEnemies(EnemyGroup.Groups[DeliveryEnemyClass.Uncommon]).ShuffleAndToList();

        foreach (EndlessEnemy enemy in uncommons)
        {
            Plugin.Log.LogInfo($"    uc{enemy.prefab.name}");
        }

        while (uncommons.Count > 2)
        {
            uncommons.RemoveAt(uncommons.Count - 1);
        }

        if (uncommons.Count == 0)
        {
            return;
        }

        if (uncommons.Count == 1)
        {
            uncommons.Add(uncommons[0]);
        }

        int[] amounts = [0, 0];

        for (int i = 0; i < uncommonAmount; i++)
        {
            if (_meleeSpawnsUsed == spawns.Count)
            {
                break;
            }

            if (GetRealCost(uncommons[0]) >= _pointsLeft && GetRealCost(uncommons[1]) >= _pointsLeft)
            {
                break;
            }

            EnemyType[] banned = spawns[_meleeSpawnsUsed].BannedEnemies;
            bool enemyOneBanned = banned.Contains(uncommons[0].enemyType);
            bool enemyTwoBanned = banned.Contains(uncommons[1].enemyType);

            int whichUncommon = UnityEngine.Random.value > 0.5f ? 0 : 1;

            if (GetRealCost(uncommons[0]) >= _pointsLeft || enemyOneBanned)
            {
                whichUncommon = 1;
            }

            if (GetRealCost(uncommons[1]) >= _pointsLeft || enemyTwoBanned)
            {
                whichUncommon = 0;
            }

            if (!enemyOneBanned && !enemyTwoBanned)
            {
                amounts[whichUncommon]++;
                SetSpawnPoint(spawns[_meleeSpawnsUsed], uncommons[whichUncommon], DeliveryEnemyClass.Melee);
                continue;
            }

            EndlessEnemy? fallbackEnemy = FindInPools(spawns[_meleeSpawnsUsed].Fallback);

            if (fallbackEnemy == null)
            {
                Plugin.Log.LogWarning($"Couldn't find fallback enemy with type (uncommon) {spawns[_meleeSpawnsUsed].Fallback}");
                continue;
            }

            SetSpawnPoint(spawns[_meleeSpawnsUsed], fallbackEnemy, DeliveryEnemyClass.Melee);
        }
    }

    private EndlessEnemy? FindInPools(EnemyType type)
    {
        foreach (KeyValuePair<DeliveryEnemyClass, EnemyGroup> group in EnemyGroup.Groups)
        {
            foreach (EndlessEnemy enemy in group.Value.Enemies)
            {
                if (enemy.enemyType == type)
                {
                    return enemy;
                }
            }
        }

        return null;
    }

    private void DecideSpecials(List<EnemySpawnPoint> spawns, ref int max)
    {
        int specialAmount = UnityEngine.Random.Range(0, max + 1);
        if (specialAmount == 0)
        {
            if (UnityEngine.Random.value < 1f - (1f / _wavesSinceSpecial))
            {
                specialAmount++;
            }

            _wavesSinceSpecial++;
        }

        max -= specialAmount;

        for (int i = 0; i < specialAmount; i++)
        {
            if (_meleeSpawnsUsed == spawns.Count)
            {
                break;
            }

            List<EndlessEnemy> specials = GetPotentialEnemies(EnemyGroup.Groups[DeliveryEnemyClass.Special]).ShuffleAndToList();
            if (specials.Count == 0)
            {
                break;
            }

            SetSpawnPoint(spawns[_meleeSpawnsUsed], specials[0], DeliveryEnemyClass.Melee);
        }
    }

    // this fills in the leftovers; what hasnt been used for specials or uncommons
    private void DecideNormalMeleeAndProjectile(List<EnemySpawnPoint> projectile, List<EnemySpawnPoint> melee)
    {
        while (_pointsLeft != 0)
        {
            if (_projectileSpawnsUsed == projectile.Count && _meleeSpawnsUsed == melee.Count)
            {
                break;
            }

            DeliveryEnemyClass type = UnityEngine.Random.value < 0.5 ? DeliveryEnemyClass.Melee : DeliveryEnemyClass.Projectile;

            if (_projectileSpawnsUsed >= projectile.Count)
            {
                type = DeliveryEnemyClass.Melee;
            }

            if (_meleeSpawnsUsed >= melee.Count)
            {
                type = DeliveryEnemyClass.Projectile;
            }

            IEnumerable<EndlessEnemy> enemies = GetPotentialEnemies(EnemyGroup.Groups[type]);
            EndlessEnemy randomEnemy = enemies.ToList().Pick();

            if (randomEnemy == null)
            {
                //enemy is only null when the list provided to pick has no items
                break;
            }

            EnemySpawnPoint spawnPoint = type == DeliveryEnemyClass.Melee ? melee[_meleeSpawnsUsed] : projectile[_projectileSpawnsUsed];
            SetSpawnPoint(spawnPoint, randomEnemy, type);
        }
    }

    private IEnumerable<EndlessEnemy> GetPotentialEnemies(EnemyGroup group)
    {
        foreach (EndlessEnemy enemy in group.Enemies)
        {
            if (enemy.spawnWave <= GameManager.Instance.RoomsComplete && GetRealCost(enemy) <= _pointsLeft)
            {
                yield return enemy;
            }
        }
    }

    private void SetSpawnPoint(EnemySpawnPoint point, EndlessEnemy enemy, DeliveryEnemyClass spawnType)
    {
        if (point.BannedEnemies.Contains(enemy.enemyType))
        {
            enemy = FindInPools(point.Fallback);

            if (enemy == null)
            {
                Plugin.Log.LogWarning($"Tried to set fallback to {point.Fallback} but couldn't find it in pools");
                return;
            }
        }

        point.Room = this;
        point.Enemy = enemy.prefab;
        _pointsLeft -= GetRealCost(enemy);

        if (!_amountSpawned.ContainsKey(enemy.enemyType))
        {
            _amountSpawned.Add(enemy.enemyType, 0);
        }

        _amountSpawned[enemy.enemyType]++;

        switch (spawnType)
        {
            case DeliveryEnemyClass.Melee:
                _meleeSpawnsUsed++;
                break;

            case DeliveryEnemyClass.Projectile:
                _projectileSpawnsUsed++;
                break;
        }

        //i hope this isnt too slow
        Arena.enemies = Arena.enemies.AddToArray(point.gameObject);
    }

    private int GetRealCost(EndlessEnemy enemy)
    {
        int amountSpawned = _amountSpawned.ContainsKey(enemy.enemyType) ? _amountSpawned[enemy.enemyType] : 0;
        return enemy.spawnCost + (enemy.costIncreasePerSpawn * amountSpawned);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.GetComponent<NewMovement>() != null && !RoomActivated)
        {
            GameManager.Instance.SetRoom(this);
            Arena?.Activate();
            RoomActivated = true;
        }
    }
}
