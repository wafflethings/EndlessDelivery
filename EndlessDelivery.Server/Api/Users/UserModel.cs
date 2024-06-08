using EndlessDelivery.Common.Inventory.Items;
using EndlessDelivery.Scores;
using EndlessDelivery.Server.Api.Scores;
using Postgrest.Attributes;
using Postgrest.Models;

namespace EndlessDelivery.Server.Api.Users;

[Table("users")]
public class UserModel : BaseModel
{
    [PrimaryKey("steam_id")]
    public ulong SteamId { get; set; } = 0;

    [Column("created_at")]
    public DateTime CreationDate { get; set; }

    [Column("name_format")]
    public string NameFormat { get; set; } = "{0}";

    [Column("banned")]
    public bool Banned { get; set; } = false;

    [Column("lifetime_stats")]
    public Score LifetimeStats { get; set; }

    [Column("inventory")]
    public List<string> OwnedItemIds { get; set; } = new();

    [Column("premium_currency")]
    public int PremiumCurrency { get; set; } = 0;

    [Column("active_items")]
    public InventoryLoadout Loadout { get; set; }

    [Column("socials")]
    public UserLinks Links { get; set; } = new();

    [Column("country")]
    public string Country { get; set; }

    [Column("admin")]
    public bool Admin { get; set; } = false;

    public async Task<ScoreModel> GetBestScore() => (await Program.Supabase.From<ScoreModel>().Where(sm => sm.SteamId == SteamId).Get()).Model;
}
