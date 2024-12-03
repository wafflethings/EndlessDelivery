using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;

namespace EndlessDelivery.Anticheat;

public class MasqueradeDivinityAnticheat : Anticheat
{
    public const string Guid = "maranara_project_prophet";

    protected override bool ShouldSubmit(out string reason)
    {
        reason = string.Empty;

        if (Chainloader.PluginInfos.ContainsKey(Guid) && GabeOn())
        {
            reason = "Masquerade Divinity gabe mode is enabled";
            return false;
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool GabeOn() => ProjectProphet.ProjectProphet.gabeOn;
}
