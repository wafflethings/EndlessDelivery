using System.Collections.Generic;
using EndlessDelivery.Components;
using EndlessDelivery.Gameplay;
using UnityEngine;

namespace EndlessDelivery.Cheats;

public class InfiniteTime : ICheat
{
    public void Enable()
    {
        _active = true;
    }

    public void Disable()
    {
        _active = false;
    }

    public void Update()
    {
        GameManager.Instance.SilentAddTime(Time.deltaTime);
    }

    public string LongName => "Infinite Time";
    public string Identifier => $"{Plugin.Guid}.inftime";
    public string ButtonEnabledOverride { get; }
    public string ButtonDisabledOverride { get; }
    public string Icon => "death";
    public bool IsActive => _active;
    public bool DefaultState { get; }
    public StatePersistenceMode PersistenceMode => StatePersistenceMode.NotPersistent;

    private bool _active;
}
