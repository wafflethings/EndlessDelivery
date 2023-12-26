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
        public JumpPad JumpPad;
        public AudioSource DeliverSound;
        public Animator Animator;
        public GameObject Teleporter;
        public ParticleSystem Particles;
        [HideInInspector] public Room Room;

        private static Vector3 _tpOffset;
        private Renderer _glowRenderer;
        private float _glowAlpha;
        private Coroutine _currentAnimation;

        private void Start()
        {
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
            Particles.startColor = ColorBlindSettings.Instance.variationColors[(int)VariantColour];
            _glowRenderer.material.color = ColorBlindSettings.Instance.variationColors[(int)VariantColour];
        }

        private void Update()
        {
            if (DeliveredText == null || Room == null)
            {
                return;
            }

            DeliveredText.text = $"{Room.AmountDelivered[VariantColour]}/{AmountToDeliver}";
        }

        private void DeliverEffect()
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

            if (collider.GetComponent<NewMovement>() != null && Room.ChimneysDone)
            {
                GameManager.Instance.RoomEnd();
                ChimneyEnter();
            }

            if (collider.gameObject.TryGetComponent(out EnemyIdentifierIdentifier eidid) && (eidid.eid?.dead ?? true))
            {
                Destroy(collider.gameObject);
            }
        }
        
        public void ChimneyEnter()
        {
            _tpOffset = NewMovement.Instance.transform.position - Teleporter.transform.position;
            NewMovement.Instance.rb.velocity = NewMovement.Instance.rb.velocity.Only(Axis.Y);
            NewMovement.Instance.enabled = false;
            NewMovement.Instance.gc.enabled = false;
            
            foreach (Collider collider in GameManager.Instance.PreviousRoom.EnvColliders)
            {
                Physics.IgnoreCollision(NewMovement.Instance.playerCollider, collider, true);    
            }
            
            NewMovement.Instance.GetComponent<KeepInBounds>().enabled = false;

            if (Room.RoomHasGameplay)
            {
                if (Room.AllDead)
                {
                    GameManager.Instance.AddTime(12f, "<color=orange>FULL CLEAR</color>");
                }
                else
                {
                    GameManager.Instance.AddTime(8, "<color=orange>ROOM CLEAR</color>");
                }
            }
        }

        // called by event in Assets/Delivery/Prefabs/Level/Chimney/Chimney Room Enter.prefab > Pit/Reenable Player/BoxCollider
        public void ChimneyLeave()
        {
            NewMovement.Instance.enabled = true;
            NewMovement.Instance.gc.enabled = true;
            NewMovement.Instance.GetComponent<KeepInBounds>().enabled = true;
        }

        // called by event in Assets/Delivery/Prefabs/Level/Chimney/Chimney.prefab > Pit/Teleport Zone/BoxCollider
        public void WarpPlayerToNextRoom()
        {
            Destroy(GameManager.Instance.PreviousRoom.gameObject);
            NewMovement.Instance.transform.position = GameManager.Instance.CurrentRoom.SpawnPoint.transform.position + _tpOffset.Only(Axis.X, Axis.Z);
        }
    }
}