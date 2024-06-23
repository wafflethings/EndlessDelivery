namespace EndlessDelivery.Server.Resources;

public class ResourcePair
{
    public string[] Urls;
    public string Location;
    public string MimeType;
    private byte[]? _data;

    public ResourcePair(string url, string location, string mimeType = "application/octet-stream")
    {
        Urls = [url];
        Location = location;
        MimeType = mimeType;
    }

    public ResourcePair(string[] urls, string location, string mimeType = "application/octet-stream")
    {
        Urls = urls;
        Location = location;
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
