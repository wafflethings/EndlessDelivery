using System.Linq;
using BepInEx;
using BepInEx.Bootstrap;
using EndlessDelivery.Common.ContentFile;
using EndlessDelivery.Online;

namespace EndlessDelivery.Anticheat;

public class BlacklistAnticheat : Anticheat
{
    protected override bool ShouldSubmit(out string reason)
    {
        Cms? cms = OnlineFunctionality.LastFetchedContent;

        if (cms == null)
        {
            reason = "Null CMS";
            return false;
        }

        reason = string.Empty;

        foreach (PluginInfo mod in Chainloader.PluginInfos.Values)
        {
            if (cms.BannedMods.ContainsKey(mod.Metadata.GUID))
            {
                reason += $"Banned mod {mod.Metadata.Name}";
                return false;
            }
        }

        return true;
    }
}
