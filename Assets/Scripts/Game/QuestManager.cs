using System;
using UnityEngine;

/// <summary>
/// Tracks ecosystem goals for the player.
/// </summary>
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public int FlowersPlanted { get; private set; }
    public int TargetFlowers = 3;

    public int InsectsSpawned { get; private set; }
    public int TargetInsects = 2;

    public int BugsEaten { get; private set; }
    public int TargetBugsEaten = 3;

    public event Action OnQuestUpdated;
    public event Action OnAllQuestsCompleted;

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
        if (obj.Type == GardenObject.ObjectType.Flower)
        {
            FlowersPlanted++;
            OnQuestUpdated?.Invoke();
            CheckCompletion();
        }
    }

    public void ReportInsectSpawned()
    {
        InsectsSpawned++;
        OnQuestUpdated?.Invoke();
        CheckCompletion();
    }

    private void Update()
    {
        // Poll hedgehog bugs eaten (since Hedgehog is spawned later and dynamically)
        var hedgehog = FindAnyObjectByType<HedgehogAI>();
        if (hedgehog != null && hedgehog.bugsEaten > BugsEaten)
        {
            BugsEaten = hedgehog.bugsEaten;
            OnQuestUpdated?.Invoke();
            CheckCompletion();
        }
    }

    private void CheckCompletion()
    {
        if (FlowersPlanted >= TargetFlowers && 
            InsectsSpawned >= TargetInsects && 
            BugsEaten >= TargetBugsEaten)
        {
            OnAllQuestsCompleted?.Invoke();
        }
    }
}
