using System;
using System.IO;
using System.Threading.RateLimiting;
using EndlessDeliveryScoreServer.Controllers;
using Microsoft.AspNetCore.Builder;
using Supabase;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace EndlessDeliveryScoreServer;

public class Program
{
    public static Client Supabase;
    private static readonly string ConfigPath = Path.Combine(AppContext.BaseDirectory, "Config", "keys.json");
    
    public static void Main(string[] args)
    {
        Keys.Instance = JsonConvert.DeserializeObject<Keys>(File.ReadAllText(ConfigPath));
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Services.AddRateLimiter(_ => _
            .AddFixedWindowLimiter(policyName: "fixed", options =>
            {
                options.PermitLimit = 5;
                options.Window = TimeSpan.FromSeconds(10);
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                options.QueueLimit = 5;
            }));

        builder.Services.AddControllers();
        var app = builder.Build();

        //app.UseHttpsRedirection();
        app.MapControllers();
        
        InitSupabase();
        app.Run();
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