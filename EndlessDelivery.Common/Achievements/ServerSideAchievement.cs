using System;
using EndlessDelivery.Common.Communication.Scores;

namespace EndlessDelivery.Common.Achievements;

public abstract class ServerSideAchievement
{
    public static ServerSideAchievement[] AllAchievements =
    [
        new PositionAchievement("first_place_ach", 0),
        new PositionAchievement("ach_top25", 24),
        new PositionAchievement("ach_top50", 49),
        new LifetimeRoomsAchievement("ach_100rooms", 100),
        new LifetimeRoomsAchievement("ach_500rooms", 500),
        new LifetimeRoomsAchievement("ach_1000rooms", 1000),
        new PlaytimeAchievement("ach_2hours", TimeSpan.FromHours(2)),
        new PlaytimeAchievement("ach_10hours", TimeSpan.FromHours(10)),
        new PlaytimeAchievement("ach_20hours", TimeSpan.FromHours(20))
    ];

    public abstract string Id { get; }
    public abstract bool ShouldGrant(OnlineScore score, Score lifetimeStats);
}
