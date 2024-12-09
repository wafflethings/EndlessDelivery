using EndlessDelivery.Components;
using EndlessDelivery.Gameplay.Ward;
using EndlessDelivery.Utils;

namespace EndlessDelivery.Gameplay.SpecialWaves;

public class WardWave : SpecialWave
{
    private const int MaxWards = 5;
    public override string Name => "DIVINE WARDS";
    public override int Cost => 15;

    public override void Start()
    {
        GameManager.Instance.EnemySpawned += OnEnemySpawned;
        int wardCount = 0;

        foreach (Present present in GameManager.Instance.CurrentRoom.Presents.ShuffleAndToList())
        {
            if (!present.gameObject.activeSelf || wardCount > MaxWards)
            {
                continue;
            }

            WardManager.Instance.CreateWard(present);
            wardCount++;
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
