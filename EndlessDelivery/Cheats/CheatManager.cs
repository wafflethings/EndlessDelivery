using EndlessDelivery.Assets;
using HarmonyLib;

namespace EndlessDelivery.Cheats;

[HarmonyPatch]
public static class CheatManager
{
    [HarmonyPatch(typeof(CheatsManager), nameof(CheatsManager.Start)), HarmonyPrefix]
    private static void AddIfShould(CheatsManager __instance)
    {
        if (!AssetManager.InSceneFromThisMod)
        {
            return;
        }

        __instance.RegisterExternalCheat(new CompleteRoom());
        __instance.RegisterExternalCheat(new InfiniteTime());
    }
}
