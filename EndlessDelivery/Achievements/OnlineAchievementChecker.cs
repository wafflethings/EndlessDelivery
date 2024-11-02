using System.Collections.Generic;
using AtlasLib.Saving;
using EndlessDelivery.Common;
using EndlessDelivery.Common.Achievements;
using EndlessDelivery.Common.Communication.Scores;
using EndlessDelivery.Online;
using EndlessDelivery.UI;

namespace EndlessDelivery.Achievements;

public class OnlineAchievementChecker
{
    public static SaveFile<List<string>> OwnedAchievements = SaveFile.RegisterFile(new SaveFile<List<string>>("dont_show_achs.json", Plugin.Name));

    public static void Check(OnlineScore score)
    {
        if (OnlineFunctionality.LastFetchedContent == null)
        {
            Plugin.Log.LogWarning("OnlineAchievementChecker.Check has somehow been called before CMS fetched.");
            return;
        }

        foreach (ServerSideAchievement serverAchievement in ServerSideAchievement.AllAchievements)
        {
            Plugin.Log.LogInfo($"Checking ach: {serverAchievement.Id}");

            if (!OnlineFunctionality.LastFetchedContent.Achievements.TryGetValue(serverAchievement.Id, out Achievement? achievement) || achievement == null)
            {
                continue;
            }

            if (!OwnedAchievements.Data.Contains(serverAchievement.Id) && serverAchievement.ShouldGrant(score))
            {
                OwnedAchievements.Data.Add(serverAchievement.Id);
                AchievementHud.Instance.AddAchievement(achievement);
            }
        }
    }
}
