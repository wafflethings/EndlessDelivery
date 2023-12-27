using System.Threading.Tasks;
using EndlessDelivery.Scores.Server;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessDelivery.UI
{
    public class LeaderboardEntry : MonoBehaviour
    {
        [Header("All monobehaviours here should implement IText")]
        public MonoBehaviour RankNumber;
        public MonoBehaviour Username;
        public MonoBehaviour Rooms;
        public MonoBehaviour Delivered;
        public Image ProfileActual;

        public async void SetValuesAndEnable(ScoreResult scoreResult)
        {
            Friend user = new(scoreResult.SteamId);
            
            ((IText)RankNumber).SetText((scoreResult.Index + 1).ToString());
            ((IText)Username).SetText(user.Name);
            ((IText)Rooms).SetText(scoreResult.Score.Rooms.ToString());
            ((IText)Delivered).SetText($"({scoreResult.Score.Deliveries})");
            SetAvatar(user);
            
            gameObject.SetActive(true);
        }

        private async void SetAvatar(Friend user)
        {
            Steamworks.Data.Image? image = await user.GetMediumAvatarAsync();
            Texture2D texture2D = new Texture2D((int)image.Value.Width, (int)image.Value.Height, TextureFormat.RGBA32, false);
            texture2D.LoadRawTextureData(image.Value.Data);
            texture2D.Apply();
            ProfileActual.sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.one / 2);
        }
    }
}