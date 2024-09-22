using System;
using System.Collections.Generic;
using EndlessDelivery.Common.Inventory.Items;
using Newtonsoft.Json;

namespace EndlessDelivery.Common.ContentFile;

public class Cms
{
    public Dictionary<string, Banner> Banners = new();
    public List<ShopRotation> ShopRotations = new();
    public Dictionary<string, string> LocalisedStrings = new();
    public DateTime LastUpdate;

    [JsonIgnore] private List<ShopRotation> _remainingRotations = new();

    public Banner GetBanner(string id) => Banners[id];

    public string GetLocalisedString(string id) => LocalisedStrings.ContainsKey(id) ? LocalisedStrings[id] : id;

    public bool TryGetItem(string id, out Item item)
    {
            if (Banners.TryGetValue(id, out Banner banner))
            {
                item = banner;
                return true;
            }

            item = null;
            return false;
        }

    public void SetValues()
    {
            _remainingRotations.AddRange(ShopRotations);
        }

    public ShopRotation GetActiveShopRotation()
    {
            for (int i = 0; i < _remainingRotations.Count; i++)
            {
                ShopRotation rotation = _remainingRotations[i];

                if (rotation.End < DateTime.UtcNow)
                {
                    _remainingRotations.Remove(rotation);
                    continue;
                }

                if (rotation.Start <= DateTime.UtcNow)
                {
                    return rotation;
                }
            }

            return null;
        }
}