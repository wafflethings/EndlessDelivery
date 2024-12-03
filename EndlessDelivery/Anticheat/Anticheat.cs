using System.Collections.Generic;
using System.Linq;

namespace EndlessDelivery.Anticheat;

public abstract class Anticheat
{
    private static List<Anticheat> _anticheats = new() { new AtlasLibAnticheat(), new BlacklistAnticheat(), new MasqueradeDivinityAnticheat(), new UltraTweakerAnticheat(), new VanillaAnticheat() };

    public static bool HasIllegalMods(out List<string> reasons)
    {
        reasons = new List<string>();
        bool cantSubmit = false;

        foreach (Anticheat ac in _anticheats)
        {
            if (!ac.ShouldSubmit(out string reason))
            {
                reasons.Add(reason);
                cantSubmit = true;
            }
        }

        return cantSubmit;
    }

    protected abstract bool ShouldSubmit(out string reason);
}
