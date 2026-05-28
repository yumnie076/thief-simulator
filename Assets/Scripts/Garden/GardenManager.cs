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

    [Header("Custom Map Setup")]
    [Tooltip("Vink dit aan als je zelf de map hebt gebouwd in de Scene View. Er spawnen dan geen willekeurige objecten.")]
    public bool useCustomMap = false;

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
        if (useCustomMap)
        {
            // Register all manually placed objects
            placedObjects.Clear();
            var allObjs = FindObjectsByType<GardenObject>(FindObjectsSortMode.None);
            foreach (var obj in allObjs)
            {
                placedObjects.Add(obj);
            }
            SpawnPlayer();
            var uic = FindAnyObjectByType<BuildPhaseUI>();
            if (uic != null) uic.UpdateActions(actionsRemaining);
            return;
        }

        int state = 1; // default Medium
        if (GameManager.Instance != null)
        {
            state = GameManager.Instance.GardenStartState;
        }

        // Clean up any previously spawned objects for Replay functionality
        foreach (var obj in placedObjects)
        {
            if (obj != null) Destroy(obj.gameObject);
        }
        placedObjects.Clear();
        
        if (state == 0) // Hard (Level 3)
        {
            GardenWidth = 14f;
            GardenHeight = 14f;
            actionsRemaining = 18; // More actions needed because of all the trash
            for (int i=0; i<1; i++) SpawnRandomObject(GardenObject.ObjectType.Bush);
            for (int i=0; i<5; i++) SpawnRandomObject(GardenObject.ObjectType.Trash); // Veel afval op hard
        }
        else if (state == 1) // Medium (Level 2)
        {
            GardenWidth = 18f;
            GardenHeight = 18f;
            actionsRemaining = 12; // Moderate budget
            for (int i=0; i<3; i++) SpawnRandomObject(GardenObject.ObjectType.Bush);
            for (int i=0; i<2; i++) SpawnRandomObject(GardenObject.ObjectType.Flower);
            for (int i=0; i<3; i++) SpawnRandomObject(GardenObject.ObjectType.Trash);
        }
        else if (state == 2) // Easy (Level 1)
        {
            GardenWidth = 24f;
            GardenHeight = 24f;
            actionsRemaining = 6; // Very tight budget, but empty clean garden
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

    [ContextMenu("🔨 Genereer Achtergrond (Editor)")]
    public void EditorGenerateGrid()
    {
        GenerateBackgroundGrid(1); // 1 is medium mix
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
                sr.sortingOrder = -1000;

                bool isPaved;
                if (state == 0) isPaved = Random.value < 0.9f;
                else if (state == 1) isPaved = Random.value < 0.5f;
                else isPaved = Random.value < 0.1f;

                sr.sprite = isPaved ? pavedSprite : grassSprite;
                
                // Fix overlap: force scale to exactly 1x1 unit
                if (sr.sprite != null && sr.sprite.bounds.size.x > 0)
                {
                    float scaleX = 1f / sr.sprite.bounds.size.x;
                    float scaleY = 1f / sr.sprite.bounds.size.y;
                    tile.transform.localScale = new Vector3(scaleX, scaleY, 1f);
                }

                // Add subtle color variation to grass tiles
                if (!isPaved)
                {
                    float variation = Random.Range(-0.08f, 0.08f);
                    sr.color = new Color(0.85f + variation, 1f + variation * 0.5f, 0.8f + variation);

                    // Randomly spawn tiny decorative daisies on some grass tiles
                    if (Random.value < 0.15f)
                    {
                        SpawnDaisy(gridParent.transform, x + 0.5f, y + 0.5f);
                    }
                }
                else
                {
                    // Slight grey variation on paved
                    float pv = Random.Range(-0.05f, 0.05f);
                    sr.color = new Color(0.9f + pv, 0.88f + pv, 0.85f + pv);
                }
            }
        }

        // Spawn fence border around garden
        SpawnFence(gridParent.transform);

        // Spawn neighborhood background outside the garden
        SpawnNeighborhood(gridParent.transform);
    }

    private void SpawnNeighborhood(Transform parent)
    {
        Sprite pavedSprite = Resources.Load<Sprite>("EgelGame/tile_paved");
        
        // Boundaries of the neighborhood (15 tiles in each direction around the garden)
        int minX = -10, maxX = Mathf.CeilToInt(GardenWidth) + 10;
        int minY = -10, maxY = Mathf.CeilToInt(GardenHeight) + 10;

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                // Skip if inside the garden
                if (x >= 0 && x < GardenWidth && y >= 0 && y < GardenHeight) continue;

                GameObject tile = new GameObject($"Neighbourhood_{x}_{y}");
                tile.transform.SetParent(parent);
                tile.transform.position = new Vector3(x + 0.5f, y + 0.5f, 0);
                var sr = tile.AddComponent<SpriteRenderer>();
                sr.sortingOrder = -1001; // Behind everything
                sr.sprite = pavedSprite;

                // Make asphalt dark grey
                float p = Random.Range(-0.02f, 0.02f);
                sr.color = new Color(0.3f + p, 0.3f + p, 0.3f + p);

                // Fix scale
                if (sr.sprite != null && sr.sprite.bounds.size.x > 0)
                {
                    float scaleX = 1f / sr.sprite.bounds.size.x;
                    float scaleY = 1f / sr.sprite.bounds.size.y;
                    tile.transform.localScale = new Vector3(scaleX, scaleY, 1f);
                }

                // Add random houses outside the immediate sidewalk
                bool isSidewalk = (x >= -2 && x <= GardenWidth + 1 && y >= -2 && y <= GardenHeight + 1);
                if (!isSidewalk)
                {
                    // Randomly spawn a house block (1 in 30 chance per tile)
                    if (Random.value < 0.03f)
                    {
                        SpawnProceduralHouse(parent, x + 0.5f, y + 0.5f);
                    }
                }
                else
                {
                    // Sidewalk color (lighter grey)
                    sr.color = new Color(0.6f + p, 0.6f + p, 0.6f + p);
                }
            }
        }
    }

    private void SpawnProceduralHouse(Transform parent, float cx, float cy)
    {
        var house = new GameObject("BgHouse");
        house.transform.SetParent(parent);
        house.transform.position = new Vector3(cx, cy, 0);

        var sr = house.AddComponent<SpriteRenderer>();
        sr.sortingOrder = -1000; // Above asphalt

        // Generate simple 16x16 house sprite
        var tex = new Texture2D(16, 16);
        tex.filterMode = FilterMode.Point;
        
        Color wallColor = Random.value < 0.5f ? new Color(0.6f, 0.2f, 0.15f) : new Color(0.7f, 0.6f, 0.5f); // Brick or beige
        Color roofColor = new Color(0.2f, 0.2f, 0.2f); // Dark roof

        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                if (y > 10)
                {
                    // Roof
                    int roofWidth = 16 - (y - 10) * 2;
                    int roofStart = (16 - roofWidth) / 2;
                    if (x >= roofStart && x < roofStart + roofWidth) tex.SetPixel(x, y, roofColor);
                    else tex.SetPixel(x, y, Color.clear);
                }
                else
                {
                    // Walls
                    if (x > 1 && x < 14) tex.SetPixel(x, y, wallColor);
                    else tex.SetPixel(x, y, Color.clear);
                }
            }
        }

        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
        house.transform.localScale = new Vector3(2f, 2f, 1f); // Make houses large
    }

    private void SpawnDaisy(Transform parent, float cx, float cy)
    {
        var daisy = new GameObject("Daisy");
        daisy.transform.SetParent(parent);
        float ox = Random.Range(-0.3f, 0.3f);
        float oy = Random.Range(-0.3f, 0.3f);
        daisy.transform.position = new Vector3(cx + ox, cy + oy, 0);

        var sr = daisy.AddComponent<SpriteRenderer>();
        sr.sortingOrder = -999; // Just above grass, below everything else

        // Generate tiny 6x6 daisy sprite
        var tex = new Texture2D(6, 6);
        tex.filterMode = FilterMode.Point;
        for (int y = 0; y < 6; y++)
            for (int x = 0; x < 6; x++)
                tex.SetPixel(x, y, Color.clear);

        Color petal = Random.value < 0.5f ? Color.white : new Color(1f, 0.95f, 0.5f);
        Color center = new Color(1f, 0.85f, 0.2f);

        // Simple cross/star shape
        tex.SetPixel(3, 5, petal); tex.SetPixel(3, 4, petal); // top
        tex.SetPixel(3, 0, petal); tex.SetPixel(3, 1, petal); // bottom
        tex.SetPixel(0, 3, petal); tex.SetPixel(1, 3, petal); // left
        tex.SetPixel(5, 3, petal); tex.SetPixel(4, 3, petal); // right
        tex.SetPixel(3, 3, center); tex.SetPixel(2, 3, center);
        tex.SetPixel(3, 2, center); tex.SetPixel(2, 2, center);

        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 6, 6), new Vector2(0.5f, 0.5f), 16f);
        daisy.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

        // Add tiny sway animation
        var anim = daisy.AddComponent<GardenAnimator>();
        anim.Setup(GardenAnimator.AnimType.Sway);
    }

    private void SpawnFence(Transform parent)
    {
        Color fenceColor = new Color(0.45f, 0.28f, 0.12f); // Warm wood brown
        Color fenceDark = new Color(0.3f, 0.18f, 0.08f);

        // Create fence post sprite (reusable)
        var fenceTex = new Texture2D(4, 16);
        fenceTex.filterMode = FilterMode.Point;
        for (int y = 0; y < 16; y++)
            for (int x = 0; x < 4; x++)
                fenceTex.SetPixel(x, y, x == 0 || x == 3 ? fenceDark : fenceColor);
        fenceTex.Apply();
        Sprite fenceSprite = Sprite.Create(fenceTex, new Rect(0, 0, 4, 16), new Vector2(0.5f, 0.5f), 16f);

        // Horizontal rail sprite
        var railTex = new Texture2D(16, 3);
        railTex.filterMode = FilterMode.Point;
        for (int y = 0; y < 3; y++)
            for (int x = 0; x < 16; x++)
                railTex.SetPixel(x, y, y == 0 ? fenceDark : fenceColor);
        railTex.Apply();
        Sprite railSprite = Sprite.Create(railTex, new Rect(0, 0, 16, 3), new Vector2(0.5f, 0.5f), 16f);

        // Bottom and top fence rails
        for (float x = 0; x < GardenWidth; x += 1f)
        {
            CreateFencePiece(parent, railSprite, x + 0.5f, -0.1f, fenceColor);
            CreateFencePiece(parent, railSprite, x + 0.5f, GardenHeight + 0.1f, fenceColor);
        }

        // Left and right fence posts
        for (float y = 0; y < GardenHeight; y += 1f)
        {
            CreateFencePiece(parent, fenceSprite, -0.1f, y + 0.5f, fenceColor);
            CreateFencePiece(parent, fenceSprite, GardenWidth + 0.1f, y + 0.5f, fenceColor);
        }
    }

    private void CreateFencePiece(Transform parent, Sprite sprite, float x, float y, Color tint)
    {
        var go = new GameObject("Fence");
        go.transform.SetParent(parent);
        go.transform.position = new Vector3(x, y, 0);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = tint;
        sr.sortingOrder = -998; // Above grass, below garden objects
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

        // Visual juice
        if (objType == GardenObject.ObjectType.Tree || objType == GardenObject.ObjectType.Bush)
            SimpleParticle.SpawnLeaves(position, 6);
        else if (objType == GardenObject.ObjectType.Flower || objType == GardenObject.ObjectType.Sunflower)
            SimpleParticle.SpawnSparkles(position, 5);
        else if (objType == GardenObject.ObjectType.Pond)
            SimpleParticle.SpawnSplash(position, 6);
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
            case PlaceableTool.ToolType.Sunflower: return GardenObject.ObjectType.Sunflower;
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
