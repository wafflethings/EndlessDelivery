namespace EndlessDelivery.Common.Inventory.Items;

public abstract class Item
{
    public ItemDescriptor Descriptor;

    public Item(ItemDescriptor descriptor)
    {
            Descriptor = descriptor;
        }

    protected Item()
    {

        }
}