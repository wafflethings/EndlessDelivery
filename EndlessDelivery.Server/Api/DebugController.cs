using EndlessDelivery.Common.ContentFile;
using EndlessDelivery.Server.Api.ContentFile;
using EndlessDelivery.Server.Api.Steam;
using EndlessDelivery.Server.Api.Users;
using EndlessDelivery.Server.Database;
using EndlessDelivery.Server.Resources;
using EndlessDelivery.Server.Website;
using EndlessDelivery.UI;
using Microsoft.AspNetCore.Mvc;

namespace EndlessDelivery.Server.Api;

[ApiController]
[Route("api/debug")]
public class DebugController : Controller
{
    [HttpGet("reset_resources")]
    public async Task<ObjectResult> ResetResources()
    {
        if (!HttpContext.TryGetLoggedInPlayer(out SteamUser user) || !(await user.GetUserModel()).Admin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Go away!!!!");
        }

        foreach (Resource resourcePair in ResourceManager.Resources)
        {
            resourcePair.Reset();
        }

        return StatusCode(StatusCodes.Status200OK, ":3");
    }

    [HttpGet("check_grants")]
    public async Task<ObjectResult> CheckGrants()
    {
        if (!HttpContext.TryGetLoggedInPlayer(out SteamUser steamUser) || !(await steamUser.GetUserModel()).Admin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Go away!!!!");
        }

        Cms cms = ContentController.CurrentContent;
        await using DeliveryDbContext dbContext = new();

        foreach (UserModel user in dbContext.Users)
        {
            foreach (string claimedDay in user.ClaimedDays)
            {
                CalendarReward day = cms.CalendarRewards[claimedDay];

                if (!day.HasItem)
                {
                    continue;
                }

                string dayRewardId = cms.CalendarRewards[claimedDay].ItemId;

                if (user.OwnedItemIds.Contains(dayRewardId))
                {
                    continue;
                }

                user.OwnedItemIds.Add(dayRewardId);
                dbContext.Users.Update(user);
            }
        }

        await dbContext.SaveChangesAsync();
        return StatusCode(StatusCodes.Status200OK, ":3");
    }
}
