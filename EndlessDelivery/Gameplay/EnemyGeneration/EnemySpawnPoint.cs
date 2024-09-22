using UnityEngine;

namespace EndlessDelivery.Gameplay.EnemyGeneration;

public class EnemySpawnPoint : MonoBehaviour
{
    [Header("SpawnPoints should only be Melee, Projectile, or Boss.")]
    public DeliveryEnemyClass Class;

    [HideInInspector] public GameObject Enemy;
    [HideInInspector] public Room Room;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        Room.Enemies.Add(Instantiate(Enemy, transform).GetComponent<EnemyIdentifier>());
    }
}
