using EndlessDelivery.Common;
using EndlessDelivery.Common.Communication.Scores;
using EndlessDelivery.Server.Api.ContentFile;
using EndlessDelivery.Server.Api.Steam;
using EndlessDelivery.Server.Api.Users;
using Microsoft.AspNetCore.Html;

namespace EndlessDelivery.Server.Website.HtmlElements;

public static class UserPageElements
{
    public static async Task AppendPlayerHeader(this HtmlContentBuilder builder, SteamUser steamUser, UserModel userModel)
    {
        builder.AppendHtml("<div class=\"user-info-box\">");
        builder.AppendSocialLinks(userModel);
        builder.AppendBanner(steamUser, userModel);
        await builder.AppendPlayerHeaderInfo(steamUser, userModel);
        builder.AppendHtml("</div>");
    }

    public static void AppendBanner(this HtmlContentBuilder builder, SteamUser steamUser, UserModel userModel)
    {
        builder.AppendHtml($"<img class=\"user-info-banner\" src=\"{ContentController.CurrentContent.Banners[userModel.Loadout.BannerId].Asset.AssetUri}\"/>");
    }

    public static async Task AppendPlayerHeaderInfo(this HtmlContentBuilder builder, SteamUser steamUser, UserModel userModel)
    {
        builder.AppendHtml("<div class=\"user-info-bottom\">");
        builder.AppendHtml($"<img class=\"user-info-pfp circle-clip\" src=\"{steamUser.AvatarFull}\"/>");
        builder.AppendHtml("<div class=\"user-info-bottom-name-group\">");
        builder.AppendHtml("<p class=\"user-info-bottom-name\">");
        builder.Append(steamUser.PersonaName);
        builder.AppendHtml("</p>");

        OnlineScore best = await userModel.GetBestScore();

        builder.AppendHtml("<div class=\"position-holder\">");
        builder.AppendIconPositionGroup(best != null ? (best.Index + 1).ToString() : "-", "/Resources/UI/global.png", "global-icon");
        if (userModel.Country != string.Empty && userModel.Country != null)
        {
            builder.AppendIconPositionGroup(best != null ? (best.CountryIndex + 1).ToString() : "-",
                $"https://community.akamai.steamstatic.com/public/images/countryflags/{userModel.Country.ToLower()}.gif", "country-flag");
        }
        builder.AppendHtml("</div>");

        /*builder.AppendHtml("<p class=\"user-info-bottom-date\">");
        builder.Append($"Account created {userModel.CreationDate.ToString("dd/MM/yyyy")}."); //fuck americans!!!
        builder.AppendHtml("</p>");*/
        builder.AppendHtml("</div>");
        builder.AppendHtml("</div>");
    }

    public static void AppendIconPositionGroup(this HtmlContentBuilder builder, string position, string icon, string styleClass)
    {
        builder.AppendHtml("<div class=\"icon-position-group\">");
        builder.AppendHtml("<p class=\"user-info-bottom-scorepos\">");
        builder.Append($"#{position}");
        builder.AppendHtml("</p>");
        builder.AppendHtml($"<img src=\"{icon}\" class=\"{styleClass}\">");
        builder.AppendHtml("</div>");
    }

    public static void AppendUnderBannerBoxHolder(this HtmlContentBuilder builder, Action buildBoxes)
    {
        builder.AppendHtml("<div class=\"under-banner-box-holder\">");
        buildBoxes();
        builder.AppendHtml("</div>");
    }

    public static void AppendUnderBannerBox(this HtmlContentBuilder builder, string title, Action buildInsideContent)
    {
        builder.AppendHtml("<div class=\"under-banner-box\">");
        builder.AppendHtml("<div class=\"under-banner-box-top\">");
        builder.AppendHtml("<p class=\"under-banner-box-title\">");
        builder.Append(title);
        builder.AppendHtml("</p>");
        builder.AppendHtml("</div>");
        builder.AppendHtml("<div class=\"under-banner-box-bottom\">");
        buildInsideContent();
        builder.AppendHtml("</div>");
        builder.AppendHtml("</div>");
    }

    public static void AppendUnderBannerBoxScoreContent(this HtmlContentBuilder builder, Score? score)
    {
        string time = "No time";

        if (score != null)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(score.Time);
            time = timeSpan.ToWordString();
        }

        builder.AppendHtml("<p class=\"profile-score-text\">");
        builder.AppendHtml(string.Format(ContentController.CurrentContent.GetLocalisedString("profile.score_box_content"), score?.Rooms ?? 0, score?.Deliveries ?? 0, time, score?.Kills ?? 0));
        builder.AppendHtml("</p>");
    }

    public static void AppendAchievements(this HtmlContentBuilder builder, IEnumerable<Achievement> achievements)
    {
        builder.AppendHtml("<div class=\"achievement-holder\">");

        foreach (Achievement achievement in achievements)
        {
            builder.AppendAchievement(achievement);
        }

        builder.AppendHtml("</div>");
    }

    public static void AppendAchievement(this HtmlContentBuilder builder, Achievement achievement)
    {
        builder.AppendHtml("<div class=\"achievement-box\">");
        builder.AppendHtml($"<img class=\"achievement-icon pixel-perfect\" src=\"{achievement.Icon.AssetUri}\">");
        builder.AppendHtml("<p class=\"achievement-text\">");
        builder.Append(ContentController.CurrentContent.GetLocalisedString(achievement.Name));
        builder.AppendHtml("</p>");
        builder.AppendHtml("<div>");
    }

    public static void AppendSocialLinks(this HtmlContentBuilder builder, UserModel userModel)
    {
        builder.AppendHtml("<div class=\"social-link-holder\">");

        if (userModel.Links.Discord != string.Empty)
        {
            builder.AppendSocialButton("/Resources/UI/Socials/discord-transparent.png", userModel.Links.GetDiscord);
        }

        if (userModel.Links.Twitter != string.Empty)
        {
            builder.AppendSocialButton("/Resources/UI/Socials/twitter-transparent.png", userModel.Links.GetTwitter);
        }

        if (userModel.Links.Youtube != string.Empty)
        {
            builder.AppendSocialButton("/Resources/UI/Socials/youtube-transparent.png", userModel.Links.GetYoutube);
        }

        builder.AppendHtml("</div>");
    }

    public static void AppendSocialButton(this HtmlContentBuilder builder, string icon, string link)
    {
        builder.AppendHtml($"<a href=\"{link}\" class=\"button social-link-button\">");
        builder.AppendHtml($"<img src=\"{icon}\">");
        builder.AppendHtml("</a>");
    }
}
