using EndlessDelivery.Server.Api.ContentFile;
using EndlessDelivery.Server.Api.Users;
using Microsoft.AspNetCore.Html;
using Newtonsoft.Json;

namespace EndlessDelivery.Server.Website.HtmlElements;

public static class AdminPanelElements
{
    public static void AppendAdminPanel(this HtmlContentBuilder builder)
    {
        builder.AppendCmsEditor();
        builder.AppendCurrencyGiver();
        builder.AppendScoreSubmitter();
    }

    public static void AppendScoreSubmitter(this HtmlContentBuilder builder)
    {
        builder.AppendSettingSection("Score Submitter", () =>
        {
            builder.AppendHtml("<form action=\"/api/scores/add_score\" method=\"get\" autocomplete=\"off\">");
            builder.AppendScoreSubmitterField("score");
            builder.AppendScoreSubmitterField("difficulty");
            builder.AppendScoreSubmitterField("ticket");
            builder.AppendScoreSubmitterField("version");
            builder.AppendHtml("<input type=\"submit\" value=\"Submit\" class=\"button\">");
            builder.AppendHtml("</form>");
        });
    }

    public static void AppendCurrencyGiver(this HtmlContentBuilder builder)
    {
        builder.AppendSettingSection("Currency Giver", () =>
        {
            builder.AppendHtml("<form action=\"/api/users/add_currency\" method=\"post\" autocomplete=\"off\">");
            builder.AppendScoreSubmitterField("user_id");
            builder.AppendScoreSubmitterField("amount");
            builder.AppendHtml("<input type=\"submit\" value=\"Submit\" class=\"button\">");
            builder.AppendHtml("</form>");
        });
    }

    public static void AppendScoreSubmitterField(this HtmlContentBuilder builder, string name)
    {
        builder.AppendHtml($"<label for=\"{name}\">");
        builder.Append(name);
        builder.AppendHtml("</label>");
        builder.AppendHtml($"<input type=\"text\" id=\"{name}\" name=\"{name}\"/>");
        builder.AppendHtml("<br>");
    }

    public static void AppendCmsEditor(this HtmlContentBuilder builder)
    {
        builder.AppendSettingSection("CMS Editor", () =>
        {
            builder.AppendHtml("<form action=\"/api/cms/update\" method=\"post\" autocomplete=\"off\">");
            builder.AppendHtml("<textarea id=\"content\" name=\"content\" rows=\"50\" cols=\"100\">");
            builder.Append(JsonConvert.SerializeObject(ContentController.CurrentContent, Formatting.Indented));
            builder.AppendHtml("</textarea>");
            builder.AppendHtml("<br>");
            builder.AppendHtml("<input type=\"submit\" value=\"Submit\" class=\"button\">");
            builder.AppendHtml("</form>");
        });
    }
}
