using EndlessDelivery.Common;
using EndlessDelivery.Common.Communication.Scores;
using EndlessDelivery.Common.Inventory.Items;

namespace EndlessDelivery.Server.Api.Users;

public class UserModel
{
    public ulong SteamId { get; set; } = 0;
    public DateTime CreationDate { get; set; }
    public string NameFormat { get; set; } = "{0}";
    public bool Banned { get; set; } = false;
    public Score LifetimeStats { get; set; }
    public List<string> OwnedItemIds { get; set; } = new();
    public int PremiumCurrency { get; set; } = 0;
    public InventoryLoadout Loadout { get; set; }
    public UserLinks Links { get; set; } = new();
    public string Country { get; set; }
    public bool Admin { get; set; } = false;
    public List<OwnedAchievement> OwnedAchievements { get; set; } = new();
}
