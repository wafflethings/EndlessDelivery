using Newtonsoft.Json;

namespace EndlessDeliveryScoreServer
{
    public class Authentication
    {
        public static readonly string TicketAuthUrl = "https://api.steampowered.com/ISteamUserAuth/AuthenticateUserTicket/v0001/?key={0}&appid={1}&ticket={2}";
        public static readonly string UltrakillAppId = "1229490";

        public static string GetAuthUrl(string ticket)
        {
            return string.Format(TicketAuthUrl, Keys.Instance.SteamKey, UltrakillAppId, ticket);
        }

        public async static Task<AuthenticateUserTicketResponse> GetAuth(string ticket)
        {
            HttpClient client = new();
            await Task.Delay(500); //sometimes it just doesnt fucking work, you have to do it several times
            // i am hoping that it just takes some time to like, register a ticket?
            // hopefully just delaying it fixes it, im at a loss

            for (int i = 0; i < 5; i++)
            {
                HttpResponseMessage response = await client.GetAsync(GetAuthUrl(ticket));
                string stringResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine(stringResponse + " | " + response.StatusCode);

                if (response.IsSuccessStatusCode && !stringResponse.Contains("error"))
                {
                    return JsonConvert.DeserializeObject<AuthenticateUserTicketResponse>(stringResponse);
                }

                //sometimes it just doesnt fucking work, i hate steam
                Console.WriteLine("attempt " + i);
                await Task.Delay(i * 1000);
            }

            throw new Exception("Steam ticket auth error.");
        }
    }
}