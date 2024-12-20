﻿using System.Collections;
using System.Threading.Tasks;
using EndlessDelivery.Api.Exceptions;
using EndlessDelivery.Api.Requests;
using EndlessDelivery.Common.Communication.Scores;
using EndlessDelivery.Online;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessDelivery.UI;

public class JollyTerminalLeaderboards : MonoBehaviour
{
    public Button[] PageButtons;
    public Button JumpToSelfButton;
    public LeaderboardEntry[] Entries;
    public LeaderboardEntry OwnEntry;
    public TMP_Text PageText;
    private OnlineScore[] _pageScores;
    private int _page;
    private int? _pageAmount = null;
    private Coroutine? _lastRefresh;
    private OnlineScore? _ownScore;

    private async Task SetStuff()
    {
        if (!await OnlineFunctionality.Context.ServerOnline())
        {
            return;
        }

        _pageAmount = Mathf.CeilToInt(await OnlineFunctionality.Context.GetLeaderboardLength() / (float)Entries.Length);
        RefreshPageText();

        try
        {
            _ownScore = await OnlineFunctionality.Context.GetLeaderboardScore(SteamClient.SteamId);
        }
        catch (NotFoundException)
        {
            _ownScore = null;
        }

        OwnEntry.SetValuesAndEnable(this, _ownScore);

        if (_ownScore == null)
        {
            JumpToSelfButton.interactable = false;
        }
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
        if ((_page + amount) < 0 || (_page + amount) > _pageAmount - 1)
        {
            return;
        }

        RefreshPageText();
        SetPage(_page + amount);
    }

    public void SetPage(int page)
    {
        _page = page;

        _lastRefresh = StartCoroutine(RefreshPage());
    }

    private void RefreshPageText()
    {
        PageText.text = (_page + 1) + " / " + (_pageAmount == null ? "-" : _pageAmount.ToString());
    }

    public void JumpToSelf()
    {
        if (_ownScore == null)
        {
            Plugin.Log.LogWarning("JumpToSelf with null _ownScore - should be impossible");
            return;
        }

        int pageWithPlayer = Mathf.FloorToInt(_ownScore.Index / (float)Entries.Length);
        SetPage(pageWithPlayer);
    }

    private static async Task<OnlineScore[]> GetPage(int pageIndex, int pageSize)
    {
        int scoreCount = await OnlineFunctionality.Context.GetLeaderboardLength();
        int startIndex = pageIndex * pageSize;
        int amount = Mathf.Min(scoreCount - startIndex, pageSize);
        return await OnlineFunctionality.Context.GetScoreRange(pageIndex * pageSize, amount);
    }

    public void OpenWebsite()
    {
        Application.OpenURL(OnlineFunctionality.LastFetchedContent.GetString("constants.website"));
    }

    public void JoinDiscord()
    {
        Application.OpenURL(OnlineFunctionality.LastFetchedContent.GetString("constants.discord"));
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

        bool jumpToSelfWasEnabled = JumpToSelfButton.interactable;
        foreach (Button button in PageButtons)
        {
            button.interactable = false;
        }

        Task<OnlineScore[]> scoreTask = GetPage(_page, Entries.Length);
        Task setStuff = SetStuff();
        yield return new WaitUntil(() => scoreTask.IsCompleted && setStuff.IsCompleted);
        _pageScores = scoreTask.Result;

        for (int i = 0; i < Entries.Length; i++)
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

        if (!jumpToSelfWasEnabled)
        {
            JumpToSelfButton.interactable = jumpToSelfWasEnabled;
        }
    }
}
