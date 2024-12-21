using System;
using System.Collections.Generic;
using System.Linq;
using AtlasLib.Saving;
using EndlessDelivery.Common;
using EndlessDelivery.Config;
using UnityEngine;

namespace EndlessDelivery.Gameplay.Tutorial;

public class TutorialPopupManager : MonoBehaviour
{
    public static SaveFile<TutorialData> TutorialData = SaveFile.RegisterFile(new SaveFile<TutorialData>("tutorial_data.json", Plugin.Name, new()));

    private void Awake()
    {
        return;
        IEnumerable<Score> scores = ScoreManagement.ScoreManager.LocalHighscores.Data.Values;

        if (scores.Any(score => score.Deliveries > 0))
        {
            TutorialData.Data.ShowDeliverPresents = false;
        }

        if (scores.Any(score => score.Rooms > 0))
        {
            TutorialData.Data.ShowJumpThroughChimney = false;
        }

        if (StartTimes.Instance.Data.DifficultyToTimes.Values.Any(startTime => startTime.SelectedWave != 0))
        {
            TutorialData.Data.ShowSelectStartRoom = false;
        }
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance.GameStarted && TutorialData.Data.ShowDeliverPresents)
        {
            HudMessageReceiver.Instance.SendHudMessage("DELIVER PRESENTS INTO CHIMNEYS OF THE SAME COLOUR");
            TutorialData.Data.ShowDeliverPresents = false;
        }

        if (GameManager.Instance.GameStarted && GameManager.Instance.CurrentRoom.RoomCleared && TutorialData.Data.ShowJumpThroughChimney)
        {
            HudMessageReceiver.Instance.SendHudMessage("JUMP THROUGH ANY CHIMNEY TO PROCEED");
            TutorialData.Data.ShowJumpThroughChimney = false;
        }

        StartTimes.StartTime current = StartTimes.Instance.Data.CurrentTimes;
        if (current.UnlockedStartTimes.Any(startWave => startWave != 0) && current.SelectedWave == 0)
        {
            HudMessageReceiver.Instance.SendHudMessage("YOU HAVE UNLOCKED A NEW STARTING ROOM.\nSELECT IT IN THE GREEN TERMINAL, IN THE OPTIONS MENU");
            TutorialData.Data.ShowJumpThroughChimney = false;
        }
    }
}
