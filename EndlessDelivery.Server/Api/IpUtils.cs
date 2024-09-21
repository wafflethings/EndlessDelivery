using System.Net;
using MaxMind.Db;
using Microsoft.Extensions.Primitives;

namespace EndlessDelivery.Server.Api;

public static class IpUtils
{
    private static Reader? s_reader = File.Exists(DbPath) ? new(DbPath) : null; //should never happen but i dont have the DB on my laptop

    private static string DbPath => Path.Combine("Assets", "Config", "GeoLite2-Country.mmdb");

    public static string GetCountry(this HttpContext context) => GetIpCountry(context.GetIp());

    public static string GetIpCountry(IPAddress address)
    {
        #if DEBUG
        if (s_reader == null)
        {
            return string.Empty;
        }

        if (address.ToString() == "::1")
        {
            // this fucking reeks but also only happens in dev! so i dont care :pray:
            HttpResponseMessage response = Program.Client.GetAsync(new Uri("https://api.ipify.org")).Result;
            address = IPAddress.Parse(response.Content.ReadAsStringAsync().Result);
        }
        #endif

        Dictionary<string, object> dict = s_reader.Find<Dictionary<string, object>>(address);

        if (dict == null || !dict.ContainsKey("country"))
        {
            return string.Empty;
        }

        return (dict["country"] as Dictionary<string, object>)["iso_code"].ToString();
    }

    public static IPAddress GetIp(this HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-Real-IP", out StringValues realIp) && IPAddress.TryParse(realIp, out IPAddress realIpAddress))
        {
            return realIpAddress;
        }

        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out StringValues forwardIp) && IPAddress.TryParse(realIp, out IPAddress forwardAddress))
        {
            return forwardAddress;
        }

        return context.Connection.RemoteIpAddress ?? IPAddress.None;
    }
}
