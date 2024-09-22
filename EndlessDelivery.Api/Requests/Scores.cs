using System.Net.Http;
using System.Threading.Tasks;
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
        return JsonConvert.DeserializeObject<OnlineScore[]>(await response.Content.ReadAsStringAsync()) ?? throw new BadResponseException();
    }

    public static async Task<int> GetLeaderboardPosition(this ApiContext context, ulong userId)
    {
        await context.EnsureAuth();
        HttpResponseMessage response = await context.Client.GetAsync(string.Format(context.BaseUri + ScoreRoot + GetPositionEndpoint, userId));
        return int.TryParse(await response.Content.ReadAsStringAsync(), out int pos) ? pos : throw new BadResponseException();
    }

    public static async Task<int> GetLeaderboardLength(this ApiContext context)
    {
        HttpResponseMessage response = await context.Client.GetAsync(string.Format(context.BaseUri + ScoreRoot + GetLengthEndpoint));
        return int.TryParse(await response.Content.ReadAsStringAsync(), out int length) ? length : throw new BadResponseException();
    }

    public static async Task<int> SubmitScore(this ApiContext context, SubmitScoreData scoreData)
    {
        await context.EnsureAuth();
        HttpRequestMessage request = new(HttpMethod.Post, context.BaseUri + ScoreRoot + SubmitScoreEndpoint);
        request.Content = new StringContent(JsonConvert.SerializeObject(scoreData));
        HttpResponseMessage response = await context.Client.SendAsync(request);
        return int.TryParse(await response.Content.ReadAsStringAsync(), out int pos) ? pos : throw new BadResponseException();
    }
}
