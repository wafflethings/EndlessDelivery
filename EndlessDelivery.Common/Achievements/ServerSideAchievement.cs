using EndlessDelivery.Common.Communication.Scores;

namespace EndlessDelivery.Common.Achievements;

public abstract class ServerSideAchievement
{
    public static ServerSideAchievement[] AllAchievements =
    [
        new PositionAchievement("first_place_ach", 0),
        new PositionAchievement("ach_top25", 24),
        new PositionAchievement("ach_top50", 49),
    ];

    public abstract string Id { get; }
    public abstract bool ShouldGrant(OnlineScore score);
}
