using UnityEngine;

namespace EndlessDelivery.Gameplay.EnemyGeneration;

public class EnemySpawnPoint : MonoBehaviour
{
    [Header("SpawnPoints should only be Melee, Projectile, or Boss.")]
    public DeliveryEnemyClass Class;

    [Header("Banned enemies - will use fallback if none available.")]
    [Header("Fallback has no cost, so should be something cheap and common.")]
    public EnemyType[] BannedEnemies = [];
    public EnemyType Fallback = EnemyType.Filth;

    [HideInInspector] public GameObject Enemy;
    [HideInInspector] public Room Room;
    [HideInInspector] public bool Radiant;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        EnemyIdentifier enemy = Instantiate(Enemy, transform).GetComponentInChildren<EnemyIdentifier>(true);
        GameManager.Instance.RegisterEnemySpawned(enemy);
    }
}
