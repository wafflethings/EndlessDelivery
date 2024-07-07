using System.Net.Http;

namespace EndlessDelivery.Server;

public static class OnlineFunctionality
{
    public static readonly HttpClient Client = new();
    public static bool UseLocalUrl = false;

    private const string ProdRootUrl = "https://delivery.wafflethings.dev/api/";
    private const string LocalRootUrl = "http://localhost:7048/api/";

    public static string RootUrl => UseLocalUrl ? LocalRootUrl : ProdRootUrl;
}
