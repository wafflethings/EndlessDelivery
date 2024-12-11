using System.Net;
using EndlessDelivery.Api.Exceptions;
using EndlessDelivery.Common;
using EndlessDelivery.Common.Communication.Scores;
using Newtonsoft.Json;

namespace EndlessDelivery.Api.Requests;

public static class Users
{
    private const string UsersRoot = "users/";
    private const string GetCurrencyAmountEndpoint = "get_currency_amount";
    private const string GetAchievementsEndpoint = "get_achievements?steamId={0}";
    private const string GrantAchievementEndpoint = "grant_achievement";
    private const string GetUsernameEndpoint = "get_username?steamId={0}";
    private const string LifetimeStatsEndpoint = "lifetime_stats?steamId={0}";
    private const string GetBestScoreEndpoint = "get_best_score?steamId={0}";

    public static async Task<int> GetCurrencyAmount(this ApiContext context)
    {
        HttpRequestMessage request = new(HttpMethod.Get, context.BaseUri + UsersRoot + GetCurrencyAmountEndpoint);
        await context.AddAuth(request);
        HttpResponseMessage response = await context.Client.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.InternalServerError)
        {
            throw new InternalServerException();
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new BadRequestException(await response.Content.ReadAsStringAsync());
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

    public static async Task<string> GetUsername(this ApiContext context, ulong userId)
    {
        HttpResponseMessage response = await context.Client.GetAsync(context.BaseUri + UsersRoot + string.Format(GetUsernameEndpoint, userId));

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new BadRequestException($"the user with ID {userId} was not found");
        }

        return await response.Content.ReadAsStringAsync();
    }

    public static async Task GrantAchievement(this ApiContext context, string achievementId)
    {
        HttpRequestMessage request = new(HttpMethod.Post, context.BaseUri + UsersRoot + GrantAchievementEndpoint);
        await context.AddAuth(request);
        request.Content = new StringContent(achievementId);
        HttpResponseMessage response = await context.Client.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.InternalServerError)
        {
            throw new InternalServerException();
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new BadRequestException(await response.Content.ReadAsStringAsync());
        }
    }

    public static async Task<Score> GetLifetimeStats(this ApiContext context, ulong userId)
    {
        HttpResponseMessage response = await context.Client.GetAsync(context.BaseUri + UsersRoot + string.Format(LifetimeStatsEndpoint, userId));

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new BadRequestException($"the user with ID {userId} was not found");
        }

        if (response.StatusCode == HttpStatusCode.InternalServerError)
        {
            throw new InternalServerException();
        }

        string content = await response.Content.ReadAsStringAsync();
        Score? deserialized = JsonConvert.DeserializeObject<Score>(content);
        return deserialized ?? throw new BadResponseException(content);
    }

    public static async Task<OnlineScore> GetBestScore(this ApiContext context, ulong userId)
    {
        HttpResponseMessage response = await context.Client.GetAsync(context.BaseUri + UsersRoot + string.Format(GetBestScoreEndpoint, userId));

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new BadRequestException($"the user with ID {userId} was not found");
        }

        if (response.StatusCode == HttpStatusCode.InternalServerError)
        {
            throw new InternalServerException();
        }

        string content = await response.Content.ReadAsStringAsync();
        OnlineScore? deserialized = JsonConvert.DeserializeObject<OnlineScore>(content);
        return deserialized ?? throw new BadResponseException(content);
    }
}
