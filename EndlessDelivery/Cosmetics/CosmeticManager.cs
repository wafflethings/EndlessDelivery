using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AtlasLib.Weapons;
using EndlessDelivery.Api.Requests;
using EndlessDelivery.Common.ContentFile;
using EndlessDelivery.Common.Inventory.Items;
using EndlessDelivery.Common.Inventory.Items.WeaponSkins;
using EndlessDelivery.Online;
using HarmonyLib;
using Steamworks;
using UnityEngine;
using UnityEngine.AddressableAssets;

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

    private static bool WeaponHasSkin(GunType gunType, int variationIndex) => WeaponHasSkin(gunType, variationIndex, out _);

    private static bool WeaponHasSkin(GunType gunType, int variationIndex, out string matPath)
    {
        matPath = string.Empty;
        Cms? cms = OnlineFunctionality.LastFetchedContent;
        Func<string, WeaponSkin>? skinDictGet = null;
        int? skinDictCount = null;
        List<string>? loadoutEntry = null;

        if (cms == null)
        {
            Debug.LogWarning("CMS was null in WeaponHasSkin!");
            return false;
        }

        switch (gunType)
        {
            case GunType.Revolver:
                skinDictGet = id => cms.Revolvers[id];
                skinDictCount = cms.Revolvers.Count;
                loadoutEntry = Loadout.RevolverIds;
                break;

            case GunType.AltRevolver:
                skinDictGet = id => cms.AltRevolvers[id];
                skinDictCount = cms.AltRevolvers.Count;
                loadoutEntry = Loadout.RevolverIds;
                break;

            case GunType.Shotgun:
                skinDictGet = id => cms.Shotguns[id];
                skinDictCount = cms.Shotguns.Count;
                loadoutEntry = Loadout.ShotgunIds;
                break;

            case GunType.AltShotgun:
                skinDictGet = id => cms.AltShotguns[id];
                skinDictCount = cms.AltShotguns.Count;
                loadoutEntry = Loadout.ShotgunIds;
                break;

            case GunType.Nailgun:
                skinDictGet = id => cms.Nailguns[id];
                skinDictCount = cms.Nailguns.Count;
                loadoutEntry = Loadout.NailgunIds;
                break;

            case GunType.AltNailgun:
                skinDictGet = id => cms.AltNailguns[id];
                skinDictCount = cms.AltNailguns.Count;
                loadoutEntry = Loadout.RevolverIds;
                break;

            case GunType.Railcannon:
                skinDictGet = id => cms.Railcannons[id];
                skinDictCount = cms.Railcannons.Count;
                loadoutEntry = Loadout.RailcannonIds;
                break;

            case GunType.RocketLauncher:
                skinDictGet = id => cms.Rockets[id];
                skinDictCount = cms.Rockets.Count;
                loadoutEntry = Loadout.RocketIds;
                break;
        }

        if (skinDictCount == null || loadoutEntry == null || skinDictGet == null)
        {
            Debug.LogWarning("skinDictCount, loadoutEntry, or skinDictGet were null in WeaponHasSkin!");
            return false;
        }

        if (variationIndex < skinDictCount)
        {
            string id = loadoutEntry[variationIndex];

            if (id == string.Empty)
            {
                return false;
            }

            matPath = skinDictGet(id).MaterialPath;
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

    private static void SetSkin(GameObject weapon, string materialPath, bool hasSkin)
    {
        if (!hasSkin)
        {
            foreach (GunColorGetter colouredObject in weapon.GetComponentsInChildren<GunColorGetter>())
            {
                colouredObject.UpdateColor();;
            }

            return;
        }

        Material? loadedMaterial = Addressables.LoadAssetAsync<Material>(materialPath).WaitForCompletion();

        if (loadedMaterial == null)
        {
            throw new Exception($"Couldn't find skin at path {materialPath}");
        }

        foreach (GunColorGetter colouredObject in weapon.GetComponentsInChildren<GunColorGetter>())
        {
            colouredObject.GetComponent<SkinnedMeshRenderer>().material = loadedMaterial;
        }
    }

    [HarmonyPatch(typeof(Revolver), nameof(Revolver.OnEnable)), HarmonyPostfix]
    private static void SetRevolverSkin(Revolver __instance)
    {
        bool hasSkin = WeaponHasSkin(__instance.altVersion ? GunType.AltRevolver : GunType.Revolver, __instance.gunVariation, out string path);
        SetSkin(__instance.gameObject, path, hasSkin);
    }

    [HarmonyPatch(typeof(Shotgun), nameof(Shotgun.OnEnable)), HarmonyPostfix]
    private static void SetShotgunSkin(Shotgun __instance)
    {
        bool hasSkin = WeaponHasSkin(GunType.Shotgun, __instance.variation, out string path);
        SetSkin(__instance.gameObject, path, hasSkin);
    }

    [HarmonyPatch(typeof(ShotgunHammer), nameof(ShotgunHammer.OnEnable)), HarmonyPostfix]
    private static void SetAltShotgunSkin(ShotgunHammer __instance)
    {
        bool hasSkin = WeaponHasSkin(GunType.AltShotgun, __instance.variation, out string path);
        SetSkin(__instance.gameObject, path, hasSkin);
    }

    [HarmonyPatch(typeof(Nailgun), nameof(Nailgun.OnEnable)), HarmonyPostfix]
    private static void SetNailgunSkin(Nailgun __instance)
    {
        bool hasSkin = WeaponHasSkin(__instance.altVersion ? GunType.AltNailgun : GunType.Nailgun, __instance.variation, out string path);
        SetSkin(__instance.gameObject, path, hasSkin);
    }

    [HarmonyPatch(typeof(Railcannon), nameof(Railcannon.OnEnable)), HarmonyPostfix]
    private static void SetRailcannonSkin(Railcannon __instance)
    {
        bool hasSkin = WeaponHasSkin(GunType.Railcannon, __instance.variation, out string path);
        SetSkin(__instance.gameObject, path, hasSkin);
    }

    [HarmonyPatch(typeof(RocketLauncher), nameof(RocketLauncher.OnEnable)), HarmonyPostfix]
    private static void SetRocketLauncherSkin(RocketLauncher __instance)
    {
        bool hasSkin = WeaponHasSkin(GunType.RocketLauncher, __instance.variation, out string path);
        SetSkin(__instance.gameObject, path, hasSkin);
    }
}
