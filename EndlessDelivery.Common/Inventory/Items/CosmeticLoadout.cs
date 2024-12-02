using System.Collections.Generic;

namespace EndlessDelivery.Common.Inventory.Items;

public class CosmeticLoadout
{
    public static CosmeticLoadout Default => new()
    {
        BannerId = "banner_default",
        PresentId = "present_default",
        RevolverIds = ["revolver_default", "revolver_default", "revolver_default"],
        AltRevolverIds = ["altrevolver_default", "altrevolver_default", "altrevolver_default"],
        ShotgunIds = ["shotgun_default", "shotgun_default", "shotgun_default"],
        AltShotgunIds = ["altshotgun_default", "altshotgun_default", "altshotgun_default"],
        NailgunIds = ["nailgun_default", "nailgun_default", "nailgun_default"],
        AltNailgunIds = ["altnailgun_default", "altnailgun_default", "altnailgun_default"],
        RailcannonIds = ["railcannon_default", "railcannon_default", "railcannon_default"],
        RocketIds = ["rocket_default", "rocket_default", "rocket_default"],
    };

    public static readonly List<string> DefaultItems =  ["revolver_default", "altrevolver_default", "shotgun_default", "altshotgun_default", "nailgun_default",
        "altnailgun_default", "railcannon_default", "rocket_default", "banner_default", "present_default"];

    public string BannerId;
    public string PresentId;
    public List<string> RevolverIds = new();
    public List<string> AltRevolverIds = new();
    public List<string> ShotgunIds = new();
    public List<string> AltShotgunIds = new();
    public List<string> NailgunIds = new();
    public List<string> AltNailgunIds = new();
    public List<string> RailcannonIds = new();
    public List<string> RocketIds = new();
}
