using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EndlessDelivery.Api.Requests;
using EndlessDelivery.Common;
using EndlessDelivery.Common.ContentFile;
using EndlessDelivery.Common.Inventory.Items;
using EndlessDelivery.Cosmetics;
using EndlessDelivery.Online;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace EndlessDelivery.UI;

public class AchievementTerminal : MonoBehaviour
{
    [SerializeField] private TMP_Text _achievementNameText;
    [SerializeField] private TMP_Text _achievementDescText;
    [SerializeField] private TMP_Text _achievementUnlocksText;
    [SerializeField] private GameObject _buttonTemplate;
    [SerializeField] private Transform _achievementHolder;

    private void Awake()
    {
        SetAchievement(null);
        StartCoroutine(PopulateAchievements());
    }

    private IEnumerator PopulateAchievements()
    {
        _achievementHolder.gameObject.SetActive(false);

        Task<Cms> cmsTask = OnlineFunctionality.GetContent();
        Task<List<OwnedAchievement>> achievementsTask = OnlineFunctionality.Context.GetAchievements(SteamClient.SteamId);
        yield return new WaitUntil(() => cmsTask.IsCompleted && achievementsTask.IsCompleted);
        Cms cms = cmsTask.Result;
        List<string> ownedAchievementIds = achievementsTask.Result.Select(x => x.Id).ToList();

        foreach (Achievement achievement in cms.Achievements.Values.OrderBy(x => !ownedAchievementIds.Contains(x.Id)).ThenBy(x => cms.GetLocalisedString(x.Id)))
        {
            if (achievement.Disabled)
            {
                continue;
            }

            AddAchievement(achievement, ownedAchievementIds.Contains(achievement.Id));
        }

        _achievementHolder.gameObject.SetActive(true);
    }

    private void AddAchievement(Achievement achievement, bool isOwned)
    {
        Instantiate(_buttonTemplate, _achievementHolder).GetComponent<AchievementTerminalButton>().SetUp(this, achievement, isOwned);
    }

    public void SetAchievement(Achievement? achievement)
    {
        Cms cms = OnlineFunctionality.LastFetchedContent;
        _achievementNameText.text = achievement == null ? "-" : cms.GetLocalisedString(achievement.Name);
        _achievementDescText.text = achievement == null ? "-" : achievement.HideDetails ? cms.GetLocalisedString("game_ui.hidden_achievement") : cms.GetLocalisedString(achievement.Description);

        if (achievement == null)
        {
            _achievementUnlocksText.text = "-";
            return;
        }

        List<Item> items = new();
        foreach (string itemGrantId in achievement.ItemGrants)
        {
            if (!cms.TryGetItem(itemGrantId, out Item item))
            {
                continue;
            }

            items.Add(item);
        }

        string unlocksString = items.Count != 0 ? string.Join(", ", items.Select(x => cms.GetLocalisedString(x.Descriptor.Name))) : cms.GetLocalisedString("game_ui.achievement_unlocks_nothing");
        _achievementUnlocksText.text = string.Format(cms.GetLocalisedString("game_ui.achievement_unlocks"), unlocksString);
    }
}
