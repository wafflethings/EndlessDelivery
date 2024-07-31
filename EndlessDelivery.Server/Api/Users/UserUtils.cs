using EndlessDelivery.Common.Communication.Scores;

namespace EndlessDelivery.Server.Api.Users;

public static class UserUtils
{
    public static async Task<OnlineScore> GetBestScore(this UserModel user) => (await Program.SupabaseClient.From<OnlineScore>().Where(sm => sm.SteamId == user.SteamId).Get()).Model;
}
