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
    private int _completeRooms;
    private static bool s_tookDmg;

    private void Awake()
    {
        GameManager.Instance.RoomComplete += room =>
        {
            if (s_tookDmg || !room.RoomHasGameplay)
            {
                _completeRooms = 0;
                s_tookDmg = false;
                return;
            }

            _completeRooms++;

            if (_completeRooms == 5)
            {
                AchievementManager.ShowAndGiveLocal(AchId);
            }
        };
    }

    [HarmonyPatch(typeof(NewMovement), nameof(NewMovement.GetHurt)), HarmonyPostfix]
    private static void PlayerDamaged()
    {
        s_tookDmg = true;
    }
}
