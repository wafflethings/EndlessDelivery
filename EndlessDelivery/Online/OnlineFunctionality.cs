using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EndlessDelivery.Common.Communication;
using EndlessDelivery.Online.Requests;

namespace EndlessDelivery.Online;

public static class OnlineFunctionality
{
    public static readonly HttpClient Client = new();
    public static bool UseLocalUrl = false;
    private static string? s_currentToken;

    private const string ProdRootUrl = "https://delivery.wafflethings.dev/api/";
    private const string LocalRootUrl = "http://localhost:7048/api/";

    public static string RootUrl => UseLocalUrl ? LocalRootUrl : ProdRootUrl;

    public static async Task<bool> AddAuth(this HttpRequestMessage message)
    {
        s_currentToken ??= await Client.Login();

        if (s_currentToken is "" or null)
        {
            return false;
        }

        message.Headers.Add("DeliveryToken", s_currentToken);
        return true;
    }

    public static async Task<bool> ServerOnline() => await Client.Ping() == HttpStatusCode.OK;
}
