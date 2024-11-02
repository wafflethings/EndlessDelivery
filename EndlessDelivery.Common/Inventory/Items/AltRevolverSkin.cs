namespace EndlessDelivery.Common.Inventory.Items;

public class AltRevolverSkin : Item
{
    public string MaterialPath;

    public AltRevolverSkin(string materialPath, ItemDescriptor descriptor) : base(descriptor)
    {
        MaterialPath = materialPath;
    }

#warning This constructor is only for the serializer, don't use it.
    public AltRevolverSkin()
    {

    }
}
