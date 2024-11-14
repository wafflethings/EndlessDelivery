using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using EndlessDelivery.Api.Requests;
using EndlessDelivery.Common.ContentFile;
using EndlessDelivery.Common.Inventory.Items;
using EndlessDelivery.Cosmetics;
using EndlessDelivery.Online;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessDelivery.UI;

public class JollyTerminalShop : MonoBehaviour
{

    [SerializeField] private GameObject _templatePanel;
    [SerializeField] private Transform _panelHolder;
    [SerializeField] private AudioSource _buySound;
    [SerializeField] private TMP_Text _moneyCounter;
    [SerializeField] private AudioSource _moneyDecreaseTick;
    [SerializeField] private float _moneyTickInterval;
    [SerializeField] private float _moneyDecreaseInterval;
    private List<ShopItemPanel> _panels = new();
    private bool _hasInitialized = false;
    private int _totalMoney = 0;
    private int _counterMoney = 0;

    private void OnEnable()
    {
        if (_hasInitialized)
        {
            return;
        }

        StartCoroutine(SetInitialMoneyCoroutine());
        StartCoroutine(RefreshShop());
    }

    private IEnumerator RefreshShop()
    {
        foreach (ShopItemPanel panel in _panels)
        {
            Destroy(panel.gameObject);
        }

        Task<ShopRotation> shopTask = OnlineFunctionality.Context.GetActiveShop();
        Task<Cms> cmsTask = OnlineFunctionality.GetContent();
        yield return new WaitUntil(() => shopTask.IsCompleted && cmsTask.IsCompleted);

        foreach (string itemId in shopTask.Result.ItemIds)
        {
            if (!cmsTask.Result.TryGetItem(itemId, out Item item))
            {
                Plugin.Log.LogWarning($"Shop rotation contains missing item: {item.Descriptor.Id}");
                continue;
            }

            AddItem(item.Descriptor);
        }
    }

    private void AddItem(ItemDescriptor item)
    {
        GameObject panelObject = Instantiate(_templatePanel, _panelHolder);
        panelObject.SetActive(false);
        ShopItemPanel panel = panelObject.GetComponent<ShopItemPanel>();
        panel.SetUp(this, item);
        _panels.Add(panel);
    }

    private void SetCounter(int amount)
    {
        _counterMoney = amount;
        _moneyCounter.text = amount.ToString();
        //todo improve this, makes the layout group refresh to prevent money and text overlap
        _moneyCounter.transform.parent.gameObject.SetActive(false);
        _moneyCounter.transform.parent.gameObject.SetActive(true);
    }

    private IEnumerator SetInitialMoneyCoroutine()
    {
        Task<int> moneyTask = OnlineFunctionality.Context.GetCurrencyAmount();
        yield return new WaitUntil(() => moneyTask.IsCompleted);

        _totalMoney = moneyTask.Result;
        SetCounter(moneyTask.Result);
        _hasInitialized = true;
    }

    public void RefreshMoney(int targetMoney)
    {
        StartCoroutine(RefreshMoneyCoroutine(targetMoney));
    }

    private IEnumerator RefreshMoneyCoroutine(int targetMoney)
    {
        while (!_hasInitialized)
        {
            yield return null;
        }

        float timeSinceTick = 0;
        float timeSinceDecrease = 0;

        while (_counterMoney != targetMoney)
        {
            timeSinceTick += Time.deltaTime;
            timeSinceDecrease += Time.deltaTime;

            if (timeSinceTick > _moneyTickInterval)
            {
                _moneyDecreaseTick.Play();
                timeSinceTick = 0;
            }

            if (timeSinceDecrease > _moneyDecreaseInterval)
            {
                SetCounter(--_counterMoney);
            }

            yield return null;
        }
    }

    public void BuyItem(ItemDescriptor item)
    {
        foreach (ShopItemPanel itemPanel in _panels)
        {
            itemPanel.Disable();
        }

        StartCoroutine(BuyItemCoroutine(item));
    }

    private IEnumerator BuyItemCoroutine(ItemDescriptor item)
    {
        Task buyTask = OnlineFunctionality.Context.BuyItem(item.Id);
        Task loadoutTask = CosmeticManager.FetchLoadout();
        yield return new WaitUntil(() => buyTask.IsCompleted && loadoutTask.IsCompleted);

        Task refreshTask = CosmeticManager.FetchLoadout();
        yield return new WaitUntil(() => refreshTask.IsCompleted);

        _buySound.Play();
        _totalMoney -= item.ShopPrice;
        RefreshMoney(_totalMoney);

        foreach (ShopItemPanel itemPanel in _panels)
        {
            itemPanel.Enable();
        }
    }
}
