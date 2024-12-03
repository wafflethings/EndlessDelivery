using EndlessDelivery.Common.Inventory.Items;
using EndlessDelivery.Server.Api.ContentFile;
using EndlessDelivery.Server.Api.Users;
using Microsoft.AspNetCore.Html;

namespace EndlessDelivery.Server.Website.HtmlElements;

public static class ShopElements
{
    public static void AppendShopRotation(this HtmlContentBuilder builder, UserModel user)
    {
        builder.AppendHtml("<div class=\"item-card-holder\">");
        foreach (string itemId in ContentController.CurrentContent.GetActiveShopRotation().ItemIds)
        {
            if (ContentController.CurrentContent.TryGetItem(itemId, out Item item))
            {
                builder.AppendItemCard(item, user);
            }
        }
        builder.AppendHtml("</div>");
    }

    public static void AppendItemCard(this HtmlContentBuilder builder, Item item, UserModel user)
    {
        builder.AppendHtml($"<div class=\"item-card\" item-id=\"{item.Descriptor.Id}\">");
        builder.AppendHtml("<div class=\"item-titles-holder\">");

        builder.AppendHtml("<p class=\"item-title scale-font-to-fit\">");
        builder.Append(ContentController.CurrentContent.GetString(item.Descriptor.Name));
        builder.AppendHtml("</p>");

        builder.AppendHtml("<p class=\"item-category\">");
        builder.Append(ContentController.CurrentContent.GetString("category." + item.Descriptor.Type.ToString().ToLower()));
        builder.AppendHtml("</p>");
        builder.AppendHtml("</div>");

        bool itemOwned = user != null ? user.OwnedItemIds.Contains(item.Descriptor.Id) : false;
        string unownedStyle = itemOwned ? "display:none" : "display:block";
        string ownedStyle = itemOwned ? "display:block" : "display:none";

        builder.AppendHtml($"<img class=\"item-card-icon\" src=\"{item.Descriptor.Icon.AssetUri}\"/>");

        builder.AppendHtml($"<div id=\"unowned-price-text\" style=\"{unownedStyle}\">");
        builder.AppendHtml("<div class=\"price-holder\">");
        builder.AppendHtml("<img class=\"price-currency\" src=\"/Resources/UI/prem-currency.png\">");
        builder.AppendHtml("<p class=\"price-text\">");
        builder.Append(item.Descriptor.ShopPrice.ToString());
        builder.AppendHtml("</p>");
        builder.AppendHtml("</div>");
        builder.AppendHtml("</div>");

        builder.AppendHtml($"<div id=\"owned-price-text\" style=\"{ownedStyle}\">");
        builder.AppendHtml("<div class=\"price-holder\">");
        builder.AppendHtml("<p class=\"price-text\">");
        builder.Append(ContentController.CurrentContent.GetString("shop.owned"));
        builder.AppendHtml("</p>");
        builder.AppendHtml("</div>");
        builder.AppendHtml("</div>");

        builder.AppendHtml("</div>");
    }

    public static void AppendStarCounter(this HtmlContentBuilder builder, UserModel user)
    {
        builder.AppendHtml("<div class=\"price-holder\">");
        builder.AppendHtml("<img class=\"price-currency\" src=\"/Resources/UI/prem-currency.png\">");
        builder.AppendHtml("<p class=\"price-text\" id=\"star-counter\">");
        builder.Append(user.PremiumCurrency.ToString());
        builder.AppendHtml("</p>");
        builder.AppendHtml("</div>");
    }
}
