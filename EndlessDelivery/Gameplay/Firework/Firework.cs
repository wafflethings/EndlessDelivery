using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace EndlessDelivery.Gameplay.Firework;

public class Firework : MonoBehaviour
{
    [SerializeField] private SphereCollider _collider;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private GameObject _warningPrefab;
    [SerializeField] private float _speed;
    private GameObject _warning;
    private Vector3 _raycastPoint;

    private void Awake()
    {
        float timeLeft = Vector3.Distance(transform.position, NewMovement.Instance.transform.position) / _speed;
        Vector3 targetPos = PlayerTracker.Instance.PredictPlayerPosition(timeLeft) + new Vector3(UnityEngine.Random.Range(-4f, 4f), 0, UnityEngine.Random.Range(-4f, 4f));
        timeLeft = Vector3.Distance(transform.position, targetPos) / _speed;
        transform.forward = targetPos - transform.position;

        if (Physics.SphereCast(transform.position + transform.forward * 3, _collider.radius, transform.forward, out RaycastHit hit, 100, 1 << 8))
        {
            Plugin.Log.LogMessage($"Spherecast hit {hit.point}, obj {hit.collider.gameObject.name}");
            _warning = Instantiate(_warningPrefab);
            _warning.GetComponent<ScaleNFade>().scaleSpeed = (-1f / timeLeft) * _warning.transform.localScale.x * 0.9f;
            _raycastPoint = hit.point;
            _warning.transform.position = _raycastPoint;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SetEnvCollisions(false);
    }

    private void Update()
    {
        _rigidbody.velocity = transform.forward * _speed;

        if ((transform.position - _raycastPoint).sqrMagnitude < 0.25)
        {
            GetComponent<Grenade>().Explode();
        }
    }

    private void OnDestroy()
    {
        Destroy(_warning);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Room>() == null)
        {
            return;
        }

        StartCoroutine(EnableAfterTime());
    }

    private IEnumerator EnableAfterTime()
    {
        yield return new WaitForSeconds(0.4f);
        SetEnvCollisions(true);
    }

    private void SetEnvCollisions(bool collisionsEnabled)
    {
        foreach (Collider collider in GameManager.Instance.CurrentRoom.EnvColliders)
        {
            Physics.IgnoreCollision(_collider, collider, !collisionsEnabled);
        }
    }
}
