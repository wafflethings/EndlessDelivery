using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EndlessDelivery.Common.ContentFile;
using EndlessDelivery.Saving;
using Newtonsoft.Json;

namespace EndlessDelivery.Online.Requests;

public static class Content
{
    private const string CmsRoot = "cms/";
    private const string UpdateRequiredEndpoint = "update_required?lastUpdate={0}";
    private const string DownloadCmsEndpoint = "content";

    private static SaveFile<Cms> s_cmsData = SaveFile.RegisterFile(new SaveFile<Cms>("content.json"));

    public static async Task<Cms> GetContent()
    {
        if (await UpdateRequired())
        {
            Cms downloaded = await DownloadCms();

            if (downloaded == null)
            {
                throw new Exception("CMS download failed!!");
            }

            s_cmsData.Data = downloaded;
        }

        return s_cmsData.Data;
    }

    private static async Task<bool> UpdateRequired()
    {
        DateTime lastDownload = s_cmsData.Data == null ? DateTime.MinValue : s_cmsData.Data.LastUpdate;
        string url = string.Format(OnlineFunctionality.RootUrl + CmsRoot + UpdateRequiredEndpoint, lastDownload.ToString("O"));
        HttpResponseMessage response = await OnlineFunctionality.Client.GetAsync(url);
        return response.StatusCode == HttpStatusCode.UpgradeRequired;
    }

    private static async Task<Cms?> DownloadCms()
    {
        HttpResponseMessage response = await OnlineFunctionality.Client.GetAsync(OnlineFunctionality.RootUrl + CmsRoot + DownloadCmsEndpoint);

        if (!response.IsSuccessStatusCode)
        {
            UnityEngine.Debug.LogError("Content download failure.");
        }

        return JsonConvert.DeserializeObject<Cms>(await response.Content.ReadAsStringAsync());
    }
}
