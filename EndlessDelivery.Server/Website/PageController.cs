using System.Diagnostics;
using System.Text.Encodings.Web;
using EndlessDelivery.Common.Inventory.Items;
using EndlessDelivery.Server.Api.Scores;
using EndlessDelivery.Server.Api.Steam;
using EndlessDelivery.Server.Api.Users;
using EndlessDelivery.Server.Website.HtmlElements;
using EndlessDelivery.Server.Api;
using EndlessDelivery.Server.Api.ContentFile;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace EndlessDelivery.Server.Website;

[ApiController]
[Route("")]
public class PageController : Controller
{
    [HttpGet("")]
    public async Task<ContentResult> Index()
    {
        Stopwatch sw = new();
        sw.Start();

        HtmlContentBuilder builder = new();

        builder.AppendHtml("<body>");
        await builder.AppendHeader(HttpContext);

        builder.AppendHtml("<head>");
        builder.AppendGenericHeadContent();
        builder.AppendHtml($"<meta property=\"og:title\" content=\"{ContentController.CurrentContent.GetLocalisedString("page_title.index")}\">");
        builder.AppendHtml($"<meta property=\"og:description\" content=\"{ContentController.CurrentContent.GetLocalisedString("page_desc.index")}\">");
        builder.AppendGenericEmbed(HttpContext);
        builder.AppendHtml("<link rel=\"stylesheet\" href=\"/resources/leaderboard.css\">");
        builder.AppendHtml("</head>");

        builder.AppendHtml("<div class=\"full-page-box hide-until-js\">");

        foreach (ScoreModel scoreModel in await ScoresController.GetScoreModels())
        {
            builder.AppendPlayerEntry(SteamUser.GetById(scoreModel.SteamId), scoreModel);
        }

        builder.AppendHtml("</div>");
        builder.AppendHtml("</body>");
        builder.AppendHtml("<script type=\"module\" src=\"/resources/scripts/index.js\"></script>"); //after html
        builder.AppendHtml("<script type=\"module\" src=\"/resources/scripts/misc.js\"></script>");

        builder.AppendHtml("<title>");
        builder.Append(ContentController.CurrentContent.GetLocalisedString("page_title.index"));
        builder.AppendHtml("</title>");

        await using StringWriter writer = new();
        builder.WriteTo(writer, HtmlEncoder.Default);
        Console.WriteLine($"Served to {HttpContext.GetIp()} in {sw.Elapsed.TotalSeconds}, ping from {HttpContext.GetCountry()}");
        return Content(writer.ToString(), "text/html");
    }

    [HttpGet("/users/{id}")]
    public async Task<ContentResult> UserPage()
    {
        if (!ulong.TryParse(Request.RouteValues["id"].ToString(), out ulong id))
        {
            return await ErrorPage(StatusCodes.Status400BadRequest);
        }

        if (!SteamUser.TryGetPlayer(id, out SteamUser player))
        {
            return await ErrorPage(StatusCodes.Status404NotFound);
        }
        UserModel userModel = await player.GetUserModel();

        HtmlContentBuilder builder = new();

        builder.AppendHtml("<body>");
        await builder.AppendHeader(HttpContext);

        builder.AppendHtml("<head>");
        builder.AppendGenericHeadContent();
        builder.AppendHtml("<link rel=\"stylesheet\" href=\"/resources/user.css\">");
        builder.AppendHtml($"<meta property=\"og:title\" content=\"{string.Format(ContentController.CurrentContent.GetLocalisedString("page_title.user"), player.PersonaName)}\">");
        builder.AppendHtml($"<meta property=\"og:description\" content=\"{string.Format(ContentController.CurrentContent.GetLocalisedString("page_desc.user"), player.PersonaName)}\">");
        builder.AppendGenericEmbed(HttpContext);
        builder.AppendHtml("</head>");

        builder.AppendHtml("<div class=\"full-page-box hide-until-js\">");
        await builder.AppendPlayerHeader(player, userModel);
        builder.AppendHtml("</div>");

        builder.AppendHtml("</body>");
        builder.AppendHtml("<script type=\"module\" src=\"/resources/scripts/misc.js\"></script>");

        builder.AppendHtml("<title>");
        builder.Append(string.Format(ContentController.CurrentContent.GetLocalisedString("page_title.user"), player.PersonaName));
        builder.AppendHtml("</title>");

        await using StringWriter writer = new();
        builder.WriteTo(writer, HtmlEncoder.Default);
        return Content(writer.ToString(), "text/html");
    }

    [HttpGet("/account_settings")]
    public async Task<ContentResult> AccountSettings()
    {
        if (!Request.HttpContext.TryGetLoggedInPlayer(out SteamUser user))
        {
            return await ErrorPage(StatusCodes.Status403Forbidden);
        }

        UserModel userModel = await user.GetUserModel();

        HtmlContentBuilder builder = new();

        builder.AppendHtml("<body>");
        await builder.AppendHeader(HttpContext);

        builder.AppendHtml("<head>");
        builder.AppendGenericHeadContent();
        builder.AppendHtml("<link rel=\"stylesheet\" href=\"/resources/settings.css\">");
        builder.AppendHtml("</head>");

        builder.AppendHtml("<div class=\"full-page-box hide-until-js\">");
        builder.AppendSettingSection(ContentController.CurrentContent.GetLocalisedString("settings.links"), () => builder.AppendSocialSettings(userModel));
        builder.AppendSettingSection(ContentController.CurrentContent.GetLocalisedString("settings.patreon"), () => builder.AppendPatreonSettings(HttpContext, userModel));
        builder.AppendHtml("</div>");

        builder.AppendHtml("</body>");
        builder.AppendHtml("<script type=\"module\" src=\"/resources/scripts/settings.js\"></script>");
        builder.AppendHtml("<script type=\"module\" src=\"/resources/scripts/misc.js\"></script>");

        builder.AppendHtml("<title>");
        builder.Append(ContentController.CurrentContent.GetLocalisedString("page_title.settings"));
        builder.AppendHtml("</title>");

        await using StringWriter writer = new();
        builder.WriteTo(writer, HtmlEncoder.Default);
        return Content(writer.ToString(), "text/html");
    }

