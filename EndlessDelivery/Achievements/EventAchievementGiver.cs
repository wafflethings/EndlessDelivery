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
        AchievementHud.Instance.AddAchievement(OnlineFunctionality.LastFetchedContent.Achievements[id]);
        Task.Run(() => OnlineFunctionality.Context.GrantAchievement(id));
    }
}
