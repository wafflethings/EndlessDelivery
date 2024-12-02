using System;
using System.Globalization;
using EndlessDelivery.Gameplay;
using EndlessDelivery.Utils;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace EndlessDelivery.UI;

[HarmonyPatch]
public class PresentTimeHud : MonoSingleton<PresentTimeHud>
{
    public const float DangerTime = 10;

    public Image[] PresentImages;
    public TMP_Text Timer;
    public TMP_Text CompleteRooms;
    public Color TimerColour;
    public Color TimerDangerColour;

    private float _lastSetSeconds;
    private float _lerpProgress;
    private float _originalFontSize;
    private Vector3 _timerStartPos;

    public void Start()
    {
        _originalFontSize = Timer.fontSize;
        _timerStartPos = Timer.transform.localPosition;
    }

    private void Update()
    {
        if (!GameManager.Instance.GameStarted)
        {
            return;
        }

        if (GameManager.Instance.TimerActive)
        {
            Timer.text = TimeSpan.FromSeconds(GameManager.Instance.TimeLeft).ToString(@"mm\:ss\.fff", new CultureInfo("en-US"));
            Timer.fontSize = Mathf.MoveTowards(Timer.fontSize, _originalFontSize, 75 * Time.deltaTime);
            _lastSetSeconds = GameManager.Instance.TimeLeft;
            _lerpProgress = 0;
        }
        else
        {
            if (GameManager.Instance.TimeLeft > DangerTime)
            {
                CameraController.Instance.StopShake();
            }

            float seconds = Mathf.Lerp(_lastSetSeconds, GameManager.Instance.TimeLeft, _lerpProgress);
            Timer.text = TimeSpan.FromSeconds(seconds).Formatted();
            Timer.fontSize = _originalFontSize * (1 + (_lerpProgress * 0.15f));
            _lerpProgress += Time.deltaTime / GameManager.TimeAddLength;
        }

        if (GameManager.Instance.TimeLeft <= DangerTime)
        {
            Timer.color = TimerDangerColour;
            Timer.transform.localPosition = _timerStartPos + new Vector3(UnityEngine.Random.Range(-1, 1), UnityEngine.Random.Range(-1, 1), 0);
            CameraController.Instance.CameraShake((DangerTime - GameManager.Instance.TimeLeft) / (DangerTime * 5));
        }
        else
        {
            Timer.color = TimerColour;
            Timer.transform.localPosition = _timerStartPos;
        }

        if (GameManager.Instance.CurrentRoom != null)
        {
            for (int i = 0; i < 4; i++)
            {
                PresentImages[i].fillAmount = FindFill((WeaponVariant)i);
            }
        }

        CompleteRooms.text = (GameManager.Instance.RoomsComplete).ToString();
    }

    //instead of filling the coloured area, the dark area is coloured.
    //this means that fill 0 means all presents, fill 1 means no presents.
    private float FindFill(WeaponVariant colour)
    {
        float max = GameManager.Instance.CurrentRoom.PresentColourAmounts[(int)colour];
        return 1 - (GameManager.Instance.CurrentRoom.AmountDelivered[colour] / max);
    }

    [HarmonyPatch(typeof(HudController), nameof(HudController.Start)), HarmonyPostfix]
    private static void AddSelf(HudController __instance)
    {
        if (!__instance.altHud && !MapInfoBase.InstanceAnyType.hideStockHUD)
        {
            GameObject presentPanel = Addressables.LoadAssetAsync<GameObject>("Assets/Delivery/Prefabs/HUD/Present Panel.prefab").WaitForCompletion();
            GameObject hud = Instantiate(presentPanel, __instance.hudpos.transform);
            hud.transform.localPosition = presentPanel.transform.localPosition; //i have no idea why it instantiates at the wrong place but wtv
            // PlayerActivatorRelay.Instance.toActivate = PlayerActivatorRelay.Instance.toActivate.AddToArray(hud);

            hud.gameObject.SetActive(false);
        }
    }
}
