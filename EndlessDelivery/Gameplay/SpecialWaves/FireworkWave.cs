using EndlessDelivery.Gameplay.Firework;

namespace EndlessDelivery.Gameplay.SpecialWaves;

public class FireworkWave : SpecialWave
{
    public override string Name => "FIREWORKS";
    public override int Cost => 5;

    public override void Start() => FireworkManager.Instance.gameObject.SetActive(true);

    public override void End() => FireworkManager.Instance.gameObject.SetActive(false);
}
