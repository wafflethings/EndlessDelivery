using EndlessDelivery.Common.Inventory.Items;
using EndlessDelivery.Scores;
using EndlessDelivery.Server.Api.Steam;
using EndlessDelivery.Server.Api.Scores;
using EndlessDelivery.Server.Website;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Primitives;
using Supabase.Interfaces;
using Supabase.Realtime;

namespace EndlessDelivery.Server.Api.Users
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : Controller
    {
        public static async Task RegisterUser(HttpContext context, ulong id)
        {
            UserModel defaultUser = new()
            {
                SteamId = id,
                CreationDate = DateTime.UtcNow,
                LifetimeStats = new Score(0,0,0,0),
                Loadout = InventoryLoadout.Default,
                Country = context.GetCountry()
            };

            Console.WriteLine($"Registering user {id}.");

            await Program.Supabase.From<UserModel>().Upsert(defaultUser); //somehow doing insert makes it error because userid is null. this shit sucks
            await SteamUser.AddPlayerToCache(id);
        }

        [HttpGet("clear_token")]
        public async Task<StatusCodeResult> ClearToken()
        {
            if (Response.HttpContext.TryGetLoggedInPlayer(out SteamUser user) && Request.Cookies.TryGetValue("token", out string token))
            {
                Console.WriteLine($"Removing {user.PersonaName}'s token.");
                await SteamLoginController.RemoveToken(token);
            }

            return StatusCode(StatusCodes.Status200OK);
        }

        [HttpGet("update_socials")]
        public async Task<StatusCodeResult> UpdateSocialLinks()
        {
            if (!Request.HttpContext.TryGetLoggedInPlayer(out SteamUser steamUser))
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            UserModel user = await steamUser.GetUserModel();

            if (Request.Query.TryGetValue("twitter", out StringValues twitterName))
            {
                user.Links.Twitter = twitterName;
            }

            if (Request.Query.TryGetValue("youtube", out StringValues youtubeName))
            {
                user.Links.Youtube = youtubeName;
            }

            if (Request.Query.TryGetValue("discord", out StringValues discordId))
            {
                user.Links.Discord = discordId;
            }

            await user.Update<UserModel>();
            return StatusCode(StatusCodes.Status204NoContent);
        }

        [HttpPost("add_currency")]
        public async Task<ObjectResult> GiftCurrency()
        {
            if (!HttpContext.TryGetLoggedInPlayer(out SteamUser user) || !(await user.GetUserModel()).Admin)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Go away!!!!");
            }

            if (!Request.Form.TryGetValue("user_id", out StringValues userIdString) || !ulong.TryParse(userIdString, out ulong userId) || !SteamUser.TryGetPlayer(userId, out SteamUser userToGive))
            {
                return StatusCode(StatusCodes.Status400BadRequest, "User ID either not found, unparsable, or no user exists.");
            }

            if (!Request.Form.TryGetValue("amount", out StringValues amountString) || !int.TryParse(amountString, out int amount))
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Amount either not found or unparsable.");
            }

            UserModel userModel = await userToGive.GetUserModel();
            userModel.PremiumCurrency += amount;
            await userModel.Update<UserModel>();

            return StatusCode(StatusCodes.Status200OK, ":3");
        }
    }
}
