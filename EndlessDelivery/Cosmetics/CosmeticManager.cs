using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EndlessDelivery.Api.Requests;
using EndlessDelivery.Common.ContentFile;
using EndlessDelivery.Common.Inventory.Items;
using EndlessDelivery.Cosmetics.Skins;
using EndlessDelivery.Online;
using HarmonyLib;
using Steamworks;
using UnityEngine;

namespace EndlessDelivery.Cosmetics;

[HarmonyPatch]
public static class CosmeticManager
{
    public static CosmeticLoadout Loadout { get; private set; }
    public static List<string> AllOwned { get; private set; }

    public static async Task FetchLoadout()
    {
        Loadout = await OnlineFunctionality.Context.GetLoadout();
        AllOwned = await OnlineFunctionality.Context.GetInventory(SteamClient.SteamId);
    }

    public static void UpdateLoadout()
    {
        Task.Run(() => OnlineFunctionality.Context.SetLoadout(Loadout));
    }

    private static bool WeaponHasSkin(GunType gunType, int variationIndex) => WeaponHasSkin(gunType, variationIndex, out _);

    private static bool WeaponHasSkin(GunType gunType, int variationIndex, out string skinId)
    {
        skinId = string.Empty;
        Dictionary<string, WeaponSkinItem>? skinDict = null;
        Cms? cms = OnlineFunctionality.LastFetchedContent;
        List<string>? loadoutEntry = null;

        if (cms == null)
        {
            Debug.LogWarning("CMS was null in WeaponHasSkin!");
            return false;
        }

        switch (gunType)
        {
            case GunType.Revolver:
                skinDict = cms.Revolvers;
                loadoutEntry = Loadout.RevolverIds;
                break;

            case GunType.AltRevolver:
                skinDict = cms.AltRevolvers;
                loadoutEntry = Loadout.AltRevolverIds;
                break;

            case GunType.Shotgun:
                skinDict = cms.Shotguns;
                loadoutEntry = Loadout.ShotgunIds;
                break;

            case GunType.AltShotgun:
                skinDict = cms.AltShotguns;
                loadoutEntry = Loadout.AltShotgunIds;
                break;

            case GunType.Nailgun:
                skinDict = cms.Nailguns;
                loadoutEntry = Loadout.NailgunIds;
                break;

            case GunType.AltNailgun:
                skinDict = cms.AltNailguns;
                loadoutEntry = Loadout.AltNailgunIds;
                break;

            case GunType.Railcannon:
                skinDict = cms.Railcannons;
                loadoutEntry = Loadout.RailcannonIds;
                break;

            case GunType.RocketLauncher:
                skinDict = cms.Rockets;
                loadoutEntry = Loadout.RocketIds;
                break;
        }

        if (loadoutEntry == null || skinDict == null)
        {
            Debug.LogWarning("loadoutEntry, or skinDictGet were null in WeaponHasSkin!");
            return false;
        }

        if (variationIndex < loadoutEntry.Count)
        {
            string id = loadoutEntry[variationIndex];

            if (CosmeticLoadout.DefaultItems.Contains(id))
            {
                return false;
            }

            skinId = skinDict[id].Descriptor.Id;
            return true;
        }

        return false;
    }

