using EndlessDelivery.Common.ContentFile;
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
    public async Task<ObjectResult> GetLoadout(ulong steamId)
    {
        if (!SteamUser.TryGetPlayer(steamId, out SteamUser steamUser))
        {
            return StatusCode(StatusCodes.Status400BadRequest, $"No user with ID {steamId}");
        }

        UserModel? user = await steamUser.GetUserModel();

        if (user == null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"Server couldn't find UserModel for {steamId}");
        }

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

        List<List<string>> allSkins = [loadout.RevolverIds, loadout.AltRevolverIds, loadout.ShotgunIds, loadout.AltShotgunIds, loadout.NailgunIds, loadout.AltNailgunIds, loadout.RailcannonIds, loadout.RocketIds];

        if (!user.OwnedItemIds.Contains(loadout.BannerId) || allSkins.Any(x => x.Any(id => !user.OwnedItemIds.Contains(id))))
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

    [HttpPost("claim_daily_reward")]
    public async Task<ObjectResult> ClaimReward()
    {
        if (!HttpContext.TryGetLoggedInPlayer(out SteamUser steamUser))
        {
            return StatusCode(StatusCodes.Status400BadRequest, "The user is not logged in.");
        }

        UserModel user = await steamUser.GetUserModel();

        CalendarReward reward = ContentController.CurrentContent.CurrentCalendarReward;
        if (reward == null)
        {
            return StatusCode(StatusCodes.Status400BadRequest, "No reward is currently active.");
        }

        if (user.ClaimedDays.Contains(reward.Id))
        {
            return StatusCode(StatusCodes.Status400BadRequest, "Reward already claimed.");
        }

        user.ClaimedDays.Add(reward.Id);

        if (reward.HasCurrency)
        {
            user.PremiumCurrency += reward.CurrencyAmount;
        }

        if (reward.HasItem)
        {
            user.OwnedItemIds.Add(reward.ItemId);
        }

        await using DeliveryDbContext dbContext = new();
        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync();

        return StatusCode(StatusCodes.Status200OK, "Success.");
    }

    [HttpGet("get_claimed_rewards")]
    public async Task<ObjectResult> GetClaimedRewards()
    {
        if (!HttpContext.TryGetLoggedInPlayer(out SteamUser steamUser))
        {
            return StatusCode(StatusCodes.Status400BadRequest, "The user is not logged in.");
        }

        UserModel user = await steamUser.GetUserModel();
        return StatusCode(StatusCodes.Status200OK, JsonConvert.SerializeObject(user.ClaimedDays));
    }
}
