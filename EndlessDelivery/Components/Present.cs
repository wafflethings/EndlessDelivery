using System.Collections;
using UnityEngine;

namespace EndlessDelivery.Components
{
    public class Present : MonoBehaviour
    {
        public WeaponVariant VariantColour;
        private ItemIdentifier _item;
        [HideInInspector] public bool Destroyed;
        
        private Color _colour => ColorBlindSettings.Instance.variationColors[(int)VariantColour];

        private void Start()
        {
            _item = GetComponent<ItemIdentifier>();
            GetComponent<Renderer>().material.color = _colour;
        }

        public void Destroy()
        {
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

            StartCoroutine(DestroyAnimation());
        }

        private IEnumerator DestroyAnimation()
        {
            yield return new WaitForSeconds(0.25f);
            Destroy(GetComponent<Rigidbody>());
            
            while (transform.localScale != Vector3.zero)
            {
                transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.zero, Time.deltaTime * 2);
                yield return null;
            }
            
            Destroy(gameObject);
        }
    }
}