using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using EndlessDelivery.Api.Requests;
using EndlessDelivery.Common;
using EndlessDelivery.Common.ContentFile;
using EndlessDelivery.Cosmetics;
using EndlessDelivery.Online;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace EndlessDelivery.UI;

public class AdventCalendar : MonoBehaviour
{
    private static readonly string[] s_buttonOrder = ["day15_2024", "day16_2024", "day4_2024", "day25_2024", "day8_2024", "day22_2024", "day24_2024", "day18_2024", "day10_2024",
        "day20_2024", "day6_2024", "day13_2024", "day23_2024", "day3_2024", "day17_2024", "day7_2024", "day19_2024", "day14_2024", "day12_2024", "day1_2024", "day5_2024", "day9_2024", "day21_2024", "day2_2024", "day11_2024"];

    [SerializeField] private GameObject _templateButton;
    [SerializeField] private Transform _buttonHolder;
    [SerializeField] private GameObject _claimPage;
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _nameTitle;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private TMP_Text _subtitleText;
    [SerializeField] private Button _claimButton;
    [SerializeField] private TMP_Text _claimButtonText;
    [SerializeField] private TMP_Text _timeRemainingText;
    private List<string> _ownedDays = new();
    private List<GameObject> _buttons = new();
    private string _timeRemainingString = "{0}";
    private DateTime _endTime;

    public void Claim()
    {
        _ownedDays.Add(OnlineFunctionality.LastFetchedContent.CurrentCalendarReward.Id);
        _claimButtonText.text = OnlineFunctionality.LastFetchedContent.GetString("game_ui.calendar_claimed");
        _claimButton.interactable = false;
        Task.Run(async () =>
        {
            await OnlineFunctionality.Context.ClaimDailyReward();
            await CosmeticManager.FetchLoadout();
        });
    }

    public void EnableClaimUi(string dayId) => StartCoroutine(EnableClaimUiCoroutine(dayId));

    private IEnumerator EnableClaimUiCoroutine(string dayId)
    {
        Cms cms = OnlineFunctionality.LastFetchedContent;
        CalendarReward reward = cms.CalendarRewards[dayId];

        AsyncOperationHandle<Sprite> loadIcon = Addressables.LoadAssetAsync<Sprite>(reward.Icon.AddressablePath);
        yield return new WaitUntil(() => loadIcon.IsDone);

        _iconImage.sprite = loadIcon.Result;
        _nameTitle.text = cms.GetString(reward.Name);
        _descriptionText.text = cms.GetString(reward.Description);
        _subtitleText.text = cms.GetString(reward.Subtitle);

        bool isClaimed = _ownedDays.Contains(dayId);
        _claimButton.interactable = !isClaimed && dayId == cms.CurrentCalendarReward?.Id;

        string unclaimedString = dayId == cms.CurrentCalendarReward?.Id ? "game_ui.calendar_unclaimed" : "game_ui.calendar_missed";
        _claimButtonText.text = cms.GetString(isClaimed ? "game_ui.calendar_claimed" : unclaimedString);

        _claimPage.SetActive(true);
    }

    private void Awake()
    {
        _timeRemainingString = OnlineFunctionality.LastFetchedContent.GetString("game_ui.shop_remaining_time");
        StartCoroutine(Refresh());
    }

    private void FixedUpdate()
    {
        _timeRemainingText.text = string.Format(_timeRemainingString, (_endTime - DateTime.UtcNow).ToWordString());

        if (DateTime.UtcNow > _endTime)
        {
            StartCoroutine(Refresh());
            _endTime = DateTime.MaxValue; // to prevent spamming this coroutine before the value sets
        }
    }

    private IEnumerator Refresh()
    {
        CalendarReward next = OnlineFunctionality.LastFetchedContent.CurrentCalendarReward;
        bool hasNextDay = next != null && !next.IsLastDay;
        _endTime = !hasNextDay ? DateTime.MaxValue : next.Date + TimeSpan.FromDays(1);
        _timeRemainingText.gameObject.SetActive(hasNextDay);

        foreach (GameObject oldButton in _buttons)
        {
            Destroy(oldButton);
        }

        Task<Cms> contentTask = OnlineFunctionality.GetContent();
        yield return new WaitUntil(() => contentTask.IsCompleted);
        StartCoroutine(CreateButtons());
    }

    private IEnumerator CreateButtons()
    {
        Task<List<string>> getClaimedTask = OnlineFunctionality.Context.GetClaimedDailyRewards();
        yield return new WaitUntil(() => getClaimedTask.IsCompleted);
        _ownedDays = getClaimedTask.Result;

        foreach (string dayId in s_buttonOrder)
        {
            if (!OnlineFunctionality.LastFetchedContent.CalendarRewards.ContainsKey(dayId))
            {
                Plugin.Log.LogWarning($"Missing day ID {dayId}");
                continue;
            }

            GameObject button = Instantiate(_templateButton, _buttonHolder);
            _buttons.Add(button);
            button.GetComponent<AdventCalendarButton>().SetUp(this, OnlineFunctionality.LastFetchedContent.CalendarRewards[dayId]);
        }
    }
}
