using System;

namespace EndlessDelivery.Common.Inventory.Items.WeaponSkins;

public abstract class WeaponSkin : Item
{
    public string MaterialPath;

    public WeaponSkin(string materialPath, ItemDescriptor descriptor) : base(descriptor)
    {
        MaterialPath = materialPath;
    }

    [Obsolete("This method should only be used by the serializer")]
    public WeaponSkin()
    {

    }
}
