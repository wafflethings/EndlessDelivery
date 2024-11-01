using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EndlessDelivery.Api.Requests;
using EndlessDelivery.Common.Inventory.Items;
using EndlessDelivery.Online;
using HarmonyLib;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EndlessDelivery.Cosmetics;

[HarmonyPatch]
public static class CosmeticManager
{
    public static InventoryLoadout Loadout { get; private set; }

    public static async Task FetchLoadout()
    {
        Loadout = await OnlineFunctionality.Context.GetLoadout();
    }

    private static bool RevolverHasSkin(int variationIndex, out string matPath)
    {
        matPath = "Assets/Common/Models/Weapons/Revolver/Pistol New.mat";

        if (variationIndex < Loadout.RevolverIds.Count)
        {
            string id = Loadout.RevolverIds[variationIndex];

            if (id == string.Empty)
            {
                return false;
            }

            matPath = OnlineFunctionality.LastFetchedContent.Revolvers[id].MaterialPath;
            return true;
        }

        return false;
    }

    private static bool AltRevolverHasSkin(int variationIndex, out string matPath)
    {
        matPath = "Assets/Common/Models/Weapons/Alternative Revolver/MinosRevolver.mat";

        if (variationIndex < Loadout.AltRevolverIds.Count)
        {
            string id = Loadout.AltRevolverIds[variationIndex];

            if (id == string.Empty)
            {
                return false;
            }

            matPath = OnlineFunctionality.LastFetchedContent.AltRevolvers[id].MaterialPath;
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
            return revolver.altVersion ? !AltRevolverHasSkin(revolver.gunVariation, out _) : !RevolverHasSkin(revolver.gunVariation, out _);
        }

        return true;
    }

    [HarmonyPatch(typeof(Revolver), nameof(Revolver.OnEnable)), HarmonyPostfix]
    private static void SetRevolverSkin(Revolver __instance)
    {
        if (__instance.altVersion)
        {
            return;
        }

        if (!RevolverHasSkin(__instance.gunVariation, out string path))
        {
            return;
        }

        Material? loadedMaterial = Addressables.LoadAssetAsync<Material>(path).WaitForCompletion();

        if (loadedMaterial == null)
        {
            throw new Exception($"Couldn't find skin at path {path}");
        }

        foreach (GunColorGetter colouredObject in __instance.GetComponentsInChildren<GunColorGetter>())
        {
            colouredObject.GetComponent<SkinnedMeshRenderer>().material = loadedMaterial;
        }
    }

    [HarmonyPatch(typeof(Revolver), nameof(Revolver.OnEnable)), HarmonyPostfix]
    private static void AltSetRevolverSkin(Revolver __instance)
    {
        if (!__instance.altVersion)
        {
            return;
        }

        if (!AltRevolverHasSkin(__instance.gunVariation, out string path))
        {
            return;
        }

        Material? loadedMaterial = Addressables.LoadAssetAsync<Material>(path).WaitForCompletion();

        if (loadedMaterial == null)
        {
            throw new Exception($"Couldn't find skin at path {path}");
        }

        foreach (GunColorGetter colouredObject in __instance.GetComponentsInChildren<GunColorGetter>())
        {
            colouredObject.GetComponent<SkinnedMeshRenderer>().material = loadedMaterial;
        }
    }
}
