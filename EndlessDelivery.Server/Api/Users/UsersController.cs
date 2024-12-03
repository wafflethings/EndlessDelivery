using EndlessDelivery.Common;
using EndlessDelivery.Common.Communication.Scores;
using EndlessDelivery.Common.Inventory.Items;
using EndlessDelivery.Server.Api.ContentFile;
using EndlessDelivery.Server.Api.Steam;
using EndlessDelivery.Server.Database;
using EndlessDelivery.Server.Website;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace EndlessDelivery.Server.Api.Users
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : Controller
    {
        public static async Task RegisterUser(HttpContext context, ulong id)
        {
            await using DeliveryDbContext dbContext = new();

            UserModel defaultUser = new()
            {
                SteamId = id,
                CreationDate = DateTime.UtcNow,
                LifetimeStats = new Score(0,0,0,0),
                Loadout = CosmeticLoadout.Default,
                Country = context.GetCountry(),
            };

            Console.WriteLine($"Registering user {id}.");
            defaultUser.OwnedItemIds.AddRange(CosmeticLoadout.DefaultItems);
            dbContext.Users.Add(defaultUser);
            await dbContext.SaveChangesAsync();
            await SteamUser.AddPlayerToCache(id);
        }

        [HttpGet("get_currency_amount")]
        public async Task<ObjectResult> GetCurrencyAmount()
        {
            if (!Request.HttpContext.TryGetLoggedInPlayer(out SteamUser? steamUser))
            {
                return StatusCode(StatusCodes.Status401Unauthorized, "Not logged in");
            }

            UserModel? user = await steamUser.GetUserModel();

            if (user == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, string.Empty);
            }

            return StatusCode(StatusCodes.Status200OK, user.PremiumCurrency);
        }

        [HttpPost("grant_achievement")]
        public async Task<ObjectResult> GrantAchievement()
        {
            if (!Request.IsOnLatestUpdate())
            {
                return StatusCode(StatusCodes.Status426UpgradeRequired, $"Delivery version {Plugin.Version} is required");
            }

            if (Request.UserModded())
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Disable mods");
            }

            if (!Request.HttpContext.TryGetLoggedInPlayer(out SteamUser? steamUser))
            {
                return StatusCode(StatusCodes.Status401Unauthorized, "Not logged in");
            }

            UserModel? user = await steamUser.GetUserModel();

            if (user == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, string.Empty);
            }

            string achievementId = await Request.ReadBody();

            if (!ContentController.CurrentContent.Achievements.TryGetValue(achievementId, out Achievement? achievement) || achievement == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, $"Achievement {achievementId} not found!");
            }

            if (achievement.Serverside || achievement.Disabled)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Nice try, stop cheating.");
            }

            if (user.OwnedAchievements.Any(x => x.Id == achievementId))
            {
                return StatusCode(StatusCodes.Status400BadRequest, $"Achievement {achievementId} already owned");
            }

            await using DeliveryDbContext dbContext = new();
            user.GetAchievement(achievement);
            dbContext.Users.Update(user);
            await dbContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status200OK, string.Empty);
        }

        [HttpGet("get_achievements")]
        public async Task<ObjectResult> GetAchievements(ulong steamId)
        {
            if (!SteamUser.TryGetPlayer(steamId, out SteamUser steamUser))
            {
                return StatusCode(StatusCodes.Status404NotFound, $"Player {steamId} not found");
            }

            UserModel? userModel = await steamUser.GetUserModel();

            if (userModel == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Server couldn't find usermodel for {steamId}");
            }

            return StatusCode(StatusCodes.Status200OK, JsonConvert.SerializeObject(userModel.OwnedAchievements));
        }

        [HttpGet("get_username")]
        public ObjectResult GetUsername(ulong steamId)
        {
            if (!SteamUser.TryGetPlayer(steamId, out SteamUser steamUser))
            {
                return StatusCode(StatusCodes.Status404NotFound, $"Player {steamId} not found");
            }

            return StatusCode(StatusCodes.Status200OK, steamUser.PersonaName);
        }

        [HttpGet("lifetime_stats")]
        public async Task<ObjectResult> GetStats(ulong steamId)
        {
            if (!SteamUser.TryGetPlayer(steamId, out SteamUser steamUser))
            {
                return StatusCode(StatusCodes.Status404NotFound, $"Player {steamId} not found");
            }

            UserModel? userModel = await steamUser.GetUserModel();

            if (userModel == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Server couldn't find usermodel for {steamId}");
            }

            return StatusCode(StatusCodes.Status200OK, JsonConvert.SerializeObject(userModel.LifetimeStats));
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

            UserModel? user = await steamUser.GetUserModel();

            if (user == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

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

            await using DeliveryDbContext dbContext = new();
            dbContext.Users.Update(user);
            await dbContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status204NoContent);
        }

        [EnableRateLimiting("fixed")]
        [HttpGet("embed/{id}")]
        public async Task<object> UserEmbed() //straight up the worst code i have ever written. i gave up and started hardcoding. but it works
        {
            if (!ulong.TryParse(Request.RouteValues["id"].ToString(), out ulong id))
            {
                return StatusCode(StatusCodes.Status400BadRequest, "");
            }

            if (!SteamUser.TryGetPlayer(id, out SteamUser steamUser))
            {
                return StatusCode(StatusCodes.Status400BadRequest, "");
            }

            UserModel user = await steamUser.GetUserModel();
            Banner currentBanner = ContentController.CurrentContent.Banners[user.Loadout.BannerId];

            string[] path = ["Assets"];
            path = path.Concat(currentBanner.Asset.AssetUri.Substring(1, currentBanner.Asset.AssetUri.Length - 1).Split("/")).ToArray();
            using Image image = await Image.LoadAsync(Path.Combine(path));

            HttpResponseMessage response = await Program.Client.GetAsync(steamUser.AvatarFull);
            using Image pfp = await Image.LoadAsync(await response.Content.ReadAsStreamAsync());

            using MemoryStream ms = new();

            const int pfpSize = 256;
            const int width = 1200;
            const int height = 600;
            const int padding = 100;
            const int fontSize = 75;
            const float charWidthPerPt = 1302f/2000f;
            const float charHeightPerPt = 1823f/2000f;

            Font font = SystemFonts.Get("VCR OSD Mono").CreateFont((fontSize * 6) / MathF.Max(6, steamUser.PersonaName.Length));
            int fontHeightOffset = (int)((charHeightPerPt * font.Size) / 2);
            pfp.Mutate(img => img.Resize(new ResizeOptions { Size = new Size(pfpSize), Sampler = KnownResamplers.NearestNeighbor }));

            int bannerResizeScalar = height / image.Height;
            image.Mutate(img => img.Resize(new Size(image.Width * bannerResizeScalar, image.Height * bannerResizeScalar)));
            image.Mutate(img => img.Crop(new Rectangle((image.Width / 2) - (width / 2), 0, width, height)));
            image.Mutate(img => img.GaussianBlur(10));
            image.Mutate(img => img.Brightness(0.75f));

            int pfpHeight = (height / 2) - (pfpSize / 2) - fontHeightOffset;
            image.Mutate(img => img.DrawImage(pfp, new Point(padding, pfpHeight), 1));

            image.Mutate(img => img.DrawText(new DrawingOptions(), steamUser.PersonaName, font, Color.White,
                new PointF(padding, pfpHeight + pfpSize)));

            int? scoreIndex = (await user.GetBestScore())?.Index;

            if (scoreIndex != null)
            {
                float bigFontSize = 680f / (1 + scoreIndex.ToString().Length);
                Font fontScaled = SystemFonts.Get("VCR OSD Mono").CreateFont(bigFontSize);

                Console.WriteLine($"{bigFontSize} * 0.5 * {charHeightPerPt}");

                image.Mutate(img => img.DrawText($"#{(scoreIndex + 1).ToString()}", fontScaled, Color.White, new PointF(width - (int)(charWidthPerPt * bigFontSize * $"#{scoreIndex + 1}".Length) - 40,
                    (height / 2) - (bigFontSize * 0.55f * charHeightPerPt)))); //i dont know how. i dont know why. but 0.55 makes it work perfectly EVERY time. what the fuck
            }

            await image.SaveAsync(ms, WebpFormat.Instance);
            return File(ms.ToArray(), "image/webp");
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
            await using DeliveryDbContext dbContext = new();
            dbContext.Users.Update(userModel);
            await dbContext.SaveChangesAsync();

            return StatusCode(StatusCodes.Status200OK, ":3");
        }
    }
}
