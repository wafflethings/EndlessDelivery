using System;
using EndlessDelivery.Common.Communication.Scores;

namespace EndlessDelivery.Common.Achievements;

public abstract class ServerSideAchievement
{
    public static ServerSideAchievement[] AllAchievements =
    [
        new BestRoomAchievement("first_place_ach", 60),
        new BestRoomAchievement("ach_top25", 45),
        new BestRoomAchievement("ach_top50", 30),
        new LifetimeRoomsAchievement("ach_100rooms", 100),
        new LifetimeRoomsAchievement("ach_500rooms", 500),
        new LifetimeRoomsAchievement("ach_1000rooms", 1000),
        new PlaytimeAchievement("ach_2hours", TimeSpan.FromHours(2)),
        new PlaytimeAchievement("ach_10hours", TimeSpan.FromHours(10)),
        new PlaytimeAchievement("ach_20hours", TimeSpan.FromHours(20)),
        new MoneyAchievement("ach_money1", 300)
    ];

    public abstract string Id { get; }
    public abstract bool ShouldGrant(OnlineScore score, OnlineScore bestScore, Score lifetimeStats);
}
