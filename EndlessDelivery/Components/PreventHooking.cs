using UnityEngine;

namespace EndlessDelivery.Components
{
    public class PreventItemHooking : MonoBehaviour
    {
        private void Update()
        {
            if (gameObject.layer == 13) //alwaysontop; means it is held, do not change
            {
                return;
            }

            gameObject.layer = HookArm.Instance.state == HookState.Throwing ? 0 : 22;  // no layer if throwing, layer item if not
        }
    }
}