using System;

namespace EndlessDelivery.Anticheat;

public class VanillaAnticheat : Anticheat
{
    protected override bool ShouldSubmit(out string reason)
    {
        if (!GameStateManager.CanSubmitScores)
        {
            reason = "Cybergrind submission disabled";
            return false;
        }

        reason = string.Empty;
        return true;
    }
}
