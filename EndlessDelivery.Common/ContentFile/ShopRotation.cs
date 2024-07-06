using System;
using System.Collections.Generic;
using EndlessDelivery.Common.Inventory.Items;
using Newtonsoft.Json;

namespace EndlessDelivery.Common.ContentFile;

public class ShopRotation
{
    public List<string> ItemIds;
    public DateTime Start;
    public TimeSpan Length;
    [JsonIgnore] public DateTime End => Start + Length;
}
