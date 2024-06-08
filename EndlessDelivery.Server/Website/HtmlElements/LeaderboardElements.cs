using EndlessDelivery.Server.Api.Scores;
using EndlessDelivery.Server.Api.Steam;
using Microsoft.AspNetCore.Html;

namespace EndlessDelivery.Server.Website.HtmlElements;

public static class LeaderboardElements
{
    public static void AppendPlayerEntry(this HtmlContentBuilder builder, SteamUser steamUser, ScoreModel scoreModel)
    {
        builder.AppendHtml("<div class=\"score-box\">");
        builder.AppendMainContent(steamUser, scoreModel);
        builder.AppendSecondaryContent(steamUser, scoreModel);
        builder.AppendHtml("</div>");
    }

    private static void AppendMainContent(this HtmlContentBuilder builder, SteamUser steamUser, ScoreModel scoreModel)
    {
        builder.AppendHtml("<div class=\"leaderboard-primary-content leaderboard-internal-content\">");
        builder.AppendProfileNameGroup(steamUser, scoreModel);
        builder.AppendDropdownRoomsGroup(scoreModel);
        builder.AppendHtml("</div>");
    }

    private static void AppendSecondaryContent(this HtmlContentBuilder builder, SteamUser steamUser, ScoreModel scoreModel)
    {
        builder.AppendHtml("<div class=\"leaderboard-secondary-content\">");
        builder.AppendHtml("<div class=\"leaderboard-internal-content\">");
        builder.AppendLeaderboardData($"Deliveries: {scoreModel.Score.Deliveries}");
        builder.AppendLeaderboardData($"Kills: {scoreModel.Score.Kills}");
        builder.AppendLeaderboardData($"Time: {TimeSpan.FromSeconds(scoreModel.Score.Time).ToString("mm':'ss'.'ff")}");
        builder.AppendHtml("</div>");
        builder.AppendHtml("</div>");
    }

    private static void AppendDropdownRoomsGroup(this HtmlContentBuilder builder, ScoreModel scoreModel)
    {
        builder.AppendHtml("<div class=\"lb-top-content-group\">");
        builder.AppendHtml("<p class=\"lb-diff-text\">");
        builder.Append(SiteUtils.IntToDifficulty(scoreModel.Difficulty));
        builder.AppendHtml("</p>");
        builder.AppendHtml("<p class=\"lb-rooms-text\">");
        builder.Append(scoreModel.Score.Rooms.ToString());
        builder.AppendHtml("</p>");
        builder.AppendHtml("<img src=\"/resources/dropdown.png\" class=\"pixel-perfect dropdown-button\">");
        builder.AppendHtml("</div>");
    }

    private static void AppendProfileNameGroup(this HtmlContentBuilder builder, SteamUser steamUser, ScoreModel scoreModel)
    {
        builder.AppendHtml("<div class=\"lb-top-content-group\">");
        builder.AppendHtml("<p class=\"leaderboard-number\">");
        builder.Append((scoreModel.Index + 1) + ".");
        builder.AppendHtml("</p>");
        builder.AppendHtml($"<img src={steamUser.Avatar} class=\"leaderboard-entry-pfp\">");
        builder.AppendHtml($"<a class=\"leaderboard-entry-name\" href=\"/users/{steamUser.SteamId}\">");
        builder.Append(steamUser.PersonaName);
        builder.AppendHtml("</a>");
        builder.AppendHtml("<p class=\"lb-date-text\">");
        builder.Append(scoreModel.Date.ToString("dd/MM/yyyy"));
        builder.AppendHtml("</p>");
        builder.AppendHtml("</div>");
    }
    private static void AppendLeaderboardData(this HtmlContentBuilder builder, string content)
    {
        builder.AppendHtml("<p class=\"leaderboard-entry\">");
        builder.Append(content);
        builder.AppendHtml("</p>");
    }
}