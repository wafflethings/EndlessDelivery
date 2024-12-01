using System.Collections.Generic;
using System.Threading.Tasks;
using AtlasLib.Saving;
using EndlessDelivery.Api.Requests;
using EndlessDelivery.Common;
using EndlessDelivery.Common.Achievements;
using EndlessDelivery.Common.Communication.Scores;
using EndlessDelivery.Online;
using EndlessDelivery.ScoreManagement;
using EndlessDelivery.UI;

namespace EndlessDelivery.Achievements;

public class AchievementManager
{
    public static SaveFile<List<string>> OwnedAchievements = SaveFile.RegisterFile(new SaveFile<List<string>>("dont_show_achs.json", Plugin.Name));

    public static void CheckOnline(OnlineScore score)
    {
        if (!ScoreManager.CanSubmit)
        {
            return;
        }

        if (OnlineFunctionality.LastFetchedContent == null)
        {
            Plugin.Log.LogWarning("OnlineAchievementChecker.CheckOnline has somehow been called before CMS fetched.");
            return;
        }

        OwnedAchievements.Data ??= new();

        foreach (ServerSideAchievement serverAchievement in ServerSideAchievement.AllAchievements)
        {
            Plugin.Log.LogInfo($"Checking ach: {serverAchievement.Id}");

            if (!OnlineFunctionality.LastFetchedContent.Achievements.TryGetValue(serverAchievement.Id, out Achievement? achievement) || achievement == null)
            {
                continue;
            }

            if (!OwnedAchievements.Data.Contains(serverAchievement.Id) && serverAchievement.ShouldGrant(score))
            {
                Plugin.Log.LogInfo("Granted");
                OwnedAchievements.Data.Add(serverAchievement.Id);
                AchievementHud.Instance.AddAchievement(achievement);
            }
        }
    }

    public static void ShowAndGiveLocal(string id)
    {
        if (!ScoreManager.CanSubmit)
        {
            return;
        }

        OwnedAchievements.Data ??= new();

        if (OwnedAchievements.Data.Contains(id))
        {
            return;
        }

        OwnedAchievements.Data.Add(id);
        AchievementHud.Instance.AddAchievement(OnlineFunctionality.LastFetchedContent.Achievements[id]);
        Task.Run(() => OnlineFunctionality.Context.GrantAchievement(id));
    }
}
