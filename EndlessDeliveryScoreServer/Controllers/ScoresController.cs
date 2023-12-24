using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EndlessDelivery.Scores;
using EndlessDeliveryScoreServer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
                
                return JsonConvert.SerializeObject(scoreModels.GetRange(start, count));
            }
            catch (Exception ex)
            {
                return Json(StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data: report this to Waffle. \n" + ex));
            }
        }
        
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
        
        [HttpGet("add_score")]
        public async Task<IActionResult> Add(string score, string ticket)
        {
            try
            {
                AuthenticateUserTicketResponse auth = await Authentication.GetAuth(ticket);
                ulong id = ulong.Parse(auth.response.@params.OwnerSteamId);

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
    }
}