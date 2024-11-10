namespace EndlessDelivery.Common.Inventory.Items;

public class PresentSkin : Item
{
    public string ColouredMaterialPath;
    public string BowMaterialPath;
    public string MeshPath;

    public PresentSkin(string colouredMaterialPath, string bowMaterialPath, string meshPath, ItemDescriptor descriptor) : base(descriptor)
    {
        ColouredMaterialPath = colouredMaterialPath;
        BowMaterialPath = bowMaterialPath;
        MeshPath = meshPath;
    }

#warning This constructor is only for the serializer, don't use it.
    public PresentSkin()
    {

    }
}
