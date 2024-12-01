using System;
using System.Collections.Generic;
using System.Linq;
using EndlessDelivery.Common.Inventory.Items;
using Newtonsoft.Json;

namespace EndlessDelivery.Common.ContentFile;

public class Cms
{
    public Dictionary<string, CalendarReward> CalendarRewards = new();
    public Dictionary<string, Achievement> Achievements = new();
    public Dictionary<string, Banner> Banners = new();
    public Dictionary<string, WeaponSkinItem> Revolvers = new();
    public Dictionary<string, WeaponSkinItem> AltRevolvers = new();
    public Dictionary<string, WeaponSkinItem> Shotguns = new();
    public Dictionary<string, WeaponSkinItem> AltShotguns = new();
    public Dictionary<string, WeaponSkinItem> Nailguns = new();
    public Dictionary<string, WeaponSkinItem> AltNailguns = new();
    public Dictionary<string, WeaponSkinItem> Railcannons = new();
    public Dictionary<string, WeaponSkinItem> Rockets = new();
    public Dictionary<string, WeaponSkinItem> Presents = new();
    public List<ShopRotation> ShopRotations = new();
    public Dictionary<string, string> LocalisedStrings = new();
    public DateTime LastUpdate;

    public string GetLocalisedString(string id) => LocalisedStrings.ContainsKey(id) ? LocalisedStrings[id] : id;

    public bool TryGetItem(string id, out Item item)
    {
        if (Banners.TryGetValue(id, out Banner banner))
        {
            item = banner;
            return true;
        }

        Dictionary<string, WeaponSkinItem>[] weaponSkinDicts = [Revolvers, AltRevolvers, Shotguns, AltShotguns, Nailguns, AltNailguns, Railcannons, Rockets, Presents];

        foreach (Dictionary<string, WeaponSkinItem> weaponSkinDict in weaponSkinDicts)
        {
            if (weaponSkinDict.TryGetValue(id, out WeaponSkinItem skin))
            {
                item = skin;
                return true;
            }
        }

        item = null;
        return false;
    }

    [JsonIgnore] public CalendarReward CurrentCalendarReward => CalendarRewards.FirstOrDefault(x => x.Value.Date == DateTime.UtcNow.Date).Value;

    public ShopRotation GetActiveShopRotation()
    {
        List<ShopRotation> allRotations = new();
        allRotations.AddRange(ShopRotations);

        for (int i = 0; i < allRotations.Count; i++)
        {
            ShopRotation rotation = allRotations[i];

            if (rotation.End < DateTime.UtcNow)
            {
                allRotations.Remove(rotation);
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
