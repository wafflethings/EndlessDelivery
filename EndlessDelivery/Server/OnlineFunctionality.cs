using System.Net.Http;

namespace EndlessDelivery.Server;

public static class OnlineFunctionality
{
    public static readonly HttpClient Client = new();
    public const string RootUrl = "https://delivery.wafflethings.dev/api/";
}
