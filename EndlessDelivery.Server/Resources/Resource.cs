namespace EndlessDelivery.Server.Resources;

public class Resource
{
    public readonly string Location;
    public readonly string UrlLocation;
    public readonly string MimeType;
    private byte[]? _data;

    public Resource(string location, string mimeType = "application/octet-stream")
    {
        Location = location;
        UrlLocation = location.Replace('\\', '/').Substring(6, location.Length - 6); // trims Assets
        MimeType = mimeType;
    }

    public Resource(string location, string urlLocation, string mimeType = "application/octet-stream")
    {
        Location = location;
        UrlLocation = urlLocation;
        MimeType = mimeType;
    }

    public async Task<byte[]> GetData()
    {
        if (_data == null || _data.Length != 0)
        {
            _data = await File.ReadAllBytesAsync(Location);
        }

        return _data;
    }

    public void Reset()
    {
        _data = null;
    }
}
