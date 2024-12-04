using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using EndlessDelivery.Common.Inventory.Items;
using Newtonsoft.Json;

namespace EndlessDelivery.Common.ContentFile;

public class Cms
{
    public List<DatedRoomPool> RoomPools = new();
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
    public Dictionary<string, string> Strings = new();
    public Dictionary<string, string> BannedMods = new();

    [JsonIgnore]
    public string Hash
    {
        get
        {
            using MD5 md5 = MD5.Create();
            string jsonContent = JsonConvert.SerializeObject(this, Formatting.None);
            byte[] bytes = Encoding.UTF8.GetBytes(jsonContent);
            byte[] hash = md5.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }

    [JsonIgnore] public CalendarReward CurrentCalendarReward => CalendarRewards.FirstOrDefault(x => x.Value.Date == DateTime.UtcNow.Date).Value;

    [JsonIgnore]
    public ShopRotation ActiveShopRotation
    {
        get
        {
            foreach (ShopRotation rotation in ShopRotations)
            {
                if (rotation.Start > DateTime.UtcNow || (rotation.Start + rotation.Length) < DateTime.UtcNow)
                {
                    continue;
                }

                return rotation;
            }

            return null;
        }
    }

    [JsonIgnore]
    public DatedRoomPool? CurrentRoomPool
    {
        get
        {
            DatedRoomPool? selectedPool = null;

            foreach (DatedRoomPool roomPool in RoomPools)
            {
                if (selectedPool == null || (roomPool.After > selectedPool.After) && roomPool.After <= DateTime.UtcNow)
                {
                    selectedPool = roomPool;
                }
            }

            return selectedPool;
        }
    }

    public string GetString(string id) => Strings.ContainsKey(id) ? Strings[id] : id;

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
}
