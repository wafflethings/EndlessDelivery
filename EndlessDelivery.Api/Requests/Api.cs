namespace EndlessDelivery.Api.Requests;

public static class Api
{
    private const string PingEndpoint = "ping";

    public static async Task<bool> ServerOnline(this ApiContext context)
    {
        HttpResponseMessage response = await context.Client.GetAsync(context.BaseUri + PingEndpoint);
        return response.IsSuccessStatusCode;
    }
}
