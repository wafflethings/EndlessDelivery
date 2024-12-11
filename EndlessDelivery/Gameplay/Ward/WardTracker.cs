using System;
using System.Linq;
using UnityEngine;

namespace EndlessDelivery.Gameplay.Ward;

public class WardTracker : MonoBehaviour
{
    private EnemyIdentifier _eid;
    private Ward? _previousWard;

    private void Awake()
    {
        _eid = GetComponent<EnemyIdentifier>();
    }

    private void FixedUpdate()
    {
        if (_eid == null)
        {
            return;
        }

        Vector3 centre = _eid.GetCenter()?.position ?? transform.position;
        Ward? currentWard = WardManager.Instance.AllWards.FirstOrDefault(x => x.InsideWard(centre));

        if (currentWard != _previousWard)
        {
            _previousWard?.RemoveEnemy(_eid);
            currentWard?.AddEnemy(_eid);
        }

        _previousWard = currentWard;
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
