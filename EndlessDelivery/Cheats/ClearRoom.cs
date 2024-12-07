using System.Collections.Generic;
using EndlessDelivery.Components;
using EndlessDelivery.Gameplay;

namespace EndlessDelivery.Cheats;

public class ClearRoom : ICheat
{
    public void Enable()
    {
        GameManager.Instance.CurrentRoom.AmountDelivered = new Dictionary<WeaponVariant, int> { { WeaponVariant.BlueVariant, 10 }, { WeaponVariant.GreenVariant, 10 }, { WeaponVariant.RedVariant, 10 }, { WeaponVariant.GoldVariant, 10 } };

        foreach (Chimney chimney in GameManager.Instance.CurrentRoom.Chimneys)
        {
            chimney.JumpPad.gameObject.SetActive(false);
        }

        NewMovement.Instance.transform.position = GameManager.Instance.CurrentRoom.Chimneys[0].transform.position;
    }

    public void Disable()
    {
    }

    public void Update()
    {
    }

    public string LongName => "Complete Room";
    public string Identifier => $"{Plugin.Guid}.completeroom";
    public string ButtonEnabledOverride => null;
    public string ButtonDisabledOverride => "COMPLETE";
    public string Icon => "death";
    public bool IsActive => false;
    public bool DefaultState => false;
    public StatePersistenceMode PersistenceMode => StatePersistenceMode.NotPersistent;
}
