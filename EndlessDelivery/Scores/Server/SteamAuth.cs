using System;
using System.Text;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace EndlessDelivery.Scores.Server;

public static class SteamAuth
{
    //https://stackoverflow.com/questions/46139474/steam-web-api-authenticate-http-request-error
    public static string GetTicket()
    {
            AuthTicket ticket = SteamUser.GetAuthSessionTicket(new NetIdentity());
            return BitConverter.ToString(ticket.Data,0, ticket.Data.Length).Replace("-", string.Empty);
        }
}