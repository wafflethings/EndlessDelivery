using System.Threading.Tasks;
using EndlessDelivery.Api.Requests;
using EndlessDelivery.Online;
using EndlessDelivery.ScoreManagement;
using EndlessDelivery.UI;
using UnityEngine;

namespace EndlessDelivery.Achievements;

public class EventAchievementGiver : MonoBehaviour
{
    public void Give(string id)
    {
        if (!ScoreManager.CanSubmit())
        {
            return;
        }

        AchievementManager.ShowAndGiveLocal(id);
    }
}
