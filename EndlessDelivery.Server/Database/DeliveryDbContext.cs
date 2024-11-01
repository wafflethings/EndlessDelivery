using System.Reflection;
using EndlessDelivery.Common;
using EndlessDelivery.Common.Communication.Scores;
using EndlessDelivery.Server.Api.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace EndlessDelivery.Server.Database;

public class DeliveryDbContext : DbContext
{
    public DbSet<UserModel> Users { get; private set; }
    public DbSet<OnlineScore> Scores { get; private set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=DeliveryDb.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        EntityTypeBuilder<OnlineScore> entityBuilder = modelBuilder.Entity<OnlineScore>();
        entityBuilder.HasKey(score => score.SteamId);
        entityBuilder.OwnsOne(score => score.Score, builder =>
        {
            builder.ToJson();
            builder.Property(x => x.Deliveries);
            builder.Property(x => x.Kills);
            builder.Property(x => x.Rooms);
            builder.Property(x => x.Time);
            builder.Property(x => x.StartRoom);
        });
        entityBuilder.Property(score => score.Index);
        entityBuilder.Property(score => score.Difficulty);
        entityBuilder.Property(score => score.Date);
        entityBuilder.Property(score => score.CountryIndex);

        EntityTypeBuilder<UserModel> userBuilder = modelBuilder.Entity<UserModel>();
        userBuilder.HasKey(user => user.SteamId);
        userBuilder.Property(user => user.CreationDate);
        userBuilder.Property(user => user.NameFormat);
        userBuilder.Property(user => user.Banned);
        userBuilder.OwnsOne(user => user.LifetimeStats, builder =>
        {
            builder.ToJson();
            builder.Property(x => x.Deliveries);
            builder.Property(x => x.Kills);
            builder.Property(x => x.Rooms);
            builder.Property(x => x.Time);
            builder.Property(x => x.StartRoom);
        });
        userBuilder.Property(user => user.OwnedItemIds);
        userBuilder.OwnsOne(user => user.Links, builder => builder.ToJson());
        userBuilder.Property(user => user.Country);
        userBuilder.Property(user => user.Admin);
        userBuilder.OwnsOne(user => user.Loadout, builder =>
        {
            builder.ToJson();
            builder.Property(x => x.BannerId);
        });
        userBuilder.Property(user => user.PremiumCurrency);
        userBuilder.Property(user => user.OwnedAchievements).HasConversion(
            a => (string)JsonConvert.SerializeObject(a),
            a => JsonConvert.DeserializeObject<List<OwnedAchievement>>(a));;
    }
}
