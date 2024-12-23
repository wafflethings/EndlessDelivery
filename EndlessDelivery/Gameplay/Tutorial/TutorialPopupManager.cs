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
    private static SaveFile<TutorialData> s_tutorialData = SaveFile.RegisterFile(new SaveFile<TutorialData>("tutorial_data.json", Plugin.Name, new()));

    private void Awake()
    {
        IEnumerable<Score> scores = ScoreManagement.ScoreManager.LocalHighscores.Data.Values;

        if (scores.Any(score => score.Deliveries > 0))
        {
            s_tutorialData.Data.ShowDeliverPresents = false;
        }

        if (scores.Any(score => score.Rooms > 0))
        {
            s_tutorialData.Data.ShowJumpThroughChimney = false;
        }

        if (StartTimes.Instance.Data.DifficultyToTimes.Values.Any(startTime => startTime.SelectedWave != 0))
        {
            s_tutorialData.Data.ShowSelectStartRoom = false;
        }
    }

    private void FixedUpdate()
    {
        if (s_tutorialData.Data.ShowDeliverPresents && GameManager.Instance.GameStarted)
        {
            HudMessageReceiver.Instance.SendHudMessage("DELIVER PRESENTS INTO CHIMNEYS OF THE SAME COLOUR");
            s_tutorialData.Data.ShowDeliverPresents = false;
        }

        if (s_tutorialData.Data.ShowJumpThroughChimney && GameManager.Instance.GameStarted && GameManager.Instance.CurrentRoom.ChimneysDone)
        {
            HudMessageReceiver.Instance.SendHudMessage("JUMP THROUGH ANY CHIMNEY TO PROCEED");
            s_tutorialData.Data.ShowJumpThroughChimney = false;
        }

        StartTimes.StartTime current = StartTimes.Instance.Data.CurrentTimes;
        if (s_tutorialData.Data.ShowSelectStartRoom && current.UnlockedStartTimes.Any(startWave => startWave != 0) && current.SelectedWave == 0)
        {
            HudMessageReceiver.Instance.SendHudMessage("YOU HAVE UNLOCKED A NEW STARTING ROOM.\nSELECT IT IN THE OPTIONS MENU OF THE GREEN TERMINAL");
            s_tutorialData.Data.ShowSelectStartRoom = false;
        }
    }
}
