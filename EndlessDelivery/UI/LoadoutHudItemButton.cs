using System.Collections;
using EndlessDelivery.Common.Inventory.Items;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace EndlessDelivery.UI;

public class LoadoutHudItemButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private Image _iconImage;

    public void SetUp(LoadoutHud loadoutHud, Item item)
    {
        loadoutHud.StartCoroutine(SetUpCoroutine(loadoutHud, item));
    }

    private IEnumerator SetUpCoroutine(LoadoutHud loadoutHud, Item item)
    {
        gameObject.SetActive(false);

        AsyncOperationHandle<Sprite?> iconLoad = Addressables.LoadAssetAsync<Sprite>(item.Descriptor.Icon.AddressablePath);
        yield return new WaitUntil(() => iconLoad.IsDone);

        if (iconLoad.Result == null)
        {
            Plugin.Log.LogWarning($"Couldn't load icon for item {item.Descriptor.Id}. Not showing button");
            yield break;
        }

        _iconImage.sprite = iconLoad.Result;
        _button.onClick.AddListener(() => loadoutHud.SetItem(item));

        gameObject.SetActive(true);
    }
}
