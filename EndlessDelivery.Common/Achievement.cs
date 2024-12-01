using EndlessDelivery.Common.Inventory.Items;

namespace EndlessDelivery.Common;

public class Achievement
{
    public string Name;
    public string Id;
    public string Description;
    public ClientWebPair Icon;
    public int OrderPriority;
    public string[] ItemGrants;
    public bool HideDetails;
    public bool Serverside;
}
