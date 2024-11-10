using EndlessDelivery.Common.Inventory.Items;
using EndlessDelivery.Server.Api.ContentFile;
using EndlessDelivery.Server.Api.Steam;
using EndlessDelivery.Server.Database;
using EndlessDelivery.Server.Website;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace EndlessDelivery.Server.Api.Users.Items;

[Route("api/users/items")]
public class ItemController : ControllerBase
{
    [HttpGet("get_loadout")]
    public async Task<ObjectResult> GetLoadout()
    {
        if (!HttpContext.TryGetLoggedInPlayer(out SteamUser steamUser))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Not logged in");
        }

        UserModel user = await steamUser.GetUserModel();
        return StatusCode(StatusCodes.Status200OK, JsonConvert.SerializeObject(user.Loadout));
    }

    [HttpPost("set_loadout")]
    public async Task<ObjectResult> UpdateLoadout()
    {
        if (!HttpContext.TryGetLoggedInPlayer(out SteamUser steamUser))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Not logged in");
        }

        UserModel user = await steamUser.GetUserModel();

        CosmeticLoadout? loadout = JsonConvert.DeserializeObject<CosmeticLoadout>(await Request.ReadBody());

        if (loadout == null)
        {
            return StatusCode(StatusCodes.Status400BadRequest, "Null request body");
        }

        if (!user.OwnedItemIds.Contains(loadout.BannerId) || loadout.RevolverIds.Any(revId => !user.OwnedItemIds.Contains(revId)))
        {
            return StatusCode(StatusCodes.Status400BadRequest, "Item not owned");
        }

        user.Loadout = loadout;
        await using DeliveryDbContext dbContext = new();
        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync();
        return StatusCode(StatusCodes.Status200OK, "Success");
    }

    [HttpGet("get_inventory")]
    public async Task<ObjectResult> GetInventory(ulong steamId)
    {
        if (!SteamUser.TryGetPlayer(steamId, out SteamUser user))
        {
            return StatusCode(StatusCodes.Status400BadRequest, $"No user found with id {steamId}");
        }

        UserModel? userModel = await user.GetUserModel();

        if (userModel == null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"Couldn't find user model for id {steamId}");
        }

        return StatusCode(StatusCodes.Status200OK, JsonConvert.SerializeObject(userModel.OwnedItemIds));
    }

    [HttpGet("active_shop")]
    public async Task<string> ActiveShopRotation() => JsonConvert.SerializeObject(ContentController.CurrentContent.GetActiveShopRotation());

    [HttpPost("buy_item")]
    public async Task<ObjectResult> BuyItem()
    {
        string itemId = await Request.ReadBody();
        if (!HttpContext.TryGetLoggedInPlayer(out SteamUser steamUser))
        {
            return StatusCode(StatusCodes.Status400BadRequest, "The user is not logged in.");
        }

        if (!ContentController.CurrentContent.TryGetItem(itemId, out Item item))
        {
            return StatusCode(StatusCodes.Status400BadRequest, $"Item not found with id {itemId}");
        }

        if (!ContentController.CurrentContent.GetActiveShopRotation().ItemIds.Contains(itemId))
        {
            return StatusCode(StatusCodes.Status400BadRequest, "Item ID not in active rotation.");
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
            await using DeliveryDbContext dbContext = new();
            dbContext.Users.Update(user);
            await dbContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status200OK, "Success.");
        }

        return StatusCode(StatusCodes.Status400BadRequest, "Insufficient currency.");
    }
}
