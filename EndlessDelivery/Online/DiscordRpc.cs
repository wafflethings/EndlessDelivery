using Discord;
using EndlessDelivery.Assets;
using EndlessDelivery.Gameplay;
using HarmonyLib;

namespace EndlessDelivery.Online;

[HarmonyPatch]
public static class DiscordRpc
{
    [HarmonyPatch(typeof(DiscordController), nameof(DiscordController.SendActivity)), HarmonyPrefix]
    private static void UpdateActivity(DiscordController __instance)
    {
        if (!AssetManager.InSceneFromThisMod)
        {
            return;
        }

        SetActivity(ref __instance.cachedActivity);
    }

    private static void SetActivity(ref Activity activity)
    {
        activity.Assets.LargeImage = GetUrl(GameManager.Instance.CurrentRoomData);
        activity.Assets.LargeText = "DIVINE DELIVERY";
        activity.Details = $"ROOMS: {GameManager.Instance.RoomsComplete}";
    }

    private static string GetUrl(RoomData? data) => $"https://delivery.wafflethings.dev/Resources/DiscordIcons/{data?.Id ?? "startroom"}.png";
}