    [HarmonyPatch(typeof(GunColorGetter), nameof(GunColorGetter.UpdateColor)), HarmonyPrefix]
    private static bool DisableCustomColours(GunColorGetter __instance)
    {
        Revolver? revolver = __instance.GetComponentInParent<Revolver>();
        if (revolver != null)
        {
            return !WeaponHasSkin(revolver.altVersion ? GunType.AltRevolver : GunType.Revolver, revolver.gunVariation);
        }

        Shotgun? shotgun = __instance.GetComponentInParent<Shotgun>();
        if (shotgun != null)
        {
            return !WeaponHasSkin(GunType.Shotgun, shotgun.variation);
        }

        ShotgunHammer? shotgunHammer = __instance.GetComponentInParent<ShotgunHammer>();
        if (shotgunHammer != null)
        {
            return !WeaponHasSkin(GunType.AltShotgun, shotgunHammer.variation);
        }

        Nailgun? nailgun = __instance.GetComponentInParent<Nailgun>();
        if (nailgun != null)
        {
            return !WeaponHasSkin(nailgun.altVersion ? GunType.AltNailgun : GunType.Nailgun, nailgun.variation);
        }

        Railcannon? railcannon = __instance.GetComponentInParent<Railcannon>();
        if (railcannon != null)
        {
            return !WeaponHasSkin(GunType.Railcannon, railcannon.variation);
        }

        RocketLauncher? rocketLauncher = __instance.GetComponentInParent<RocketLauncher>();
        if (rocketLauncher != null)
        {
            return !WeaponHasSkin(GunType.RocketLauncher, rocketLauncher.variation);
        }

        return true;
    }

    private static void SetSkin(GameObject weapon, string skinId, bool hasSkin)
    {
        if (!hasSkin)
        {
            foreach (GunColorGetter colouredObject in weapon.GetComponentsInChildren<GunColorGetter>())
            {
                colouredObject.rend.materials = colouredObject.defaultMaterials;
            }

            return;
        }

        Material? material = SkinDb.GetSkin(skinId)?.Material;

        if (material == null)
        {
            Plugin.Log.LogWarning($"SkinDb didn't contain skin {skinId} or it has no material.");
            return;
        }

        foreach (GunColorGetter colouredObject in weapon.GetComponentsInChildren<GunColorGetter>())
        {
            colouredObject.GetComponent<SkinnedMeshRenderer>().material = material;
        }
    }

    [HarmonyPatch(typeof(Revolver), nameof(Revolver.OnEnable)), HarmonyPostfix]
    private static void SetRevolverSkin(Revolver __instance)
    {
        bool hasSkin = WeaponHasSkin(__instance.altVersion ? GunType.AltRevolver : GunType.Revolver, __instance.gunVariation, out string id);
        SetSkin(__instance.gameObject, id, hasSkin);
    }

    [HarmonyPatch(typeof(Shotgun), nameof(Shotgun.OnEnable)), HarmonyPostfix]
    private static void SetShotgunSkin(Shotgun __instance)
    {
        bool hasSkin = WeaponHasSkin(GunType.Shotgun, __instance.variation, out string id);
        SetSkin(__instance.gameObject, id, hasSkin);
    }

    [HarmonyPatch(typeof(ShotgunHammer), nameof(ShotgunHammer.OnEnable)), HarmonyPostfix]
    private static void SetAltShotgunSkin(ShotgunHammer __instance)
    {
        bool hasSkin = WeaponHasSkin(GunType.AltShotgun, __instance.variation, out string id);
        SetSkin(__instance.gameObject, id, hasSkin);
    }

    [HarmonyPatch(typeof(Nailgun), nameof(Nailgun.OnEnable)), HarmonyPostfix]
    private static void SetNailgunSkin(Nailgun __instance)
    {
        bool hasSkin = WeaponHasSkin(__instance.altVersion ? GunType.AltNailgun : GunType.Nailgun, __instance.variation, out string id);
        SetSkin(__instance.gameObject, id, hasSkin);
    }

    [HarmonyPatch(typeof(Railcannon), nameof(Railcannon.OnEnable)), HarmonyPostfix]
    private static void SetRailcannonSkin(Railcannon __instance)
    {
        bool hasSkin = WeaponHasSkin(GunType.Railcannon, __instance.variation, out string id);
        SetSkin(__instance.gameObject, id, hasSkin);
    }

    [HarmonyPatch(typeof(RocketLauncher), nameof(RocketLauncher.OnEnable)), HarmonyPostfix]
    private static void SetRocketLauncherSkin(RocketLauncher __instance)
    {
        bool hasSkin = WeaponHasSkin(GunType.RocketLauncher, __instance.variation, out string id);
        SetSkin(__instance.gameObject, id, hasSkin);
    }
}
