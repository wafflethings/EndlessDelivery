using System.Linq;
using AtlasLib.Weapons;

namespace EndlessDelivery.Anticheat;

public class AtlasLibAnticheat : Anticheat
{
    protected override bool ShouldSubmit(out string reason)
    {
        if (WeaponRegistry.Weapons.Any(x => x.Selection is WeaponSelection.Standard or WeaponSelection.Alternate))
        {
            reason = "Atlaslib weapon equipped";
            return false;
        }

        reason = string.Empty;
        return true;
    }
}
