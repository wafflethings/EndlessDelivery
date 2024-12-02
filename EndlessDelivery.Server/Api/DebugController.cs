using EndlessDelivery.Server.Api.Steam;
using EndlessDelivery.Server.Resources;
using EndlessDelivery.Server.Website;
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
}
