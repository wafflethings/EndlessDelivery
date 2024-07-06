using System.Linq;
using BepInEx.Bootstrap;

namespace EndlessDelivery.Anticheat;

public class DetectPresenceAnticheat : Anticheat
{
    private string[] _mods =
        {
            "ironfarm.uk.uc",
            "ironfarm.uk.muda",
            "plonk.rocketgatling",
            "maranara_whipfix"
        };
    
    protected override bool ShouldSubmit => _mods.All(mod => !Chainloader.PluginInfos.ContainsKey(mod));
}