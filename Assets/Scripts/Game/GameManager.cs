using UnityEngine;

/// <summary>
/// Central singleton for Egel op Expeditie.
/// Holds the garden start state chosen by the player in Phase 1.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    /// <summary>0 = hard (80% paved), 1 = medium (50/30/20), 2 = easy (20/50/30)</summary>
    public int GardenStartState { get; set; } = 1;

    /// <summary>Whether the hedgehog successfully found shelter.</summary>
    public bool HedgehogSafe { get; set; } = false;

    /// <summary>Total score accumulated across all phases.</summary>
    public float TotalScore { get; set; } = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>Reset all runtime state for a new playthrough.</summary>
    public void ResetGame()
    {
        GardenStartState = 1;
        HedgehogSafe = false;
        TotalScore = 0f;
    }
}
