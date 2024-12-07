using System;
using EndlessDelivery.Utils;
using UnityEngine;

namespace EndlessDelivery.Gameplay.Firework;

public class FireworkManager : MonoSingleton<FireworkManager>
{
    private const float FireworkInterval = 2;
    [SerializeField] private GameObject _firework;
    [SerializeField] private float _spawnDistance = 5;
    [SerializeField] private float _heightOverPlayer = 15;
    private float _timer;

    private void OnEnable()
    {
        _timer = 0;
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        if (_timer > FireworkInterval)
        {
            SpawnFirework();
            _timer = 0;
        }
    }

    private void SpawnFirework()
    {
        Vector3 position = (NewMovement.Instance.transform.position + (UnityEngine.Random.onUnitSphere * _spawnDistance)).Only(Axis.X, Axis.Z) + (transform.up * _heightOverPlayer);
        GameObject firework = Instantiate(_firework, position, Quaternion.identity);
    }
}
