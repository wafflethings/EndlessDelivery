using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EndlessDelivery.Api.Requests;
using EndlessDelivery.Common.ContentFile;
using EndlessDelivery.Common.Inventory.Items;
using EndlessDelivery.Cosmetics;
using EndlessDelivery.Online;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessDelivery.UI;

public class LoadoutHud : MonoBehaviour
{
    private static readonly Dictionary<StoreItemType, StoreItemType> s_alternateSwapDict = new()
    {
        { StoreItemType.Revolver, StoreItemType.AltRevolver },
        { StoreItemType.Shotgun, StoreItemType.AltShotgun },
        { StoreItemType.Nailgun, StoreItemType.AltNailgun },
        { StoreItemType.AltRevolver, StoreItemType.Revolver },
        { StoreItemType.AltShotgun, StoreItemType.Shotgun },
        { StoreItemType.AltNailgun, StoreItemType.Nailgun },
    };

    [SerializeField] private TMP_Text _itemNameText;
    [SerializeField] private TMP_Text _categoryTitle;
    [SerializeField] private GameObject _page;
    [SerializeField] private GameObject _templateItemButton;
    [SerializeField] private Transform _itemButtonHolder;
    [SerializeField] private GameObject _alternateButton;
    [SerializeField] private Button _equipButton;
    [SerializeField] private TMP_Text _equipButtonText;
    [SerializeField] private LoadoutHudVariantButton[] _variantButtons;
    private List<GameObject> _currentItemButtons = new();
    private Item? _selectedItem;
    private StoreItemType _currentItemType;

    public bool HasNoVariations(StoreItemType itemType) => itemType is StoreItemType.Banner or StoreItemType.Present;

    public bool HasAlternates(StoreItemType itemType) => s_alternateSwapDict.ContainsKey(itemType);

    private void Awake()
    {
        SelectItemType((int)StoreItemType.Revolver);
    }

    private void OnDisable()
    {
        CosmeticManager.UpdateLoadout();
    }

    public void SelectItemType(int itemType)
    {
        _currentItemType = (StoreItemType)itemType;
        StartCoroutine(SetSlotPage());
    }

    public void SwapToAlt()
    {
        if (!HasAlternates(_currentItemType))
        {
            Plugin.Log.LogWarning($"SwapToAlt called with unsupported item {_currentItemType} - shouldn't happen.");
            return;
        }

        SelectItemType((int)s_alternateSwapDict[_currentItemType]);
    }

    private IEnumerator SetSlotPage()
    {
        _page.SetActive(false);

        Task<Cms> cmsTask = OnlineFunctionality.GetContent();
        Task loadoutTask = CosmeticManager.FetchLoadout();
        yield return new WaitUntil(() => cmsTask.IsCompleted && loadoutTask.IsCompleted);
        Cms cms = cmsTask.Result;

        _categoryTitle.text = cms.GetLocalisedString("category." + _currentItemType.ToString().ToLower());

        List<Item> itemList = new();
        foreach (string itemId in CosmeticManager.AllOwned)
        {
            if (!cms.TryGetItem(itemId, out Item item) || item.Descriptor.Type != _currentItemType)
            {
                continue;
            }

            itemList.Add(item);
        }

        foreach (GameObject oldItemButton in _currentItemButtons)
        {
            Destroy(oldItemButton);
        }

        IEnumerable<Item> orderedList =  itemList.OrderBy(x => CosmeticLoadout.DefaultItems.Contains(x.Descriptor.Id) ? string.Empty : cms.GetLocalisedString(x.Descriptor.Name)); // sucks but string.empty makes defaults always first
        foreach (Item item in orderedList)
        {
            AddItem(item);
        }

        string? targetId;

        if (HasNoVariations(_currentItemType))
        {
            targetId = GetEquippedItemId(_currentItemType);
        }
        else
        {
            targetId = GetEquippedItemId(_currentItemType, 0);
        }

        if (targetId != null)
        {
            SetItem(orderedList.FirstOrDefault(item => item.Descriptor.Id == targetId));
        }
        else
        {
            SetItem(null);
        }

        _alternateButton.SetActive(HasAlternates(_currentItemType));

        _page.SetActive(true);
    }

    private void AddItem(Item item)
    {
        GameObject itemButton = Instantiate(_templateItemButton, _itemButtonHolder);
        itemButton.GetComponent<LoadoutHudItemButton>().SetUp(this, item);
        _currentItemButtons.Add(itemButton);
    }

    public void SetItem(Item? item)
    {
        _itemNameText.text = item != null ? OnlineFunctionality.LastFetchedContent.GetLocalisedString(item.Descriptor.Name) : "-";
        _selectedItem = item;
        UpdateEquipButtons(item);
    }

