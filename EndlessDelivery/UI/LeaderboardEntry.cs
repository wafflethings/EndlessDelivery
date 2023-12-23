using EndlessDelivery.Scores.Server;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessDelivery.UI
{
    public class LeaderboardEntry : MonoBehaviour
    {
        public Text RankNumber;
        public Text Username;
        public Text Rooms;
        public Text Delivered;
        public Image ProfileActual;

        public void SetValuesAndEnable(ScoreResult scoreResult)
        {
            Friend user = new(scoreResult.SteamId);
            
            RankNumber.text = (scoreResult.Index + 1).ToString();
            Username.text = user.Name;
            Rooms.text = scoreResult.Score.Rooms.ToString();
            Delivered.text = $"({scoreResult.Score.Deliveries})";
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