using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace EndlessDelivery.Components
{
    public class Chimney : MonoBehaviour
    {
        public WeaponVariant VariantColour;
        public GameObject Glow;
        public TMP_Text DeliveredText;
        public int AmountToDeliver;
        public AudioSource DeliverSound;
        public Animator Animator;
        
        private Renderer _glowRenderer;
        private float _glowAlpha;
        private Coroutine _currentAnimation;
        private int _amountDelivered;

        public bool Done => _amountDelivered >= AmountToDeliver;
        private Color _colour => ColorBlindSettings.Instance.variationColors[(int)VariantColour];
        
        private void Start()
        {
            _glowRenderer = Glow.GetComponent<Renderer>();
            _glowAlpha = _glowRenderer.material.GetFloat("_OpacScale");
            _glowRenderer.material.color = _colour;
        }

        private void Update()
        {
            DeliveredText.text = $"{_amountDelivered}/{AmountToDeliver}";
        }

        public void Complete()
        {
            _amountDelivered++;
            DeliverSound.Play();
            
            if (_currentAnimation != null)
            {
                StopCoroutine(_currentAnimation);
            }

            _currentAnimation = StartCoroutine(CompleteAnimation());
        }

        private IEnumerator CompleteAnimation()
        {
            Animator.Play("Glow Animation");

           yield return new WaitForSeconds((1 / 60f) * 10); //10frames
           
           Material glowMaterial = _glowRenderer.material;
           
           
           while (glowMaterial.GetFloat("_OpacScale") != 0)
           {
               glowMaterial.SetFloat("_OpacScale", Mathf.MoveTowards(glowMaterial.GetFloat("_OpacScale"), 0, Time.deltaTime * 2));
               yield return null;
           }

           if (!Done)
           {
               yield return new WaitForSeconds(1f);

               while (glowMaterial.GetFloat("_OpacScale") != _glowAlpha)
               {
                   glowMaterial.SetFloat("_OpacScale", Mathf.MoveTowards(glowMaterial.GetFloat("_OpacScale"), _glowAlpha, Time.deltaTime));
                   yield return null;
               }
           }
           else
           {
               yield return new WaitForSeconds(0.25f);
               Vector3 target = DeliveredText.transform.localScale;
               target.y = 0;
               
               while (DeliveredText.transform.localScale.y != 0)
               {
                   DeliveredText.transform.localScale = Vector3.MoveTowards(DeliveredText.transform.localScale, target, Time.deltaTime * 2);
                   yield return null;
               }
           }
        }
        
        private void OnTriggerEnter(Collider collider)
        {
            if (collider.TryGetComponent(out Present present))
            {
                if (present.VariantColour == VariantColour && !present.Destroyed)
                {
                    present.Destroy();
                    Complete();
                }
                else
                {
                    Vector3 playerVector = (NewMovement.Instance.transform.position - present.transform.position) * 2.5f;
                    playerVector.y = 0;
                    Vector3 newVelocity = (Vector3.up * 25) + (playerVector);
                    present.GetComponent<Rigidbody>().velocity = newVelocity;
                }
            }
        }
    }
}