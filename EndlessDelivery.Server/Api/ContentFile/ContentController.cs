using EndlessDelivery.Common.ContentFile;
using EndlessDelivery.Server.Api.Steam;
using EndlessDelivery.Server.Website;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace EndlessDelivery.Server.Api.ContentFile;

[Route("api/cms")]
public class ContentController : Controller
{
    public static Cms CurrentContent;
    private static readonly string s_cmsPath = Path.Combine("Assets", "Config", "cms.json");

    public static void LoadCms()
    {
        if (!Path.Exists(s_cmsPath))
        {
            return;
        }

        CurrentContent = JsonConvert.DeserializeObject<Cms>(System.IO.File.ReadAllText(s_cmsPath));
        CurrentContent.SetValues();
    }

    public static void SaveCms()
    {
        System.IO.File.WriteAllText(s_cmsPath, JsonConvert.SerializeObject(CurrentContent, Formatting.Indented));
    }

    [HttpGet("update_required")]
    public StatusCodeResult ShouldUpdateContent(DateTime lastUpdate)
    {
        if (lastUpdate + TimeSpan.FromSeconds(1) < CurrentContent.LastUpdate) // this SUCKS but i think comparison is inaccurate
        {
            return StatusCode(StatusCodes.Status426UpgradeRequired);
        }

        return StatusCode(StatusCodes.Status200OK);
    }

    [HttpGet("content")]
    public async Task<ObjectResult> GetContent() => StatusCode(StatusCodes.Status200OK, await Task.Run(() => JsonConvert.SerializeObject(CurrentContent)));

    [HttpPost("update")]
    public async Task<object> UpdateContent()
    {
        if (!Request.HttpContext.TryGetLoggedInPlayer(out SteamUser user) || !(await user.GetUserModel()).Admin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Go away!!!");
        }

        IFormCollection formParams = await Request.ReadFormAsync();
        if (!formParams.TryGetValue("content", out StringValues contentString))
        {
            return StatusCode(StatusCodes.Status400BadRequest, "Missing content header.");
        }

        Cms deserialized = JsonConvert.DeserializeObject<Cms>(contentString);
        if (deserialized == null)
        {
            return StatusCode(StatusCodes.Status400BadRequest, "CMS is null on deserialization.");
        }

        deserialized.LastUpdate = DateTime.UtcNow;

        CurrentContent = deserialized;
        CurrentContent.SetValues();
        return StatusCode(StatusCodes.Status204NoContent);
    }
}
