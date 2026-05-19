using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Singleton managing the freeform garden ecosystem.
/// Spawns objects freely and tracks their positions.
/// </summary>
public class GardenManager : MonoBehaviour
{
    public static GardenManager Instance { get; private set; }

    // ── Actions & tools ──────────────────────────────────────────
    public int actionsRemaining = 15;
    public PlaceableTool.ToolType selectedTool = PlaceableTool.ToolType.Flower;

    // ── Events ───────────────────────────────────────────────────
    public event Action<int> OnActionUsed;
    public event Action<GardenObject> OnObjectPlaced;

    // ── Ecosystem Tracking ───────────────────────────────────────
    private List<GardenObject> placedObjects = new List<GardenObject>();
    public IReadOnlyList<GardenObject> PlacedObjects => placedObjects;

    public float GardenWidth = 12f;
    public float GardenHeight = 12f;

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
        if (PhaseController.Instance != null)
            PhaseController.Instance.OnPhaseChanged += OnPhaseChanged;
    }

    private void OnDestroy()
    {
        if (PhaseController.Instance != null)
            PhaseController.Instance.OnPhaseChanged -= OnPhaseChanged;
    }

    private void OnPhaseChanged(PhaseController.GamePhase phase)
    {
        if (phase == PhaseController.GamePhase.GardenBuild)
        {
            InitializeGarden();
        }
    }

    public void InitializeGarden()
    {
        SpawnPlayer();
        // Generate a few random bushes to start with
        for (int i=0; i<5; i++)
        {
            Vector3 randomPos = new Vector3(Random.Range(2f, GardenWidth-2f), Random.Range(2f, GardenHeight-2f), 0);
            SpawnObject(GardenObject.ObjectType.Bush, randomPos);
        }
    }

    private void SpawnPlayer()
    {
        var playerGO = new GameObject("Player");
        playerGO.transform.position = new Vector3(GardenWidth / 2f, GardenHeight / 2f, -1f);
        
        var sr = playerGO.AddComponent<SpriteRenderer>();
        var tex = Resources.Load<Sprite>("EgelGame/player_gardener"); 
        if (tex != null) sr.sprite = tex;
        sr.sortingOrder = 100;

        var rb = playerGO.AddComponent<Rigidbody2D>();
        var pc = playerGO.AddComponent<PlayerController>();
        var pa = playerGO.AddComponent<PlayerAction>();

        var cam = Camera.main;
        if (cam != null)
        {
            var cf = cam.GetComponent<CameraFollow>();
            if (cf != null) cf.target = playerGO.transform;
        }
    }

    public void SelectTool(PlaceableTool.ToolType tool)
    {
        selectedTool = tool;
    }

    /// <summary>
    /// Attempts to place the selected tool at the given position.
    /// Called by PlayerAction.
    /// </summary>
    public void PlaceToolAt(Vector3 position)
    {
        if (actionsRemaining <= 0) return;
        
        // Remove object tool
        if (selectedTool == PlaceableTool.ToolType.RemoveTile)
        {
            RemoveObjectAt(position);
            return;
        }

        int cost = PlaceableTool.GetCost(selectedTool);
        if (cost > actionsRemaining) return;

        // Check for collisions (don't place on top of another object)
        Collider2D hit = Physics2D.OverlapCircle(position, 0.4f);
        if (hit != null && hit.GetComponent<GardenObject>() != null && hit.isTrigger == false)
        {
            // Too close to a solid object
            return; 
        }

        GardenObject.ObjectType objType = MapToolToObjectType(selectedTool);
        GardenObject newObj = SpawnObject(objType, position);

        actionsRemaining -= cost;
        OnActionUsed?.Invoke(actionsRemaining);
        OnObjectPlaced?.Invoke(newObj);

        // Show education popup
        string factKey = GetFactKey(selectedTool);
        if (factKey != null && EducationPopup.Instance != null)
            EducationPopup.Instance.Show(factKey);
    }

    private GardenObject SpawnObject(GardenObject.ObjectType type, Vector3 position)
    {
        GameObject go = new GameObject($"Object_{type}");
        go.transform.SetParent(transform);
        go.transform.position = position;

        GardenObject obj = go.AddComponent<GardenObject>();
        obj.Initialize(type);
        
        placedObjects.Add(obj);
        return obj;
    }

    private void RemoveObjectAt(Vector3 position)
    {
        Collider2D hit = Physics2D.OverlapCircle(position, 0.4f);
        if (hit != null)
        {
            GardenObject obj = hit.GetComponent<GardenObject>();
            if (obj != null)
            {
                placedObjects.Remove(obj);
                Destroy(obj.gameObject);
                
                actionsRemaining -= 1; // Removal costs 1 action
                OnActionUsed?.Invoke(actionsRemaining);
            }
        }
    }

    private GardenObject.ObjectType MapToolToObjectType(PlaceableTool.ToolType tool)
    {
        switch (tool)
        {
            case PlaceableTool.ToolType.Flower: return GardenObject.ObjectType.Flower;
            case PlaceableTool.ToolType.Bush: return GardenObject.ObjectType.Bush;
            case PlaceableTool.ToolType.Tree: return GardenObject.ObjectType.Tree;
            case PlaceableTool.ToolType.Pond: return GardenObject.ObjectType.Pond;
            case PlaceableTool.ToolType.LeafPile: return GardenObject.ObjectType.LeafPile;
            case PlaceableTool.ToolType.HedgehogHouse: return GardenObject.ObjectType.HedgehogHouse;
            default: return GardenObject.ObjectType.Paved;
        }
    }

    private string GetFactKey(PlaceableTool.ToolType tool)
    {
        switch (tool)
        {
            case PlaceableTool.ToolType.RemoveTile:     return "RemoveTile";
            case PlaceableTool.ToolType.Flower:         return "Flower";
            case PlaceableTool.ToolType.Bush:            return "Bush";
            case PlaceableTool.ToolType.Tree:            return "Tree";
            case PlaceableTool.ToolType.Pond:            return "Pond";
            case PlaceableTool.ToolType.LeafPile:        return "LeafPile";
            case PlaceableTool.ToolType.HedgehogHouse:   return "House";
            default: return null;
        }
    }
}
