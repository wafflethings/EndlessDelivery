using EndlessDelivery.Common.Communication;
using Microsoft.AspNetCore.Mvc;

namespace EndlessDelivery.Server.Api;

[ApiController]
[Route("api")]
public class ApiController : Controller
{
    [HttpGet("ping")]
    public StatusCodeResult ResetResources()
    {
        return StatusCode(StatusCodes.Status200OK);
    }

    [HttpGet]
    public void RedirectToDocs()
    {
        Response.Redirect("https://github.com/wafflethings/EndlessDelivery/wiki");
    }
}
