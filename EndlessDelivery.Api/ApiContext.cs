using EndlessDelivery.Api.Exceptions;
using EndlessDelivery.Api.Requests;

namespace EndlessDelivery.Api;

public class ApiContext
{
    private const string ProdUrl = "https://delivery.wafflethings.dev/api/";
    public readonly Uri BaseUri;
    public readonly HttpClient Client;
    public string? Token;
    private readonly Func<string> _getTicket;

    public ApiContext(HttpClient client, Func<string> getTicket, Uri? baseUri = null)
    {
        Client = client;
        _getTicket = getTicket;

        if (baseUri != null && !baseUri.AbsoluteUri.EndsWith("/"))
        {
            baseUri = new(baseUri.OriginalString + "/");
        }

        BaseUri = baseUri == null ? new Uri(ProdUrl) : baseUri;
    }

    public async Task Login()
    {
        Token = await this.GetToken(_getTicket());
    }

    public async Task AddAuth(HttpRequestMessage request)
    {
        try
        {
            if (Token == null)
            {
                throw new PermissionException("You must be logged in.");
            }
            request.Headers.Add("DeliveryToken", Token);
        }
        catch (InternalServerException ex)
        {
            await Task.FromException(ex);
        }
    }
}
