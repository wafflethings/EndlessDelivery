using System.Collections.Generic;

namespace EndlessDelivery.Cosmetics.Skins;

public static class SkinDb
{
    private static Dictionary<string, BaseSkin> s_skins = new();

    public static void Init(IEnumerable<BaseSkin> skins)
    {
        foreach (BaseSkin skin in skins)
        {
            s_skins.Add(skin.Id, skin);
        }
    }

    public static BaseSkin? GetSkin(string id) => s_skins.TryGetValue(id, out BaseSkin? skin) ? skin : null;
}
