using System.Collections.Generic;
using System.IO;
using EndlessDelivery.Config;
using EndlessDelivery.Gameplay;
using Newtonsoft.Json;
using UnityEngine;

namespace EndlessDelivery.Scores
{
    public static class BestTimes
    {
        public static string TimesFilePath => Path.Combine(Option.SavePath, $"besttimes_{PrefsManager.Instance.GetInt("difficulty")}.json");
        public static float CurrentStartTime => GetRoomTime((int) ((Option<long>) Option.AllOptions["start_wave"]).Value);
        
        public static Dictionary<int, float> Times
        {
            get
            {
                if (_times == null)
                {
                    if (File.Exists(TimesFilePath))
                    {
                        _times = JsonConvert.DeserializeObject<Dictionary<int, float>>(File.ReadAllText(TimesFilePath));
                    }
                    else
                    {
                        _times = new();
                    }
                }

                return _times;
            }
        }

        private static Dictionary<int, float> _times;

        public static void SetIfHigher(int room, float time)
        {
            Debug.Log($"{room} at {time}");
            if (Times.ContainsKey(room))
            {
                if (time > Times[room])
                {
                    Times[room] = time;
                }

                return;
            }
            Debug.Log("doesnt contain");
            Times.Add(room, time);
            Debug.Log("add?");
            Save();
        }

        public static float GetRoomTime(int room)
        {
            if (room != 0)
            {
                return Times[room];
            }

            return GameManager.StartTime;
        }

        private static async void Save()
        {
            Directory.CreateDirectory(Option.SavePath);

            using (StreamWriter sw = new(TimesFilePath))
            {
                await sw.WriteAsync(JsonConvert.SerializeObject(Times));
            }
        }
    }
}