using AtlasLib.Utils;
using EndlessDelivery.Assets;
using EndlessDelivery.Utils;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessDelivery.UI
{
    [PatchThis($"{Plugin.GUID}.DeliveryButton")]
    public class DeliveryButton
    {
        [HarmonyPatch(typeof(CanvasController), nameof(CanvasController.Awake)), HarmonyPostfix]
        private static void AddButton(CanvasController __instance)
        {
            GameObject chapterSelect = __instance.gameObject.GetChild("Chapter Select");

            if (chapterSelect == null)
            {
                return;
            }

            GameObject cgButton = chapterSelect.GetChild("The Cyber Grind");
            RectTransform cgButtonTransform = cgButton.GetComponent<RectTransform>();
            cgButtonTransform.sizeDelta -= new Vector2(55, 0);
            cgButtonTransform.position -= new Vector3(55f / 2, 0, 0);
            
            GameObject jollyButton = Object.Instantiate(AddressableManager.Load<GameObject>("Assets/Delivery/Prefabs/HUD/Jolly Chapter Select Button.prefab"), 
                chapterSelect.transform);
            jollyButton.GetComponent<Button>().onClick.AddListener(() => AddressableManager.LoadScene("Assets/Delivery/Scenes/Test Scene.unity"));
        }
    }
}