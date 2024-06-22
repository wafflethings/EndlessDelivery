using System.Web;
using EndlessDelivery.Server.Config;

namespace EndlessDelivery.Server.Api.Patreon;

public static class PatreonApi
{
    public const ulong CampaignOwnerId = 90279900;
    public const string CampaignId = "10208653";
    public static readonly string Identity = $"https://www.patreon.com/api/oauth2/v2/identity?include=memberships&fields{HttpUtility.UrlEncode("[member]")}=pledge_relationship_star";
    public const string MemberInfo = $"https://www.patreon.com/api/oauth2/api/campaigns/{CampaignId}/pledges"; //cba to urlencode manually bc it fucks the https
    public const string OAuthToken = "https://www.patreon.com/api/oauth2/token";
    public const string OAuthTokenValidation = "code={0}&grant_type={1}&client_id={2}&client_secret={3}&redirect_uri={4}";
    public const string OAuthTokenRefresh = "grant_type={0}&refresh_token={1}&client_id={2}&client_secret={3}";
    public const string OAuthLogin = "https://www.patreon.com/oauth2/authorize?response_type={0}&client_id={1}&redirect_uri={2}&scope=";

    public static string BuildOAuthLogin(HttpContext context, bool isCampaignOwner, string returnTo) => string.Format(OAuthLogin, "code", Keys.Instance.PatreonClientId, $"{context.Request.Scheme}://{context.Request.Host}{returnTo}") +
                                                                                                        (isCampaignOwner ? "campaigns.members campaigns.members%5Bemail%5D campaigns.members.address" : "identity campaigns identity.memberships");

    public static string BuildOAuthValidation(HttpContext context, string code, string returnTo) => string.Format(OAuthTokenValidation, code, "authorization_code",
        Keys.Instance.PatreonClientId, Keys.Instance.PatreonSecret, $"{context.Request.Scheme}://{context.Request.Host}{returnTo}");

    public static string BuildOAuthRefresh(PatreonLoginController.PatreonToken token) => string.Format(OAuthTokenRefresh, "refresh_token", token.RefreshToken, Keys.Instance.PatreonClientId, Keys.Instance.PatreonSecret);
}
