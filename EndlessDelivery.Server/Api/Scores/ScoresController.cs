using EndlessDelivery.Common;
using EndlessDelivery.Common.Achievements;
using EndlessDelivery.Common.Communication.Scores;
using EndlessDelivery.Server.Api.ContentFile;
using EndlessDelivery.Server.Api.Steam;
using EndlessDelivery.Server.Api.Users;
using EndlessDelivery.Server.Database;
using EndlessDelivery.Server.Website;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace EndlessDelivery.Server.Api.Scores
{
    [ApiController]
    [Route("api/scores")]
    public class ScoresController : Controller
    {
        public static async Task<List<OnlineScore>> GetOnlineScores()
        {
            await using DeliveryDbContext dbContext = new();
            return await Sort(dbContext.Scores.AsAsyncEnumerable());
        }

        private static async Task<List<OnlineScore>> Sort(IAsyncEnumerable<OnlineScore> models)
        {
            return await models.OrderByDescending(x => x.Score.Rooms).ThenByDescending(x => x.Score.Deliveries)
                .ThenByDescending(x => x.Score.Kills).ThenBy(x => x.Score.Time).ToListAsync();
        }

        [HttpGet("get_range")]
        public async Task<ObjectResult> Get(int start, int count)
        {
            List<OnlineScore> onlineScores = await GetOnlineScores();

            if (start > onlineScores.Count - 1)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Start was too high");
            }

            if (start + count > onlineScores.Count)
            {
                count = onlineScores.Count - start;
            }

            List<OnlineScore> models = onlineScores.GetRange(start, count);
            return StatusCode(StatusCodes.Status200OK, JsonConvert.SerializeObject(models.ToArray()));
        }

        [HttpGet("get_length")]
        public async Task<ObjectResult> GetLength()
        {
            List<OnlineScore> onlineScores = await GetOnlineScores();
            return StatusCode(StatusCodes.Status200OK, onlineScores.Count);
        }

        [HttpGet("get_position")]
        public async Task<ObjectResult> GetPosition(ulong steamId)
        {
            List<OnlineScore> onlineScores = await GetOnlineScores();
            return StatusCode(StatusCodes.Status200OK, onlineScores.FindIndex(x => x.SteamId == steamId));
        }

        [HttpGet("get_score")]
        public async Task<ObjectResult> GetScore(ulong steamId)
        {
            List<OnlineScore> onlineScores = await GetOnlineScores();
            OnlineScore? score = onlineScores.Find(x => x.SteamId == steamId);

            if (score == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, $"User {steamId} has no score");
            }

            return StatusCode(StatusCodes.Status200OK, JsonConvert.SerializeObject(score));
        }

        [HttpPost("submit_score")]
        public async Task<ObjectResult> Add()
        {
            string readScore = await Request.ReadBody();
            SubmitScoreData? scoreRequest = JsonConvert.DeserializeObject<SubmitScoreData>(readScore);
            Console.WriteLine($"Sent score {readScore}");

            if (scoreRequest == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Null body");
            }

            if (scoreRequest.Version != Plugin.Version)
            {
                return StatusCode(StatusCodes.Status426UpgradeRequired, $"Delivery version {Plugin.Version} is required");
            }

            if (!HttpContext.TryGetLoggedInPlayer(out SteamUser steamUser))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "User does not have SteamUser");
            }

            UserModel? user = await steamUser.GetUserModel();

            if (user == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Couldn't get user model");
            }

            if (user.Banned)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "User banned");
            }

            OnlineScore newScore = new()
            {
                SteamId = user.SteamId,
                Score = scoreRequest.Score,
                Difficulty = scoreRequest.Difficulty,
                Date = DateTime.UtcNow
            };

            user.LifetimeStats += newScore.Score;
            user.Country = HttpContext.GetCountry();
            user.PremiumCurrency += newScore.Score.MoneyGain;
            user.CheckOnlineAchievements(newScore);
            await using DeliveryDbContext dbContext = new();
            dbContext.Users.Update(user);

            OnlineScore? userOnlineScore = await user.GetBestScore();
            if (userOnlineScore != null && userOnlineScore.Score > scoreRequest.Score)
            {
                await dbContext.SaveChangesAsync();
                return StatusCode(StatusCodes.Status200OK, JsonConvert.SerializeObject(newScore));
            }

            if (await dbContext.Scores.AnyAsync(score => score.SteamId == user.SteamId))
            {
               dbContext.Scores.Update(newScore);
            }
            else
            {
                dbContext.Scores.Add(newScore);
            }

            await SetIndexes(dbContext);
            await dbContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status200OK, JsonConvert.SerializeObject(newScore));
        }

        [HttpGet("force_reset_indexes")]
        public async Task<ObjectResult> ForceSetIndexes()
        {
            if (!HttpContext.TryGetLoggedInPlayer(out SteamUser user) || !(await user.GetUserModel()).Admin || user.SteamId == "76561199074883531") //todo remove hardcoded
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Go away!!!!");
            }

            await SetIndexes(new DeliveryDbContext());

            return StatusCode(StatusCodes.Status200OK, "Indexes reset.");
        }

        public async Task SetIndexes(DeliveryDbContext dbContext)
        {
            Dictionary<string, int> countryIndexes = new();
            List<OnlineScore> models = await GetOnlineScores();
            Dictionary<ulong, UserModel> idToUm = dbContext.Users.ToDictionary(model => model.SteamId, model => model);

            int index = 0;
            foreach (OnlineScore sm in models)
            {
                sm.Index = index;
                index++;

                string country = idToUm[sm.SteamId].Country;
                if (!countryIndexes.ContainsKey(country) && country != string.Empty)
                {
                    countryIndexes.Add(country, 0);
                }
                sm.CountryIndex = countryIndexes[country]++;
                dbContext.Scores.Update(sm);
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
