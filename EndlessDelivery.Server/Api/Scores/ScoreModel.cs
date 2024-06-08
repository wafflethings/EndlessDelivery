using EndlessDelivery.Scores;
using Postgrest.Attributes;
using Postgrest.Models;

namespace EndlessDelivery.Server.Api.Scores
{
    [Table("new_scores")]
    public class ScoreModel : BaseModel
    {
        [PrimaryKey("steam_id")]
        public ulong SteamId { get; set; }
        
        [Column("data")]
        public Score Score { get; set; }
        
        [Column("index")]
        public int Index { get; set; }
        
        [Column("country_index")]
        public int CountryIndex { get; set; }
        
        [Column("difficulty")]
        public short Difficulty { get; set; }
        
        [Column("date")]
        public DateTime Date { get; set; }
    } 
}