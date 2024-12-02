using System.Collections;
using System.Collections.Generic;
using EndlessDelivery.Common;
using EndlessDelivery.Online;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace EndlessDelivery.UI;

[HarmonyPatch]
public class AchievementHud : MonoSingleton<AchievementHud>
{
    private static GameObject? s_asset;

    [SerializeField] private AudioSource? _soundEffect;
    [SerializeField] private GameObject? _holder;
    [SerializeField] private TMP_Text? _name;
    [SerializeField] private TMP_Text? _desc;
    [SerializeField] private Image? _icon;
    private bool _canShowNextAchievement = true;
    private Queue<Achievement> _achievements = new();

    public void AddAchievement(Achievement achievement)
    {
        _achievements.Enqueue(achievement);
    }

    private void Update()
    {
        if (_canShowNextAchievement && _achievements.Count != 0)
        {
            StartCoroutine(ShowCoroutine(_achievements.Dequeue()));
            _canShowNextAchievement = false;
        }
    }

    private IEnumerator ShowCoroutine(Achievement achievement)
    {
        if (_holder == null || _name == null || _desc == null || _icon == null)
        {
            Plugin.Log.LogWarning("AchievementHud has _holder, _name, _desc, or _icon null! Returning.");
            _canShowNextAchievement = true;
            yield break;
        }

        _name.text = OnlineFunctionality.LastFetchedContent?.GetString(achievement.Name) ?? achievement.Name;
        _desc.text = OnlineFunctionality.LastFetchedContent?.GetString(achievement.Description) ?? achievement.Description;

        AsyncOperationHandle<Sprite> spriteLoad = Addressables.LoadAssetAsync<Sprite>(achievement.Icon.AddressablePath);
        yield return new WaitUntil(() => spriteLoad.IsDone);

        _soundEffect?.Play();
        _icon.sprite = spriteLoad.Result;
        _holder.SetActive(true);

        yield return new WaitForSecondsRealtime(3f);
        Vector3 oldScale = _holder.transform.localScale;

        while (!_holder.transform.localScale.x.Equals(0))
        {
            _holder.transform.localScale = Vector3.MoveTowards(_holder.transform.localScale, new Vector3(0, _holder.transform.localScale.y), Time.unscaledDeltaTime * 20);
            yield return null;
        }

        _holder.SetActive(false);
        _holder.transform.localScale = oldScale;
        _canShowNextAchievement = true;
    }

    [HarmonyPatch(typeof(CanvasController), nameof(CanvasController.Awake)), HarmonyPostfix]
    private static void AddSelf(CanvasController __instance)
    {
        s_asset ??= Addressables.LoadAssetAsync<GameObject>("Assets/Delivery/Prefabs/HUD/Achievement UI.prefab").WaitForCompletion();
        Instantiate(s_asset, __instance.transform);
    }
}
