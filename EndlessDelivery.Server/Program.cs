using System.Threading.RateLimiting;
using EndlessDelivery.Common.Inventory.Items;
using EndlessDelivery.Server.Api.ContentFile;
using EndlessDelivery.Server.Api.Steam;
using EndlessDelivery.Server.Config;
using EndlessDelivery.Server.Resources;
using Supabase;
using Microsoft.AspNetCore.RateLimiting;
using Newtonsoft.Json;

namespace EndlessDelivery.Server;

public class Program
{
    public static Client Supabase;

    public static HttpClient Client
    {
        get
        {
            if (s_client == null)
            {
                s_client = new HttpClient();
                s_client.DefaultRequestHeaders.Add("Accept", "application/json");
            }

            return s_client;
        }
    }

    private static HttpClient s_client;
    private static List<Thread> s_threads = new();
    public static bool Running { get; private set; } = true;

    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRateLimiter(_ => _.AddFixedWindowLimiter(policyName: "fixed",
            options =>
            {
                options.PermitLimit = 10;
                options.Window = TimeSpan.FromSeconds(20);
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                options.QueueLimit = 5;
            }));

        builder.Services.AddControllers();
        WebApplication app = builder.Build();

        // app.UseHttpsRedirection();
        app.MapControllers();
        app.MapResources();

        Keys.Load();
        InitSupabase();
        StartThreads();

        ContentController.LoadCms();
        SteamLoginController.LoadTokens();
        SteamUser.LoadPlayerCache();
        Console.WriteLine(JsonConvert.SerializeObject(ContentController.CurrentContent));
        app.Run();
        ContentController.SaveCms();
        SteamLoginController.SaveTokens();
        SteamUser.SavePlayerCache();
        StopThreads();
    }

    private static void StopThreads()
    {
        Running = false;
        foreach (Thread thread in s_threads)
        {
            thread.Interrupt();
        }
    }

    private static void StartThreads()
    {
        Thread userCacheThread = new(SteamUser.CacheUpdateThread);
        userCacheThread.Start();
        s_threads.Add(userCacheThread);
    }

    private static async void InitSupabase()
    {
        string url = "https://acmzlmynozfuwuposgqm.supabase.co";
        SupabaseOptions options = new SupabaseOptions
        {
            AutoConnectRealtime = true
        };

        Supabase = new Client(url, Keys.Instance.SupabaseKey, options);
        await Supabase.InitializeAsync();
    }
}
