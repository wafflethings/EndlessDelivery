using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using AtlasLib.Utils;
using EndlessDelivery.Config;
using EndlessDelivery.Scores.Server;
using Newtonsoft.Json;
using Steamworks;
using UnityEngine;

namespace EndlessDelivery.Scores
{
    public class Score
    {
        public static string HighscoreFilePath => Path.Combine(Option.SavePath, $"highscore_{PrefsManager.Instance.GetInt("difficulty")}.json");
        public static bool CanSubmit => !Anticheat.Anticheat.HasIllegalMods && !CheatsController.Instance.cheatsEnabled;
        
        public static Score Highscore
        {
            get
            {
                if (_highscore == null)
                {
                    Score fileScore = File.Exists(HighscoreFilePath) ? JsonConvert.DeserializeObject<Score>(File.ReadAllText(HighscoreFilePath)) : null;
                    
                    if (fileScore == null)
                    {
                        Debug.Log("Using fallback score!");
                        Highscore = new(0, 0, 0, 0);
                    }
                    else
                    {
                        Highscore = fileScore;
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

        public static async Task GetServerScoreAndSetIfHigher()
        {
            int playerPos = await Endpoints.GetUserPosition(SteamClient.SteamId);
            Score serverScore = (await Endpoints.GetScoreRange(playerPos, 1))[0].Score;
            if (IsLargerThanOtherScore(serverScore, Highscore))
            {
                Highscore = serverScore;
            }
        }

        private static void SaveScore(Score score)
        {
            Directory.CreateDirectory(Option.SavePath);
            File.WriteAllText(HighscoreFilePath, JsonConvert.SerializeObject(score));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="score">The score that you're checking; true if this is larger.</param>
        /// <param name="other">The score being compared to; false if this is larger.</param>
        /// <returns></returns>
        public static bool IsLargerThanOtherScore(Score score, Score other)
        {
            if (score.Rooms != other.Rooms)
            {
                if (score.Rooms > other.Rooms)
                {
                    return true;
                }
                return false;
            }

            if (score.Deliveries != other.Deliveries)
            {
                if (score.Deliveries > other.Deliveries)
                {
                    return true;
                }
                return false;
            }

            if (score.Kills != other.Kills)
            {
                if (score.Kills > other.Kills)
                {
                    return true;
                }
                return false;
            }

            if (score.Time != other.Time)
            {
                if (score.Time < other.Time)
                {
                    return true;
                }
                return false;
            }

            return false;
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

        [JsonIgnore] public int MoneyGain => (int)(Time * 10);
    }
}