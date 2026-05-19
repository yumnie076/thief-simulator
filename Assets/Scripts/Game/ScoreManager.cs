using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int TotalScore { get; private set; }
    public int TotalWeight { get; private set; }
    public int ItemCount { get; private set; }
    public int TotalItemsInLevel { get; set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // Find all items at start
        TotalItemsInLevel = FindObjectsByType<ItemPickup>(FindObjectsSortMode.None).Length;
    }

    public void AddItem(int value, int weight)
    {
        TotalScore += value;
        TotalWeight += weight;
        ItemCount++;
        UIManager.Instance?.RefreshHUD(TotalScore, TotalWeight, ItemCount);
    }
}
