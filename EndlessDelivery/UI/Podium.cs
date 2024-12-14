using System.Collections;
using System.Threading.Tasks;
using EndlessDelivery.Api.Requests;
using EndlessDelivery.Common.Communication.Scores;
using EndlessDelivery.Online;
using Steamworks;
using TMPro;
using UnityEngine;

namespace EndlessDelivery.UI;

public class Podium : MonoBehaviour
{
    [SerializeField] private MeshRenderer[] _pfpRenderers;
    [SerializeField] private TMP_Text[] _nameTexts;

    private void Awake()
    {
        StartCoroutine(SetValues());
    }

    private IEnumerator SetValues()
    {
        Task<OnlineScore[]> scoresTask = OnlineFunctionality.Context.GetScoreRange(0, 3);
        yield return new WaitUntil(() => scoresTask.IsCompleted);

        int index = 0;
        foreach (OnlineScore score in scoresTask.Result)
        {
            StartCoroutine(SetPfp(score.SteamId, index));

            Task<string> usernameTask = OnlineFunctionality.Context.GetUsername(score.SteamId);
            yield return new WaitUntil(() => usernameTask.IsCompleted);
            _nameTexts[index].text = $"#{index + 1} - " + usernameTask.Result.ToUpperInvariant();

            index++;
        }
    }

    private IEnumerator SetPfp(ulong steamId, int index)
    {
        Task<Steamworks.Data.Image?> imageTask = new Friend(steamId).GetLargeAvatarAsync();
        float startTime = Time.time;
        yield return new WaitUntil(() => imageTask == null || imageTask.IsCompleted || imageTask.IsFaulted || imageTask.IsCanceled || startTime - Time.time > 10);
        if (imageTask?.Result != null)
        {
            Texture2D texture2D = new((int)imageTask.Result.Value.Width, (int)imageTask.Result.Value.Height, TextureFormat.RGBA32, false);
            texture2D.LoadRawTextureData(imageTask.Result.Value.Data);
            texture2D.Apply();
            Material[] materialArray = _pfpRenderers[index].materials;
            materialArray[1].mainTextureScale = new Vector2(1, -1);
            materialArray[1].mainTexture = texture2D;
            _pfpRenderers[index].materials = materialArray;
        }
    }
}
