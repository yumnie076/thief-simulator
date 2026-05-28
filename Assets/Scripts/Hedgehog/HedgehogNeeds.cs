using System;
using UnityEngine;

/// <summary>
/// Tracks the hedgehog's hunger and safety stats.
/// Hunger starts at 50 and decreases over time (8/sec).
/// Provides events for UI binding.
/// </summary>
public class HedgehogNeeds : MonoBehaviour
{
    // ── Events for UI binding ────────────────────────────────────
    public event Action<float> OnHungerChanged;
    public event Action<float> OnSafetyChanged;

    // ── Stats ────────────────────────────────────────────────────
    [SerializeField] private float _hunger = 50f;
    [SerializeField] private float _safety = 100f;

    /// <summary>Rate at which hunger decreases per second.</summary>
    private const float HungerDecayRate = 3.5f;

    public float Hunger
    {
        get => _hunger;
        private set
        {
            _hunger = Mathf.Clamp(value, 0f, 100f);
            OnHungerChanged?.Invoke(_hunger);
        }
    }

    public float Safety
    {
        get => _safety;
        set
        {
            _safety = Mathf.Clamp(value, 0f, 100f);
            OnSafetyChanged?.Invoke(_safety);
        }
    }

    private void Update()
    {
        // Hunger decays over time
        Hunger -= HungerDecayRate * Time.deltaTime;
    }

    /// <summary>Feed the hedgehog, increasing hunger satisfaction.</summary>
    public void Feed(float amount)
    {
        Hunger += amount;
    }

    /// <summary>Give the hedgehog water, increasing hunger satisfaction.</summary>
    public void Drink(float amount)
    {
        Hunger += amount;
    }

    /// <summary>Mark the hedgehog as safe (shelter reached).</summary>
    public void SetSafe()
    {
        Safety = 100f;
    }
}
