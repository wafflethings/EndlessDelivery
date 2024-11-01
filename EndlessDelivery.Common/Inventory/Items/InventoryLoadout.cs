using System.Collections.Generic;

namespace EndlessDelivery.Common.Inventory.Items;

public class InventoryLoadout
{
    public static InventoryLoadout Default => new()
    {
        BannerId = "banner_default"
    };

    public string BannerId;
    public List<string> RevolverIds = new();
    public List<string> AltRevolverIds = new();
    public List<string> ShotgunIds = new();
    public List<string> AltShotgunIds = new();
    public List<string> NailgunIds = new();
    public List<string> AltNailgunIds = new();
    public List<string> RailcannonIds = new();
    public List<string> RocketIds = new();
}
