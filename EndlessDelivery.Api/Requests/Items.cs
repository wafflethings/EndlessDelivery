using System.Net;
using EndlessDelivery.Api.Exceptions;
using EndlessDelivery.Common.ContentFile;
using EndlessDelivery.Common.Inventory.Items;
using Newtonsoft.Json;

namespace EndlessDelivery.Api.Requests;

public static class Items
{
    private const string ItemsRoot = "users/items/";
    private const string BuyItemEndpoint = "buy_item";
    private const string ActiveShopEndpoint = "active_shop";
    private const string GetLoadoutEndpoint = "get_loadout?steamId={0}";
    private const string SetLoadoutEndpoint = "set_loadout";
    private const string GetInventoryEndpoint = "get_inventory?steamId={0}";

    public static async Task BuyItem(this ApiContext context, string itemId)
    {
        HttpRequestMessage request = new(HttpMethod.Post, context.BaseUri + ItemsRoot + BuyItemEndpoint);
        await context.AddAuth(request);
        request.Content = new StringContent(itemId);
        HttpResponseMessage response = await context.Client.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new BadRequestException(response.ReasonPhrase);
        }
    }

    public static async Task<ShopRotation> GetActiveShop(this ApiContext context)
    {
        HttpResponseMessage response = await context.Client.GetAsync(context.BaseUri + ItemsRoot + ActiveShopEndpoint);
        string content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<ShopRotation>(content) ?? throw new BadResponseException(content);
    }

    public static async Task<CosmeticLoadout> GetLoadout(this ApiContext context, ulong steamId)
    {
        HttpResponseMessage response = await context.Client.GetAsync(context.BaseUri + ItemsRoot + string.Format(GetLoadoutEndpoint, steamId));
        string content = await response.Content.ReadAsStringAsync();
        CosmeticLoadout? deserialized = JsonConvert.DeserializeObject<CosmeticLoadout>(content);
        return deserialized ?? throw new BadResponseException(content);
    }

    public static async Task SetLoadout(this ApiContext context, CosmeticLoadout loadout)
    {
        HttpRequestMessage request = new(HttpMethod.Post, context.BaseUri + ItemsRoot + SetLoadoutEndpoint);
        await context.AddAuth(request);
        request.Content = new StringContent(JsonConvert.SerializeObject(loadout));
        HttpResponseMessage response = await context.Client.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new BadRequestException(response.ReasonPhrase);
        }
    }

    public static async Task<List<string>> GetInventory(this ApiContext context, ulong steamId)
    {
        HttpResponseMessage response = await context.Client.GetAsync(context.BaseUri + ItemsRoot + string.Format(GetInventoryEndpoint, steamId));
        string content = await response.Content.ReadAsStringAsync();

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new BadRequestException(response.ReasonPhrase);
        }

        if (response.StatusCode == HttpStatusCode.InternalServerError)
        {
            throw new InternalServerException();
        }

        List<string>? deserialized = JsonConvert.DeserializeObject<List<string>>(content);

        if (deserialized == null)
        {
            throw new BadResponseException(content);
        }

        return deserialized;
    }
}
