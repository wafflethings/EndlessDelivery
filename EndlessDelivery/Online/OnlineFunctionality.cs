using System;
using System.Net.Http;
using System.Threading.Tasks;
using AtlasLib.Saving;
using EndlessDelivery.Api;
using EndlessDelivery.Api.Requests;
using EndlessDelivery.Common.ContentFile;
using EndlessDelivery.Cosmetics;
using Steamworks;
using Steamworks.Data;

namespace EndlessDelivery.Online;

public static class OnlineFunctionality
{
    public static readonly ApiContext Context = new(new HttpClient(), GetTicket, new Uri("http://localhost:7048/api/"));
    private static SaveFile<Cms?> s_cmsData = SaveFile.RegisterFile(new SaveFile<Cms?>("content.json", Plugin.Name));

    public static Cms? LastFetchedContent => s_cmsData.Data;

    public static void Init()
    {
        Task.Run(GetContent);
        Task.Run(CosmeticManager.FetchLoadout);
    }

    //https://stackoverflow.com/questions/46139474/steam-web-api-authenticate-http-request-error
    public static string GetTicket()
    {
        AuthTicket ticket = SteamUser.GetAuthSessionTicket(new NetIdentity());
        return BitConverter.ToString(ticket.Data, 0, ticket.Data.Length).Replace("-", string.Empty);
    }

    public static async Task<Cms> GetContent()
    {
        if (await Context.ContentUpdateRequired(s_cmsData.Data?.LastUpdate ?? DateTime.MinValue))
        {
            Cms? downloaded = await Context.DownloadCms();

            if (downloaded == null)
            {
                throw new Exception("CMS download failed!");
            }

            s_cmsData.Data = downloaded;
        }

        return s_cmsData.Data ?? throw new Exception("Couldn't get content and no old content cached.");;
    }
}
