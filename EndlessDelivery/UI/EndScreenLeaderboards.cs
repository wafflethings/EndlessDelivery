using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using EndlessDelivery.Scores;
using EndlessDelivery.Scores.Server;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessDelivery.UI
{
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
            
            if (!await Endpoints.IsServerOnline())
            {
                foreach (Text text in ConnectingToServerText)
                {
                    text.text = "FAILED TO CONNECT :^(";
                }

                return;
            }

            if (EndScreen.Instance.NewBest)
            {
                await Endpoints.SendScoreAndReturnPosition(Score.Highscore, PrefsManager.Instance.GetInt("difficulty"));
            }
            
            List<ScoreResult> nearScores = await Endpoints.GetUserPage(await Endpoints.GetUserPosition(SteamClient.SteamId));
            List<ScoreResult> topScores = await Endpoints.GetScoreRange(0, 10);

            foreach (ScoreResult scoreResult in nearScores)
            {
                Instantiate(EntryTemplate, NearbyLeaderboardContainer).GetComponent<LeaderboardEntry>().SetValuesAndEnable(scoreResult);
            }
            
            foreach (ScoreResult scoreResult in topScores)
            {
                Instantiate(EntryTemplate, TopLeaderboardContainer).GetComponent<LeaderboardEntry>().SetValuesAndEnable(scoreResult);
            }
            
            foreach (Text text in ConnectingToServerText)
            {
                text.gameObject.SetActive(false);
            }
        }
    }
}