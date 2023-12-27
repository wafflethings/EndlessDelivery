using EndlessDelivery.Assets;
using EndlessDelivery.Utils;
using HarmonyLib;

namespace EndlessDelivery.Cheats
{
    [PatchThis($"{Plugin.GUID}.CheatManager")]
    public static class CheatManager
    {
        [HarmonyPatch(typeof(CheatsManager), nameof(CheatsManager.Start)), HarmonyPrefix]
        private static void AddIfShould(CheatsManager __instance)
        {
            if (!AddressableManager.InSceneFromThisMod)
            {
                return;
            }
            __instance.RegisterExternalCheat(new CompleteRoom());
        }
    }
}