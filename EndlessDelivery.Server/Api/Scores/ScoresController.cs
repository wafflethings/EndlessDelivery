using EndlessDelivery.Common.Communication;
using EndlessDelivery.Common.Communication.Scores;
using EndlessDelivery.Server.Api.Steam;
using EndlessDelivery.Server.Api.Users;
using EndlessDelivery.Server.Website;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Newtonsoft.Json;
using Supabase.Postgrest.Responses;

namespace EndlessDelivery.Server.Api.Scores
{
    [ApiController]
    [Route("api/scores")]
    public class ScoresController : Controller
    {
        public static async Task<List<OnlineScore>> GetOnlineScores()
        {
            ModeledResponse<OnlineScore> onlineScoreResponse = await Program.SupabaseClient.From<OnlineScore>().Get();
            return Sort(onlineScoreResponse.Models);
        }

        private static List<OnlineScore> Sort(List<OnlineScore> models)
        {
            return models.OrderByDescending(x => x.Score.Rooms).ThenByDescending(x => x.Score.Deliveries)
                .ThenByDescending(x => x.Score.Kills).ThenBy(x => x.Score.Time).ToList();
        }

        [EnableRateLimiting("fixed")]
        [HttpGet("get_range")]
        public async Task<object> Get(int start, int count)
        {
            List<OnlineScore> onlineScores = await GetOnlineScores();

            if (count > 10)
            {
                return StatusCode(StatusCodes.Status400BadRequest, Json(new Response<OnlineScore[]>([])));
            }

            if (start + count > onlineScores.Count)
            {
                count = onlineScores.Count - start;
            }

            List<OnlineScore> models = onlineScores.GetRange(start, count+1);
            return StatusCode(StatusCodes.Status200OK, JsonConvert.SerializeObject(new Response<OnlineScore[]>(models.ToArray())));
        }

        [EnableRateLimiting("fixed")]
        [HttpGet("get_length")]
        public async Task<ObjectResult> GetLength()
        {
            List<OnlineScore> onlineScores = await GetOnlineScores();
            return StatusCode(StatusCodes.Status200OK, Json(new Response<int>(onlineScores.Count)));
        }

        [EnableRateLimiting("fixed")]
        [HttpGet("get_position")]
        public async Task<ObjectResult> GetPosition(ulong steamId)
        {
            List<OnlineScore> onlineScores = await GetOnlineScores();
            return StatusCode(StatusCodes.Status200OK, JsonConvert.SerializeObject(new Response<int>(onlineScores.FindIndex(x => x.SteamId == steamId))));
        }

        [HttpPost("submit_score")]
        public async Task<ObjectResult> Add()
        {
            using StreamReader reader = new(Request.Body);
            SubmitScoreData? scoreRequest = JsonConvert.DeserializeObject<SubmitScoreData>(await reader.ReadToEndAsync());

            if (scoreRequest == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, Json(new Response<int>(-1)));
            }

            if (scoreRequest.Version != Plugin.Version)
            {
                return StatusCode(StatusCodes.Status426UpgradeRequired, Json(new Response<int>(-1)));
            }

            if (!HttpContext.TryGetLoggedInPlayer(out SteamUser steamUser))
            {
                return StatusCode(StatusCodes.Status400BadRequest, Json(new Response<int>(-1)));
            }

            UserModel user = await steamUser.GetUserModel();

            if (user.Banned)
            {
                return StatusCode(StatusCodes.Status403Forbidden, Json(new Response<int>(-1)));
            }

            if ((await user.GetBestScore()).Score > scoreRequest.Score)
            {
                return StatusCode(StatusCodes.Status400BadRequest, Json(new Response<int>(-1)));
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
            await user.Update<UserModel>();

            ModeledResponse<OnlineScore> responses = await Program.SupabaseClient.From<OnlineScore>().Upsert(newScore);
            await SetIndexes();
            return StatusCode(StatusCodes.Status200OK, JsonConvert.SerializeObject(new Response<int>(responses.Models.FindIndex(x => x.SteamId == user.SteamId))));
        }

        [HttpGet("force_reset_indexes")]
        public async Task<ObjectResult> SetIndexes()
        {
            if (!HttpContext.TryGetLoggedInPlayer(out SteamUser user) || !(await user.GetUserModel()).Admin)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Go away!!!!");
            }

            Dictionary<string, int> countryIndexes = new();
            List<OnlineScore> models = await GetOnlineScores();
            Dictionary<ulong, UserModel> idToUm = (await Program.SupabaseClient.From<UserModel>().Get()).Models.ToDictionary(model => model.SteamId, model => model);

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
            }

            await Program.SupabaseClient.From<OnlineScore>().Upsert(models);
            return StatusCode(StatusCodes.Status200OK, "Indexes reset.");
        }
    }
}
