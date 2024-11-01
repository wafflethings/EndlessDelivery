using System.Net;
using System.Security.Cryptography;
using System.Text;
using EndlessDelivery.Server.Api.Users;
using EndlessDelivery.Server.Config;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EndlessDelivery.Server.Api.Steam;

[ApiController]
[Route("api/auth/steam")]
public class SteamLoginController : Controller
{
    private static List<SteamToken> s_tokens = new();
    private static Dictionary<string, ulong> s_tokenToUser = new();
    private static Aes s_aes;
    private static SHA256 s_sha = SHA256.Create();

    private static Aes Aes
    {
        get
        {
            if (s_aes == null)
            {
                s_aes = Aes.Create();
                s_aes.Key = Encoding.Unicode.GetBytes(Keys.Instance.TokenAes);
            }

            return s_aes;
        }
    }

    private static readonly string TokensPath = Path.Combine("Assets", "Config", "tokens.json");

    public static void RemoveExpiredTokensThread()
    {
        List<SteamToken> toRemove = new();
        List<SteamToken> tokensCached = new();

        while (true)
        {
            tokensCached.AddRange(s_tokens); // would fail if iterating over s_tokens when added

            foreach (SteamToken token in tokensCached)
            {
                if (token.Expiry > DateTime.UtcNow)
                {
                    continue;
                }

                Console.WriteLine("Removed steam token " + token.Value);
                toRemove.Add(token);
            }

            foreach (SteamToken token in toRemove)
            {
                s_tokenToUser.Remove(token.Value);
                s_tokens.Remove(token);
            }

            toRemove.Clear();
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

        s_tokens = JsonConvert.DeserializeObject<List<SteamToken>>(System.IO.File.ReadAllText(TokensPath));

        foreach (SteamToken token in s_tokens)
        {
            s_tokenToUser.Add(token.Value, token.User);
        }
    }

    public static void SaveTokens()
    {
        System.IO.File.WriteAllText(TokensPath, JsonConvert.SerializeObject(s_tokens));
    }

    public static async Task RemoveToken(string tokenToRemove)
    {
        await Task.Run(() => s_tokens.RemoveAll(token => token.Value == tokenToRemove));
        s_tokenToUser.Remove(tokenToRemove);
    }

    public static bool TryUserFromToken(string token, out ulong id)
    {
        if (s_tokenToUser.ContainsKey(token))
        {
            id = UserFromToken(token);
            return true;
        }

        id = 0;
        return false;
    }

    public static ulong UserFromToken(string token) => s_tokenToUser[token];

    [HttpGet("return_url")]
    public async Task ProcessOpenId()
    {
        if (Request.Query["openid.mode"] == "error")
        {
            throw new Exception($"OpenID error: \"{WebUtility.UrlDecode(Request.Query["openid.error"])}\"");
        }

        ulong claimedId = ulong.Parse(Request.Query["openid.claimed_id"].ToString().Split('/')[^1]);

        string newQueryString = Request.QueryString.Value.Replace("openid.mode=id_res", "openid.mode=check_authentication"); // replace the mode
        HttpResponseMessage response = await Program.Client.GetAsync(SteamApi.OpenIdLoginRoot + newQueryString);
        bool isValid = (await response.Content.ReadAsStringAsync()).Split(':')[^1].TrimEnd() == "true";

        if (isValid)
        {
            string token = CreateTokenString(Request.Query["openid.response_nonce"]);
            SteamToken.Create(token, claimedId);

            Response.Cookies.Append("token", token, new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(7),
                Secure = true,
                IsEssential = true
            });

            if (!SteamUser.CacheHasId(claimedId))
            {
                await UsersController.RegisterUser(HttpContext, claimedId);
            }
        }

        Response.Redirect($"/?token_success={isValid}");
    }

    [HttpPost("login")]
    public async Task<ObjectResult> LoginWithTicket()
    {
        string loginRequest = await Request.ReadBody();

        AuthTicket auth;
        try
        {
            auth = await AuthTicket.GetAuth(loginRequest);
        }
        catch
        {
            return StatusCode(StatusCodes.Status500InternalServerError, null);
        }

        if (!ulong.TryParse(auth.OwnerSteamId, out ulong id))
        {
            return StatusCode(StatusCodes.Status424FailedDependency, string.Empty);
        }

        Console.WriteLine("Login");
        if (!SteamUser.CacheHasId(id))
        {
            await UsersController.RegisterUser(HttpContext, id);
        }

        string token = CreateTokenString(loginRequest);
        SteamToken.Create(token, id);
        return StatusCode(StatusCodes.Status200OK, token);
    }

    private string CreateTokenString(string nonce) => DoSha(Convert.ToBase64String(Aes.EncryptCbc(Encoding.Unicode.GetBytes(RandomNumberGenerator.GetInt32(100000, 1000000) + nonce), s_aes.IV)));

    private static string DoSha(string content)
    {
        byte[] bytes = s_sha.ComputeHash(Encoding.UTF8.GetBytes(content));

        StringBuilder builder = new();

        foreach (byte b in bytes)
        {
            builder.Append(b.ToString("x2"));
        }

        return builder.ToString();
    }

    [Serializable]
    public class SteamToken
    {
        public DateTime Expiry = DateTime.UtcNow + TimeSpan.FromDays(7);
        public readonly string Value;
        public readonly ulong User;

        public SteamToken(string value, ulong user)
        {
            Value = value;
            User = user;
        }

        public static SteamToken Create(string value, ulong user)
        {
            SteamToken token = new(value, user);
            s_tokens.Add(token);
            s_tokenToUser.Add(value, user);
            return token;
        }
    }
}
