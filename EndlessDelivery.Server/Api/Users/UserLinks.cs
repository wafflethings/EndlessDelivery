using Newtonsoft.Json;

namespace EndlessDelivery.Server.Api.Users;

public class UserLinks
{
    public const string YoutubeRoot = "https://www.youtube.com/@";
    public const string TwitterRoot = "https:/twitter.com/@"; //erm! its x
    public const string DiscordRoot = "https://discord.com/users/";

    public string Youtube { get; set; }
    public string Twitter { get; set; }
    public string Discord { get; set; }

    [JsonIgnore] public string GetYoutube => YoutubeRoot + Youtube;

    [JsonIgnore] public string GetTwitter => TwitterRoot + Twitter;

    [JsonIgnore] public string GetDiscord => DiscordRoot + Discord;
}