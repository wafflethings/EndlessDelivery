using EndlessDelivery.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace EndlessDelivery.Gameplay.Firework;

public class MaterialRandomiser : MonoBehaviour
{
    [SerializeField] private Color[] _glowColours;
    [SerializeField] private Color[] _particleStartColours;
    [SerializeField] private Color[] _particleEndColours;
    [SerializeField] private Material[] _materials;
    [SerializeField] private SpriteRenderer _glowRenderer;
    [SerializeField] private MeshRenderer _renderer;
    [SerializeField] private ParticleSystem _particleSystem;

    private void Awake()
    {
        int index = Random.Range(0, _materials.Length);
        _renderer.material = _materials[index];
        _glowRenderer.material.color = _glowColours[index];
        ParticleSystem.TrailModule trails = _particleSystem.trails;
        trails.colorOverTrail = new ParticleSystem.MinMaxGradient(_particleStartColours[index], _particleEndColours[index]);
    }
}
