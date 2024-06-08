using Newtonsoft.Json;

namespace EndlessDelivery.Server.Config
{
    public class Keys
    {
        private static readonly string KeysPath = Path.Combine("Assets", "Config", "keys.json");
        public static Keys Instance;

        public static void Load()
        {
            Instance = JsonConvert.DeserializeObject<Keys>(File.ReadAllText(KeysPath));
        }
        
        public string SupabaseKey { get; set; }
        public string SteamKey { get; set; }
        public string TokenAes { get; set; }
    }
}