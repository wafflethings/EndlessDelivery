using System.Threading.RateLimiting;
using EndlessDelivery.Server.Api.ContentFile;
using EndlessDelivery.Server.Api.Steam;
using EndlessDelivery.Server.Config;
using EndlessDelivery.Server.Database;
using EndlessDelivery.Server.Resources;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace EndlessDelivery.Server;

public class Program
{
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
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });

        WebApplication app = builder.Build();

        app.Use((context, next) => // body stream is an empty string unless this middleware is used, https://stackoverflow.com/questions/40494913/how-to-read-request-body-in-an-asp-net-core-webapi-controller
        {
            context.Request.EnableBuffering();
            return next();
        });

        app.UseForwardedHeaders();
        app.UseHttpsRedirection();

        app.MapControllers();
        app.MapResources();

        using (DeliveryDbContext dbContext = new())
        {
            dbContext.Database.EnsureCreated();
        }

        Keys.Load();

        ContentController.LoadCms();
        SteamLoginController.LoadTokens();
        SteamUser.LoadPlayerCache();
        StartThreads();
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

        Thread steamTokenThread = new(SteamLoginController.RemoveExpiredTokensThread);
        steamTokenThread.Start();
        s_threads.Add(steamTokenThread);
    }
}
