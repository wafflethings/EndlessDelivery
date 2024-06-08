using EndlessDelivery;
using EndlessDelivery.Scores;
using EndlessDelivery.Server.Api.Steam;
using EndlessDelivery.Server.Api.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Newtonsoft.Json;
using Postgrest.Responses;

namespace EndlessDelivery.Server.Api.Scores
{
    [ApiController]
    [Route("api/scores")]
    public class ScoresController : Controller
    {
        public static async Task<List<ScoreModel>> GetScoreModels()
        {
            ModeledResponse<ScoreModel> scoreModelResponse = await Program.Supabase.From<ScoreModel>().Get();
            return Sort(scoreModelResponse.Models);
        }

        private static List<ScoreModel> Sort(List<ScoreModel> models)
        {
            return models.OrderByDescending(x => x.Score.Rooms).ThenByDescending(x => x.Score.Deliveries)
                .ThenByDescending(x => x.Score.Kills).ThenBy(x => x.Score.Time).ToList();
        }
        
        [EnableRateLimiting("fixed")]
        [HttpGet("get_range")]
        public async Task<object> Get(int start, int count)
        {
            try
            {
                List<ScoreModel> scoreModels = await GetScoreModels();
                
                if (count > 10)
                {
                    return Json(BadRequest("Please make sure that count is smaller than 10."));
                }

                if (start + count > scoreModels.Count)
                {
                    count = scoreModels.Count - start;
                }

                List<ScoreModel> models = scoreModels.GetRange(start, count);

                //foreach (ScoreModel model in models)
               // {
                    //model.Format = (await Program.Supabase.From<SpecialUserModel>().Match(new SpecialUserModel {SteamId = model.SteamId})?.Single())?.NameFormat ?? "{0}";
               // }
                
                return JsonConvert.SerializeObject(models);
            }
            catch (Exception ex)
            {
                return Json(StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data: report this to Waffle. \n" + ex));
            }
        }
        
        [EnableRateLimiting("fixed")]
        [HttpGet("get_length")]
        public async Task<IActionResult> GetLength()
        {
            try
            {
                List<ScoreModel> scoreModels = await GetScoreModels();
                return Json(Ok(scoreModels.Count));
            }
            catch (Exception ex)
            {
                return Json(StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data: report this to Waffle. \n" + ex));
            }
        }
        
        [EnableRateLimiting("fixed")]
        [HttpGet("get_position")]
        public async Task<IActionResult> GetPosition(ulong steamId)
        {
            try
            {
                List<ScoreModel> scoreModels = await GetScoreModels();
                return Json(Ok(scoreModels.FindIndex(x => x.SteamId == steamId)));
            }
            catch (Exception ex)
            {
                return Json(StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data: report this to Waffle. \n" + ex));
            }
        }
        
        [EnableRateLimiting("fixed")]
        [HttpGet("add_score")]
        public async Task<IActionResult> Add(string score, short difficulty, string ticket, string version)
        {
            if (version != Plugin.Version)
            {
                return Json(StatusCode(StatusCodes.Status426UpgradeRequired, $"Version {Plugin.Version} required; please update."));
            }
            
            try
            {
                AuthTicket auth = await AuthTicket.GetAuth(ticket);
                ulong id = ulong.Parse(auth.OwnerSteamId);

                if (await IsUserBanned(id))
                {
                    return Json(StatusCode(StatusCodes.Status403Forbidden, "You have been banned."));
                }
                
                ScoreModel newScore = new()
                {
                    SteamId = id,
                    Score = JsonConvert.DeserializeObject<Score>(score),
                    Difficulty = difficulty,
                    Date = DateTime.UtcNow
                };

                if (!SteamUser.CacheHasId(id))
                {
                    await UsersController.RegisterUser(HttpContext, id);
                }

                UserModel userModel = await SteamUser.GetById(id).GetUserModel();
                userModel.LifetimeStats += newScore.Score;
                userModel.Country = HttpContext.GetCountry();
                await userModel.Update<UserModel>();
                
                // this will update regardless of if it is bigger but that shouldnt be happening anyway, client should prevent it
                ModeledResponse<ScoreModel> responses = await Program.Supabase.From<ScoreModel>().Upsert(newScore);
                await SetIndexes();
                return Json(Ok(responses.Models.FindIndex(x => x.SteamId == id)));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                return Json(StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data: report this to Waffle. \n" + ex));
            }
        }

        #if DEBUG
        [HttpGet("force_reset_indexes")]
        #endif
        public async Task<string> SetIndexes()
        {
            Dictionary<string, int> countryIndexes = new();
            List<ScoreModel> models = await GetScoreModels();
            Dictionary<ulong, UserModel> idToUm = (await Program.Supabase.From<UserModel>().Get()).Models.ToDictionary(model => model.SteamId, model => model);

            int index = 0;
            foreach (ScoreModel sm in models)
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

            await Program.Supabase.From<ScoreModel>().Upsert(models);
            return "Scores reset!";
        }

        private async Task<bool> IsUserBanned(ulong steamId)
        {
            ModeledResponse<UserModel> models = await Program.Supabase.From<UserModel>().Get();
            List<UserModel> specialUsers = models.Models;
            
            //foreach is bad and i probably need to fix this! or maybe cache (TODO?)
            foreach (UserModel user in specialUsers)
            {
                if (user.SteamId == steamId && user.Banned)
                {
                    return true;
                }
            }

            return false;
        }
    }
}