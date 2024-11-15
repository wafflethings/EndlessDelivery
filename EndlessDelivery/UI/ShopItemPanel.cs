using System.Collections;
using System.Threading.Tasks;
using EndlessDelivery.Common.ContentFile;
using EndlessDelivery.Common.Inventory.Items;
using EndlessDelivery.Cosmetics;
using EndlessDelivery.Online;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace EndlessDelivery.UI;

public class ShopItemPanel : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _itemName;
    [SerializeField] private TMP_Text _itemCost;
    [SerializeField] private Image _iconImage;
    [SerializeField] private Image _moneyIcon;
    private JollyTerminalShop _parentShop;
    private ItemDescriptor _itemDescriptor;
    private string _ownedString;

    public void SetUp(JollyTerminalShop parentShop, ItemDescriptor itemDescriptor)
    {
        gameObject.SetActive(false);
        _itemDescriptor = itemDescriptor;
        _parentShop = parentShop;
        parentShop.StartCoroutine(SetUpCoroutine());
    }

    private IEnumerator SetUpCoroutine()
    {
        Task<Cms> cmsTask = OnlineFunctionality.GetContent();
        yield return new WaitUntil(() => cmsTask.IsCompleted);
        Cms cms = cmsTask.Result;

        _ownedString = cms.GetLocalisedString("shop.owned");
        _itemName.text = cms.GetLocalisedString(_itemDescriptor.Name);
        _itemCost.text = _itemDescriptor.ShopPrice.ToString();

        //todo improve this, makes the layout group refresh to prevent money and text overlap
        _itemCost.transform.parent.gameObject.SetActive(false);
        yield return null;
        _itemCost.transform.parent.gameObject.SetActive(true);

        if (CosmeticManager.AllOwned.Contains(_itemDescriptor.Id))
        {
            SetOwned();
        }

        AsyncOperationHandle<Sprite> iconCoroutine = Addressables.LoadAssetAsync<Sprite>(_itemDescriptor.Icon.AddressablePath);
        yield return new WaitUntil(() => iconCoroutine.IsDone);
        _iconImage.sprite = iconCoroutine.Result;

        gameObject.SetActive(true);
    }

    public void Buy()
    {
        _parentShop.BuyItem(_itemDescriptor);
    }

    public void Enable()
    {
        if (CosmeticManager.AllOwned.Contains(_itemDescriptor.Id))
        {
            SetOwned();
            return;
        }

        _button.interactable = true;
    }

    public void Disable()
    {
        _button.interactable = false;
    }

    private void SetOwned()
    {
        _itemCost.text = _ownedString;
        Disable();
        _moneyIcon.gameObject.SetActive(false);
    }
}
