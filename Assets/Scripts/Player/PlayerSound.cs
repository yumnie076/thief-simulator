using UnityEngine;

/// <summary>
/// Manages the player's audible footprint radius.
/// Attach to the Player GameObject.
/// </summary>
public class PlayerSound : MonoBehaviour
{
    public float baseSoundRadius = 1.0f;

    [Header("Visual")]
    public SoundRadius soundRadiusVisualizer; // assign the child SoundRadius component

    private PlayerController _controller;

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (_controller == null) return;

        float sneakMult = _controller.IsSneaking ? _controller.sneakSoundMultiplier : 1f;
        float currentRadius = baseSoundRadius * _controller.soundMult * sneakMult;

        soundRadiusVisualizer?.SetRadius(currentRadius);
    }

    public void UpdateRadius()
    {
        // Explicit call from RecalculateWeight — radius will refresh next Update()
    }

    /// <summary>Returns current world-space radius for guard hearing check.</summary>
    public float GetCurrentRadius()
    {
        float sneakMult = (_controller != null && _controller.IsSneaking)
            ? _controller.sneakSoundMultiplier : 1f;
        return baseSoundRadius * (_controller?.soundMult ?? 1f) * sneakMult;
    }
}
