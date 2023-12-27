using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EndlessDelivery.Scores.Server
{
    public class SpecialUserResult
    {
        public static Dictionary<ulong, SpecialUserResult>? SteamIdToUser;
        
        public ulong SteamId { get; set; }
        public bool IsBanned { get; set; }
        public string NameFormat { get; set; }

        private static async Task EnsureDictSet()
        {
            if (SteamIdToUser != null)
            {
                return;
            }
            
            SteamIdToUser = (await Endpoints.GetSpecialUsers()).ToDictionary(x => x.SteamId, y => y);
        }

        public static async Task<string> GetFormat(ulong steamId)
        {
            await EnsureDictSet();
            
            if (SteamIdToUser.TryGetValue(steamId, out SpecialUserResult result))
            {
                return result.NameFormat;
            }

            return "{0}";
        }
    }
}