using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EndlessDelivery.Common;
using EndlessDelivery.Common.Communication;
using EndlessDelivery.Common.Communication.Scores;
using EndlessDelivery.Online.Requests;
using EndlessDelivery.Saving;

namespace EndlessDelivery.ScoreManagement;

public class ScoreManager
{
    public static bool CanSubmit => !Anticheat.Anticheat.HasIllegalMods && !CheatsController.Instance.cheatsEnabled;
    public static SaveFile<Dictionary<int, Score>> LocalHighscores = SaveFile.RegisterFile(new SaveFile<Dictionary<int, Score>>("local_scores.json"));

    public static Score CurrentDifficultyHighscore => LocalHighscores.Data.TryGetValue(PrefsManager.Instance.GetInt("difficulty"), out Score score) ? score : new Score(0,0,0,0);

    public static async Task<int> SubmitScore(Score score, short difficulty)
    {
        if (!LocalHighscores.Data.TryAdd(difficulty, score))
        {
            LocalHighscores.Data[difficulty] = score;
        }

        try
        {
            int newPosition = await Scores.SubmitScore(new SubmitScoreData(score, difficulty, Plugin.Version));

            if (newPosition == -1)
            {
                HudMessageReceiver.Instance.SendHudMessage("Score submit error!");
            }

            return newPosition;
        }
        catch
        {
            HudMessageReceiver.Instance.SendHudMessage("Score submit error!");
            return -1;
        }
    }

    public static async Task<OnlineScore[]> GetPage(int pageIndex)
    {
        int scoreCount = await Scores.GetLength();
        int startIndex = pageIndex * 5;
        int amount = Math.Min(scoreCount - startIndex, 5);
        return await Scores.GetRange(pageIndex * 5, amount);
    }
}
