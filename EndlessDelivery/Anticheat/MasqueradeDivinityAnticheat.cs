using System;
using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using UnityEngine;

namespace EndlessDelivery.Anticheat;

public class MasqueradeDivinityAnticheat : Anticheat
{
    public const string GUID = "maranara_project_prophet";

    protected override bool ShouldSubmit
    {
        get
        {
                if (!Chainloader.PluginInfos.ContainsKey(GUID))
                {
                    return true;
                }

                return GabeOn();
            }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool GabeOn() => ProjectProphet.ProjectProphet.gabeOn;
}