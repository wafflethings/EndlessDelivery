namespace EndlessDelivery.Common.Inventory.Items;

public class RevolverSkin : Item
{
    public string MaterialPath;

    public RevolverSkin(string materialPath, ItemDescriptor descriptor) : base(descriptor)
    {
        MaterialPath = materialPath;
    }

#warning This constructor is only for the serializer, don't use it.
    public RevolverSkin()
    {

    }
}
