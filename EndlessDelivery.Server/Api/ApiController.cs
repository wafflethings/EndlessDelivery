using Microsoft.AspNetCore.Mvc;

namespace EndlessDelivery.Server.Api;

[ApiController]
[Route("api")]
public class ApiController : Controller
{
    [HttpGet("ping")]
    public StatusCodeResult Ping()
    {
        return StatusCode(StatusCodes.Status200OK);
    }

    [HttpGet("update_required")]
    public ObjectResult UpdateRequired()
    {
        return StatusCode(StatusCodes.Status200OK, !Request.IsOnLatestUpdate());
    }

    [HttpGet]
    public void RedirectToDocs()
    {
        Response.Redirect("https://github.com/wafflethings/EndlessDelivery/wiki");
    }
}
