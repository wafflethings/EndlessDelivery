namespace EndlessDelivery.Common.Inventory.Items;

public class ItemDescriptor
{
    public ItemType Type;
    public string Id;
    public string Name;
    public int ShopPrice;
    public ClientWebPair Icon;

    public ItemDescriptor(string id, string name, ClientWebPair icon, ItemType type, int shopPrice = 0)
    {
        Id = id;
        Name = name;
        Icon = icon;
        Type = type;
        ShopPrice = shopPrice;
    }

#warning This constructor is only for the serializer, don't use it.
    public ItemDescriptor()
    {

    }
}
