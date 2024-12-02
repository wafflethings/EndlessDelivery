using System;
using EndlessDelivery.Common.Inventory.Items;

namespace EndlessDelivery.Common.ContentFile;

public class CalendarReward
{
    public bool IsLastDay;
    public DateTime Date;
    public string Name;
    public string Description;
    public string Subtitle;
    public ClientWebPair Icon;
    public string Id;
    public bool HasItem;
    public string ItemId;
    public bool HasCurrency;
    public int CurrencyAmount;
}
