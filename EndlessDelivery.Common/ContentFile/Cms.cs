using System;
using System.Collections.Generic;
using EndlessDelivery.Common.Inventory.Items;
using Newtonsoft.Json;

namespace EndlessDelivery.Common.ContentFile;

public class Cms
{
    public Dictionary<string, Achievement> Achievements = new();
    public Dictionary<string, Banner> Banners = new();
    public Dictionary<string, RevolverSkin> Revolvers = new();
    public Dictionary<string, AltRevolverSkin> AltRevolvers = new();
    public Dictionary<string, ShotgunSkin> Shotguns = new();
    // public Dictionary<string, AltShotgunSkin> AltShotguns = new();
    // public Dictionary<string, NailgunSkin> Nailguns = new();
    // public Dictionary<string, AltNailgunSkin> AltNailguns = new();
    // public Dictionary<string, RailcannonSkin> Railcannons = new();
    // public Dictionary<string, RocketSkin> Rockets = new();
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

        if (Revolvers.TryGetValue(id, out RevolverSkin revolverSkin))
        {
            item = revolverSkin;
            return true;
        }

        item = null;
        return false;
    }

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
