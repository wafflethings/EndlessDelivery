namespace EndlessDelivery.Server.Api;

public static class ServerUtils
{
    public static async Task<string> ReadBody(this HttpRequest request)
    {
        request.EnableBuffering();
        request.Body.Seek(0, SeekOrigin.Begin);
        using StreamReader reader = new(request.Body);
        return await reader.ReadToEndAsync();
    }
}
