using System;
using System.Collections;
using System.Threading.Tasks;
using EndlessDelivery.Api.Requests;
using EndlessDelivery.Common;
using EndlessDelivery.Common.Communication.Scores;
using EndlessDelivery.Common.ContentFile;
using EndlessDelivery.Common.Inventory.Items;
using EndlessDelivery.Cosmetics;
using EndlessDelivery.Online;
using Steamworks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace EndlessDelivery.UI;

public class LeaderboardEntry : MonoBehaviour
{
    [Header("All monobehaviours here should implement IText")]
    public MonoBehaviour RankNumber;

    public MonoBehaviour Username;
    public MonoBehaviour Rooms;
    public MonoBehaviour Difficulty;
    public Image ProfileActual;
    public Image Banner;
    [SerializeField] private Sprite _loadingPfp;
    private Coroutine? _lastPfpSetter;
    private Coroutine? _lastBannerSetter;
    private Coroutine? _lastUsernameSetter;
    private Friend _user;
    private OnlineScore? _score;

    public void SetValuesAndEnable(MonoBehaviour coroutineRunner, OnlineScore? onlineScore)
    {
        if (onlineScore == null)
        {
            SetNullValues();
            return;
        }

        _score = onlineScore;
        _user = new Friend(onlineScore.SteamId);

        ((IText)RankNumber).SetText((onlineScore.Index + 1).ToString());
        ((IText)Rooms).SetText(onlineScore.Score.Rooms.ToString());
        ((IText)Difficulty).SetText(DdUtils.IntToDifficulty(onlineScore.Difficulty));

        if (_lastUsernameSetter != null)
        {
            coroutineRunner.StopCoroutine(_lastUsernameSetter);
        }
        _lastUsernameSetter = coroutineRunner.StartCoroutine(SetUsername(_user));

        if (_lastPfpSetter != null)
        {
            coroutineRunner.StopCoroutine(_lastPfpSetter);
        }
        _lastPfpSetter = coroutineRunner.StartCoroutine(SetAvatar(_user));

        if (Banner != null)
        {
            if (_lastBannerSetter != null)
            {
                coroutineRunner.StopCoroutine(_lastBannerSetter);
            }

            _lastBannerSetter = coroutineRunner.StartCoroutine(SetBanner(_user));
        }

        gameObject.SetActive(true);
    }

    public void SetNullValues()
    {
        ((IText)RankNumber).SetText("#-");
        ((IText)Username).SetText("-");
        ((IText)Rooms).SetText("-");
        ((IText)Difficulty).SetText("(-)");
        ProfileActual.sprite = _loadingPfp;
        gameObject.SetActive(true);
    }

    private IEnumerator SetUsername(Friend user)
    {
        Task<string> usernameTask = OnlineFunctionality.Context.GetUsername(user.Id);
        yield return new WaitUntil(() => usernameTask.IsCompleted);
        string username = usernameTask.Result;

        if (_score.SteamId == SteamClient.SteamId)
        {
            username = $"<color=orange>{username}</color>";
        }

        ((IText)Username).SetText(username);
    }

    private IEnumerator SetAvatar(Friend user)
    {
        ProfileActual.sprite = _loadingPfp;
        Task<Steamworks.Data.Image?> imageTask = user.GetLargeAvatarAsync();
        yield return new WaitUntil(() => imageTask.IsCompleted);
        Texture2D texture2D = new((int)imageTask.Result.Value.Width, (int)imageTask.Result.Value.Height, TextureFormat.RGBA32, false);
        texture2D.LoadRawTextureData(imageTask.Result.Value.Data);
        texture2D.Apply();
        ProfileActual.sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.one / 2);
    }

    private IEnumerator SetBanner(Friend user)
    {
        Banner.gameObject.SetActive(false);
        Task<CosmeticLoadout> loadoutTask = OnlineFunctionality.Context.GetLoadout(user.Id);
        Task<Cms> cmsTask = OnlineFunctionality.GetContent();
        yield return new WaitUntil(() => cmsTask.IsCompleted && loadoutTask.IsCompleted);

        if (!cmsTask.Result.Banners.TryGetValue(loadoutTask.Result.BannerId, out Banner banner))
        {
            yield break;
        }

        AsyncOperationHandle<Sprite?> bannerLoadTask = Addressables.Instance.LoadAssetAsync<Sprite>(banner.Asset.AddressablePath);
        yield return new WaitUntil(() => bannerLoadTask.IsDone);

        if (bannerLoadTask.Result == null)
        {
            yield break;
        }

        Banner.sprite = bannerLoadTask.Result;
        Banner.gameObject.SetActive(true);
    }

    public void OpenProfile()
    {
        Application.OpenURL(string.Format(OnlineFunctionality.LastFetchedContent.GetString("constants.website_users"), _user.Id));
    }
}
