using EndlessDelivery.Common.Inventory.Items;

namespace EndlessDelivery.Common;

public class Achievement
{
    public string Name;
    public string Id;
    public string Description;
    public ClientWebPair Icon;
    public string[] ItemGrants;
    public bool HideDetails;
    public bool Serverside;
}
