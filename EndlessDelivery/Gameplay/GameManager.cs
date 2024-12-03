using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EndlessDelivery.Api.Requests;
using EndlessDelivery.Assets;
using EndlessDelivery.Common;
using EndlessDelivery.Config;
using EndlessDelivery.Gameplay.EnemyGeneration;
using EndlessDelivery.Online;
using EndlessDelivery.ScoreManagement;
using EndlessDelivery.UI;
using EndlessDelivery.Utils;
using HarmonyLib;
using UnityEngine;
using UnityEngine.AI;

namespace EndlessDelivery.Gameplay;

[HarmonyPatch]
public class GameManager : MonoSingleton<GameManager>
{
    public const float StartTime = 50;
    public const float TimeAddLength = 0.5f;

    public AudioSource TimeAddSound;
    public RoomPool RoomPool;

    public bool GameStarted { get; private set; }
    public float TimeLeft { get; private set; }
    public float TimeElapsed { get; private set; }
    public int DeliveredPresents { get; set; }
    public int RoomsComplete { get; private set; }
    public Room CurrentRoom { get; private set; }
    public RoomData CurrentRoomData { get; private set; }
    public Room PreviousRoom { get; private set; }
    public bool TimerActive { get; private set; }
    public int PointsPerWave { get; private set; }
    public Score CurrentScore => new(RoomsComplete, StatsManager.Instance.kills, DeliveredPresents, TimeElapsed, StartTimes.Instance.Data.CurrentTimes.SelectedWave);

    public delegate void RoomEvent(Room room);
    public event RoomEvent RoomStarted;
    public event RoomEvent RoomCleared;
    public event RoomEvent RoomComplete;

    [SerializeField] private Vector3 _baseRoomPosition = new(0, 0, 100);
    private Coroutine _pauseCoroutine;
    private List<RoomData> _remainingRooms = new();

    private const int StartingPoints = 15;
    private const int MaxPointGain = 15;

    public static int GetRoomPoints(int roomNumber)
    {
        int points = StartingPoints;

        for (int i = 0; i < roomNumber; i++)
        {
            points += Mathf.Min(3 + (i + 1) / 3, MaxPointGain);
        }

        return points;
    }

    private void Awake()
    {
        EnemyHack.AddToPools(EnemyGroup.Groups[DeliveryEnemyClass.Projectile], EnemyGroup.Groups[DeliveryEnemyClass.Uncommon], EnemyGroup.Groups[DeliveryEnemyClass.Special], EnemyGroup.Groups[DeliveryEnemyClass.Melee]);
    }

    private void Update()
    {
        if (GameStarted && GunControl.Instance.activated)
        {
            TimeLeft = Mathf.MoveTowards(TimeLeft, 0, Time.deltaTime);
            TimeElapsed += Time.deltaTime;

            if (TimeLeft == 0)
            {
                EndGame();
            }

            if (!CurrentRoom.RoomCleared && CurrentRoom.RoomActivated && CurrentRoom.AllEnemiesSpawned && EnemyTracker.Instance.enemies.All(enemy => enemy.dead))
            {
                CurrentRoom.RoomCleared = true;
                MusicManager.Instance.PlayCleanMusic();
                RoomCleared?.Invoke(CurrentRoom);
                AddTime(6, "<color=orange>FULL CLEAR</color>");
            }
        }
    }

    public void AddTime(float seconds, string reason)
    {
        TimeAddSound?.Play();
        TimerActive = false;

        if (_pauseCoroutine != null)
        {
            StopCoroutine(_pauseCoroutine);
        }

        _pauseCoroutine = StartCoroutine(UnpauseTimer());

        StyleHUD.Instance.AddPoints(5, $"{reason} <size=20>({seconds}s)</size>");

        TimeLeft += seconds;
    }

    public void SilentAddTime(float seconds)
    {
        TimeLeft += seconds;
    }

    private IEnumerator UnpauseTimer()
    {
        yield return new WaitForSeconds(TimeAddLength);
        TimerActive = true;
    }

    private Room GenerateNewRoom()
    {
        CurrentRoomData = GetRandomRoom();
        return Instantiate(CurrentRoomData.Prefab, _baseRoomPosition + (Vector3.right * (RoomsComplete % 3) * 200), Quaternion.identity).GetComponent<Room>();
    }

    private RoomData GetRandomRoom()
    {
        if (_remainingRooms.Count == 0)
        {
            _remainingRooms.AddRange(RoomPool.Rooms);
        }

        RoomData picked = _remainingRooms.Pick();
        _remainingRooms.Remove(picked);
        return picked;
    }

