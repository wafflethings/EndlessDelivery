using System.Net.Http;
using System.Threading.Tasks;
using EndlessDelivery.Common.Communication;
using EndlessDelivery.Common.Communication.Scores;
using Newtonsoft.Json;
using UnityEngine;

namespace EndlessDelivery.Online.Requests;

public class Scores
{
    private const string ScoreRoot = "scores/";
    private const string GetRangeEndpoint = "get_range?start={0}&count={1}";
    private const string GetPositionEndpoint = "get_position?steamId={0}";
    private const string GetLengthEndpoint = "get_length";
    private const string SubmitScoreEndpoint = "submit_score";

    public static async Task<OnlineScore[]> GetRange(int startIndex, int amount)
    {
        HttpResponseMessage response = await OnlineFunctionality.Client.GetAsync(string.Format(OnlineFunctionality.RootUrl + ScoreRoot + GetRangeEndpoint, startIndex, amount));
        return JsonConvert.DeserializeObject<Response<OnlineScore[]>>(await response.Content.ReadAsStringAsync())?.Value ?? [];
    }

    public static async Task<int> GetPosition(ulong userId)
    {
        HttpResponseMessage response = await OnlineFunctionality.Client.GetAsync(string.Format(OnlineFunctionality.RootUrl + ScoreRoot + GetPositionEndpoint, userId));
        return JsonConvert.DeserializeObject<Response<int>>(await response.Content.ReadAsStringAsync())?.Value ?? -1;
    }

    public static async Task<int> GetLength()
    {
        HttpResponseMessage response = await OnlineFunctionality.Client.GetAsync(OnlineFunctionality.RootUrl + ScoreRoot + GetLengthEndpoint);
        return JsonConvert.DeserializeObject<Response<int>>(await response.Content.ReadAsStringAsync())?.Value ?? 0;
    }

    public static async Task<int> SubmitScore(SubmitScoreData scoreData)
    {
        HttpRequestMessage request = new(HttpMethod.Post, OnlineFunctionality.RootUrl + ScoreRoot + SubmitScoreEndpoint);
        if (!await request.AddAuth())
        {
            Debug.LogError("Failed to submit score - authentication failed.");
            return -1;
        }

        request.Content = new StringContent(JsonConvert.SerializeObject(scoreData));
        HttpResponseMessage response = await OnlineFunctionality.Client.SendAsync(request);
        return JsonConvert.DeserializeObject<Response<int>>(await response.Content.ReadAsStringAsync())?.Value ?? -1;
    }
}
