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
        int state = 1; // default Medium
        if (GameManager.Instance != null)
        {
            state = GameManager.Instance.GardenStartState;
        }

        if (state == 0) // Hard
        {
            GardenWidth = 14f;
            GardenHeight = 14f;
            actionsRemaining = 12;
            for (int i=0; i<1; i++) SpawnRandomObject(GardenObject.ObjectType.Bush);
            for (int i=0; i<5; i++) SpawnRandomObject(GardenObject.ObjectType.Trash); // Veel afval op hard
        }
        else if (state == 1) // Medium
        {
            GardenWidth = 18f;
            GardenHeight = 18f;
            actionsRemaining = 18;
            for (int i=0; i<3; i++) SpawnRandomObject(GardenObject.ObjectType.Bush);
            for (int i=0; i<2; i++) SpawnRandomObject(GardenObject.ObjectType.Flower);
            for (int i=0; i<3; i++) SpawnRandomObject(GardenObject.ObjectType.Trash);
        }
        else if (state == 2) // Easy
        {
            GardenWidth = 24f;
            GardenHeight = 24f;
            actionsRemaining = 25;
            for (int i=0; i<5; i++) SpawnRandomObject(GardenObject.ObjectType.Bush);
            for (int i=0; i<5; i++) SpawnRandomObject(GardenObject.ObjectType.Flower);
            for (int i=0; i<1; i++) SpawnRandomObject(GardenObject.ObjectType.Tree);
            for (int i=0; i<1; i++) SpawnRandomObject(GardenObject.ObjectType.Trash);
        }

        GenerateBackgroundGrid(state);
        SpawnPlayer();

        // Update UI immediately
        var ui = FindAnyObjectByType<BuildPhaseUI>();
        if (ui != null) ui.UpdateActions(actionsRemaining);
    }

    private void GenerateBackgroundGrid(int state)
    {
        // Destroy bootstrapper's fallback background
        var oldBg = GameObject.Find("Background");
        if (oldBg != null) Destroy(oldBg);

        // Destroy previously generated grid if any
        var oldGrid = transform.Find("BackgroundGrid");
        if (oldGrid != null) Destroy(oldGrid.gameObject);

        GameObject gridParent = new GameObject("BackgroundGrid");
        gridParent.transform.SetParent(transform);
        
        Sprite grassSprite = Resources.Load<Sprite>("EgelGame/tile_grass");
        Sprite pavedSprite = Resources.Load<Sprite>("EgelGame/tile_paved");

        for (int x = 0; x < GardenWidth; x++)
        {
            for (int y = 0; y < GardenHeight; y++)
            {
                GameObject tile = new GameObject($"Tile_{x}_{y}");
                tile.transform.SetParent(gridParent.transform);
                tile.transform.position = new Vector3(x + 0.5f, y + 0.5f, 0);
                var sr = tile.AddComponent<SpriteRenderer>();
                sr.sortingOrder = -1000; // Force strictly behind ALL objects (GardenObject can be down to -200)

                if (state == 0) // Hard: 90% paved, 10% grass
                {
                    sr.sprite = Random.value < 0.9f ? pavedSprite : grassSprite;
                }
                else if (state == 1) // Medium: 50% paved, 50% grass
                {
                    sr.sprite = Random.value < 0.5f ? pavedSprite : grassSprite;
                }
                else // Easy: 10% paved, 90% grass
                {
                    sr.sprite = Random.value < 0.1f ? pavedSprite : grassSprite;
                }
                
                // Fix overlap: force scale to exactly 1x1 unit in the world
                if (sr.sprite != null && sr.sprite.bounds.size.x > 0)
                {
                    float scaleX = 1f / sr.sprite.bounds.size.x;
                    float scaleY = 1f / sr.sprite.bounds.size.y;
                    tile.transform.localScale = new Vector3(scaleX, scaleY, 1f);
                }
            }
        }
    }

    private void SpawnRandomObject(GardenObject.ObjectType type)
    {
        Vector3 randomPos = new Vector3(Random.Range(2f, GardenWidth-2f), Random.Range(2f, GardenHeight-2f), 0);
        SpawnObject(type, randomPos);
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

        if (AudioManager.Instance != null) AudioManager.Instance.PlayPlop();

        // Show education popup
        string factKey = GetFactKey(selectedTool);
        if (factKey != null && EducationPopup.Instance != null)
            EducationPopup.Instance.Show(factKey);
    }

    public GardenObject SpawnObject(GardenObject.ObjectType type, Vector3 position)
    {
        GameObject go = new GameObject($"Object_{type}");
        go.transform.SetParent(transform);
        go.transform.position = position;

        GardenObject obj = go.AddComponent<GardenObject>();
        obj.Initialize(type);
        
        placedObjects.Add(obj);

        // Spawn ecosystem animals
        if (type == GardenObject.ObjectType.Pond)
        {
            var frog = new GameObject("Frog");
            frog.transform.position = position + new Vector3(0, 0.5f, 0);
            frog.transform.SetParent(transform);
            frog.AddComponent<FrogAI>();
        }
        else if (type == GardenObject.ObjectType.Flower)
        {
            var bee = new GameObject("Bee");
            bee.transform.position = position + new Vector3(0, 0.5f, 0);
            bee.transform.SetParent(transform);
            bee.AddComponent<BeeAI>();
        }

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
                if (obj.Type == GardenObject.ObjectType.Trash && ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.AddBiodiversity(10f);
                    Debug.Log("[GardenManager] Trash removed! +10 Biodiversity");
                }

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
