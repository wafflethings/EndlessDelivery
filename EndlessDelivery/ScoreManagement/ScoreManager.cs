using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AtlasLib.Saving;
using EndlessDelivery.Achievements;
using EndlessDelivery.Api.Requests;
using EndlessDelivery.Common;
using EndlessDelivery.Common.Communication.Scores;
using EndlessDelivery.Online;
using Steamworks;

namespace EndlessDelivery.ScoreManagement;

public class ScoreManager
{
    public static SaveFile<Dictionary<int, Score>> LocalHighscores = SaveFile.RegisterFile(new SaveFile<Dictionary<int, Score>>("local_scores.json", Plugin.Name));
    public static Score CurrentDifficultyHighscore => LocalHighscores.Data.TryGetValue(PrefsManager.Instance.GetInt("difficulty"), out Score score) ? score : new Score(0,0,0,0);

    public static bool CanSubmit() => CanSubmit(out _);

    public static bool CanSubmit(out List<string> reasons) => !Anticheat.Anticheat.HasIllegalMods(out reasons) && !CheatsController.Instance.cheatsEnabled;

    public static bool CanSubmitOnline() => CanSubmit(out _) && PrefsManager.Instance.GetInt("difficulty") >= 3;

    public static async Task<OnlineScore?> SubmitScore(Score score, short difficulty)
    {
        if (!CanSubmitOnline())
        {
            return null;
        }

        if (await OnlineFunctionality.Context.UpdateRequired())
        {
            return null;
        }

        if (score > CurrentDifficultyHighscore)
        {
            LocalHighscores.Data[difficulty] = score;
        }

        try
        {
            OnlineScore submittedScore = await OnlineFunctionality.Context.SubmitScore(new SubmitScoreData(score, difficulty));
            Score lifetimeStats = await OnlineFunctionality.Context.GetLifetimeStats(SteamClient.SteamId);
            AchievementManager.CheckOnline(submittedScore, lifetimeStats);
            return submittedScore;
        }
        catch (Exception ex)
        {
            HudMessageReceiver.Instance.SendHudMessage($"Score submit error!\nUpdate or upload your logs. {ex.GetType()}.");
            Plugin.Log.LogError(ex.ToString());
            return null;
        }
    }
}
