using EndlessDelivery.Common.Communication.Scores;

namespace EndlessDelivery.Common.Achievements;

public class NumberOneScore : ServerSideAchievement
{
    public override string Id => "first_place_ach";

    public override bool ShouldGrant(OnlineScore score) => score.Index == 0;
}
