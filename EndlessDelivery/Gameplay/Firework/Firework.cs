using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace EndlessDelivery.Gameplay.Firework;

public class Firework : MonoBehaviour
{
    [SerializeField] private Collider _collider;
    [SerializeField] private GameObject _warningPrefab;
    [SerializeField] private float _speed;

    private void Awake()
    {
        float timeLeft = Vector3.Distance(transform.position, NewMovement.Instance.transform.position) / _speed;
        Vector3 targetPos = PlayerTracker.Instance.PredictPlayerPosition(timeLeft);
        transform.forward = targetPos - transform.position;
        GameObject warning = Instantiate(_warningPrefab);
        warning.GetComponent<ScaleNFade>().fadeSpeed = 1f / timeLeft;
        warning.transform.position = targetPos;
        SetEnvCollisions(false);
    }

    private void SetEnvCollisions(bool collide)
    {
        foreach (Collider collider in GameManager.Instance.CurrentRoom.EnvColliders)
        {
            if (collider == null)
            {
                continue;
            }

            Physics.IgnoreCollision(_collider, collider, !collide);
        }
    }

    private void Update()
    {
        transform.position += transform.forward * (Time.deltaTime * _speed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 0 || other.GetComponent<Room>() == null)
        {
            return;
        }

        StartCoroutine(EnableAfterTime());
    }

    private IEnumerator EnableAfterTime()
    {
        yield return new WaitForSeconds(0.1f);
        SetEnvCollisions(true);
    }
}
