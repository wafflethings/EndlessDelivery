namespace EndlessDelivery.Common.Inventory.Items.WeaponSkins;

public class AltNailgunSkin : WeaponSkin
{
    public string SawPath;

    public AltNailgunSkin(string materialPath, string sawPath, ItemDescriptor descriptor) : base(materialPath, descriptor)
    {
        SawPath = sawPath;
    }
}
