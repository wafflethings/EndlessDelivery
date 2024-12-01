using System.Collections.Generic;
using System.Threading.Tasks;
using AtlasLib.Saving;
using EndlessDelivery.Api.Requests;
using EndlessDelivery.Config;
using EndlessDelivery.Gameplay;
using EndlessDelivery.Online;
using EndlessDelivery.ScoreManagement;
using EndlessDelivery.UI;
using UnityEngine;

namespace EndlessDelivery.Components;

public class GoblinPortrait : MonoBehaviour
{
    public static readonly EncryptedSaveFile<List<string>> Instance = SaveFile.RegisterFile(new EncryptedSaveFile<List<string>>("gob.ddenc")) as EncryptedSaveFile<List<string>> ?? throw new();
    private const int RequiredCount = 10;

    private void OnDestroy()
    {
        if (!ScoreManager.CanSubmit)
        {
            return;
        }

        string roomId = GameManager.Instance.CurrentRoomData.Id;
        Instance.Data ??= new List<string>();
        List<string> data = Instance.Data;

        if (data.Contains(roomId))
        {
            return;
        }

        data.Add(roomId);
        CheckStatus();
    }

    private void CheckStatus()
    {
        int remaining = RequiredCount - Instance.Data.Count;

        if (remaining > 0)
        {
            HudMessageReceiver.Instance.SendHudMessage($"<color=red>GOBLIN PORTRAIT FOUND. {RequiredCount - Instance.Data.Count} REMAIN.");
            return;
        }

        string achId = "ach_gobstopper";
        AchievementHud.Instance.AddAchievement(OnlineFunctionality.LastFetchedContent.Achievements[achId]);
        Task.Run(() => OnlineFunctionality.Context.GrantAchievement(achId));
    }
}
