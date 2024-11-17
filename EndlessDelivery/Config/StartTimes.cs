using System;
using System.Collections.Generic;
using System.Linq;
using AtlasLib.Saving;
using EndlessDelivery.Gameplay;
using EndlessDelivery.ScoreManagement;
using Newtonsoft.Json;

namespace EndlessDelivery.Config;

public class StartTimes
{
    [JsonIgnore] public static readonly EncryptedSaveFile<StartTimes> Instance = SaveFile.RegisterFile(new EncryptedSaveFile<StartTimes>("times.ddenc")) as EncryptedSaveFile<StartTimes> ?? throw new();
    public Dictionary<int, StartTime> DifficultyToTimes = new();

    [JsonIgnore]
    public StartTime CurrentTimes
    {
        get
        {
            int difficulty = PrefsManager.Instance.GetInt("difficulty");

            if (!DifficultyToTimes.ContainsKey(difficulty))
            {
                DifficultyToTimes.Add(difficulty, new StartTime());
            }

            return DifficultyToTimes[difficulty];
        }
    }

    public void UpdateAllLowerDifficulty(int wave, float time)
    {
        if (!ScoreManager.CanSubmit)
        {
            return;
        }

        for (int i = PrefsManager.Instance.GetInt("difficulty"); i >= 0; i--)
        {
            if (!DifficultyToTimes.ContainsKey(i))
            {
                DifficultyToTimes.Add(i, new StartTime());
            }
            DifficultyToTimes[i].SetValues(wave, time);
        }
    }

    [Serializable]
    public class StartTime
    {
        public static readonly int[] StartableWaves = [0, 5, 10, 25, 50, 75];
        public Dictionary<int, float> WaveToTime = new() { { 0, GameManager.StartTime } };
        public List<int> UnlockedStartTimes = new();
        public int SelectedWave;

        public void SetValues(int wave, float time)
        {
            Plugin.Log.LogMessage($"SetValues(wave {wave}, time {time}) | {string.Join(",", UnlockedStartTimes)}");
            if (!UnlockedStartTimes.Contains(0))
            {
                UnlockedStartTimes.Add(0);
            }

            if (StartableWaves.Contains(wave))
            {
                if (WaveToTime.ContainsKey(wave))
                {
                    if (time > WaveToTime[wave])
                    {
                        WaveToTime[wave] = time;
                    }
                }
                else
                {
                    WaveToTime.Add(wave, time);
                }
            }

            Plugin.Log.LogMessage("Foreach");
            foreach (int startableWave in StartableWaves)
            {
                Plugin.Log.LogMessage($"Check if {wave} != {startableWave * 2}");
                if (wave != startableWave * 2)
                {
                    Plugin.Log.LogMessage("Continue");
                    continue;
                }

                if (!UnlockedStartTimes.Contains(startableWave))
                {
                    Plugin.Log.LogMessage($"Added {startableWave}");
                    UnlockedStartTimes.Add(startableWave);
                }
            }
        }

        public float GetRoomTime(int room)
        {
            if (!StartableWaves.Contains(room))
            {
                throw new Exception("Room needs to be in startablewaves.");
            }

            return WaveToTime[room];
        }
    }
}
