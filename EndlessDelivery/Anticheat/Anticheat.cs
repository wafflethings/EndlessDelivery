using System.Collections.Generic;
using System.Linq;

namespace EndlessDelivery.Anticheat;

public abstract class Anticheat
{
    private static List<Anticheat> _anticheats = new()
    {
        new DetectPresenceAnticheat(),
        new MasqueradeDivinityAnticheat(),
        new UltraTweakerAnticheat()
    };

    public static bool HasIllegalMods = !_anticheats.All(ac => ac.ShouldSubmit);

    protected abstract bool ShouldSubmit { get; }
}