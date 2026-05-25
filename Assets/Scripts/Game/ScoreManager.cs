using UnityEngine;

/// <summary>
/// Tracks all scoring categories for Egel op Expeditie.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public float BiodiversityScore { get; set; } = 0f;
    public float FoodScore { get; set; } = 0f;
    public float WaterScore { get; set; } = 0f;
    public float ShelterScore { get; set; } = 0f;

    public float TotalScore => BiodiversityScore + FoodScore + WaterScore + ShelterScore;

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

    public void AddFood(float points)
    {
        FoodScore += points;
        OnScoreChanged?.Invoke(TotalScore);
    }

    public void AddWater(float points)
    {
        WaterScore += points;
        OnScoreChanged?.Invoke(TotalScore);
    }

    public void AddShelter(float points)
    {
        ShelterScore += points;
        OnScoreChanged?.Invoke(TotalScore);
    }

    public void AddBiodiversity(float points)
    {
        BiodiversityScore += points;
        OnScoreChanged?.Invoke(TotalScore);
    }

    public void SetBiodiversity(float score)
    {
        BiodiversityScore = score;
        OnScoreChanged?.Invoke(TotalScore);
    }

    public void ResetScores()
    {
        BiodiversityScore = 0f;
        FoodScore = 0f;
        WaterScore = 0f;
        ShelterScore = 0f;
        OnScoreChanged?.Invoke(0f);
    }
}
