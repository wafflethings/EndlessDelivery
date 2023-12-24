using EndlessDelivery.Scores;
using Newtonsoft.Json;
using Postgrest.Attributes;
using Postgrest.Models;

namespace EndlessDeliveryScoreServer.Models
{
    [Table("scores")]
    public class ScoreModel : BaseModel
    {
        [PrimaryKey("steam_id")]
        public ulong SteamId { get; set; }
        
        [Column("data")]
        public Score Score { get; set; }
        
        [Column("index")]
        public int Index { get; set; }
    }
}