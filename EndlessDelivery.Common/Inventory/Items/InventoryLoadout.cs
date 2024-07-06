using EndlessDelivery.Common.Inventory.Items;

namespace EndlessDelivery.Common.Inventory.Items;

public class InventoryLoadout
{
    public static InventoryLoadout Default => new()
    {
        BannerId = "banner_default"
    };

    public string BannerId;
}