using System;
using EndlessDelivery.Utils;
using UnityEngine;

namespace EndlessDelivery.Components;

public class PresentDragZone : MonoBehaviour
{
    public float PullStrength;
    public Chimney Chimney;

    private void OnTriggerStay(Collider collider)
    {
        if (!collider.TryGetComponent(out Present present) || present.VariantColour != Chimney.VariantColour)
        {
            return;
        }

        Rigidbody rb = collider.GetComponent<Rigidbody>();
        Vector3 presentToChimney = (collider.transform.position - transform.position).Only(Axis.X, Axis.Z);
        Vector3 targetVelocity = (-presentToChimney.normalized * rb.velocity.magnitude) + (rb.velocity.y * Vector3.up * 1.25f);
        rb.velocity = Vector3.MoveTowards(rb.velocity, targetVelocity, Time.deltaTime * PullStrength);
    }
}
