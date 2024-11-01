using EndlessDelivery.Common.Communication.Scores;

namespace EndlessDelivery.Common.Achievements;

public abstract class ServerSideAchievement
{
    public static ServerSideAchievement[] AllAchievements =
    [
        new NumberOneScore()
    ];

    public abstract string Id { get; }
    public abstract bool ShouldGrant(OnlineScore score);
}
