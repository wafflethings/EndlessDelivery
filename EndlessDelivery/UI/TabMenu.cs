using System;
using EndlessDelivery.Assets;
using EndlessDelivery.Gameplay;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EndlessDelivery.UI;

[HarmonyPatch]
public class TabMenu : MonoBehaviour
{
    private static GameObject? s_menuObject;
    [SerializeField] private TMP_Text _map;
    [SerializeField] private TMP_Text _time;
    [SerializeField] private TMP_Text _kills;
    private int _roomEnemyCount;
    private int _killsOnEnter;

    private void Awake()
    {
        GameManager.Instance.RoomStarted += room =>
        {
            if (room.Arena == null || StatsManager.Instance == null)
            {
                return;
            }

            _roomEnemyCount = room.Arena.enemies.Length;
            _killsOnEnter = StatsManager.Instance.kills;
        };
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentRoomData == null)
        {
            return;
        }

        _map.text = GameManager.Instance.CurrentRoomData.Name;
        _time.text = TimeSpan.FromSeconds(GameManager.Instance.TimeElapsed).ToString("mm':'ss'.'ff");
        _kills.text = (_roomEnemyCount - (StatsManager.Instance.kills - _killsOnEnter)).ToString();
    }

    [HarmonyPatch(typeof(LevelStatsEnabler), nameof(LevelStatsEnabler.Start)), HarmonyPostfix]
    private static void AddSelf(LevelStatsEnabler __instance)
    {
        if (!AssetManager.InSceneFromThisMod)
        {
            return;
        }

        s_menuObject ??= Addressables.LoadAssetAsync<GameObject>("Assets/Delivery/Prefabs/HUD/Game Stats.prefab").WaitForCompletion();
        __instance.gameObject.SetActive(true);
        __instance.levelStats = Instantiate(s_menuObject, __instance.transform);
        __instance.levelStats.SetActive(false);
    }
}
