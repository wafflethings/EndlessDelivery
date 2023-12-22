using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EndlessDelivery.Assets;
using EndlessDelivery.Gameplay;
using EndlessDelivery.Utils;
using HarmonyLib;
using UnityEngine;

namespace EndlessDelivery.Components
{
    [PatchThis($"{Plugin.GUID}.Present")]
    public class Present : MonoBehaviour
    {
        private static List<Collider> _allPresentColliders = new();
        
        public WeaponVariant VariantColour;
        public GameObject Particles;
        private ItemIdentifier _item;
        private Collider _collider;
        [HideInInspector] public bool Destroyed;
        
        private Color _colour => ColorBlindSettings.Instance.variationColors[(int)VariantColour];

        private void Start()
        {
            _collider = GetComponent<Collider>();
            _item = GetComponent<ItemIdentifier>();
            _allPresentColliders.Add(_collider);
            SetColour(VariantColour);
        }

        private void OnDisable()
        {
            _allPresentColliders.Remove(_collider);
        }
        
        private void OnDestroy()
        {
            _allPresentColliders.Remove(_collider);
        }

        public void SetColour(WeaponVariant colour)
        {
            VariantColour = colour;
            GetComponent<Renderer>().material.color = _colour;
        }

        public void Deliver(Chimney chimney)
        {
            chimney.Room.Deliver(VariantColour);
            GameManager.Instance.AddTime(5, $"<color=#{ColorUtility.ToHtmlStringRGB(_colour)}>DELIVERY</color>");
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

        [HarmonyPatch(typeof(Explosion), nameof(Explosion.Start)), HarmonyPostfix]
        private static void DisableExplosionToPresentCollisions(Explosion __instance)
        {
            if (!AddressableManager.InSceneFromThisMod)
            {
                return;
            }

            foreach (Collider collider in _allPresentColliders)
            {
                Physics.IgnoreCollision(collider, __instance.scol);
            }
        }
    }
}