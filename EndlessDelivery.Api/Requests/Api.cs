using System.Net.Http;
using System.Threading.Tasks;

namespace EndlessDelivery.Api.Requests;

public static class Api
{
    private const string ApiRoot = "api/";
    private const string PingEndpoint = "ping";

    public static async Task<bool> ServerOnline(this ApiContext context)
    {
        HttpResponseMessage response = await context.Client.GetAsync(context.BaseUri + ApiRoot + PingEndpoint);
        return response.IsSuccessStatusCode;
    }
}
