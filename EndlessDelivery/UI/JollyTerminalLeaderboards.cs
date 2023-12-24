using EndlessDelivery.Scores.Server;
using UnityEngine;

namespace EndlessDelivery.UI
{
    public class JollyTerminalLeaderboards : MonoBehaviour
    {
        private int _page = 0;
        private int _pageAmount;
        
        public void Start()
        {
            SetStuff();
        }

        private async void SetStuff()
        {
            _pageAmount = Mathf.CeilToInt(await Endpoints.GetScoreAmount() / 5f);
        }
        
        public void OnEnable()
        {
            
        }
        
        public 
    }
}