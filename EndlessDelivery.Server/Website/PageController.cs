using System.Diagnostics;
using System.Text.Encodings.Web;
using EndlessDelivery.Common;
using EndlessDelivery.Common.Communication.Scores;
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
        builder.AppendEmbed(HttpContext, ContentController.CurrentContent.GetLocalisedString("page_title.index"), ContentController.CurrentContent.GetLocalisedString("page_desc.index"));
        builder.AppendHtml("<link rel=\"stylesheet\" href=\"/Resources/Stylesheets/leaderboard.css\">");
        builder.AppendHtml("</head>");

        builder.AppendHtml("<div class=\"full-page-box hide-until-js\">");

        foreach (OnlineScore onlineScore in await ScoresController.GetOnlineScores())
        {
            builder.AppendPlayerEntry(SteamUser.GetById(onlineScore.SteamId), onlineScore);
        }

        builder.AppendHtml("</div>");
        builder.AppendHtml("</body>");
        builder.AppendHtml("<script type=\"module\" src=\"/Resources/Scripts/build/index.js\"></script>"); //after html
        builder.AppendHtml("<script type=\"module\" src=\"/Resources/Scripts/build/misc.js\"></script>");

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
        builder.AppendHtml("<link rel=\"stylesheet\" href=\"/Resources/Stylesheets/user.css\">");
        builder.AppendEmbed(HttpContext, string.Format(ContentController.CurrentContent.GetLocalisedString("page_title.user"), player.PersonaName),
            string.Format(ContentController.CurrentContent.GetLocalisedString("page_desc.user"), player.PersonaName), "/api/users/embed/" + player.SteamId);
        builder.AppendHtml("</head>");

        builder.AppendHtml("<div class=\"full-page-box hide-until-js\">");
        await builder.AppendPlayerHeader(player, userModel);

        // OnlineScore? bestScore = await userModel.GetBestScore();
        // builder.AppendUnderBannerBoxHolder(() =>
        // {
        //     builder.AppendUnderBannerBox("Best Score", () =>
        //     {
        //         builder.AppendUnderBannerBoxScoreContent(bestScore?.Score);
        //     });
        // });

        builder.AppendUnderBannerBoxHolder(() =>
        {
            builder.AppendUnderBannerBox("Lifetime Stats", () =>
            {
                builder.AppendUnderBannerBoxScoreContent(userModel.LifetimeStats);
            });
        });

        builder.AppendUnderBannerBoxHolder(() =>
        {
            builder.AppendUnderBannerBox("Achievements", () =>
            {
                List<Achievement> achievements = new();
                foreach (OwnedAchievement owned in userModel.OwnedAchievements)
                {
                    if (!ContentController.CurrentContent.Achievements.TryGetValue(owned.Id, out Achievement? achievement))
                    {
                        Console.Error.WriteLine($"User {userModel.SteamId} has achievement not in CMS: {owned.Id}");
                        continue;
                    }

                    achievements.Add(achievement);
                }

                builder.AppendAchievements(achievements.OrderBy(x => ContentController.CurrentContent.GetLocalisedString(x.Name)));
            });
        });

        builder.AppendHtml("</div>");
        builder.AppendHtml("<br>");

        builder.AppendHtml("</body>");
        builder.AppendHtml("<script type=\"module\" src=\"/Resources/Scripts/build/misc.js\"></script>");

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
        builder.AppendHtml("<link rel=\"stylesheet\" href=\"/Resources/Stylesheets/settings.css\">");
        builder.AppendHtml("</head>");

        builder.AppendHtml("<div class=\"full-page-box hide-until-js\">");
        builder.AppendSettingSection(ContentController.CurrentContent.GetLocalisedString("settings.links"), () => builder.AppendSocialSettings(userModel));
        builder.AppendHtml("</div>");

        builder.AppendHtml("</body>");
        builder.AppendHtml("<script type=\"module\" src=\"/Resources/Scripts/build/settings.js\"></script>");
        builder.AppendHtml("<script type=\"module\" src=\"/Resources/Scripts/build/misc.js\"></script>");

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
        builder.AppendEmbed(HttpContext, ContentController.CurrentContent.GetLocalisedString("page_title.shop"), ContentController.CurrentContent.GetLocalisedString("page_desc.shop"));
        builder.AppendHtml("<link rel=\"stylesheet\" href=\"/Resources/Stylesheets/shop.css\">");
        builder.AppendHtml("</head>");

        builder.AppendHtml("<div class=\"full-page-box hide-until-js\">");

        builder.AppendShopRotation(userModel);
        if (loggedIn)
        {
            builder.AppendStarCounter(userModel);
        }

        builder.AppendHtml("</div>");

        builder.AppendHtml("</body>");
        builder.AppendHtml("<script type=\"module\" src=\"/Resources/Scripts/build/shop.js\"></script>");
        builder.AppendHtml("<script type=\"module\" src=\"/Resources/Scripts/build/misc.js\"></script>");

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
        builder.AppendHtml("<link rel=\"stylesheet\" href=\"/Resources/Stylesheets/settings.css\">");
        builder.AppendHtml("</head>");

        builder.AppendHtml("<div class=\"full-page-box hide-until-js\">");
        builder.AppendAdminPanel(HttpContext);
        builder.AppendHtml("</div>");

        builder.AppendHtml("</body>");
        builder.AppendHtml("<script type=\"module\" src=\"/Resources/Scripts/build/misc.js\"></script>");

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
        builder.AppendEmbed(HttpContext, string.Format(ContentController.CurrentContent.GetLocalisedString("page_title.error"), statusCode),
            ContentController.CurrentContent.GetLocalisedString("page_desc.error"));
        builder.AppendHtml("<link rel=\"stylesheet\" href=\"/Resources/Stylesheets/error_page.css\">");
        builder.AppendHtml("</head>");

        builder.AppendHtml("</body>");
        builder.AppendHtml("<script type=\"module\" src=\"/Resources/Scripts/src/misc.js\"></script>");

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