    public void RoomEnd()
    {
        RoomComplete?.Invoke(CurrentRoom);
        RoomsComplete++;

        if (CurrentRoom.RoomHasGameplay)
        {
            AddTime(8, "<color=orange>ROOM CLEAR</color>");
            PointsPerWave += Mathf.Min(3 + RoomsComplete / 3, StartingPoints);
            StartTimes.Instance.Data.UpdateAllLowerDifficulty(RoomsComplete, TimeLeft);
        }
        else if (!GameStarted)
        {
            StartGame();
        }

        SetRoom(GenerateNewRoom());
        // BlackFade.Instance.Flash(0.125f);
    }

    public void SetRoom(Room room)
    {
        if (CurrentRoom != room)
        {
            PreviousRoom = CurrentRoom;
            CurrentRoom = room;
            room.Initialize();
        }

        if (room.RoomHasGameplay && !room.RoomAlreadyVisited)
        {
            room.RoomAlreadyVisited = true;
        }

        RoomStarted?.Invoke(CurrentRoom);
    }

    public void StartGame()
    {
        if (GameStarted)
        {
            return;
        }

        PresentTimeHud.Instance.gameObject.SetActive(true);
        StatsManager.Instance.GetComponentInChildren<MusicManager>(true).gameObject.SetActive(true);
        MusicManager.Instance.StartMusic();
        RoomsComplete = StartTimes.Instance.Data.CurrentTimes.SelectedWave;
        PointsPerWave = GetRoomPoints(RoomsComplete);
        TimeLeft = StartTimes.Instance.Data.CurrentTimes.GetRoomTime(RoomsComplete);
        TimerActive = true;
        GameStarted = true;
    }

    public void EndGame()
    {
        if (!GameStarted)
        {
            return;
        }

        if (!NewMovement.Instance.dead)
        {
            NewMovement.Instance.GetHurt(1000, false);
        }

        NewMovement.Instance.blackScreen.gameObject.SetActive(false);
        NewMovement.Instance.hp = 100; // to prevent restart working - StatsManager.Update
        GameStarted = false;

        int difficulty = PrefsManager.Instance.GetInt("difficulty");

        //if more rooms, or more deliveries

        if (ScoreManager.CanSubmit(out List<string> reasons))
        {
            if (!ScoreManager.LocalHighscores.Data.ContainsKey(difficulty) || CurrentScore > ScoreManager.LocalHighscores.Data[difficulty])
            {
                EndScreen.Instance.PreviousHighscore = ScoreManager.CurrentDifficultyHighscore;
                EndScreen.Instance.NewBest = true;
            }

            StartCoroutine(ShowAfterSubmit(difficulty));
        }
        else
        {
            HudMessageReceiver.Instance.SendHudMessage($"Score not submitting due to other mods, or cheats enabled. {string.Join(", ", reasons)}");
            EndScreen.Instance.Appear();
        }
    }

    private IEnumerator ShowAfterSubmit(int difficulty)
    {
        Task<bool> onlineTask = OnlineFunctionality.Context.ServerOnline();
        yield return new WaitUntil(() => onlineTask.IsCompleted);

        if (onlineTask.Result)
        {
            Task<bool> updateRequiredTask = OnlineFunctionality.Context.UpdateRequired();
            yield return new WaitUntil(() => updateRequiredTask.IsCompleted);

            if (!updateRequiredTask.Result)
            {
                Task submitTask = ScoreManager.SubmitScore(CurrentScore, (short)difficulty);
                yield return new WaitUntil(() => submitTask.IsCompleted);
            }
            else
            {
                HudMessageReceiver.Instance.SendHudMessage("Update required to submit scores!");
            }
        }
        else
        {
            HudMessageReceiver.Instance.SendHudMessage("Server offline!");
        }

        EndScreen.Instance.Appear();
    }

    [HarmonyPatch(typeof(SeasonalHats), nameof(SeasonalHats.Start)), HarmonyPrefix]
    private static void EnableHats(SeasonalHats __instance)
    {
        if (AssetManager.InSceneFromThisMod)
        {
            __instance.easter.SetActive(false);
            __instance.halloween.SetActive(false);
            __instance.christmas.SetActive(true);
        }
    }

    [HarmonyPatch(typeof(NewMovement), nameof(NewMovement.GetHurt)), HarmonyPostfix]
    private static void CustomDeath(NewMovement __instance)
    {
        if (__instance.dead && AssetManager.InSceneFromThisMod)
        {
            Instance.EndGame();
        }
    }
}
