using System.Collections;
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
    private int _page = 0;
    private int _pageAmount;
    private Coroutine? _lastRefresh;

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
        foreach (LeaderboardEntry entry in Entries)
        {
            entry.gameObject.SetActive(false);
        }

        _lastRefresh = StartCoroutine(RefreshPage());
    }

    public void ScrollPage(int amount)
    {
        if ((_page + amount) >= 0 && (_page + amount) <= _pageAmount - 1)
        {
            _page += amount;
        }

        PageText.text = (_page + 1).ToString();
        _lastRefresh = StartCoroutine(RefreshPage());
    }

    private IEnumerator RefreshPage()
    {
        if (_lastRefresh != null)
        {
            StopCoroutine(_lastRefresh);
        }

        Task<bool> onlineTask = OnlineFunctionality.Context.ServerOnline();
        yield return new WaitUntil(() => onlineTask.IsCompleted);

        if (!onlineTask.Result)
        {
            HudMessageReceiver.Instance.SendHudMessage("Server offline!");
            gameObject.SetActive(false);
            yield break;
        }

        Plugin.Log.LogWarning("Done");

        foreach (Button button in PageButtons)
        {
            button.interactable = false;
        }

        Task<OnlineScore[]> scoreTask = ScoreManager.GetPage(_page);
        yield return new WaitUntil(() => scoreTask.IsCompleted);
        _pageScores = scoreTask.Result;

        for (int i = 0; i < 5; i++)
        {
            if (i < _pageScores.Length)
            {
                Entries[i].SetValuesAndEnable(this, _pageScores[i]);
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
