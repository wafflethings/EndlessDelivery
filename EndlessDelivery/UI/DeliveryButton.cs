using System.Collections;
using System.Threading.Tasks;
using AtlasLib.Utils;
using EndlessDelivery.Api.Requests;
using EndlessDelivery.Assets;
using EndlessDelivery.Online;
using HarmonyLib;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace EndlessDelivery.UI;

[HarmonyPatch]
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

        GameObject jollyButton = Object.Instantiate(Addressables.LoadAssetAsync<GameObject>("Assets/Delivery/Prefabs/HUD/Jolly Chapter Select Button.prefab").WaitForCompletion(), chapterSelect.transform);
        jollyButton.GetComponent<Button>().onClick.AddListener(() => CoroutineRunner.Instance.StartCoroutine(LoadWhenLogged()));
    }

    private static IEnumerator LoadWhenLogged()
    {
        Task<bool> isOnlineTask = OnlineFunctionality.Context.ServerOnline();
        yield return new WaitUntil(() => isOnlineTask.IsCompleted);
        yield return new WaitUntil(() => OnlineFunctionality.LoggedIn || !isOnlineTask.Result);
        AssetManager.LoadSceneUnsanitzed("Assets/Delivery/Scenes/Game Scene.unity");
    }
}
