using EndlessDelivery.Server.Api.ContentFile;
using EndlessDelivery.Server.Api.Scores;
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
        builder.AppendHtml($"<img class=\"user-info-banner\" src=\"{ContentController.CurrentContent.GetBanner(userModel.Loadout.BannerId).AssetUri}\"/>");
    }

    public static async Task AppendPlayerHeaderInfo(this HtmlContentBuilder builder, SteamUser steamUser, UserModel userModel)
    {
        builder.AppendHtml("<div class=\"user-info-bottom\">");
        builder.AppendHtml($"<img class=\"user-info-pfp circle-clip\" src=\"{steamUser.AvatarFull}\"/>");
        builder.AppendHtml("<div class=\"user-info-bottom-name-group\">");
        builder.AppendHtml("<p class=\"user-info-bottom-name\">");
        builder.Append(steamUser.PersonaName);
        builder.AppendHtml("</p>");

        ScoreModel best = await userModel.GetBestScore();

        builder.AppendHtml("<div class=\"position-holder\">");
        builder.AppendIconPositionGroup(best != null ? (best.Index + 1).ToString() : "-", "/resources/global.png", "global-icon");
        if (userModel.Country != string.Empty && userModel.Country != null)
        {
            builder.AppendIconPositionGroup(best != null ? (best.CountryIndex + 1).ToString() : "-",
                $"https://community.akamai.steamstatic.com/public/images/countryflags/{userModel.Country.ToLower()}.gif", "country-flag");
        }
        builder.AppendHtml("</div>");

        //builder.AppendHtml("<p class=\"user-info-bottom-date\">");
        //builder.Append($"Account created {userModel.CreationDate.ToString("dd/MM/yyyy")}."); //fuck americans!!!
        //builder.AppendHtml("</p>");
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

    public static void AppendSocialLinks(this HtmlContentBuilder builder, UserModel userModel)
    {
        builder.AppendHtml("<div class=\"social-link-holder\">");

        builder.AppendSocialButton("/resources/icons/discord-transparent.png", userModel.Links.GetDiscord);
        builder.AppendSocialButton("/resources/icons/twitter-transparent.png", userModel.Links.GetTwitter);
        builder.AppendSocialButton("/resources/icons/youtube-transparent.png", userModel.Links.GetYoutube);

        builder.AppendHtml("</div>");
    }

    public static void AppendSocialButton(this HtmlContentBuilder builder, string icon, string link)
    {
        builder.AppendHtml($"<a href=\"{link}\" class=\"button social-link-button\">");
        builder.AppendHtml($"<img src=\"{icon}\">");
        builder.AppendHtml("</a>");
    }
}
