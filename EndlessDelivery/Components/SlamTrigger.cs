using System;
using UnityEngine;
using UnityEngine.Events;

namespace EndlessDelivery.Components;

public class SlamTrigger : MonoBehaviour
{
    [SerializeField] private UnityEvent _event;

    private void OnTriggerEnter(Collider other)
    {
        if (other != NewMovement.Instance.playerCollider)
        {
            return;
        }

        if (NewMovement.Instance.gc.heavyFall)
        {
            _event.Invoke();
        }
    }
}
