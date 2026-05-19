using UnityEngine;

/// <summary>
/// Simplified tracker since we moved to the freeform ecosystem model.
/// </summary>
public class BiodiversityTracker : MonoBehaviour
{
    public static BiodiversityTracker Instance { get; private set; }

    public int BiodiversityScore { get; private set; }
    public event System.Action<float> OnScoreChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (GardenManager.Instance != null)
        {
            GardenManager.Instance.OnObjectPlaced += HandleObjectPlaced;
        }
    }

    private void OnDestroy()
    {
        if (GardenManager.Instance != null)
        {
            GardenManager.Instance.OnObjectPlaced -= HandleObjectPlaced;
        }
    }

    private void HandleObjectPlaced(GardenObject obj)
    {
        RecalculateScore();
    }

    public void RecalculateScore()
    {
        if (GardenManager.Instance == null) return;
        
        BiodiversityScore = GardenManager.Instance.PlacedObjects.Count * 10;
        if (EcosystemManager.Instance != null)
            BiodiversityScore += EcosystemManager.Instance.AllInsects.Count * 20;

        OnScoreChanged?.Invoke(BiodiversityScore);
    }
}
