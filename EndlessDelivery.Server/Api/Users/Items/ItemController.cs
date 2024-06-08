using EndlessDelivery.Common.ContentFile;
using EndlessDelivery.Common.Inventory.Items;
using EndlessDelivery.Server.Api.ContentFile;
using EndlessDelivery.Server.Api.Steam;
using EndlessDelivery.Server.Website;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace EndlessDelivery.Server.Api.Users.Items;

[Route("api/users/items")]
public class ItemController : ControllerBase
{
    [HttpGet("set_loadout")]
    public async Task<StatusCodeResult> UpdateLoadout(InventoryLoadout loadout)
    {
        if (!HttpContext.TryGetLoggedInPlayer(out SteamUser steamUser))
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        UserModel user = await steamUser.GetUserModel();

        if (!user.OwnedItemIds.Contains(loadout.BannerId))
        {
            return StatusCode(StatusCodes.Status400BadRequest);
        }

        user.Loadout = loadout;
        await user.Update<UserModel>();
        return StatusCode(StatusCodes.Status200OK);
    }

    [HttpGet("active_shop")]
    public async Task<string> ActiveShopRotation() => JsonConvert.SerializeObject(ContentController.CurrentContent.GetActiveShopRotation());

    [HttpPost("buy_item")]
    public async Task<ObjectResult> BuyItem()
    {
        if (!HttpContext.TryGetLoggedInPlayer(out SteamUser steamUser) || !Request.Form.TryGetValue("item_id", out StringValues itemId) || !ContentController.CurrentContent.TryGetItem(itemId, out Item item))
        {
            return StatusCode(StatusCodes.Status400BadRequest, "Item ID either missing, doesn't correspond to an item, or the user is not logged in.");
        }

        UserModel user = await steamUser.GetUserModel();

        if (user.OwnedItemIds.Contains(item.Descriptor.Id))
        {
            return StatusCode(StatusCodes.Status400BadRequest, "Item already owned.");
        }

        if (user.PremiumCurrency >= item.Descriptor.ShopPrice)
        {
            user.PremiumCurrency -= item.Descriptor.ShopPrice;
            user.OwnedItemIds.Add(item.Descriptor.Id);
            await user.Update<UserModel>();
            return StatusCode(StatusCodes.Status200OK, "Success.");
        }

        return StatusCode(StatusCodes.Status400BadRequest, "Insufficient currency.");
    }
}
