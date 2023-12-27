using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EndlessDelivery.Assets;
using EndlessDelivery.Scores;
using EndlessDelivery.Scores.Server;
using EndlessDelivery.UI;
using EndlessDelivery.Utils;
using HarmonyLib;
using UnityEngine;
using UnityEngine.AI;

namespace EndlessDelivery.Gameplay
{
    [PatchThis($"{Plugin.GUID}.GameManager")]
    public class GameManager : MonoSingleton<GameManager>
    {
        public const float StartTime = 45;
        public const float TimeAddLength = 0.5f;

        public NavMeshSurface Navmesh;
        public AudioSource TimeAddSound;
        public RoomPool RoomPool;
        
        public bool GameStarted { get; private set; }
        public float TimeLeft { get; private set; }
        public float TimeElapsed { get; private set; }
        public int DeliveredPresents { get; set; }
        public int RoomsEntered { get; private set; }
        public Room CurrentRoom { get; private set; }
        public Room PreviousRoom { get; private set; }
        public bool TimerActive { get; private set; }
        public int PointsPerWave { get; private set; } = 10;
        public Score CurrentScore => new(RoomsEntered - 1, StatsManager.Instance.kills, DeliveredPresents, TimeElapsed);
        
        private Coroutine _pauseCoroutine;
        private List<RoomData> _remainingRooms = new();

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

                if (!CurrentRoom.RoomCleared && CurrentRoom.RoomActivated && EnemyTracker.Instance.enemies.All(enemy => enemy.dead))
                {
                    CurrentRoom.RoomCleared = true;
                    MusicManager.Instance.PlayCleanMusic();
                    AddTime(4, "<color=orange>FULL CLEAR</color>");
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

        public Room GenerateNewRoom()
        {
            Collider collider = CurrentRoom.GetComponent<Collider>();
            return Instantiate(GetRandomRoom().Prefab, CurrentRoom.gameObject.transform.position + (Vector3.right * collider.bounds.size.x * 2.5f), Quaternion.identity)
                .GetComponent<Room>();
        }

        public RoomData GetRandomRoom()
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
            PointsPerWave += 3 + RoomsEntered / 3;
            SetRoom(GenerateNewRoom());
            Navmesh.BuildNavMesh();
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
                if (!GameStarted)
                {
                    StartGame();
                }
                
                RoomsEntered++;
                room.RoomAlreadyVisited = true;
            }
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
            TimeLeft = StartTime;
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
            
            //if more rooms, or more deliveries
            if (Score.IsLargerThanHighscore(CurrentScore) && !CheatsController.Instance.cheatsEnabled)
            {
                Score.Highscore = CurrentScore;
                EndScreen.Instance.NewBest = true;
            }
            
            EndScreen.Instance.Appear();
            GameProgressSaver.AddMoney(CurrentScore.MoneyGain);
        }
        
        [HarmonyPatch(typeof(SeasonalHats), nameof(SeasonalHats.Start)), HarmonyPrefix]
        private static void EnableHats(SeasonalHats __instance)
        {
            if (AddressableManager.InSceneFromThisMod)
            {
                __instance.easter.SetActive(false);
                __instance.halloween.SetActive(false);
                __instance.christmas.SetActive(true);
            }
        }

        [HarmonyPatch(typeof(NewMovement), nameof(NewMovement.GetHurt)), HarmonyPostfix]
        private static void CustomDeath(NewMovement __instance)
        {
            if (__instance.dead && AddressableManager.InSceneFromThisMod)
            {
                Instance.EndGame();
            }
        }
    }
}