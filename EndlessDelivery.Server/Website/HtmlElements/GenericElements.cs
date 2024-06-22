using EndlessDelivery.Server.Api.ContentFile;
using EndlessDelivery.Server.Api.Steam;
using EndlessDelivery.Server.Api.Users;
using Microsoft.AspNetCore.Html;

namespace EndlessDelivery.Server.Website.HtmlElements;

public static class GenericElements
{
    public static void AppendGenericHeadContent(this HtmlContentBuilder builder)
    {
        builder.AppendHtml("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        builder.AppendHtml("<link rel=\"stylesheet\" href=\"/resources/misc.css\">");
        builder.AppendHtml("<link rel=\"stylesheet\" href=\"/resources/colours.css\">");
        builder.AppendHtml("<link rel=\"icon\" href=\"/resources/icons/favicon.ico\">");
        builder.AppendHtml("<link rel=\"icon\" type=\"image/png\" sizes=\"32x32\" href=\"/resources/icons/favicon-32x32.png\">");
        builder.AppendHtml("<link rel=\"icon\" type=\"image/png\" sizes=\"64x64\" href=\"/resources/icons/favicon-64x64.png\">");
        builder.AppendHtml("<link rel=\"icon\" type=\"image/png\" sizes=\"192x192\" href=\"/resources/icons/favicon-192x192.png\">");
        builder.AppendHtml("<link rel=\"icon\" type=\"image/png\" sizes=\"256x256\" href=\"/resources/icons/favicon-256x256.png\">");
        builder.AppendHtml("<link rel=\"stylesheet\" href=\"https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined:opsz,wght,FILL,GRAD@20..48,100..700,0..1,-50..200\"/>");
    }

    public static async Task AppendHeader(this HtmlContentBuilder builder, HttpContext context)
    {
        builder.AppendHtml("<div class=\"top-bar\">");
        builder.AppendHtml("<div class=\"top-bar-group\">");
        builder.AppendHtml("<img src=\"/resources/logo-transparent.png\" class=\"top-bar-icon pixel-perfect\">");
        builder.AppendHtml("<a href=\"/\" class=\"top-bar-title-text\">");
        builder.Append(ContentController.CurrentContent.GetLocalisedString("navbar.mod_name"));
        builder.AppendHtml("</a>");
        builder.AppendHtml("</div>");
        builder.AppendHtml("<div class=\"top-bar-group\">");
        builder.AppendHtml("<a href=\"/shop\">");
        builder.Append(ContentController.CurrentContent.GetLocalisedString("navbar.shop_link"));
        builder.AppendHtml("</a>");
        builder.AppendHtml("<a href=\"https://thunderstore.io/c/ultrakill/p/Waff1e/Divine_Delivery/\">");
        builder.Append(ContentController.CurrentContent.GetLocalisedString("navbar.download_link"));
        builder.AppendHtml("</a>");

        bool loggedIn = context.TryGetLoggedInPlayer(out SteamUser player);
        string imageOrigin = loggedIn ? player.Avatar : "/resources/fallback-pfp.png";

        builder.AppendHtml($"<img src=\"{imageOrigin}\" class=\"top-bar-pfp circle-clip\"/>");

        builder.AppendMiniProfile(context, loggedIn, player, loggedIn ? await player.GetUserModel() : null);
        builder.AppendHtml("</div>");
        builder.AppendHtml("</div>");
    }

    public static void AppendMiniProfile(this HtmlContentBuilder builder, HttpContext context, bool loggedIn, SteamUser steamUser, UserModel userModel)
    {
        builder.AppendHtml("<div class=\"top-mini-profile\">");

        if (loggedIn)
        {
            if (loggedIn && userModel.Admin)
            {
                builder.AppendHtml("<a href=\"/admin_panel\">");
                builder.Append(ContentController.CurrentContent.GetLocalisedString("sidebar.admin"));
                builder.AppendHtml("</a>");
            }

            builder.AppendHtml($"<a href=\"/users/{steamUser.SteamId}\">");
            builder.Append(ContentController.CurrentContent.GetLocalisedString("sidebar.profile"));
            builder.AppendHtml("</a>");

            builder.AppendHtml("<a href=\"/account_settings\">");
            builder.Append(ContentController.CurrentContent.GetLocalisedString("sidebar.acc_settings"));
            builder.AppendHtml("</a>");

            builder.AppendHtml("<a href=\"#\" id=\"log-out-link\">");
            builder.Append(ContentController.CurrentContent.GetLocalisedString("sidebar.logout"));
            builder.AppendHtml("</a>");
        }
        else
        {
            builder.AppendHtml($"<a href=\"{SteamApi.BuildOpenIdLogin(context, "/api/auth/steam/return_url")}/\">");
            builder.Append(ContentController.CurrentContent.GetLocalisedString("sidebar.login"));
            builder.AppendHtml("</a>");
        }

        builder.AppendHtml("</div>");
    }
}
