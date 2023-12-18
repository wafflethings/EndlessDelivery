using System.Collections;
using EndlessDelivery.Gameplay;
using EndlessDelivery.Utils;
using UnityEngine;

namespace EndlessDelivery.Components
{
    public class Present : MonoBehaviour
    {
        public WeaponVariant VariantColour;
        public GameObject Particles;
        private ItemIdentifier _item;
        [HideInInspector] public bool Destroyed;
        
        private Color _colour => ColorBlindSettings.Instance.variationColors[(int)VariantColour];

        private void Start()
        {
            _item = GetComponent<ItemIdentifier>();
            SetColour(VariantColour);
        }

        public void SetColour(WeaponVariant colour)
        {
            VariantColour = colour;
            GetComponent<Renderer>().material.color = _colour;
        }

        public void Deliver(Chimney chimney)
        {
            chimney.Room.AmountDelivered[VariantColour]++;
            GameManager.Instance.AddTime(5);
            CreateParticles();
            Destroyed = true;
            
            if (_item.hooked)
            {
                HookArm.Instance.StopThrow(0, false);
            }

            if (FistControl.Instance.currentPunch.heldItem == _item)
            {
                FistControl.Instance.currentPunch.ForceThrow();
            }

            Destroy(_item);
            GetComponent<Collider>().enabled = false;
            GetComponent<Rigidbody>().detectCollisions = false;
            StartCoroutine(DestroyAnimation());
        }

        public void CreateParticles()
        {
            ParticleSystem particles = Instantiate(Particles, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
            particles.GetComponentInChildren<Light>().color = _colour;
            particles.startColor = _colour; //i dont care if this is deprecated, fuck unity
        }

        private IEnumerator DestroyAnimation()
        {
            yield return new WaitForSeconds(0.25f);
            
            while (transform.localScale != Vector3.zero)
            {
                transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.zero, Time.deltaTime * 2);
                yield return null;
            }
            
            Destroy(gameObject);
        }
        
        private void OnTriggerStay(Collider collider)
        {
            if (Destroyed)
            {
                return;
            }
            
            if (collider.TryGetComponent(out Chimney chimney) && chimney.VariantColour != VariantColour)
            {
                Vector3 playerVector = (NewMovement.Instance.transform.position - transform.position) * 2.5f;
                Vector3 newVelocity = (Vector3.up * 25) + (playerVector.Only(Axis.X, Axis.Z));
                GetComponent<Rigidbody>().velocity = newVelocity;
            }
        }
    }
}