using System;
using AtlasLib.Utils;
using EndlessDelivery.Assets;
using EndlessDelivery.Gameplay;
using EndlessDelivery.Utils;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EndlessDelivery.UI;

[HarmonyPatch]
public class AltPresentHud : MonoBehaviour
{
    public TMP_Text[] PresentTexts;
    public TMP_Text TimerText;
    public TMP_Text RoomText;
    public Color TimerColour;
    public Color TimerDangerColour;

    public void Update()
    {
        if (!GameManager.Instance.GameStarted)
        {
            return;
        }

        TimerText.text = TimeSpan.FromSeconds(GameManager.Instance.TimeLeft).Formatted();
        RoomText.text = (GameManager.Instance.RoomsComplete).ToString();

        if (GameManager.Instance.TimeLeft < 10)
        {
            TimerText.color = TimerDangerColour;
        }
        else
        {
            TimerText.color = TimerColour;
        }

        int index = 0;
        Room room = GameManager.Instance.CurrentRoom;
        foreach (TMP_Text text in PresentTexts)
        {
            text.text = $"{room.AmountDelivered[(WeaponVariant)(index)]}/{room.PresentColourAmounts[index]}";
            index++;
        }
    }

    [HarmonyPatch(typeof(HudController), nameof(HudController.Start)), HarmonyPostfix]
    private static void AddSelf(HudController __instance)
    {
        if (__instance.altHud && !MapInfoBase.InstanceAnyType.hideStockHUD && AssetManager.InSceneFromThisMod)
        {
            GameObject presentPanel = Addressables.LoadAssetAsync<GameObject>("Assets/Delivery/Prefabs/HUD/Classic Present Hud.prefab").WaitForCompletion();
            Instantiate(presentPanel, __instance.gameObject.GetChild("Filler").transform);
        }
    }
}
