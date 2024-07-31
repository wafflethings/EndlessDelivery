using System.Collections.Generic;
using System.IO;
using EndlessDelivery.Common;
using EndlessDelivery.Config;
using EndlessDelivery.Gameplay;
using EndlessDelivery.Saving;
using Newtonsoft.Json;
using UnityEngine;

namespace EndlessDelivery.ScoreManagement;

public static class BestTimes
{
    private static readonly SaveFile<List<Dictionary<int, float>>> s_timeFile = SaveFile.RegisterFile(new SaveFile<List<Dictionary<int, float>>>("local_times.json"));

    private static Dictionary<int, float> s_currentDifficultyTimes => s_timeFile.Data[PrefsManager.Instance.GetInt("difficulty")];

    public static void SetIfHigher(int room, float time)
    {
        if (!ScoreManager.CanSubmit)
        {
            return;
        }

        Dictionary<int, float> data = s_currentDifficultyTimes;
        if (data.ContainsKey(room))
        {
            if (time > data[room])
            {
                data[room] = time;
                return;
            }

            return;
        }

        data.Add(room, time);
    }

    public static float GetRoomTime(int room)
    {
        if (room != 0)
        {
            return s_currentDifficultyTimes[room];
        }

        return GameManager.StartTime;
    }
}
