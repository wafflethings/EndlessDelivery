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

    private const string ProdRootUrl = "https://delivery.wafflethings.dev/api/";
    private const string LocalRootUrl = "http://localhost:7048/api/";

    public static string RootUrl => UseLocalUrl ? LocalRootUrl : ProdRootUrl;

    public static async Task<bool> AddAuth(this HttpRequestMessage message)
    {
        string loginToken = await Client.Login();

        if (loginToken == string.Empty)
        {
            return false;
        }

        message.Headers.Add("DeliveryToken", loginToken);
        return true;
    }

    public static async Task<bool> ServerOnline() => await Client.Ping() == HttpStatusCode.OK;
}
