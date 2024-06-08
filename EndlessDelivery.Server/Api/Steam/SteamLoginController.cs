﻿using System.Net;
using System.Security.Cryptography;
using System.Text;
using EndlessDelivery.Scores;
using EndlessDelivery.Server.Api.Users;
using EndlessDelivery.Server.Config;
using EndlessDelivery.Server.Api.Scores;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Supabase.Interfaces;
using Supabase.Realtime;

namespace EndlessDelivery.Server.Api.Steam;

[ApiController]
[Route("api/auth")]
public class SteamLoginController : Controller
{
    private static List<Token> s_tokens = new();
    private static Dictionary<string, ulong> s_tokenToUser = new();
    private static Aes s_aes;

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

    public static void LoadTokens()
    {
        if (!Path.Exists(TokensPath))
        {
            return;
        }

        s_tokens = JsonConvert.DeserializeObject<List<Token>>(System.IO.File.ReadAllText(TokensPath));

        foreach (Token token in s_tokens)
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
            string token = CreateToken(Request.Query["openid.response_nonce"]);
            Token.Create(token, claimedId);
            
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

    private string CreateToken(string nonce) => Convert.ToBase64String(Aes.EncryptCbc(Encoding.Unicode.GetBytes(RandomNumberGenerator.GetInt32(100000, 1000000) + nonce), s_aes.IV));
    
    [Serializable]
    public class Token
    {
        public DateTime Expiry = DateTime.Now + TimeSpan.FromDays(7);
        public readonly string Value;
        public readonly ulong User;

        public Token(string value, ulong user)
        {
            Value = value;
            User = user;
        }

        public static Token Create(string value, ulong user)
        {
            Token token = new(value, user);
            s_tokens.Add(token);
            s_tokenToUser.Add(value, user);
            return token;
        }
    }
}