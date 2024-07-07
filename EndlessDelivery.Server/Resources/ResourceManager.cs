using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace EndlessDelivery.Server.Resources;

public static class ResourceManager
{
    public static readonly List<Resource> Resources = new();

    private static Dictionary<string, string> s_extToType = new()
    {
        { "png", "image/png" },
        { "js", "text/javascript" },
        { "ico", "image/x-icon" }
    };

    private static string[] s_ignore = ["Resources/Scripts/src", ".gitignore", "compile.bat", "compile.sh", "tsconfig.json", "favicon.ico"];

    public static void MapResources(this WebApplication app)
    {
        foreach (string file in Directory.GetFiles(Path.Combine("Assets", "Resources"), "*.*", SearchOption.AllDirectories))
        {
            if (s_ignore.Any(toIgnore => file.Contains(toIgnore)))
            {
                Console.WriteLine($"Skipped {file}!");
                continue;
            }

            string extension = file.Contains('.') ? file.Split('.')[^1] : string.Empty;
            Resources.Add(new Resource(file, s_extToType.GetValueOrDefault(extension, "application/octet-stream")));
        }

        Resources.Add(new Resource(Path.Combine("Assets", "Resources", "Icons", "favicon.ico"), "favicon.ico", "application/octet-stream"));

        foreach (Resource resource in Resources)
        {
            Console.WriteLine($"File at {resource.Location} of type {resource.MimeType} hosted at {resource.UrlLocation}!");
            app.MapGet(resource.UrlLocation, async () => Results.File(await resource.GetData(), resource.MimeType));
        }
    }
}
