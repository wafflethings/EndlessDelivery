using EndlessDelivery.Server.Api.ContentFile;
using Microsoft.Extensions.Primitives;

namespace EndlessDelivery.Server.Api;

public static class ServerUtils
{
    public static async Task<string> ReadBody(this HttpRequest request)
    {
        request.EnableBuffering();
        request.Body.Seek(0, SeekOrigin.Begin);
        using StreamReader reader = new(request.Body);
        return await reader.ReadToEndAsync();
    }

    public static bool IsOnLatestUpdate(this HttpRequest request) => request.Headers.TryGetValue("DdVersion", out StringValues value) && value.ToString() == Plugin.Version;

    public static bool UserModded(this HttpRequest request)
    {
        if (!request.Headers.TryGetValue("DdMods", out StringValues value))
        {
            Console.WriteLine("no ddmods");
            return true;
        }

        foreach (string mod in value.ToString().Split(","))
        {
            Console.WriteLine($"Mod {mod}");
            if (ContentController.CurrentContent.BannedMods.ContainsKey(mod))
            {
                return true;
            }
        }

        return false;
    }
}
