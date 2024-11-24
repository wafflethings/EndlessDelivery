using System;
using System.Collections;
using System.Threading.Tasks;
using AtlasLib.Utils;
using EndlessDelivery.Api.Requests;
using EndlessDelivery.Online;
using UnityEngine;

namespace EndlessDelivery.UI;

public class UpdateScreen : MonoBehaviour
{
    private void Awake()
    {
        gameObject.SetActive(false);
        CoroutineRunner.Instance.StartCoroutine(EnableIfNeeded());
    }

    private IEnumerator EnableIfNeeded()
    {
        Task<bool> onlineTask = OnlineFunctionality.Context.ServerOnline();
        yield return new WaitUntil(() => onlineTask.IsCompleted);

        if (!onlineTask.Result)
        {
            yield break;
        }

        Task<bool> updateRequiredTask = OnlineFunctionality.Context.UpdateRequired(Plugin.Version);
        yield return new WaitUntil(() => updateRequiredTask.IsCompleted);

        gameObject.SetActive(updateRequiredTask.Result);
        GameStateManager.Instance.RegisterState(new GameState("pause", [gameObject])
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

    public void OpenThunderstore() => Application.OpenURL("https://thunderstore.io/c/ultrakill/p/Waff1e/Divine_Delivery/");
}