    private void UpdateEquipButtons(Item? item)
    {
        bool oneEquipButton = HasNoVariations(_currentItemType);

        if (oneEquipButton)
        {
            EnableSingleEquipButton(item);
            return;
        }

        EnableVariationEquipButtons(item);
    }

    private void EnableSingleEquipButton(Item? item)
    {
        foreach (LoadoutHudVariantButton variantButton in _variantButtons)
        {
            variantButton.gameObject.SetActive(false);
        }

        _equipButton.gameObject.SetActive(true);

        if (item != null)
        {
            string equippedOrUnequipped = GetEquippedItemId(_currentItemType) == item.Descriptor.Id ? "equipped" : "unequipped";
            _equipButtonText.text = OnlineFunctionality.LastFetchedContent.GetLocalisedString("shop." + equippedOrUnequipped);
            _equipButton.interactable = true;
        }
        else
        {
            _equipButtonText.text = OnlineFunctionality.LastFetchedContent.GetLocalisedString("shop.unequipped");
            _equipButton.interactable = false;
        }
    }

    private void EnableVariationEquipButtons(Item? item)
    {
        _equipButton.gameObject.SetActive(false);

        foreach (LoadoutHudVariantButton variantButton in _variantButtons)
        {
            variantButton.gameObject.SetActive(true);

            if (item != null)
            {
                variantButton.SetState(GetEquippedItemId(_currentItemType, variantButton.VariationIndex) == item.Descriptor.Id);
                variantButton.SetInteractable(true);
            }
            else
            {
                variantButton.SetState(false);
                variantButton.SetInteractable(false);
            }
        }
    }

    public string? GetEquippedItemId(StoreItemType itemType)
    {
        switch (itemType)
        {
            case StoreItemType.Banner:
                return CosmeticManager.Loadout.BannerId;

            case StoreItemType.Present:
                return CosmeticManager.Loadout.PresentId;

            default:
                Plugin.Log.LogWarning($"Item type {_currentItemType} is not supported for GetEquippedItemId(StoreItemType)");
                return null;
        }
    }

    public string? GetEquippedItemId(StoreItemType itemType, int variation)
    {
        List<string>? loadoutSlotList = GetLoadoutSlotList(_currentItemType);

        if (loadoutSlotList == null)
        {
            Plugin.Log.LogWarning($"{_currentItemType} loadoutSlotList was null.");
            return string.Empty;
        }

        if (loadoutSlotList.Count <= variation)
        {
            Plugin.Log.LogWarning($"{variation} out of range of slot {itemType}.");
            return string.Empty;
        }

        return loadoutSlotList[variation];
    }

    public List<string>? GetLoadoutSlotList(StoreItemType itemType)
    {
        switch (itemType)
        {
            case StoreItemType.Revolver:
                return CosmeticManager.Loadout.RevolverIds;

            case StoreItemType.AltRevolver:
                return CosmeticManager.Loadout.AltRevolverIds;

            case StoreItemType.Shotgun:
                return CosmeticManager.Loadout.ShotgunIds;

            case StoreItemType.AltShotgun:
                return CosmeticManager.Loadout.AltShotgunIds;

            case StoreItemType.Nailgun:
                return CosmeticManager.Loadout.NailgunIds;

            case StoreItemType.AltNailgun:
                return CosmeticManager.Loadout.NailgunIds;

            case StoreItemType.Rail:
                return CosmeticManager.Loadout.RailcannonIds;

            case StoreItemType.Rocket:
                return CosmeticManager.Loadout.RocketIds;

            default:
                Plugin.Log.LogWarning($"Item type {_currentItemType} is not supported for GetLoadoutSlotList");
                return null;
        }
    }

    public void EquipForVariation(int variationIndex)
    {
        List<string>? loadoutSlotList = GetLoadoutSlotList(_currentItemType);

        if (loadoutSlotList == null)
        {
            Plugin.Log.LogWarning($"{_currentItemType} loadoutSlotList was null.");
            return;
        }

        while (loadoutSlotList.Count < 3)
        {
            loadoutSlotList.Add(string.Empty);
        }

        loadoutSlotList[variationIndex] = _selectedItem.Descriptor.Id;
        UpdateEquipButtons(_selectedItem);
    }

    public void EquipNoVariation()
    {
        switch (_currentItemType)
        {
            case StoreItemType.Banner:
                CosmeticManager.Loadout.BannerId = _selectedItem.Descriptor.Id;
                break;

            case StoreItemType.Present:
                CosmeticManager.Loadout.PresentId = _selectedItem.Descriptor.Id;
                break;

            default:
                Plugin.Log.LogWarning($"Item type {_currentItemType} is not supported for EquipNoVariation");
                return;
        }

        UpdateEquipButtons(_selectedItem);
    }
}
