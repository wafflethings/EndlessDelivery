using System;

namespace EndlessDelivery.Common;

public class OwnedAchievement
{
    public DateTime Achieved;
    public string Id;

    public OwnedAchievement(DateTime achieved, string id)
    {
        Achieved = achieved;
        Id = id;
    }

    // only for EF core
    public OwnedAchievement()
    {

    }
}
