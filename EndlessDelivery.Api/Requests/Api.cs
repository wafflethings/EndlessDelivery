using EndlessDelivery.Api.Exceptions;

namespace EndlessDelivery.Api.Requests;

public static class Api
{
    private const string PingEndpoint = "ping";
    private const string UpdateRequiredEndpoint = "update_required";

    public static async Task<bool> ServerOnline(this ApiContext context)
    {
        HttpResponseMessage response = await context.Client.GetAsync(context.BaseUri + PingEndpoint);
        return response.IsSuccessStatusCode;
    }

    public static async Task<bool> UpdateRequired(this ApiContext context)
    {
        HttpResponseMessage response = await context.Client.GetAsync(context.BaseUri + string.Format(UpdateRequiredEndpoint));
        string content = await response.Content.ReadAsStringAsync();
        return bool.TryParse(content, out bool result) ? result : throw new BadResponseException(content);
    }
}
