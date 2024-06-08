using System.Net;
using EndlessDelivery.Server.Api.Steam;
using Microsoft.Extensions.Primitives;

namespace EndlessDelivery.Server.Website;

public static class SiteUtils
{
    public static bool TryGetLoggedInPlayer(this HttpContext context, out SteamUser steamUser)
    {
        if (context.Request.Cookies.TryGetValue("token", out string token) && SteamLoginController.TryUserFromToken(token, out ulong playerId) && SteamUser.CacheHasId(playerId))
        {
            steamUser = SteamUser.GetById(playerId);
            return true;
        }

        steamUser = null;
        return false;
    }

    public static string IntToDifficulty(int diff) => diff switch
    {
        0 => "HARMLESS",
        1 => "LENIENT",
        2 => "STANDARD",
        3 => "VIOLENT",
        4 => "BRUTAL",
        5 => "UKMD", //i think its a good idea to shorten this
        _ => throw new Exception("Invalid difficulty.")
    };
}