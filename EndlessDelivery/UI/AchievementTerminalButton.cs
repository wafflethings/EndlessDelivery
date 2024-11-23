using System.Collections;
using EndlessDelivery.Common;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace EndlessDelivery.UI;

public class AchievementTerminalButton : MonoBehaviour
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private Button _button;
    [SerializeField] private Image _buttonImage;
    [SerializeField] private Color _buttonAchievedColour;
    [SerializeField] private Color _buttonNotAchievedColour;
    private AchievementTerminal _parentTerminal;

    public void SetUp(AchievementTerminal parentTerminal, Achievement achievement, bool isOwned)
    {
        _parentTerminal = parentTerminal;
        parentTerminal.StartCoroutine(SetUpCoroutine(achievement, isOwned));
    }

    private IEnumerator SetUpCoroutine(Achievement achievement, bool isOwned)
    {
        gameObject.SetActive(false);
        AsyncOperationHandle<Sprite> spriteLoad = Addressables.LoadAssetAsync<Sprite>(achievement.Icon.AddressablePath);
        yield return new WaitUntil(() => spriteLoad.IsDone);
        _iconImage.sprite = spriteLoad.Result;
        _buttonImage.color = isOwned ? _buttonAchievedColour : _buttonNotAchievedColour;
        _button.onClick.AddListener(() => _parentTerminal.SetAchievement(achievement));
        gameObject.SetActive(true);
    }
}
