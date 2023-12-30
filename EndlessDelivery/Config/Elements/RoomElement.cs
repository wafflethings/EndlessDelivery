using System;
using EndlessDelivery.Scores;
using EndlessDelivery.Utils;
using TMPro;
using UnityEngine;

namespace EndlessDelivery.Config.Elements
{
    public class RoomElement : MonoBehaviour
    {
        public TMP_Text RoomText;
        public TMP_Text TimeText;
        private Option<long> _option;
        
        private void Start()
        {
            _option = (Option<long>)Option.AllOptions["start_wave"];
            UpdateText();
        }

        public void ChangeBy(int change)
        {
            int amountOfRoomsWithTimes = Mathf.Min(Score.Highscore.Rooms / 2, BestTimes.Times.Count);
            _option.Value = Mathf.Clamp((int)(_option.Value + change), 0, amountOfRoomsWithTimes);
            UpdateText();
        }

        private void UpdateText()
        {
            RoomText.text = $"ROOM {_option.Value}";
            TimeText.text = TimeSpan.FromSeconds(BestTimes.GetRoomTime((int)_option.Value)).Formatted();
        }
    }
}