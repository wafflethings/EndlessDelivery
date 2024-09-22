using System.Threading.Tasks;
using EndlessDelivery.Api.Requests;
using EndlessDelivery.Common.Communication.Scores;
using EndlessDelivery.Gameplay;
using EndlessDelivery.Online;
using EndlessDelivery.ScoreManagement;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessDelivery.UI;

public class EndScreenLeaderboards : MonoBehaviour
{
    public Text[] ConnectingToServerText;
    public Transform TopLeaderboardContainer;
    public Transform NearbyLeaderboardContainer;
    public GameObject EntryTemplate;

    public void OnEnable()
    {
        DoStuff();
    }

    private async Task DoStuff()
    {
        if (PrefsManager.Instance.GetInt("difficulty") > 3)
        {
            foreach (Text text in ConnectingToServerText)
            {
                text.text = "LEADERBOARDS ARE ONLY ON VIOLENT AND ABOVE";
            }

            return;
        }

        if (!await OnlineFunctionality.Context.ServerOnline())
        {
            foreach (Text text in ConnectingToServerText)
            {
                text.text = "FAILED TO CONNECT :^(";
            }

            return;
        }

        if (EndScreen.Instance.NewBest)
        {
            await ScoreManager.SubmitScore(GameManager.Instance.CurrentScore, (short)PrefsManager.Instance.GetInt("difficulty"));
        }

        OnlineScore[] nearScores = await ScoreManager.GetPage(await OnlineFunctionality.Context.GetLeaderboardPosition(SteamClient.SteamId) / 5);
        OnlineScore[] topScores = await OnlineFunctionality.Context.GetScoreRange(0, 10);

        foreach (OnlineScore scoreResult in nearScores)
        {
            Instantiate(EntryTemplate, NearbyLeaderboardContainer).GetComponent<LeaderboardEntry>().SetValuesAndEnable(scoreResult);
        }

        foreach (OnlineScore scoreResult in topScores)
        {
            Instantiate(EntryTemplate, TopLeaderboardContainer).GetComponent<LeaderboardEntry>().SetValuesAndEnable(scoreResult);
        }

        foreach (Text text in ConnectingToServerText)
        {
            text.gameObject.SetActive(false);
        }
    }
}
