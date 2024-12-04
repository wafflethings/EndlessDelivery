using System;
using System.Collections;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AtlasLib.Saving;
using AtlasLib.Utils;
using BepInEx.Bootstrap;
using EndlessDelivery.Api;
using EndlessDelivery.Api.Requests;
using EndlessDelivery.Common.ContentFile;
using EndlessDelivery.Cosmetics;
using HarmonyLib;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace EndlessDelivery.Online;

public static class OnlineFunctionality
{
    public static readonly ApiContext Context = new(new HttpClient(), GetTicket);
    private static SaveFile<Cms?> s_cmsData = SaveFile.RegisterFile(new SaveFile<Cms?>("content.json", Plugin.Name));

    public static Cms? LastFetchedContent => s_cmsData.Data;

    public static void Init()
    {
        Plugin.Log.LogMessage("Init called!");
        CoroutineRunner.Instance.StartCoroutine(InitWhenSteamReady());
    }

    private static IEnumerator InitWhenSteamReady()
    {
        while (!SteamClient.IsValid)
        {
            yield return null;
        }

        Context.Client.DefaultRequestHeaders.Add("DdVersion", Plugin.Version);
        Context.Client.DefaultRequestHeaders.Add("DdMods", string.Join(",", Chainloader.PluginInfos.Values.Select(x => x.Metadata.GUID.Replace(",", string.Empty))));

        Task loginTask = Task.Run(Context.Login);
        yield return new WaitUntil(() => loginTask.IsCompleted);

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
        if (await Context.ContentUpdateRequired(s_cmsData.Data?.Hash ?? string.Empty))
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
