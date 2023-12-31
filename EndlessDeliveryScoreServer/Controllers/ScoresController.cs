using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EndlessDelivery;
using EndlessDelivery.Scores;
using EndlessDeliveryScoreServer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Newtonsoft.Json;
using Postgrest.Responses;

namespace EndlessDeliveryScoreServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
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
        public async Task<IActionResult> Add(string score, string ticket, string version)
        {
            if (version != Plugin.Version)
            {
                return Json(StatusCode(StatusCodes.Status426UpgradeRequired, $"Version {Plugin.Version} required; please update."));
            }
            
            try
            {
                AuthenticateUserTicketResponse auth = await Authentication.GetAuth(ticket);
                ulong id = ulong.Parse(auth.response.@params.OwnerSteamId);

                if (await IsUserBanned(id))
                {
                    return Json(StatusCode(StatusCodes.Status403Forbidden, "You have been banned."));
                }
                
                ScoreModel newScore = new()
                {
                    SteamId = id,
                    Score = JsonConvert.DeserializeObject<Score>(score)
                };

                // this will update regardless of if it is bigger but that shouldnt be happening anyway, client should prevent it
                ModeledResponse<ScoreModel> responses = await Program.Supabase.From<ScoreModel>().Upsert(newScore);
                await SetIndexes();
                return Json(Ok(responses.Models.FindIndex(x => x.SteamId == id)));
            }
            catch (Exception ex)
            {
                return Json(StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data: report this to Waffle. \n" + ex));
            }
        }

        private async Task SetIndexes()
        {
            List<ScoreModel> models = await GetScoreModels();

            int index = 0;
            foreach (ScoreModel sm in models)
            {
                sm.Index = index;
                index++;
                await sm.Update<ScoreModel>();
            }
        }

        private async Task<bool> IsUserBanned(ulong steamId)
        {
            ModeledResponse<SpecialUserModel> models = await Program.Supabase.From<SpecialUserModel>().Get();
            List<SpecialUserModel> specialUsers = models.Models;
            
            //foreach is bad but linq is worse and this table shouldnt have many
            foreach (SpecialUserModel user in specialUsers)
            {
                if (user.SteamId == steamId && user.IsBanned)
                {
                    return true;
                }
            }

            return false;
        }
    }
}