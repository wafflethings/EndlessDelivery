namespace EndlessDelivery.Common.Inventory.Items;

public class ShotgunSkin : Item
{
    public string MaterialPath;
    public string SawMaterialPath;

    public ShotgunSkin(string materialPath, string sawMaterialPath, ItemDescriptor descriptor) : base(descriptor)
    {
        MaterialPath = materialPath;
        SawMaterialPath = sawMaterialPath;
    }

#warning This constructor is only for the serializer, don't use it.
    public ShotgunSkin()
    {

    }
}
