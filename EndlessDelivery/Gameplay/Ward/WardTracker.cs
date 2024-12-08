using System;
using System.Linq;
using UnityEngine;

namespace EndlessDelivery.Gameplay.Ward;

public class WardTracker : MonoBehaviour
{
    private EnemyIdentifier _eid;

    private void Awake()
    {
        _eid = GetComponent<EnemyIdentifier>();
    }

    private void FixedUpdate()
    {
        if (_eid == null)
        {
            Destroy(this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out Ward ward))
        {
            return;
        }

        ward.AddEnemy(_eid);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent(out Ward ward))
        {
            return;
        }

        ward.RemoveEnemy(_eid);
    }
}
