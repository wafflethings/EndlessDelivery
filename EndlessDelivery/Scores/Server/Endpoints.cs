using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Steamworks;
using UnityEngine;
using Ping = System.Net.NetworkInformation.Ping;

namespace EndlessDelivery.Scores.Server
{
    public class Endpoints
    {
        private static readonly HttpClient _client = new();
        private const string Url = "http://localhost:7048/";
        private const string HostName = "localhost";
        private const string ScoresGetRange = Url + "scores/get_range?start={0}&count={1}";
        private const string ScoresGetAmount = Url + "scores/get_length";
        private const string ScoresGetPosition = Url + "scores/get_position?steamId={0}";
        private const string ScoresAdd = Url + "scores/add_score?score={0}&ticket={1}";

        public static async Task<bool> IsServerOnline()
        {
            return true;
            
            Ping ping = new();
            Debug.Log("pinging");
            PingReply reply = await ping.SendPingAsync(HostName, 10 * 1000);
            Debug.Log("pinged");
            return reply.Status == IPStatus.Success;
        }
        
        /// <summary>
        /// Returns an array of scores.
        /// </summary>
        /// <param name="start">Start index.</param>
        /// <param name="count">Amount of scores (max 10)</param>
        /// <returns>Array of scores.</returns>
        public static async Task<List<ScoreResult>> GetScoreRange(int start, int count)
        {
            HttpResponseMessage response = await _client.GetAsync(string.Format(ScoresGetRange, start, count));
            string responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseBody);
            return JsonConvert.DeserializeObject<List<ScoreResult>>(responseBody);
        }
        
        /// <summary>
        /// Gets the amount of scores.
        /// </summary>
        /// <returns>The amount.</returns>
        public static async Task<int> GetScoreAmount()
        {
            HttpResponseMessage response = await _client.GetAsync(ScoresGetAmount);
            string responseBody = await response.Content.ReadAsStringAsync();
            Debug.Log(responseBody + " " + JsonConvert.DeserializeObject<Response>(responseBody).Value.GetType());
            return (int)(long)JsonConvert.DeserializeObject<Response>(responseBody).Value;
        }
        
        /// <summary>
        /// Returns the index of the user's score (-1 if no score present).
        /// </summary>
        /// <param name="steamId">The user's steam ID.</param>
        /// <returns>The index of the user's score (-1 if no score present)</returns>
        public static async Task<int> GetUserPosition(ulong steamId)
        {
            HttpResponseMessage response = await _client.GetAsync(string.Format(ScoresGetPosition, steamId));
            string responseBody = await response.Content.ReadAsStringAsync();
            return (int)(long)JsonConvert.DeserializeObject<Response>(responseBody).Value;
        }
        
        /// <summary>
        /// Upserts a score to the database, and returns the index in the DB.
        /// </summary>
        /// <param name="score">The score achieved.</param>
        /// <returns>Index in the list.</returns>
        public static async Task<int> SendScoreAndReturnPosition(Score score)
        {
            try
            {
                // i know this should be a POST but i cant figure them out so FUCK YOU!!!
                HttpResponseMessage response = await _client.GetAsync(string.Format(ScoresAdd, JsonConvert.SerializeObject(score), SteamAuth.GetTicket()));
                Debug.Log("sending " + response.RequestMessage.RequestUri);
                string responseBody = await response.Content.ReadAsStringAsync();
                Debug.Log("received " + responseBody);
                return (int)(long)JsonConvert.DeserializeObject<Response>(responseBody).Value;
            }
            catch (Exception ex)
            {
                DisplayError(ex.ToString());
                return -1;
            }
        }

        public static void DisplayError(string error)
        {
            HudMessageReceiver.Instance.SendHudMessage($"<color=red>AN ERROR HAS OCCURED WITH THE SERVER! Contact Waffle and send your logs.</color>");
            Debug.LogError("DISPLAYERROR: " + error);
        }

        public static async Task<List<ScoreResult>> GetUserPage(float index)
        {
            int start = (int)(index / 10);
            return await GetScoreRange(start, 10);
        }
    }
}