using System.Net;
using EndlessDelivery.Api.Exceptions;
using EndlessDelivery.Common.ContentFile;
using EndlessDelivery.Common.Inventory.Items;
using Newtonsoft.Json;

namespace EndlessDelivery.Api.Requests;

public static class Items
{
    private const string ItemsRoot = "users/items/";
    private const string ActiveShopEndpoint = "active_shop";
    private const string GetLoadoutEndpoint = "get_loadout";
    private const string SetLoadoutEndpoint = "set_loadout";

    public static async Task<ShopRotation> GetActiveShop(this ApiContext context)
    {
        HttpResponseMessage response = await context.Client.GetAsync(context.BaseUri + ItemsRoot + ActiveShopEndpoint);
        string content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<ShopRotation>(content) ?? throw new BadResponseException(content);
    }

    public static async Task<InventoryLoadout> GetLoadout(this ApiContext context)
    {
        HttpRequestMessage request = new(HttpMethod.Get, context.BaseUri + ItemsRoot + GetLoadoutEndpoint);
        await context.EnsureAuth(request);
        HttpResponseMessage response = await context.Client.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();
        InventoryLoadout? deserialized = JsonConvert.DeserializeObject<InventoryLoadout>(content);
        return deserialized ?? throw new BadResponseException(content);
    }

    public static async Task SetLoadout(this ApiContext context, InventoryLoadout loadout)
    {
        HttpRequestMessage request = new(HttpMethod.Post, context.BaseUri + ItemsRoot + SetLoadoutEndpoint);
        await context.EnsureAuth(request);
        request.Content = new StringContent(JsonConvert.SerializeObject(loadout));
        HttpResponseMessage response = await context.Client.SendAsync(request);
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new BadRequestException(response.ReasonPhrase);
        }
    }
}
