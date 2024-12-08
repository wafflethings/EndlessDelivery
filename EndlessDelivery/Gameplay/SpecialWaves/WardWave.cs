using EndlessDelivery.Components;
using EndlessDelivery.Gameplay.Ward;

namespace EndlessDelivery.Gameplay.SpecialWaves;

public class WardWave : SpecialWave
{
    public override string Name => "WARDED";
    public override int Cost => 15;

    public override void Start()
    {
        GameManager.Instance.EnemySpawned += OnEnemySpawned;

        foreach (Present present in GameManager.Instance.CurrentRoom.Presents)
        {
            if (!present.gameObject.activeSelf)
            {
                continue;
            }

            WardManager.Instance.CreateWard(present);
        }
    }

    public override void End()
    {
        GameManager.Instance.EnemySpawned -= OnEnemySpawned;
    }

    private void OnEnemySpawned(EnemyIdentifier enemy)
    {
        if (enemy == null)
        {
            return;
        }

        enemy.gameObject.AddComponent<WardTracker>();
    }
}
