using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace EndlessDelivery.Scores.Server
{
    public class ScoreResult
    {
        public ulong SteamId { get; set; }
        public Score Score { get; set; }
        public int Index { get; set; }
    }
}