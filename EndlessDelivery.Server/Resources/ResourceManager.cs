using Microsoft.AspNetCore.Mvc;

namespace EndlessDelivery.Server.Resources;

public static class ResourceManager
{
    public const string ResourcePath = "resources/";
    public static readonly ResourcePair[] Resources =
    {
        new(ResourcePath + "logo-transparent.png", Path.Combine("Assets", "Resources", "UI", "logo-transparent.png"), "image/png"),
        new(ResourcePath + "dropdown.png", Path.Combine("Assets", "Resources", "UI", "dropdown.png"), "image/png"),
        new(ResourcePath + "global.png", Path.Combine("Assets", "Resources", "UI", "global.png"), "image/png"),
        new(ResourcePath + "fallback-pfp.png", Path.Combine("Assets", "Resources", "UI", "fallback-pfp.png"), "image/png"),
        new(ResourcePath + "prem-currency.png", Path.Combine("Assets", "Resources", "UI", "prem-currency.png"), "image/png"),
        new(ResourcePath + "vcr_osd_mono.woff2", Path.Combine("Assets", "Resources", "UI", "vcr_osd_mono.woff2")),

        new(ResourcePath + "banners/average-british-neighbourhood.png", Path.Combine("Assets", "Resources", "UI", "Banners", "average-british-neighbourhood.png"), "image/png"),
        new(ResourcePath + "banners/gniesbert.png", Path.Combine("Assets", "Resources", "UI", "Banners", "gniesbert.png"), "image/png"),

        new(ResourcePath + "banners/abn-preview.png", Path.Combine("Assets", "Resources", "UI", "Banners", "abn-preview.png"), "image/png"),
        new(ResourcePath + "banners/gnies-preview.png", Path.Combine("Assets", "Resources", "UI", "Banners", "gnies-preview.png"), "image/png"),

        new(ResourcePath + "colours.css", Path.Combine("Assets", "Resources", "Stylesheets", "colours.css")),
        new(ResourcePath + "error_page.css", Path.Combine("Assets", "Resources", "Stylesheets", "error_page.css")),
        new(ResourcePath + "leaderboard.css", Path.Combine("Assets", "Resources", "Stylesheets", "leaderboard.css")),
        new(ResourcePath + "misc.css", Path.Combine("Assets", "Resources", "Stylesheets", "misc.css")),
        new(ResourcePath + "settings.css", Path.Combine("Assets", "Resources", "Stylesheets", "settings.css")),
        new(ResourcePath + "shop.css", Path.Combine("Assets", "Resources", "Stylesheets", "shop.css")),
        new(ResourcePath + "user.css", Path.Combine("Assets", "Resources", "Stylesheets", "user.css")),

        new(ResourcePath + "scripts/index.js", Path.Combine("Assets", "Resources", "Scripts", "build", "index.js"), "text/javascript"),
        new(ResourcePath + "scripts/misc.js", Path.Combine("Assets", "Resources", "Scripts", "build", "misc.js"), "text/javascript"),
        new(ResourcePath + "scripts/settings.js", Path.Combine("Assets", "Resources", "Scripts", "build", "settings.js"), "text/javascript"),
        new(ResourcePath + "scripts/shop.js", Path.Combine("Assets", "Resources", "Scripts", "build", "shop.js"), "text/javascript"),

        new([ResourcePath + "icons/favicon.ico", "favicon.ico"], Path.Combine("Assets", "Resources", "Icons", "favicon.ico"), "image/x-icon"),
        new(ResourcePath + "icons/favicon-16x16.png", Path.Combine("Assets", "Resources", "Icons", "favicon-16x16.png"), "image/png"),
        new(ResourcePath + "icons/favicon-32x32.png", Path.Combine("Assets", "Resources", "Icons", "favicon-32x32.png"), "image/png"),
        new(ResourcePath + "icons/favicon-192x192.png", Path.Combine("Assets", "Resources", "Icons", "favicon-192x192.png"), "image/png"),
        new(ResourcePath + "icons/favicon-256x256.png", Path.Combine("Assets", "Resources", "Icons", "favicon-256x256.png"), "image/png"),

        new(ResourcePath + "icons/discord-transparent.png", Path.Combine("Assets", "Resources", "UI", "Socials", "discord-transparent.png"), "image/png"),
        new(ResourcePath + "icons/twitter-transparent.png", Path.Combine("Assets", "Resources", "UI", "Socials", "twitter-transparent.png"), "image/png"),
        new(ResourcePath + "icons/youtube-transparent.png", Path.Combine("Assets", "Resources", "UI", "Socials", "youtube-transparent.png"), "image/png")
    };

    public static void MapResources(this WebApplication app)
    {
        foreach (ResourcePair resource in Resources)
        {
            foreach (string url in resource.Urls)
            {
                Console.WriteLine($"File at {resource.Location} of type {resource.MimeType} hosted at {url}!");
                app.MapGet(url, async () => Results.File(await resource.GetData(), resource.MimeType));
            }
        }
    }
}
