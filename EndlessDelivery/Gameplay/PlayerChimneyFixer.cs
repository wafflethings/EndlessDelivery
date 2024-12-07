using EndlessDelivery.Components;
using EndlessDelivery.Utils;
using UnityEngine;

namespace EndlessDelivery.Gameplay;

public class PlayerChimneyFixer : MonoSingleton<PlayerChimneyFixer>
{
    private bool _isInChimney;
    private Chimney _currentChimney;

    public void EnterChimney(Chimney chimney)
    {
        _isInChimney = true;
        _currentChimney = chimney;
    }

    public void Exit()
    {
        _isInChimney = false;
    }

    private void Update()
    {
        if (_isInChimney)
        {
            Vector3 distanceToCentre = (_currentChimney != null ? _currentChimney.transform.position : NewMovement.Instance.transform.position).Only(Axis.X, Axis.Z) - NewMovement.Instance.transform.position;
            NewMovement.Instance.rb.velocity = Vector3.MoveTowards(NewMovement.Instance.rb.velocity, new Vector3(0, -100, 0) + distanceToCentre, Time.deltaTime * 50);
            NewMovement.Instance.enabled = false;
        }
    }
}
