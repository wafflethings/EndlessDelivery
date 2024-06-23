using System.Net.Http.Headers;
using System.Web;
using EndlessDelivery.Server.Api.Steam;
using EndlessDelivery.Server.Config;
using EndlessDelivery.Server.Website;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace EndlessDelivery.Server.Api.Patreon;

[Route("api/auth/patreon")]
public class PatreonLoginController : Controller
{
    private static List<PatreonToken> s_tokens = new();
    private static Dictionary<ulong, ulong> s_steamIdToPatreonId = new();
    private static Dictionary<ulong, string> s_userToToken = new();

    private static readonly string TokensPath = Path.Combine("Assets", "Config", "patreon_tokens.json");

    public static void RefreshTokensThread()
    {
        List<PatreonToken> tokensCached = new();

        while (true)
        {
            tokensCached.AddRange(s_tokens);

            foreach (PatreonToken token in tokensCached)
            {
                if (token.Expiry > DateTime.UtcNow || token.DontRefresh)
                {
                    continue;
                }

                Console.WriteLine("Refreshing expired patreon token for user " + token.PatreonId);
                token.DontRefresh = true;
                Task.Run(() => token.RefreshSelf());
            }

            tokensCached.Clear();
            Thread.Sleep(5000);
        }
    }

    public static void LoadTokens()
    {
        if (!Path.Exists(TokensPath))
        {
            return;
        }

        s_tokens = JsonConvert.DeserializeObject<List<PatreonToken>>(System.IO.File.ReadAllText(TokensPath));

        foreach (PatreonToken token in s_tokens)
        {
            Console.WriteLine("Loaded token - " + token.SteamId);
            s_userToToken.Add(token.PatreonId, token.Value);
            s_steamIdToPatreonId.Add(token.SteamId, token.PatreonId);
        }
    }

    public static void SaveTokens()
    {
        System.IO.File.WriteAllText(TokensPath, JsonConvert.SerializeObject(s_tokens));
    }

    public static bool TryGetToken(ulong id, out string token)
    {
        if (s_userToToken.ContainsKey(id))
        {
            token = GetToken(id);
            return true;
        }

        token = string.Empty;
        return false;
    }

    public static string GetToken(ulong id) => s_userToToken[id];

    public static async Task<SubscriptionStatus> GetSubStatus(SteamUser user)
    {
        if (!s_steamIdToPatreonId.TryGetValue(ulong.Parse(user.SteamId), out ulong patreonId) || !TryGetToken(patreonId, out string token))
        {
            return SubscriptionStatus.Unsubscribed;
        }


        return SubscriptionStatus.Subscribed;
    }

    [HttpGet("rizz")]
    public async Task GetMembers()
    {
        HttpRequestMessage request = new(HttpMethod.Get, PatreonApi.MemberInfo);
        request.Headers.TryAddWithoutValidation("Authorization", "Bearer " + GetToken(PatreonApi.CampaignOwnerId));
        HttpResponseMessage response = await Program.Client.SendAsync(request);
        Console.WriteLine(await response.Content.ReadAsStringAsync());
    }

    [HttpGet("return_url")]
    public async Task ProcessOAuth(string code, string state)
    {
        if (!Request.HttpContext.TryGetLoggedInPlayer(out SteamUser user))
        {
            Response.Redirect("/account_settings/?patreon_token_success=false");
            return;
        }

        HttpRequestMessage request = new(HttpMethod.Post, PatreonApi.OAuthToken);
        request.Content = new StringContent(PatreonApi.BuildOAuthValidation(HttpContext, code, HttpUtility.HtmlEncode(Request.Path.Value)));
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

        HttpResponseMessage response = await Program.Client.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            await PatreonToken.Create(await response.Content.ReadAsStringAsync(), ulong.Parse(user.SteamId));
            await GetSubStatus(user);
        }

        Response.Redirect($"/account_settings/?patreon_token_success={response.IsSuccessStatusCode}");
    }

    [Serializable]
    public class PatreonToken
    {
        [JsonIgnore] public DateTime Expiry => CreatedAt + TimeSpan.FromSeconds(ExpiresIn);
        [JsonProperty("access_token")] public string Value { get; private set; }
        [JsonProperty("refresh_token")] public string RefreshToken { get; private set; }
        [JsonProperty("expires_in")] public int ExpiresIn { get; private set; }
        [JsonProperty("steam_id")]  public ulong SteamId { get; private set; }
        [JsonProperty("patreon_id")] public ulong PatreonId { get; private set; }
        [JsonProperty("created_at")] public DateTime CreatedAt { get; private set; }

        [JsonIgnore] public bool DontRefresh;

        public static async Task<PatreonToken> Create(string jsonData, ulong steamId)
        {
            PatreonToken token = JsonConvert.DeserializeObject<PatreonToken>(jsonData);

            HttpRequestMessage request = new(HttpMethod.Get, PatreonApi.Identity);
            request.Headers.TryAddWithoutValidation("Authorization", "Bearer " + token.Value);
            HttpResponseMessage response = await Program.Client.SendAsync(request);
            string stringResponse = await response.Content.ReadAsStringAsync();
            Console.WriteLine(stringResponse);

            const string idStart = "\"id\":\"";
            const string idEnd = "\",\"type\"";
            token.PatreonId = ulong.Parse(stringResponse[(stringResponse.IndexOf(idStart) + idStart.Length) .. stringResponse.IndexOf(idEnd)]);
            token.SteamId = steamId;
            token.CreatedAt = DateTime.UtcNow;

            s_tokens.Add(token);
            s_userToToken.Add(token.PatreonId, token.Value);
            s_steamIdToPatreonId.Add(token.SteamId, token.PatreonId);

            Console.WriteLine($"Made token - {token.SteamId}");
            return token;
        }

        public async Task RefreshSelf()
        {
            HttpRequestMessage request = new(HttpMethod.Post, PatreonApi.OAuthToken);
            request.Content = new StringContent(PatreonApi.BuildOAuthRefresh(this));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            HttpResponseMessage response = await Program.Client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                PatreonToken parsed = JsonConvert.DeserializeObject<PatreonToken>(await response.Content.ReadAsStringAsync());
                CreatedAt = DateTime.UtcNow;
                Value = parsed.Value;
                RefreshToken = parsed.RefreshToken;
                ExpiresIn = parsed.ExpiresIn;
                DontRefresh = false;
                Console.WriteLine("Refreshed, now expires at " + Expiry);
            }
        }
    }
}
