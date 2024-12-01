using System.Collections.Generic;
using System.Threading.Tasks;
using AtlasLib.Saving;
using EndlessDelivery.Api.Requests;
using EndlessDelivery.Config;
using EndlessDelivery.Gameplay;
using EndlessDelivery.Online;
using EndlessDelivery.UI;
using HarmonyLib;
using UnityEngine;

namespace EndlessDelivery.Achievements;

[HarmonyPatch]
public class NoDamage : MonoBehaviour
{
    private const string AchId = "ach_nodmg";
    private static bool s_tookDmg;

    private void Awake()
    {
        GameManager.Instance.RoomComplete += _ =>
        {
            if (s_tookDmg)
            {
                s_tookDmg = false;
                return;
            }

            AchievementHud.Instance.AddAchievement(OnlineFunctionality.LastFetchedContent.Achievements[AchId]);
            Task.Run(() => OnlineFunctionality.Context.GrantAchievement(AchId));
        };
    }

    [HarmonyPatch(typeof(NewMovement), nameof(NewMovement.GetHurt)), HarmonyPostfix]
    private static void PlayerDamaged()
    {
        s_tookDmg = true;
    }
}
