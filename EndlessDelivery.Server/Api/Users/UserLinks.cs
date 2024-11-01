using Newtonsoft.Json;

namespace EndlessDelivery.Server.Api.Users;

public class UserLinks
{
    public const string YoutubeRoot = "https://www.youtube.com/@";
    public const string TwitterRoot = "https:/twitter.com/@"; //erm! its x
    public const string DiscordRoot = "https://discord.com/users/";

    public string Youtube { get; set; } = string.Empty;
    public string Twitter { get; set; } = string.Empty;
    public string Discord { get; set; } = string.Empty;

    [JsonIgnore] public string GetYoutube => YoutubeRoot + Youtube;

    [JsonIgnore] public string GetTwitter => TwitterRoot + Twitter;

    [JsonIgnore] public string GetDiscord => DiscordRoot + Discord;
}
