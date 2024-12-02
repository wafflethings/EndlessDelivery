namespace EndlessDelivery.Common.Inventory.Items;

public class Banner : Item
{
    public ClientWebPair Asset;

    public Banner(ClientWebPair asset, ItemDescriptor descriptor) : base(descriptor)
    {
        Asset = asset;
    }

#warning This constructor is only for the serializer, don't use it.
    public Banner()
    {

    }
}
