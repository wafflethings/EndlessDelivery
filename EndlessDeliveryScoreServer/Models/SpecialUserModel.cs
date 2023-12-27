using EndlessDelivery.Scores;
using Postgrest.Attributes;
using Postgrest.Models;

namespace EndlessDeliveryScoreServer.Models
{
    [Table("special_user")]
    public class SpecialUserModel : BaseModel
    {
        [PrimaryKey("steam_id")]
        public ulong SteamId { get; set; }
        
        [Column("name_format")]
        public string NameFormat { get; set; }
        
        [Column("is_banned")]
        public bool IsBanned { get; set; }
    }
}