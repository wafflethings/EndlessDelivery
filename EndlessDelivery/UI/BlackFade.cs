using HarmonyLib;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace EndlessDelivery.UI;

[HarmonyPatch]
public class BlackFade : MonoSingleton<BlackFade>
{
    private Image _image;
    private float _timeBeforeFade;

    private void Start()
    {
        _image = GetComponent<Image>();
    }

    private void Update()
    {
        _timeBeforeFade = Mathf.MoveTowards(_timeBeforeFade, 0, Time.deltaTime);
        if (_timeBeforeFade != 0)
        {
            return;
        }

        _image.color = new Color(0, 0, 0, Mathf.MoveTowards(_image.color.a, 0, Time.deltaTime / 1.5f));
    }

    public void Flash(float timeBeforeFade = 0)
    {
        _timeBeforeFade = timeBeforeFade;
        _image.color = Color.black;
    }

    [HarmonyPatch(typeof(CanvasController), nameof(CanvasController.Awake)), HarmonyPostfix]
    private static void CreateSelf(CanvasController __instance)
    {
        Instantiate(Addressables.LoadAssetAsync<GameObject>("Assets/Delivery/Prefabs/HUD/Black Fade.prefab").WaitForCompletion(), __instance.transform);
    }
}
