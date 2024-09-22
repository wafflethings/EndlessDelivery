namespace EndlessDelivery.Common.Inventory.Items;

public class Banner : Item
{
    public string AssetUri;

    public Banner(string assetUri, ItemDescriptor descriptor) : base(descriptor)
    {
            AssetUri = assetUri;
        }

#warning This constructor is only for the serializer, don't use it.
    public Banner()
    {

        }
}