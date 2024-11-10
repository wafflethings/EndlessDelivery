using System.Net;
using EndlessDelivery.Api.Exceptions;
using EndlessDelivery.Common.Communication.Scores;
using Newtonsoft.Json;

namespace EndlessDelivery.Api.Requests;

public static class Scores
{
    private const string ScoreRoot = "scores/";
    private const string GetRangeEndpoint = "get_range?start={0}&count={1}";
    private const string GetPositionEndpoint = "get_position?steamId={0}";
    private const string GetScoreEndpoint = "get_score?steamId={0}";
    private const string GetLengthEndpoint = "get_length";
    private const string SubmitScoreEndpoint = "submit_score";

    public static async Task<OnlineScore[]> GetScoreRange(this ApiContext context, int startIndex, int amount)
    {
        HttpResponseMessage response = await context.Client.GetAsync(context.BaseUri + ScoreRoot + string.Format(GetRangeEndpoint, startIndex, amount));
        string content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<OnlineScore[]>(content) ?? throw new BadResponseException(content);
    }

    public static async Task<int> GetLeaderboardPosition(this ApiContext context, ulong userId)
    {
        HttpResponseMessage response = await context.Client.GetAsync(context.BaseUri + ScoreRoot + string.Format(GetPositionEndpoint, userId));
        string content = await response.Content.ReadAsStringAsync();
        return int.TryParse(content, out int pos) ? pos : throw new BadResponseException(content);
    }

    public static async Task<OnlineScore> GetLeaderboardScore(this ApiContext context, ulong userId)
    {
        HttpResponseMessage response = await context.Client.GetAsync(context.BaseUri + ScoreRoot + string.Format(GetScoreEndpoint, userId));

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new NotFoundException(response.ReasonPhrase);
        }

        string content = await response.Content.ReadAsStringAsync();
        OnlineScore? deserialized = JsonConvert.DeserializeObject<OnlineScore>(content);
        return deserialized ?? throw new BadResponseException(content);
    }

    public static async Task<int> GetLeaderboardLength(this ApiContext context)
    {
        HttpResponseMessage response = await context.Client.GetAsync(context.BaseUri + ScoreRoot + GetLengthEndpoint);
        string content = await response.Content.ReadAsStringAsync();
        return int.TryParse(content, out int length) ? length : throw new BadResponseException(content);
    }

    public static async Task<OnlineScore> SubmitScore(this ApiContext context, SubmitScoreData scoreData)
    {
        HttpRequestMessage request = new(HttpMethod.Post, context.BaseUri + ScoreRoot + SubmitScoreEndpoint);
        await context.EnsureAuth(request);
        request.Content = new StringContent(JsonConvert.SerializeObject(scoreData));
        HttpResponseMessage response = await context.Client.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();
        OnlineScore? score = JsonConvert.DeserializeObject<OnlineScore>(content);
        return score ?? throw new BadResponseException(content);
    }
}
