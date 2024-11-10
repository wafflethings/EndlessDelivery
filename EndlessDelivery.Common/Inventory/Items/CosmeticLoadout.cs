using System.Collections.Generic;

namespace EndlessDelivery.Common.Inventory.Items;

public class CosmeticLoadout
{
    public static CosmeticLoadout Default => new()
    {
        BannerId = "banner_default",
        PresentId = string.Empty
    };

    public string BannerId;
    public string PresentId;
    public List<string> RevolverIds = new();
    public List<string> ShotgunIds = new();
    public List<string> NailgunIds = new();
    public List<string> RailcannonIds = new();
    public List<string> RocketIds = new();
}
