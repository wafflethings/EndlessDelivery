using System;
using EndlessDelivery.Utils;
using UnityEngine;

namespace EndlessDelivery.Gameplay.Firework;

[ConfigureSingleton(SingletonFlags.NoAutoInstance)]
public class FireworkManager : MonoSingleton<FireworkManager>
{
    private const float FireworkInterval = 1.5f;
    [SerializeField] private GameObject _firework;
    [SerializeField] private float _spawnDistance = 5;
    [SerializeField] private float _heightOverPlayer = 15;
    private float _timer;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false); // needs to be enabled to set .Instance
    }

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
        Vector3 position = (NewMovement.Instance.transform.position + (UnityEngine.Random.onUnitSphere * _spawnDistance)).Only(Axis.X, Axis.Z) + (transform.up * (NewMovement.Instance.transform.position.y + _heightOverPlayer + UnityEngine.Random.Range(0, 10f)));
        GameObject firework = Instantiate(_firework, position, Quaternion.identity);
    }
}
