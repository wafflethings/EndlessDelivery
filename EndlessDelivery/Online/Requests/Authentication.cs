using System;
using System.Net.Http;
using System.Threading.Tasks;
using EndlessDelivery.Common.Communication;
using Newtonsoft.Json;
using Steamworks;
using Steamworks.Data;

namespace EndlessDelivery.Online.Requests;

public static class Authentication
{
    private const string LoginEndpoint = "auth/steam/login";

    //https://stackoverflow.com/questions/46139474/steam-web-api-authenticate-http-request-error
    public static string GetTicket()
    {
        AuthTicket ticket = SteamUser.GetAuthSessionTicket(new NetIdentity());
        return BitConverter.ToString(ticket.Data, 0, ticket.Data.Length).Replace("-", string.Empty);
    }

    public static async Task<string> Login(this HttpClient client)
    {
        HttpResponseMessage response = await client.PostAsync(OnlineFunctionality.RootUrl + LoginEndpoint, new StringContent(JsonConvert.SerializeObject(new Response<string>(GetTicket()))));
        return JsonConvert.DeserializeObject<Response<string>>(await response.Content.ReadAsStringAsync())?.Value ?? string.Empty;
    }
}
