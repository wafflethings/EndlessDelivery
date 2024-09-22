using EndlessDelivery.Assets;
using HarmonyLib;

namespace EndlessDelivery.Gameplay;

[HarmonyPatch]
public class TimeBonusPatches
{
    [HarmonyPatch(typeof(StyleCalculator), nameof(StyleCalculator.AddPoints)), HarmonyPrefix]
    private static void AddBigKillTime(ref string pointName)
    {
        if (!AssetManager.InSceneFromThisMod)
        {
            return;
        }

        if (pointName == "ultrakill.bigkill")
        {
            GameManager.Instance.AddTime(2f, "<color=orange>BIG KILL</color>");
            pointName = ""; // StyleHUD.AddPoints => if localizedName is "", it won't add it
        }
    }
}
