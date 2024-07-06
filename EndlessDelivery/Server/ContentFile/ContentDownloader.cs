using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EndlessDelivery.Common.ContentFile;
using EndlessDelivery.Saving;
using Newtonsoft.Json;

namespace EndlessDelivery.Server.ContentFile;

public static class ContentDownloader
{
    private const string CmsRoot = "cms/";
    private const string UpdateRequiredEndpoint = "update_required?lastUpdate={0}";
    private const string DownloadCmsEndpoint = "content";

    private static JsonSaveData<Cms> s_cmsData = SaveData.RegisterData(new JsonSaveData<Cms>("content.json")) as JsonSaveData<Cms>;

    public static async Task<Cms> GetContent()
    {
        if (await UpdateRequired())
        {
            Console.WriteLine("update required");
            s_cmsData.Value = await DownloadCms();
        }

        return s_cmsData.Value;
    }

    private static async Task<bool> UpdateRequired()
    {
        DateTime lastDownload = s_cmsData.Value == null ? DateTime.MinValue : s_cmsData.Value.LastUpdate;
        HttpResponseMessage response = await OnlineFunctionality.Client.GetAsync(string.Format(OnlineFunctionality.RootUrl + CmsRoot + UpdateRequiredEndpoint, lastDownload));
        return response.StatusCode == HttpStatusCode.UpgradeRequired;
    }

    private static async Task<Cms> DownloadCms()
    {
        HttpResponseMessage response = await OnlineFunctionality.Client.GetAsync(OnlineFunctionality.RootUrl + CmsRoot + DownloadCmsEndpoint);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Content download result: {response.StatusCode}");
        }

        return JsonConvert.DeserializeObject<Cms>(await response.Content.ReadAsStringAsync());
    }
}
