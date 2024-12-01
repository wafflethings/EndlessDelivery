using System.Collections.Generic;
using System.Threading.Tasks;
using AtlasLib.Saving;
using EndlessDelivery.Api.Requests;
using EndlessDelivery.Config;
using EndlessDelivery.Gameplay;
using EndlessDelivery.Online;
using EndlessDelivery.UI;
using UnityEngine;

namespace EndlessDelivery.Achievements;

public class AllRooms : MonoBehaviour
{
    public static readonly EncryptedSaveFile<List<string>> Instance = SaveFile.RegisterFile(new EncryptedSaveFile<List<string>>("cleared_rooms.ddenc")) as EncryptedSaveFile<List<string>> ?? throw new();
    private const string AchId = "ach_all_rooms";

    private void Awake()
    {
        GameManager.Instance.RoomComplete += _ =>
        {
            Instance.Data ??= new List<string>();
            List<string> data = Instance.Data;
            string? id = GameManager.Instance.CurrentRoomData?.Id;

            if (id == null)
            {
                return;
            }

            if (!data.Contains(id))
            {
                data.Add(id);
            }

            if (data.Count == GameManager.Instance.RoomPool.Rooms.Length)
            {
                AchievementManager.ShowAndGiveLocal(AchId);
            }
        };
    }
}
