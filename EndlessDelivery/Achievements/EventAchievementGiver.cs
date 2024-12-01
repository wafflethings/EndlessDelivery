using System.Threading.Tasks;
using EndlessDelivery.Api.Requests;
using EndlessDelivery.Online;
using EndlessDelivery.UI;
using UnityEngine;

namespace EndlessDelivery.Achievements;

public class EventAchievementGiver : MonoBehaviour
{
    public void Give(string id)
    {
        AchievementManager.ShowAndGiveLocal(id);
    }
}
