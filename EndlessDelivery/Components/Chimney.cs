using System;
using System.Collections;
using EndlessDelivery.Gameplay;
using EndlessDelivery.Utils;
using TMPro;
using UnityEngine;

namespace EndlessDelivery.Components
{
    public class Chimney : MonoBehaviour
    {
        [field: SerializeField] public WeaponVariant VariantColour { get; private set; }
        public GameObject Glow;
        public TMP_Text DeliveredText;
        public int AmountToDeliver;
        public AudioSource DeliverSound;
        public Animator Animator;
        [HideInInspector] public Room Room;

        private Collider _collider;
        private Renderer _glowRenderer;
        private float _glowAlpha;
        private Coroutine _currentAnimation;

        private void Start()
        {
            _collider = GetComponent<Collider>();
            SetRenderer();
            SetColour(VariantColour);
        }

        private void SetRenderer()
        {
            _glowRenderer = Glow.GetComponent<Renderer>();
            _glowAlpha = _glowRenderer.material.GetFloat("_OpacScale");
        }

        public void SetColour(WeaponVariant colour)
        {
            if (_glowRenderer == null)
            {
                SetRenderer();
            }

            VariantColour = colour;
            _glowRenderer.material.color = ColorBlindSettings.Instance.variationColors[(int)VariantColour];
        }

        private void Update()
        {
            DeliveredText.text = $"{Room.AmountDelivered[VariantColour]}/{AmountToDeliver}";
        }

        public void DeliverEffect()
        {
            DeliverSound.Play();

            if (_currentAnimation != null)
            {
                StopCoroutine(_currentAnimation);
            }

            _currentAnimation = StartCoroutine(DeliverEffectAnimation());
        }

        private IEnumerator DeliverEffectAnimation()
        {
            Animator.Play("Glow Animation");

            yield return new WaitForSeconds((1 / 60f) * 10); //10frames

            Material glowMaterial = _glowRenderer.material;
            
            while (glowMaterial.GetFloat("_OpacScale") != 0)
            {
                glowMaterial.SetFloat("_OpacScale", Mathf.MoveTowards(glowMaterial.GetFloat("_OpacScale"), 0, Time.deltaTime * 2));
                yield return null;
            }

            if (!Room.Done(VariantColour))
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
                Vector3 target = DeliveredText.transform.localScale;
                target.y = 0;

                while (glowMaterial.GetFloat("_OpacScale") != _glowAlpha / 2)
                {
                    glowMaterial.SetFloat("_OpacScale", Mathf.MoveTowards(glowMaterial.GetFloat("_OpacScale"), _glowAlpha / 2, Time.deltaTime));
                    yield return null;
                }

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
                    present.Deliver(this);
                    DeliverEffect();
                }
            }
        }
    }
}