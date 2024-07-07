using EndlessDelivery.Server.Api.ContentFile;
using EndlessDelivery.Server.Api.Patreon;
using EndlessDelivery.Server.Api.Users;
using Microsoft.AspNetCore.Html;

namespace EndlessDelivery.Server.Website.HtmlElements;

public static class SettingsElements
{
    public static void AppendSettingSection(this HtmlContentBuilder builder, string name, Action midSetting)
    {
        builder.AppendHtml("<div class=\"settings-card\">");
        builder.AppendHtml("<p class=\"setting-card-title\">");
        builder.Append(name);
        builder.AppendHtml("</p>");
        builder.AppendHtml("<div class=\"setting-holder\">");
        midSetting();
        builder.AppendHtml("</div>");
        builder.AppendHtml("</div>");
    }

    public static void AppendSocialSettings(this HtmlContentBuilder builder, UserModel userModel)
    {
        builder.AppendHtml("<form action=\"/api/users/update_socials\" method=\"get\" autocomplete=\"off\">");
        builder.AppendSocialLink(ContentController.CurrentContent.GetLocalisedString("settings.youtube"), UserLinks.YoutubeRoot, "youtube", "youtube-transparent.png", userModel.Links.Youtube);
        builder.AppendSocialLink(ContentController.CurrentContent.GetLocalisedString("settings.twitter"), UserLinks.TwitterRoot, "twitter", "twitter-transparent.png", userModel.Links.Twitter);
        builder.AppendSocialLink(ContentController.CurrentContent.GetLocalisedString("settings.discord"), UserLinks.DiscordRoot, "discord", "discord-transparent.png", userModel.Links.Discord);
        builder.AppendHtml("<input type=\"submit\" value=\"Submit\" class=\"button\">");
        builder.AppendHtml("</form>");
    }

    public static void AppendPatreonSettings(this HtmlContentBuilder builder, HttpContext context, UserModel userModel)
    {
        builder.AppendHtml($"<a href=\"{PatreonApi.BuildOAuthLogin(context, false, "/api/auth/patreon/return_url")}\" class=\"button\">");
        builder.Append(ContentController.CurrentContent.GetLocalisedString("settings.patreon.link"));
        builder.AppendHtml("</a>");
    }

    public static void AppendSocialLink(this HtmlContentBuilder builder, string title, string root, string id, string image, string defaultValue)
    {
        builder.AppendHtml("<div class=\"social-link-box\">");
        builder.AppendHtml($"<image src=\"/Resources/UI/Socials/{image}\" class=\"social-link-icon\">");
        builder.AppendHtml("<div>");
        builder.AppendHtml("<p class=\"social-link-name\">");
        builder.Append(title);
        builder.AppendHtml("</p>");
        builder.AppendHtml($"<label for=\"{id}\">");
        builder.Append(root);
        builder.AppendHtml("</label>");
        builder.AppendHtml($"<input type=\"text\" id=\"{id}\" name=\"{id}\" value=\"{defaultValue}\">");
        builder.AppendHtml("</div>");
        builder.AppendHtml("</div>");
    }
}
