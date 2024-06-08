namespace EndlessDelivery.Common.Inventory.Items
{
    public class ItemDescriptor
    {
        public ItemType Type;
        public string Id;
        public string Name;
        public int ShopPrice;
        public string PreviewUri;

        public ItemDescriptor(string id, string name, string previewUri, ItemType type, int shopPrice = 0)
        {
            Id = id;
            Name = name;
            PreviewUri = previewUri;
            Type = type;
            ShopPrice = shopPrice;
        }

        #warning This constructor is only for the serializer, don't use it.
        public ItemDescriptor()
        {

        }
    }
}
