using System.Net.Http;
using System.Threading.Tasks;
using EndlessDelivery.Api.Exceptions;

namespace EndlessDelivery.Api.Requests;

public static class Authentication
{
    private const string LoginEndpoint = "auth/steam/login";

    public static async Task<string> GetToken(this ApiContext context, string ticket)
    {
        HttpResponseMessage response = await context.Client.PostAsync(context.BaseUri + LoginEndpoint, new StringContent(ticket));
        if (!response.IsSuccessStatusCode)
        {
            throw new InternalServerException();
        }
        return await response.Content.ReadAsStringAsync();
    }
}
