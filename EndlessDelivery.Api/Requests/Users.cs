using System.Net;
using EndlessDelivery.Api.Exceptions;
using EndlessDelivery.Common;
using Newtonsoft.Json;

namespace EndlessDelivery.Api.Requests;

public static class Users
{
    private const string UsersRoot = "users/";
    private const string GetCurrencyAmountEndpoint = "get_currency_amount";
    private const string GetAchievementsEndpoint = "get_achievements?steamId={0}";
    private const string GrantAchievementEndpoint = "grant_achievement";

    public static async Task<int> GetCurrencyAmount(this ApiContext context)
    {
        HttpRequestMessage request = new(HttpMethod.Get, context.BaseUri + UsersRoot + GetCurrencyAmountEndpoint);
        await context.EnsureAuth(request);
        HttpResponseMessage response = await context.Client.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.InternalServerError)
        {
            throw new InternalServerException();
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new BadRequestException(response.ReasonPhrase);
        }

        string content = await response.Content.ReadAsStringAsync();
        return int.TryParse(content, out int amount) ? amount : throw new BadResponseException(content);
    }

    public static async Task<List<OwnedAchievement>> GetAchievements(this ApiContext context, ulong userId)
    {
        HttpResponseMessage response = await context.Client.GetAsync(context.BaseUri + UsersRoot + string.Format(GetAchievementsEndpoint, userId));

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new BadRequestException($"the user with ID {userId} was not found");
        }

        if (response.StatusCode == HttpStatusCode.InternalServerError)
        {
            throw new InternalServerException();
        }

        string content = await response.Content.ReadAsStringAsync();
        List<OwnedAchievement>? list = JsonConvert.DeserializeObject<List<OwnedAchievement>>(content);
        return list ?? throw new BadResponseException(content);
    }

    public static async Task GrantAchievement(this ApiContext context, string achievementId)
    {
        HttpRequestMessage request = new(HttpMethod.Post, context.BaseUri + UsersRoot + GrantAchievementEndpoint);
        await context.EnsureAuth(request);
        request.Content = new StringContent(achievementId);
        HttpResponseMessage response = await context.Client.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.InternalServerError)
        {
            throw new InternalServerException();
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new BadRequestException(response.ReasonPhrase);
        }
    }
}