    [HttpGet("/shop")]
    public async Task<ContentResult> Shop()
    {
        bool loggedIn = false;
        UserModel userModel = null;
        if (Request.HttpContext.TryGetLoggedInPlayer(out SteamUser user))
        {
            userModel = await user.GetUserModel();
            loggedIn = true;
        }

        HtmlContentBuilder builder = new();

        builder.AppendHtml("<body>");
        await builder.AppendHeader(HttpContext);

        builder.AppendHtml("<head>");
        builder.AppendGenericHeadContent();
        builder.AppendHtml($"<meta property=\"og:title\" content=\"{ContentController.CurrentContent.GetLocalisedString("page_title.shop")}\">");
        builder.AppendHtml($"<meta property=\"og:description\" content=\"{ContentController.CurrentContent.GetLocalisedString("page_desc.shop")}\">");
        builder.AppendGenericEmbed(HttpContext);
        builder.AppendHtml("<link rel=\"stylesheet\" href=\"/resources/shop.css\">");
        builder.AppendHtml("</head>");

        builder.AppendHtml("<div class=\"full-page-box hide-until-js\">");

        if (loggedIn)
        {
            builder.AppendHtml("<p>");
            builder.Append($"{userModel.PremiumCurrency} vbucks");
            builder.AppendHtml("</p>");
        }

        builder.AppendShopRotation(userModel);

        builder.AppendHtml("</div>");

        builder.AppendHtml("</body>");
        builder.AppendHtml("<script type=\"module\" src=\"/resources/scripts/shop.js\"></script>");
        builder.AppendHtml("<script type=\"module\" src=\"/resources/scripts/misc.js\"></script>");

        builder.AppendHtml("<title>");
        builder.Append(ContentController.CurrentContent.GetLocalisedString("page_title.shop"));
        builder.AppendHtml("</title>");

        await using StringWriter writer = new();
        builder.WriteTo(writer, HtmlEncoder.Default);
        return Content(writer.ToString(), "text/html");
    }

    [HttpGet("/admin_panel")]
    public async Task<ContentResult> AdminPanel()
    {
        if (!Request.HttpContext.TryGetLoggedInPlayer(out SteamUser user) || !(await user.GetUserModel()).Admin)
        {
            return await ErrorPage(StatusCodes.Status403Forbidden, "Go away!!!");
        }

        HtmlContentBuilder builder = new();

        builder.AppendHtml("<body>");
        await builder.AppendHeader(HttpContext);

        builder.AppendHtml("<head>");
        builder.AppendGenericHeadContent();
        builder.AppendHtml("<link rel=\"stylesheet\" href=\"/resources/settings.css\">");
        builder.AppendHtml("</head>");

        builder.AppendHtml("<div class=\"full-page-box hide-until-js\">");
        builder.AppendAdminPanel(HttpContext);
        builder.AppendHtml("</div>");

        builder.AppendHtml("</body>");
        builder.AppendHtml("<script type=\"module\" src=\"/resources/scripts/misc.js\"></script>");

        builder.AppendHtml("<title>");
        builder.Append(ContentController.CurrentContent.GetLocalisedString("page_title.admin"));
        builder.AppendHtml("</title>");

        await using StringWriter writer = new();
        builder.WriteTo(writer, HtmlEncoder.Default);
        return Content(writer.ToString(), "text/html");
    }

    public async Task<ContentResult> ErrorPage(int statusCode, string subtitle = "")
    {
        HtmlContentBuilder builder = new();

        builder.AppendHtml("<body>");
        await builder.AppendHeader(HttpContext);

        builder.AppendHtml("<head>");
        builder.AppendGenericHeadContent();
        builder.AppendHtml($"<meta property=\"og:title\" content=\"{string.Format(ContentController.CurrentContent.GetLocalisedString("page_title.error"), statusCode)}\">");
        builder.AppendHtml($"<meta property=\"og:description\" content=\"{ContentController.CurrentContent.GetLocalisedString("page_desc.error")}\">");
        builder.AppendGenericEmbed(HttpContext);
        builder.AppendHtml("<link rel=\"stylesheet\" href=\"/resources/error_page.css\">");
        builder.AppendHtml("</head>");

        builder.AppendHtml("</body>");
        builder.AppendHtml("<script type=\"module\" src=\"/resources/scripts/misc.js\"></script>");

        builder.AppendHtml("<title>");
        builder.Append(string.Format(ContentController.CurrentContent.GetLocalisedString("page_title.error"), statusCode));
        builder.AppendHtml("</title>");

        builder.AppendHtml("<div class=\"full-page-box error-page-box\">");
        builder.AppendHtml("<p class=\"error-page-title\">");
        builder.Append($"{statusCode} - {ReasonPhrases.GetReasonPhrase(statusCode)}");

        if (subtitle != string.Empty)
        {
            builder.Append(subtitle);
        }

        builder.AppendHtml("</p>");
        builder.AppendHtml("</div>");

        await using StringWriter writer = new();
        builder.WriteTo(writer, HtmlEncoder.Default);
        return Content(writer.ToString(), "text/html");
    }
}
