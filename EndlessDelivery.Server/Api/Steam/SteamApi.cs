using EndlessDelivery.Server.Config;

namespace EndlessDelivery.Server.Api.Steam;

public static class SteamApi
{
    public const string OpenIdLoginRoot = "https://steamcommunity.com/openid/login";
    private const string AuthenticateUserTicket = "https://api.steampowered.com/ISteamUserAuth/AuthenticateUserTicket/v0001/?key={0}&appid=" + AppId + "&ticket={1}"; 
    private const string GetPlayerSummaries = "https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key={0}&format=json&steamids={1}";
    private const string OpenIdLoginUser =  OpenIdLoginRoot + "?openid.mode={0}&openid.ns={1}&openid.identity={2}&openid.claimed_id={3}&openid.return_to={4}";
    public const string AppId = "1229490";

    public static string BuildAuthenticateUserTicket(string ticket) => string.Format(AuthenticateUserTicket, Keys.Instance.SteamKey, ticket);
    public static string BuildGetUserSummaries(ulong[] steamIds) => string.Format(GetPlayerSummaries, Keys.Instance.SteamKey, string.Join(',', steamIds));
    public static string BuildOpenIdLogin(HttpContext context, string returnTo) => string.Format(OpenIdLoginUser, "checkid_setup", "http://specs.openid.net/auth/2.0",
        "http://specs.openid.net/auth/2.0/identifier_select", "http://specs.openid.net/auth/2.0/identifier_select",
        $"{context.Request.Scheme}://{context.Request.Host}{returnTo}"); //this sucks
}