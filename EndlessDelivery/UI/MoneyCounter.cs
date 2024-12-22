using System.Collections;
using System.Threading.Tasks;
using EndlessDelivery.Api.Requests;
using EndlessDelivery.Online;
using TMPro;
using UnityEngine;

namespace EndlessDelivery.UI;

public class MoneyCounter : MonoBehaviour
{
    [SerializeField] private AudioSource _moneyDecreaseTick;
    [SerializeField] private float _moneyTickInterval;
    [SerializeField] private float _moneyDecreaseInterval;
    [SerializeField] private TMP_Text _moneyCounter;
    private Coroutine? _lastMoneyRefresher;
    private int _counterMoney = 0;

    public void SetValue(int amount)
    {
        _counterMoney = amount;
        _moneyCounter.text = amount.ToString();
    }

    public void RefreshMoney(int targetMoney)
    {
        if (_lastMoneyRefresher != null)
        {
            StopCoroutine(_lastMoneyRefresher);
        }

        _lastMoneyRefresher = StartCoroutine(RefreshMoneyCoroutine(targetMoney));
    }

    private IEnumerator RefreshMoneyCoroutine(int targetMoney)
    {
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
                SetValue(--_counterMoney);
            }

            yield return null;
        }
    }
}
