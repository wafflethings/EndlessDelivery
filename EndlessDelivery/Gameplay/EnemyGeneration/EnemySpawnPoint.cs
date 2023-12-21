using System;
using UnityEngine;
using EndlessDelivery.Utils;

namespace EndlessDelivery.Gameplay.EnemyGeneration
{
    public class EnemySpawnPoint : MonoBehaviour
    {
        [Header("SpawnPoints should only be Melee, Projectile, or Boss.")]
        public DeliveryEnemyClass Class;
        [HideInInspector] public GameObject Enemy;

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            Instantiate(Enemy, transform);
        }
    }
}