using System;
using System.IO;
using System.Reflection;
using AtlasLib.Utils;
using Newtonsoft.Json;

namespace EndlessDelivery.Scores
{
    public class Score
    {
        public static string HighscorePath => Path.Combine(PathUtils.ModPath(), "Savedata");
        public static string HighscoreFilePath => Path.Combine(HighscorePath, $"highscore_{PrefsManager.Instance.GetInt("difficulty")}.json");
        
        public static Score Highscore
        {
            get
            {
                if (_highscore == null)
                {
                    if (File.Exists(HighscoreFilePath))
                    {
                        Highscore = JsonConvert.DeserializeObject<Score>(File.ReadAllText(HighscoreFilePath));
                    }
                    else
                    {
                        Highscore = new(0, 0, 0, 0);
                    }
                }

                return _highscore;
            }

            set
            {
                PreviousHighscore = _highscore;
                _highscore = value;
                SaveScore(value);
            }
        }

        public static Score PreviousHighscore { get; private set; }

        private static void SaveScore(Score score)
        {
            Directory.CreateDirectory(HighscorePath);
            File.WriteAllText(HighscoreFilePath, JsonConvert.SerializeObject(score));
        }

        private static Score _highscore; 
        
        public readonly int Rooms;
        public readonly int Kills;
        public readonly int Deliveries;
        public readonly float Time;
        
        public Score(int rooms, int kills, int deliveries, float time)
        {
            Rooms = rooms;
            Kills = kills;
            Deliveries = deliveries;
            Time = time;
        }

        public int MoneyGain => (int)(Time * 10);
    }
}