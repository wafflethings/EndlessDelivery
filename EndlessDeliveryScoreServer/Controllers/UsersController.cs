using EndlessDeliveryScoreServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Newtonsoft.Json;

namespace EndlessDeliveryScoreServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : Controller
    {
        [EnableRateLimiting("fixed")]
        [HttpGet("get_special_users")]
        public async Task<object> GetUsers()
        {
            try
            {
                return JsonConvert.SerializeObject((await Program.Supabase.From<SpecialUserModel>().Get()).Models);
            }
            catch (Exception ex)
            {
                return Json(StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving data: report this to Waffle. \n" + ex));
            }
        }
    }
}