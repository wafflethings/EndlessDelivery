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
            Vector3 targetPos = (_currentChimney != null ? _currentChimney.gameObject : GameManager.Instance.CurrentRoom.SpawnPoint).transform.position;
            targetPos.y = NewMovement.Instance.transform.position.y;
            NewMovement.Instance.transform.position = Vector3.MoveTowards(NewMovement.Instance.transform.position, targetPos, Time.deltaTime * 5);
            NewMovement.Instance.rb.velocity = Vector3.MoveTowards(NewMovement.Instance.rb.velocity.Only(Axis.Y), new Vector3(0, -100, 0), Time.deltaTime * 15);
            NewMovement.Instance.enabled = false;
        }
    }
}
