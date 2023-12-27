using UnityEngine;

namespace EndlessDelivery.Components
{
    public class PreventItemHooking : MonoBehaviour
    {
        public float EnableWhippingAtDistance = 7;
        
        private void Update()
        {
            if (gameObject.layer == 13) //alwaysontop; means it is held, so we do not change
            {
                return;
            }

            if (Vector3.Distance(transform.position, NewMovement.instance.transform.position) <= EnableWhippingAtDistance)
            {
                gameObject.layer = 22; // layer item
                return;
            }

            gameObject.layer = HookArm.Instance.state == HookState.Throwing ? 0 : 22;  // no layer if throwing, layer item if not
        }
    }
}