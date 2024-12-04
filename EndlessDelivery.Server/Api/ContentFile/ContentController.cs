using System.Security.Cryptography;
using System.Text;
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

        CurrentContent = JsonConvert.DeserializeObject<Cms>(System.IO.File.ReadAllText(s_cmsPath)) ?? throw new Exception("Failed to deserialize content!");;
    }

    public static void SaveCms()
    {
        System.IO.File.WriteAllText(s_cmsPath, JsonConvert.SerializeObject(CurrentContent, Formatting.Indented));
    }

    [HttpGet("update_required")]
    public StatusCodeResult ShouldUpdateContent(string hash)
    {
        if (hash != CurrentContent.Hash)
        {
            Console.WriteLine("Update");
            return StatusCode(StatusCodes.Status426UpgradeRequired);
        }

        Console.WriteLine("OK");
        return StatusCode(StatusCodes.Status200OK);
    }

    [HttpGet("content")]
    public async Task<ObjectResult> GetContent() => StatusCode(StatusCodes.Status200OK, await Task.Run(() => JsonConvert.SerializeObject(CurrentContent, Formatting.None)));

    [HttpPost("update")]
    public async Task<ObjectResult> UpdateContent()
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

        CurrentContent = deserialized;
        return StatusCode(StatusCodes.Status204NoContent, null);
    }
}
