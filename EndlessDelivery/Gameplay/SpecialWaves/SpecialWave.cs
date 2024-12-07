namespace EndlessDelivery.Gameplay.SpecialWaves;

public abstract class SpecialWave
{
    public abstract string Name { get; }
    public abstract int Cost { get; }

    public virtual void Start()
    {

    }

    public virtual void End()
    {

    }
}
