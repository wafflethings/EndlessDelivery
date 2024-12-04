using System;
using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using UltraTweaker.Tweaks;

namespace EndlessDelivery.Anticheat;

public class UltraTweakerAnticheat : Anticheat
{
    private const string Guid = "waffle.ultrakill.ultratweaker";

    protected override bool ShouldSubmit(out string reason)
    {
        reason = string.Empty;

        if (!Chainloader.PluginInfos.ContainsKey(Guid))
        {
            return true;
        }

        if (HasBadTweaks())
        {
            reason = "Banned UltraTweaker tweak enabled";
            return false;
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool HasBadTweaks()
    {
        foreach (Tweak tweak in UltraTweaker.UltraTweaker.AllTweaks.Values)
        {
            TweakMetadata? meta = Attribute.GetCustomAttribute(tweak.GetType(), typeof(TweakMetadata)) as TweakMetadata;
            if (tweak.IsEnabled && !(meta?.AllowCG ?? false))
            {
                return true;
            }
        }

        return false;
    }
}
