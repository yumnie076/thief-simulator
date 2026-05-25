using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Listens to GardenManager and spawns insects based on placed flora.
/// </summary>
public class EcosystemManager : MonoBehaviour
{
    public static EcosystemManager Instance { get; private set; }
    
    private List<Insect> allInsects = new List<Insect>();
    public IReadOnlyList<Insect> AllInsects => allInsects;

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
        // 50% chance to spawn an animal per object
        if (Random.value > 0.5f) return;

        if (obj.Type == GardenObject.ObjectType.Flower)
        {
            SpawnInsect(Insect.InsectType.Butterfly, obj.transform.position);
        }
        else if (obj.Type == GardenObject.ObjectType.Bush)
        {
            SpawnInsect(Insect.InsectType.Bug, obj.transform.position);
        }
        else if (obj.Type == GardenObject.ObjectType.LeafPile)
        {
            SpawnInsect(Insect.InsectType.Spider, obj.transform.position);
        }
        else if (obj.Type == GardenObject.ObjectType.Pond)
        {
            // Spawn Snail or Frog (50% chance each)
            var type = Random.value > 0.5f ? Insect.InsectType.Snail : Insect.InsectType.Frog;
            SpawnInsect(type, obj.transform.position);
        }
        else if (obj.Type == GardenObject.ObjectType.Tree)
        {
            SpawnInsect(Insect.InsectType.Bird, obj.transform.position);
        }
    }

    private void SpawnInsect(Insect.InsectType type, Vector3 position)
    {
        GameObject go = new GameObject($"Insect_{type}");
        go.transform.position = position;
        go.transform.SetParent(transform);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Insect.GetSpriteForType(type);

        var insect = go.AddComponent<Insect>();
        insect.type = type;

        allInsects.Add(insect);
        
        if (QuestManager.Instance != null) 
            QuestManager.Instance.ReportInsectSpawned();
    }

    public void RemoveInsect(Insect insect)
    {
        allInsects.Remove(insect);
        Destroy(insect.gameObject);
    }
}
