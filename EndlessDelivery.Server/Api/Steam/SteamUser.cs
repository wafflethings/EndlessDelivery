using EndlessDelivery.Server.Api.Users;
using EndlessDelivery.Server.Database;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace EndlessDelivery.Server.Api.Steam;

public class SteamUser
{
    public const int MaximumPlayersFetched = 100;
    public static readonly TimeSpan CacheRenewInterval = TimeSpan.FromMinutes(30);
    public static readonly TimeSpan CacheGroupGetInterval = TimeSpan.FromSeconds(10);
    public static readonly TimeSpan CacheGroupGetAfterFailInterval = TimeSpan.FromSeconds(30);

    private static DateTime s_lastCacheUpdate = DateTime.MinValue;
    private static Dictionary<ulong, SteamUser> s_playerCache = new();

    private static SteamUser s_fallback = new()
    {
        Avatar = "https://avatars.steamstatic.com/fef49e7fa7e1997310d705b2a6158ff8dc1cdfeb.jpg",
        AvatarMedium = "https://avatars.steamstatic.com/fef49e7fa7e1997310d705b2a6158ff8dc1cdfeb.jpg",
        AvatarFull = "https://avatars.steamstatic.com/fef49e7fa7e1997310d705b2a6158ff8dc1cdfeb.jpg",
        PersonaName = "FALLBACK - ERROR",
        ProfileUrl = "https://soggy.cat",
        SteamId = "0"
    };

    public string SteamId { get; set; }
    public string PersonaName { get; set; }
    public string ProfileUrl { get; set; }
    public string Avatar { get; set; }
    public string AvatarMedium { get; set; }
    public string AvatarFull { get; set; }

    private static readonly string PlayerCachePath = Path.Combine("Assets", "Config", "players.json");

    public static void LoadPlayerCache()
    {
        if (!Path.Exists(PlayerCachePath))
        {
            UpdateCache().Wait();
            return;
        }

        s_playerCache = JsonConvert.DeserializeObject<Dictionary<ulong, SteamUser>>(File.ReadAllText(PlayerCachePath));
    }

    public static void SavePlayerCache()
    {
        File.WriteAllText(PlayerCachePath, JsonConvert.SerializeObject(s_playerCache));
    }

    public static void CacheUpdateThread()
    {
        Console.WriteLine("Started cache update thread.");

        while (Program.Running)
        {
            Thread.Sleep(CacheRenewInterval);
            UpdateCache().Wait();
        }
    }

    public static async Task UpdateCache()
    {
        await using DeliveryDbContext dbContext = new();
        List<UserModel> models = await dbContext.Users.ToListAsync();
        int requiredGroups = (int)MathF.Ceiling(models.Count / (float)MaximumPlayersFetched);
        ulong[] allUserIds = models.Select(score => score.SteamId).ToArray();
        ulong[][] idGroups = new ulong[requiredGroups][];
        int usersLeft = allUserIds.Length;

        for (int groupIndex = 0; groupIndex < idGroups.Length; groupIndex++)
        {
            int startPoint = MaximumPlayersFetched * groupIndex;
            int amountOfUsersToGet = (int)MathF.Min(usersLeft, MaximumPlayersFetched);

            idGroups[groupIndex] = allUserIds[startPoint .. (startPoint + amountOfUsersToGet)];
            usersLeft -= amountOfUsersToGet;
        }

        Dictionary<ulong, SteamUser> newCache = new(); //adding to this new one in case someone requests mid clear

        foreach (ulong[] idGroup in idGroups)
        {
            bool thisGroupDone = false;

            while (!thisGroupDone)
            {
                HttpResponseMessage response = await Program.Client.GetAsync(SteamApi.BuildGetUserSummaries(idGroup));
                string stringResponse = FormatResponse(await response.Content.ReadAsStringAsync());

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Player cache update fail! Code {response.StatusCode}");
                    await Task.Delay((int)CacheGroupGetAfterFailInterval.TotalMilliseconds);
                    continue;
                }

                thisGroupDone = true;
                SteamUser[] players = JsonConvert.DeserializeObject<SteamUser[]>(stringResponse);

                foreach (SteamUser player in players)
                {
                    // Console.WriteLine($"Adding {player.SteamId} to cache");
                    newCache.Add(ulong.Parse(player.SteamId), player);
                }

                Console.WriteLine("Did one cache group! Waiting.");
                await Task.Delay((int)CacheGroupGetInterval.TotalMilliseconds);
            }
        }

        s_lastCacheUpdate = DateTime.UtcNow;
        s_playerCache = newCache;
    }

    #warning This is generally not recommended - scores should be submitted and you should wait for the refresh, batching is preferred.
    public static async Task AddPlayerToCache(ulong id)
    {
        HttpResponseMessage response = await Program.Client.GetAsync(SteamApi.BuildGetUserSummaries([id]));
        string stringResponse = FormatResponse(await response.Content.ReadAsStringAsync());
        try
        {
            SteamUser[] players = JsonConvert.DeserializeObject<SteamUser[]>(stringResponse);
            s_playerCache.Add(id, players[0]);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to add to cache! Response was " + stringResponse);
        }
    }

    public static bool TryGetPlayer(ulong id, out SteamUser steamUser)
    {
        if (CacheHasId(id))
        {
            steamUser = GetById(id);
            return true;
        }

        steamUser = null;
        return false;
    }

    public async Task<UserModel?> GetUserModel()
    {
        await using DeliveryDbContext dbContext = new();
        return await dbContext.Users.FindAsync(ulong.Parse(SteamId));
    }

    public static SteamUser GetById(ulong id) => CacheHasId(id) ? s_playerCache[id] : s_fallback;

    public static bool CacheHasId(ulong id) => s_playerCache.ContainsKey(id);

    private static string FormatResponse(string stringResponse) => stringResponse.Substring(23, stringResponse.Length - 25); // trims "{"response":{"players":" and "}}" on the end
}
