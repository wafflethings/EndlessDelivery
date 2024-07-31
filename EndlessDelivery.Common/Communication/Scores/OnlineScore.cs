using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace EndlessDelivery.Common.Communication.Scores;

[Table("new_scores")]
public class OnlineScore : BaseModel
{
    [PrimaryKey("steam_id")] public ulong SteamId { get; set; }
    [Column("data")] public Score Score { get; set; }
    [Column("index")] public int Index { get; set; }
    [Column("country_index")] public int CountryIndex { get; set; }
    [Column("difficulty")] public short Difficulty { get; set; }
    [Column("date")] public DateTime Date { get; set; }
}
