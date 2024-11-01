using System.Collections;
using System.Threading.Tasks;
using EndlessDelivery.Common.Communication.Scores;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessDelivery.UI;

public class LeaderboardEntry : MonoBehaviour
{
    [Header("All monobehaviours here should implement IText")]
    public MonoBehaviour RankNumber;

    public MonoBehaviour Username;
    public MonoBehaviour Rooms;
    public MonoBehaviour Delivered;
    public Image ProfileActual;
    [SerializeField] private Sprite _loadingPfp;
    private Coroutine? _lastPfpSetter;

    public void SetValuesAndEnable(MonoBehaviour coroutineRunner, OnlineScore onlineScore)
    {
        Plugin.Log.LogInfo($"Received score {onlineScore.Index} {onlineScore.Score.Rooms}");
        Friend user = new(onlineScore.SteamId);

        ((IText)RankNumber).SetText((onlineScore.Index + 1).ToString());
        ((IText)Username).SetText(user.Name);
        ((IText)Rooms).SetText(onlineScore.Score.Rooms.ToString());
        ((IText)Delivered).SetText($"({onlineScore.Score.Deliveries})");

        if (_lastPfpSetter != null)
        {
            coroutineRunner.StopCoroutine(_lastPfpSetter);
        }
        _lastPfpSetter = coroutineRunner.StartCoroutine(SetAvatar(user));

        gameObject.SetActive(true);
    }

    private IEnumerator SetAvatar(Friend user)
    {
        ProfileActual.sprite = _loadingPfp;
        Task<Steamworks.Data.Image?> imageTask = user.GetMediumAvatarAsync();
        yield return new WaitUntil(() => imageTask.IsCompleted);
        Plugin.Log.LogInfo("Pfp");
        Texture2D texture2D = new((int)imageTask.Result.Value.Width, (int)imageTask.Result.Value.Height, TextureFormat.RGBA32, false);
        texture2D.LoadRawTextureData(imageTask.Result.Value.Data);
        texture2D.Apply();
        ProfileActual.sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.one / 2);
    }
}
