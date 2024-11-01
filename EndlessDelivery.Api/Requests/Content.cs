using System.Net;
using EndlessDelivery.Api.Exceptions;
using EndlessDelivery.Common.ContentFile;
using Newtonsoft.Json;

namespace EndlessDelivery.Api.Requests;

public static class Content
{
    private const string CmsRoot = "cms/";
    private const string UpdateRequiredEndpoint = "update_required?lastUpdate={0}";
    private const string DownloadCmsEndpoint = "content";

    public static async Task<bool> ContentUpdateRequired(this ApiContext context, DateTime lastDownload)
    {
        string url = string.Format(context.BaseUri + CmsRoot + UpdateRequiredEndpoint, lastDownload.ToString("O"));
        HttpResponseMessage response = await context.Client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            return false;
        }

        if (response.StatusCode != HttpStatusCode.UpgradeRequired)
        {
            throw new BadResponseException("CMS download error " + response.StatusCode);
        }

        return true;
    }

    public static async Task<Cms> DownloadCms(this ApiContext context)
    {
        HttpResponseMessage response = await context.Client.GetAsync(context.BaseUri + CmsRoot + DownloadCmsEndpoint);
        string content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<Cms>(content) ?? throw new BadResponseException(content);
    }
}
