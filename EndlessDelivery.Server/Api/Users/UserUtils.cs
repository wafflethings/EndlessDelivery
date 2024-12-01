using EndlessDelivery.Common;
using EndlessDelivery.Common.Achievements;
using EndlessDelivery.Common.Communication.Scores;
using EndlessDelivery.Server.Api.ContentFile;
using EndlessDelivery.Server.Database;

namespace EndlessDelivery.Server.Api.Users;

public static class UserUtils
{
    public static async Task<OnlineScore?> GetBestScore(this UserModel user)
    {
        await using DeliveryDbContext dbContext = new();
        return await dbContext.Scores.FindAsync(user.SteamId);
    }

    public static void GetAchievement(this UserModel user, Achievement achievement)
    {
        user.OwnedAchievements.Add(new OwnedAchievement(DateTime.UtcNow, achievement.Id));

        foreach (string itemId in achievement.ItemGrants)
        {
            if (ContentController.CurrentContent.TryGetItem(itemId, out _))
            {
                user.OwnedItemIds.Add(itemId);
            }
        }
    }

    public static void CheckOnlineAchievements(this UserModel user, OnlineScore newScore)
    {
        foreach (ServerSideAchievement serverAchievement in ServerSideAchievement.AllAchievements)
        {
            if (!ContentController.CurrentContent.Achievements.TryGetValue(serverAchievement.Id, out Achievement? achievement) || achievement == null)
            {
                continue;
            }

            if (user.OwnedAchievements.All(x => x.Id != serverAchievement.Id) && serverAchievement.ShouldGrant(newScore) && !serverAchievement.Disabled)
            {
                using DeliveryDbContext dbContext = new();
                user.GetAchievement(achievement);
                dbContext.Users.Update(user);
                dbContext.SaveChanges();
            }
        }
    }
}
