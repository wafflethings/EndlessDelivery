using EndlessDelivery.Common;
using EndlessDelivery.Common.Communication.Scores;
using EndlessDelivery.Server.Api.ContentFile;
using EndlessDelivery.Server.Api.Steam;
using Microsoft.AspNetCore.Html;

namespace EndlessDelivery.Server.Website.HtmlElements;

public static class LeaderboardElements
{
    public static void AppendPlayerEntry(this HtmlContentBuilder builder, SteamUser steamUser, OnlineScore onlineScore)
    {
        builder.AppendHtml("<div class=\"score-box\">");
        builder.AppendMainContent(steamUser, onlineScore);
        builder.AppendSecondaryContent(steamUser, onlineScore);
        builder.AppendHtml("</div>");
    }

    private static void AppendMainContent(this HtmlContentBuilder builder, SteamUser steamUser, OnlineScore onlineScore)
    {
        builder.AppendHtml("<div class=\"leaderboard-primary-content leaderboard-internal-content\">");
        builder.AppendProfileNameGroup(steamUser, onlineScore);
        builder.AppendDropdownRoomsGroup(onlineScore);
        builder.AppendHtml("</div>");
    }

    private static void AppendSecondaryContent(this HtmlContentBuilder builder, SteamUser steamUser, OnlineScore onlineScore)
    {
        builder.AppendHtml("<div class=\"leaderboard-secondary-content\">");
        builder.AppendHtml("<div class=\"leaderboard-internal-content\">");
        builder.AppendHtml("<p class=\"leaderboard-entry\">");
        builder.AppendHtml(string.Format(ContentController.CurrentContent.GetString("leaderboard.extra_info"), onlineScore.Score.Deliveries, onlineScore.Score.Kills,
            TimeSpan.FromSeconds(onlineScore.Score.Time).ToString("mm':'ss'.'ff")));
        builder.AppendHtml("</p>");
        builder.AppendHtml("<p class=\"lb-date-text\">");
        builder.Append(string.Format(ContentController.CurrentContent.GetString("leaderboard.achieved_on"), onlineScore.Date.ToString("dd/MM/yyyy")));
        builder.AppendHtml("</p>");
        builder.AppendHtml("</div>");
        builder.AppendHtml("</div>");
    }

    private static void AppendDropdownRoomsGroup(this HtmlContentBuilder builder, OnlineScore onlineScore)
    {
        builder.AppendHtml("<div class=\"lb-top-content-group\">");
        builder.AppendHtml("<p class=\"lb-diff-text\">");
        builder.Append(DdUtils.IntToDifficulty(onlineScore.Difficulty));
        builder.AppendHtml("</p>");
        builder.AppendHtml("<p class=\"lb-rooms-text\">");
        builder.Append(onlineScore.Score.Rooms.ToString());
        builder.AppendHtml("</p>");
        builder.AppendHtml("<img src=\"/Resources/UI/dropdown.png\" class=\"pixel-perfect dropdown-button\">");
        builder.AppendHtml("</div>");
    }

    private static void AppendProfileNameGroup(this HtmlContentBuilder builder, SteamUser steamUser, OnlineScore onlineScore)
    {
        builder.AppendHtml("<div class=\"lb-top-content-group\">");
        builder.AppendHtml("<p class=\"leaderboard-number\">");
        builder.Append((onlineScore.Index + 1) + ".");
        builder.AppendHtml("</p>");
        builder.AppendHtml($"<img src={steamUser.Avatar} class=\"leaderboard-entry-pfp\">");
        builder.AppendHtml($"<a class=\"leaderboard-entry-name\" href=\"/users/{steamUser.SteamId}\">");
        builder.Append(steamUser.PersonaName);
        builder.AppendHtml("</a>");
        builder.AppendHtml("</div>");
    }
}
