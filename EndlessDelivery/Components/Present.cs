using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EndlessDelivery.Assets;
using EndlessDelivery.Common.Inventory.Items;
using EndlessDelivery.Cosmetics;
using EndlessDelivery.Cosmetics.Skins;
using EndlessDelivery.Gameplay;
using EndlessDelivery.Online;
using EndlessDelivery.UI;
using EndlessDelivery.Utils;
using HarmonyLib;
using UnityEngine;

namespace EndlessDelivery.Components;

[HarmonyPatch]
public class Present : MonoBehaviour
{
    private static List<Collider> _allPresentColliders = new();

    public WeaponVariant VariantColour;
    public GameObject Particles;
    public SpriteRenderer Glow;
    private ItemIdentifier _item;
    private Collider[] _colliders;
    [HideInInspector] public bool Destroyed;
    private float _lastHook = 0;
    [SerializeField] private GameObject _hookDeclineEffect;
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private MeshFilter _meshFilter;

    private Color _colour => ColourSetter.DefaultColours[(int)VariantColour];

    private void Start()
    {
        SetSkin();
        _colliders = GetComponents<Collider>();
        _item = GetComponent<ItemIdentifier>();
        _allPresentColliders.AddRange(_colliders);
        SetColour(VariantColour);
    }

    private void SetSkin()
    {
        if (CosmeticLoadout.Default.PresentId == CosmeticManager.Loadout.PresentId)
        {
            return;
        }

        PresentSkin? skin = SkinDb.GetSkin(CosmeticManager.Loadout.PresentId) as PresentSkin;

        if (skin == null)
        {
            Plugin.Log.LogWarning($"Present skin was null: {CosmeticManager.Loadout.PresentId}");
            return;
        }

        _meshFilter.mesh = skin.Mesh;
        _meshRenderer.materials = skin.Materials;
    }

    private void Update()
    {
        Glow.gameObject.layer = 0; //picking up an item makes all children AlwaysOnTop.
        // the glow is disabled on pickup, so when it is dropped, it is not set back to the original layer.
        // it has to be set back manually
    }

    private void FixedUpdate()
    {
        if (Mathf.Abs((HookArm.Instance.hook.position - transform.position).sqrMagnitude) < (2f * 2f))
        {
            OnHookAttempt();
        }
    }

    private void OnHookAttempt()
    {
        if (HookArm.Instance?.state != HookState.Throwing || Time.time - _lastHook < 0.25f || _item.pickedUp)
        {
            return;
        }

        _lastHook = Time.time;
        Instantiate(_hookDeclineEffect, transform.position, transform.rotation);
    }

    private void OnDisable()
    {
        RemoveCollidersFromAll();
    }

    private void OnDestroy()
    {
        RemoveCollidersFromAll();
    }

    private void RemoveCollidersFromAll()
    {
        if (_colliders == null)
        {
            return;
        }

        foreach (Collider collider in _colliders)
        {
            _allPresentColliders?.Remove(collider);
        }
    }

    public void SetColour(WeaponVariant colour)
    {
        VariantColour = colour;
        _meshRenderer.material.color = _colour;
        GetComponent<Light>().color = _colour;
        GetComponent<ParticleSystem>().startColor = _colour;
        Glow.color = new Color(_colour.r, _colour.g, _colour.b, 0.5f);
    }

    public void Deliver(Chimney chimney)
    {
        chimney.Room.Deliver(VariantColour);
        GameManager.Instance.AddTime(6, $"<color=#{ColorUtility.ToHtmlStringRGB(_colour)}>DELIVERY</color>");
        CreateParticles();
        Destroyed = true;

        if (_item.hooked)
        {
            HookArm.Instance.StopThrow();
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
        if (!AssetManager.InSceneFromThisMod)
        {
            return;
        }

        foreach (Collider collider in _allPresentColliders)
        {
            if (collider == null)
            {
                Plugin.Log.LogWarning("Null collider in _allPresentColliders, should be impossible");
                continue;
            }

            Physics.IgnoreCollision(collider, __instance.scol);
        }
    }
}
