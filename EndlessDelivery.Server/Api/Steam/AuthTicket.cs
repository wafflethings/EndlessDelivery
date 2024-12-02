using Newtonsoft.Json;

namespace EndlessDelivery.Server.Api.Steam
{
    public class AuthTicket
    {
        public const int MaxRetries = 5;
        public static readonly TimeSpan WaitBeforeAttempting = TimeSpan.FromSeconds(0.5f);
        public static readonly TimeSpan RetryInterval = TimeSpan.FromSeconds(1);
            
        public string SteamId { get; set; }
        public string OwnerSteamId { get; set; }
        public bool VacBanned { get; set; }
        public bool PublisherBanned { get; set; }

        public static async Task<AuthTicket> GetAuth(string ticket)
        {
            await Task.Delay((int)WaitBeforeAttempting.TotalMilliseconds); // sometimes it just doesnt fucking work, you have to do it several times
            // i am hoping that it just takes some time to like, register a ticket?
            // hopefully just delaying it fixes it, im at a loss

            for (int i = 0; i < MaxRetries; i++)
            {
                HttpResponseMessage response = await Program.Client.GetAsync(SteamApi.BuildAuthenticateUserTicket(ticket));
                string stringResponse = await response.Content.ReadAsStringAsync(); // normally looks like {"response":{"params":{"result":"OK","steamid":"76561199074883531","ownersteamid":"76561199074883531","vacbanned":false,"publisherbanned":false}}}

                if (response.IsSuccessStatusCode && !stringResponse.Contains("error"))
                {
                    stringResponse = stringResponse.Substring(22, stringResponse.Length - 24); // trims "{"response":{"params":{" and last two "}}"
                    return JsonConvert.DeserializeObject<AuthTicket>(stringResponse);
                }

                //sometimes it just doesnt fucking work, i hate steam
                Console.WriteLine($"Auth ticket failed to work? Retrying, attempt {i} / {MaxRetries}");
                await Task.Delay(i * (int)RetryInterval.TotalMilliseconds);
            }

            throw new Exception("Steam ticket auth error.");
        }
    }
}