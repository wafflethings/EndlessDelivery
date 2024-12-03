using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace EndlessDelivery.UI;

public class CheatsFoundScreen : MonoBehaviour
{
    [SerializeField] private TMP_Text _reasonsText;

    private void Awake()
    {
        if (!Anticheat.Anticheat.HasIllegalMods(out List<string> reasons))
        {
            gameObject.SetActive(false);
            return;
        }

        DisablePlayer();
        _reasonsText.text = string.Join("; ", reasons);
        gameObject.SetActive(true);
    }

    private void Update()
    {
        NewMovement.Instance.enabled = false;
        GunControl.Instance.enabled = false;
        FistControl.Instance.enabled = false;
        Time.timeScale = 0;
    }

    private void DisablePlayer()
    {
        GameStateManager.Instance.RegisterState(new GameState("pause_cheatscreen", [gameObject])
        {
            cursorLock = LockMode.Unlock,
            cameraInputLock = LockMode.Lock,
            playerInputLock = LockMode.Lock
        });

        NewMovement.Instance.enabled = false;
        GunControl.Instance.enabled = false;
        FistControl.Instance.enabled = false;
        Time.timeScale = 0;
    }

    public void ReenablePlayer()
    {
        NewMovement.Instance.enabled = true;
        GunControl.Instance.enabled = true;
        FistControl.Instance.enabled = true;
        TimeController.Instance.RestoreTime();
    }
}
