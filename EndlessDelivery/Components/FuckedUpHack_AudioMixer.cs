using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;

namespace EndlessDelivery.Components;

public class FuckedUpHack_AudioMixer : MonoBehaviour
{
    private static AudioMixerGroup? s_mixer;

    private void Awake()
    {
        s_mixer ??= Addressables.Instance.LoadAssetAsync<AudioMixerGroup>("MusicAudio").WaitForCompletion();

        foreach (AudioSource audioSource in GetComponents<AudioSource>())
        {
            audioSource.outputAudioMixerGroup = s_mixer;
        }
    }
}
