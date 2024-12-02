using System.Threading.Tasks;
using EndlessDelivery.Api.Requests;
using EndlessDelivery.Gameplay;
using EndlessDelivery.Online;
using EndlessDelivery.UI;
using UnityEngine;

namespace EndlessDelivery.Achievements;

public class QuickClearChecker : MonoBehaviour
{
    private const float TimeAmount = 4;
    private const string AchId = "ach_quick_clear";
    private float _enteredTime;

    private void Awake()
    {
        GameManager.Instance.RoomStarted += _ => _enteredTime = Time.time;
        GameManager.Instance.RoomCleared += _ =>
        {
            if (Time.time - _enteredTime < TimeAmount)
            {
                AchievementManager.ShowAndGiveLocal(AchId);
            }
        };
    }
}
