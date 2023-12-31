using EndlessDelivery.Assets;
using EndlessDelivery.Utils;
using HarmonyLib;
using UnityEngine;

namespace EndlessDelivery.Components
{
    [PatchThis($"{Plugin.GUID}.MauriceFitter")]
    public class MauriceFitter : MonoBehaviour
    {
        private float _startHeight;
        private float _targetHeight;
        private EnemyIdentifier _eid;
        
        private void Start()
        {
            _startHeight = transform.localPosition.y;
            _eid = GetComponent<EnemyIdentifier>();
        }
        private void Update()
        {
            if (_eid.dead)
            {
                return;
            }
            
            if (Physics.Raycast(transform.parent.position, Vector3.up, out RaycastHit hit, 20, LayerMaskDefaults.Get(LMD.Environment)))
            {
                _targetHeight = hit.distance - 1;
            }
            else
            {
                _targetHeight = _startHeight;
            }
            
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, 
                transform.localPosition.Only(Axis.X, Axis.Z) + Vector3.up * _targetHeight, Time.deltaTime * 5);
        }

        [HarmonyPatch(typeof(SpiderBody), nameof(SpiderBody.Start)), HarmonyPostfix]
        private static void AddMauriceFitter(SpiderBody __instance)
        {
            if (!AddressableManager.InSceneFromThisMod)
            {
                return;
            }

            __instance.gameObject.AddComponent<MauriceFitter>();
        }
    }
}