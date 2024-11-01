using EndlessDelivery.Api.Exceptions;
using EndlessDelivery.Common.Communication.Scores;
using Newtonsoft.Json;

namespace EndlessDelivery.Api.Requests;

public static class Scores
{
    private const string ScoreRoot = "scores/";
    private const string GetRangeEndpoint = "get_range?start={0}&count={1}";
    private const string GetPositionEndpoint = "get_position?steamId={0}";
    private const string GetLengthEndpoint = "get_length";
    private const string SubmitScoreEndpoint = "submit_score";

    public static async Task<OnlineScore[]> GetScoreRange(this ApiContext context, int startIndex, int amount)
    {
        HttpResponseMessage response = await context.Client.GetAsync(string.Format(context.BaseUri + ScoreRoot + GetRangeEndpoint, startIndex, amount));
        string content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<OnlineScore[]>(content) ?? throw new BadResponseException(content);
    }

    public static async Task<int> GetLeaderboardPosition(this ApiContext context, ulong userId)
    {
        HttpRequestMessage request = new(HttpMethod.Get, string.Format(context.BaseUri + ScoreRoot + GetPositionEndpoint, userId));
        await context.EnsureAuth(request);
        HttpResponseMessage response = await context.Client.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();
        return int.TryParse(content, out int pos) ? pos : throw new BadResponseException(content);
    }

    public static async Task<int> GetLeaderboardLength(this ApiContext context)
    {
        HttpResponseMessage response = await context.Client.GetAsync(string.Format(context.BaseUri + ScoreRoot + GetLengthEndpoint));
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
