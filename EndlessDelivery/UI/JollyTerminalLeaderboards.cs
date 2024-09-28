using System.Threading.Tasks;
using EndlessDelivery.Api.Requests;
using EndlessDelivery.Common.Communication.Scores;
using EndlessDelivery.Online;
using EndlessDelivery.ScoreManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessDelivery.UI;

public class JollyTerminalLeaderboards : MonoBehaviour
{
    public Button[] PageButtons;
    public LeaderboardEntry[] Entries;
    public TMP_Text PageText;
    private OnlineScore[] _pageScores;
    private bool _hasLoadedScores;
    private int _page = 0;
    private int _pageAmount;

    public void Start()
    {
        SetStuff();
    }

    private async void SetStuff()
    {
        _pageAmount = Mathf.CeilToInt(await OnlineFunctionality.Context.GetLeaderboardLength() / 5f);
    }

    public void OnEnable()
    {
        if (!_hasLoadedScores)
        {
            foreach (LeaderboardEntry entry in Entries)
            {
                entry.gameObject.SetActive(false);
            }

            Task.Run(RefreshPage);
        }
    }

    public void ScrollPage(int amount)
    {
        if ((_page + amount) >= 0 && (_page + amount) <= _pageAmount - 1)
        {
            _page += amount;
        }

        PageText.text = (_page + 1).ToString();
        Task.Run(RefreshPage);
    }

    public async Task RefreshPage()
    {
        Debug.LogWarning(OnlineFunctionality.Context.BaseUri.ToString());
        if (!await OnlineFunctionality.Context.ServerOnline())
        {
            HudMessageReceiver.Instance.SendHudMessage("Server offline!");
            gameObject.SetActive(false);
            return;
        }
        Debug.LogWarning("Done");

        foreach (Button button in PageButtons)
        {
            button.interactable = false;
        }

        _pageScores = await ScoreManager.GetPage(_page);

        for (int i = 0; i < 5; i++)
        {
            if (i < _pageScores.Length)
            {
                Entries[i].SetValuesAndEnable(_pageScores[i]);
            }
            else
            {
                Entries[i].gameObject.SetActive(false);
            }
        }

        foreach (Button button in PageButtons)
        {
            button.interactable = true;
        }
    }
}
