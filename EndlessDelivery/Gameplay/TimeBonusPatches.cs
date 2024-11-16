using System.Linq;
using EndlessDelivery.Assets;
using HarmonyLib;

namespace EndlessDelivery.Gameplay;

[HarmonyPatch]
public static class TimeBonusPatches
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
            pointName = ""; // StyleHUD.AddPoints => if localizedName is "", it won't add it
        }
    }

    public static readonly EnemyType[] BigKillEnemies = [EnemyType.MaliciousFace, EnemyType.Cerberus, EnemyType.HideousMass, EnemyType.Swordsmachine];
    public static readonly EnemyType[] HugeKillEnemies = [EnemyType.Mindflayer, EnemyType.Ferryman];

    [HarmonyPatch(typeof(EnemyIdentifier), nameof(EnemyIdentifier.Awake)), HarmonyPostfix]
    private static void AddKillBonusEvent(EnemyIdentifier __instance)
    {
        if (!AssetManager.InSceneFromThisMod)
        {
            return;
        }

        __instance.onDeath.AddListener(() => OnKill(__instance));
    }

    private static void OnKill(EnemyIdentifier identifier)
    {
        if (BigKillEnemies.Contains(identifier.enemyType))
        {
            GameManager.Instance.AddTime(2f, "<color=orange>BIG KILL</color>");
            return;
        }

        if (HugeKillEnemies.Contains(identifier.enemyType))
        {
            GameManager.Instance.AddTime(4f, "<color=orange>HUGE KILL</color>");
        }
    }
}
