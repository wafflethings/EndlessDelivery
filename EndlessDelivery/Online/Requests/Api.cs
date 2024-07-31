using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EndlessDelivery.Common.Communication;
using Newtonsoft.Json;

namespace EndlessDelivery.Online.Requests;

public static class Api
{
    private const string ApiRoot = "api/";
    private const string PingEndpoint = "ping";

    public static async Task<HttpStatusCode> Ping(this HttpClient client)
    {
        HttpResponseMessage response = await client.GetAsync(OnlineFunctionality.RootUrl + ApiRoot + PingEndpoint);
        return response.StatusCode;
    }
}
