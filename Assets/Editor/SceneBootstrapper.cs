using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using TMPro;

public static class SceneBootstrapper
{
    [MenuItem("ThiefSim/Build Scene")]
    public static void BuildScene()
    {
        EnsureLayer("Walls");
        EnsureLayer("Player");
        EnsureLayer("Guard");
        EnsureLayer("Items");

        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            if (go.transform.parent == null) Object.DestroyImmediate(go);

        BuildCamera();
        BuildLighting();
        BuildMap();
        BuildPlayer();
        BuildGuard();
        BuildItems();
        BuildExit();
        BuildKeyAndDoor();
        BuildUI();

        Debug.Log("[ThiefSim] Scene built! Press Play.");
    }

    // ── CAMERA ────────────────────────────────────────────────────────────────
    static void BuildCamera()
    {
        var go = new GameObject("Main Camera");
        go.tag = "MainCamera";
        var cam = go.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 12f;
        cam.backgroundColor = new Color(0.05f, 0.05f, 0.10f);
        go.AddComponent<UniversalAdditionalCameraData>();
        go.transform.position = new Vector3(0, 0, -10);
    }

    // ── LIGHTING ──────────────────────────────────────────────────────────────
    static void BuildLighting()
    {
        var gl = new GameObject("GlobalLight");
        var g = gl.AddComponent<Light2D>();
        g.lightType = Light2D.LightType.Global;
        g.intensity = 0.15f;
        g.color = new Color(0.04f, 0.04f, 0.08f); // Very dark nights (#0a0a14)

        // Room point lights - warm orange-yellow
        Color warm = new Color(1f, 0.66f, 0.26f); // #ffaa44
        RoomLight("LampLiving",  new Vector2(-15, -10), warm, 6f);
        RoomLight("LampKitchen", new Vector2(-15,  12), warm, 6f);
        RoomLight("LampStudy",   new Vector2(-15,  -3), warm, 6f);
        RoomLight("LampBed1",    new Vector2( 15, -10), warm, 6f);
        RoomLight("LampMaster",  new Vector2( 15,  12), warm, 6f);
        RoomLight("LampHall",    new Vector2( 0,    0), warm, 6f);
    }

    static void RoomLight(string n, Vector2 pos, Color col, float radius)
    {
        var go = new GameObject(n);
        go.transform.position = pos;
        var l = go.AddComponent<Light2D>();
        l.lightType = Light2D.LightType.Point;
        l.color = col; l.intensity = 1.1f;
        l.pointLightInnerRadius = 2f;
        l.pointLightOuterRadius = radius;
    }

    static void BuildMap()
    {
        var mapRoot = new GameObject("Map");
        var grid = mapRoot.AddComponent<Grid>();

        // Create Floor Tilemap
        var floorGO = new GameObject("Floor");
        floorGO.transform.SetParent(mapRoot.transform);
        var floorTM = floorGO.AddComponent<UnityEngine.Tilemaps.Tilemap>();
        var floorTR = floorGO.AddComponent<UnityEngine.Tilemaps.TilemapRenderer>();
        floorTR.sortingOrder = 0;

        // Create Walls Tilemap
        var wallsGO = new GameObject("Walls");
        wallsGO.transform.SetParent(mapRoot.transform);
        wallsGO.layer = LayerMask.NameToLayer("Walls");
        var wallsTM = wallsGO.AddComponent<UnityEngine.Tilemaps.Tilemap>();
        var wallsTR = wallsGO.AddComponent<UnityEngine.Tilemaps.TilemapRenderer>();
        wallsTR.sortingOrder = 3;

        var rb = wallsGO.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
        var comp = wallsGO.AddComponent<CompositeCollider2D>();
        wallsGO.AddComponent<UnityEngine.Tilemaps.TilemapCollider2D>().usedByComposite = true;

        // Generate Procedural Tiles
        var floorSprite = MakeRect();
        var wallSprite  = MakeRect();
        var floorTile = GetOrGenerateTile("FloorTile", floorSprite, new Color(0.32f, 0.22f, 0.13f));
        var wallTile = GetOrGenerateTile("WallTile", wallSprite, new Color(0.18f, 0.14f, 0.10f));
        wallTile.colliderType = UnityEngine.Tilemaps.Tile.ColliderType.Grid;

        // Layout parameters
        int width = 65, height = 45;
        int ox = -width / 2, oy = -height / 2;

        // Fill Floor
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                floorTM.SetTile(new Vector3Int(ox + x, oy + y, 0), floorTile);
                
                // Room colors
                Color floorCol = new Color(0.32f, 0.22f, 0.13f); // Default (Living)
                if (x >= 20 && y >= 25) floorCol = new Color(0.83f, 0.83f, 0.78f); // Kitchen
                else if (x >= 30 && x < 45 && y < 15) floorCol = new Color(0.36f, 0.16f, 0.16f); // Guest Bed
                else if (x < 20 && y >= 20) floorCol = new Color(0.12f, 0.24f, 0.16f); // Study
                else if (x >= 45 && y < 35) floorCol = new Color(0.18f, 0.12f, 0.32f); // Master Suite (purple)
                else if (x >= 20 && y >= 15 && y < 25) floorCol = new Color(0.16f, 0.14f, 0.09f); // Hallway
                floorTM.SetColor(new Vector3Int(ox + x, oy + y, 0), floorCol);
            }
        }

        // Draw Outer Walls
        for (int x = 0; x < width; x++)
        {
            wallsTM.SetTile(new Vector3Int(ox + x, oy, 0), wallTile);
            wallsTM.SetTile(new Vector3Int(ox + x, oy + height - 1, 0), wallTile);
        }
        for (int y = 0; y < height; y++)
        {
            wallsTM.SetTile(new Vector3Int(ox, oy + y, 0), wallTile);
            wallsTM.SetTile(new Vector3Int(ox + width - 1, oy + y, 0), wallTile);
        }

        // Helper to draw walls
        System.Action<int, int, int, int> DrawWall = (x1, y1, x2, y2) => {
            for (int x = x1; x <= x2; x++)
                for (int y = y1; y <= y2; y++)
                    wallsTM.SetTile(new Vector3Int(ox + x, oy + y, 0), wallTile);
        };

        // Draw Interior Walls
        DrawWall(20, 0, 20, 20);     // Living/Study divider
        DrawWall(20, 25, 20, 44);    // Kitchen divider
        DrawWall(30, 0, 30, 25);     // Guest Bedroom divider
        DrawWall(45, 0, 45, 35);     // Master Suite divider
        DrawWall(0, 20, 20, 20);     // Study/Living divider
        DrawWall(20, 25, 64, 25);    // Hallway/Kitchen horizontal
        DrawWall(30, 15, 45, 15);    // Guest Bedroom horizontal

        // Draw Doors (clear walls)
        System.Action<int, int, int, int> ClearWall = (x1, y1, x2, y2) => {
            for (int x = x1; x <= x2; x++)
                for (int y = y1; y <= y2; y++)
                    wallsTM.SetTile(new Vector3Int(ox + x, oy + y, 0), null);
        };

        ClearWall(20, 10, 20, 13);    // Door to Living
        ClearWall(20, 30, 20, 33);    // Door to Kitchen
        ClearWall(30, 18, 30, 21);    // Door to Guest Bed
        ClearWall(45, 10, 45, 13);    // Door to Master (LOCKED)
        ClearWall(10, 20, 13, 20);    // Door to Study
        
        // Clear exit door path
        ClearWall(30, 0, 34, 0);

        // ── Furniture ─────────────────────────────────────────────────────────
        Color table = new Color(0.45f, 0.30f, 0.15f);
        Color rug   = new Color(0.50f, 0.18f, 0.18f);
        Color shelf = new Color(0.35f, 0.22f, 0.10f);
        Color bed   = new Color(0.22f, 0.22f, 0.45f);
        Color safe  = new Color(0.40f, 0.40f, 0.45f);

        // Room Labels
        RoomLabel("LIVING ROOM",  new Vector2(ox + 10, oy + 10));
        RoomLabel("STUDY",        new Vector2(ox + 10, oy + 35));
        RoomLabel("KITCHEN",      new Vector2(ox + 40, oy + 35));
        RoomLabel("GUEST ROOM",   new Vector2(ox + 37, oy + 7));
        RoomLabel("MASTER SUITE", new Vector2(ox + 55, oy + 17));

        // Living Room (bottom left)
        Decor(mapRoot, "Rug",     new Vector2(-15, -10),  new Vector2(6, 4),    rug,   1);
        Decor(mapRoot, "Table",   new Vector2(-15, -10),  new Vector2(3, 1.5f), table, 2);
        Decor(mapRoot, "Sofa",    new Vector2(-15, -13),  new Vector2(4, 1.5f), new Color(0.35f, 0.22f, 0.38f), 2);
        Decor(mapRoot, "Shelf",   new Vector2(-23, -5),   new Vector2(1, 4),    shelf, 2);

        // Kitchen (top left)
        Decor(mapRoot, "Counter", new Vector2(-15, 12),   new Vector2(8, 2),    table, 2);
        Decor(mapRoot, "Stove",   new Vector2(-22, 12),   new Vector2(2, 2),    new Color(0.2f,0.2f,0.22f), 2);
        Decor(mapRoot, "Table",   new Vector2(-15, 5),    new Vector2(3, 3),    table, 2);

        // Study (mid left)
        Decor(mapRoot, "Desk",    new Vector2(-15, -3),   new Vector2(4, 2),    table, 2);
        Decor(mapRoot, "Safe",    new Vector2(-23, -3),   new Vector2(2, 2),    safe,  2);

        // Bed 1 (bottom right)
        Decor(mapRoot, "Bed",     new Vector2(15, -10),   new Vector2(3, 4),    bed,   2);
        Decor(mapRoot, "Shelf",   new Vector2(23, -5),    new Vector2(1, 4),    shelf, 2);

        // Bed 2 / Master (top right)
        Decor(mapRoot, "BigBed",  new Vector2(15, 12),    new Vector2(5, 5),    new Color(0.28f,0.15f,0.32f), 2);
        Decor(mapRoot, "Rug",     new Vector2(15, 6),     new Vector2(6, 4),    rug,   1);
    }

    static void RoomLabel(string text, Vector2 pos)
    {
        var go = new GameObject("RoomLabel_" + text);
        go.transform.position = pos;
        var txt = go.AddComponent<TMPro.TextMeshPro>();
        txt.text = text;
        txt.fontSize = 12;
        txt.alignment = TMPro.TextAlignmentOptions.Center;
        txt.color = new Color(1, 1, 1, 0.15f); // Subtle ghost text
        txt.fontStyle = FontStyles.Bold;
        var mr = txt.GetComponent<MeshRenderer>();
        if (mr) mr.sortingOrder = 1; // Below items but above floor
    }

    static void Wall(GameObject parent, string name, Vector2 pos, Vector2 size, int layer, Color col)
    {
        var go = MakeQuad(parent, name, pos, size, col, 3);
        go.layer = layer;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
        var bc = go.AddComponent<BoxCollider2D>();
        bc.size = size;
    }

    static GameObject MakeQuad(GameObject parent, string name, Vector2 pos, Vector2 size,
                                Color col, int sortOrder)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform);
        go.transform.position = pos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = MakeRect();
        sr.color = col;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = size;
        sr.sortingOrder = sortOrder;
        // Intentionally no UnlitMat() here so it catches light
        return go;
    }

    static void Decor(GameObject parent, string name, Vector2 pos, Vector2 size, Color col, int sort)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform);
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = MakeRect();
        sr.color = col;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = size;
        sr.sortingOrder = sort;
        // Intentionally no UnlitMat() here so it catches light
    }

    // ── PLAYER ────────────────────────────────────────────────────────────────
    static void BuildPlayer()
    {
        var go = new GameObject("Player");
        go.tag = "Player";
        go.layer = LayerMask.NameToLayer("Player");
        go.transform.position = new Vector3(0, -15f, 0);

        var sr = go.AddComponent<SpriteRenderer>();
        var blue = new Color(0.25f, 0.55f, 0.90f);
        sr.sprite = MakeCircle();
        sr.color = blue;
        sr.sortingOrder = 10;
        // Intentionally no UnlitMat() here so it catches light

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0; rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.4f;

        var pc = go.AddComponent<PlayerController>();
        pc.spriteUp = MakeCircle();
        pc.spriteDown = MakeCircle();
        pc.spriteLeft = MakeCircle();
        pc.spriteRight = MakeCircle();
        
        // Flashlight (Step 2 Forward Light)
        var flGO = new GameObject("Flashlight");
        flGO.transform.SetParent(go.transform, false);
        var fl = flGO.AddComponent<Light2D>();
        fl.lightType = Light2D.LightType.Point;
        fl.pointLightInnerAngle = 30f;
        fl.pointLightOuterAngle = 65f;
        fl.pointLightInnerRadius = 0.5f;
        fl.pointLightOuterRadius = 10f;
        fl.intensity = 1.2f;
        fl.color = new Color(1, 0.95f, 0.8f); // Warm flashlight
        flGO.AddComponent<PlayerFlashlight>();

        go.AddComponent<PlayerInventory>();
        var ps = go.AddComponent<PlayerSound>();

        var sc = new GameObject("SoundRadius");
        sc.transform.SetParent(go.transform);
        sc.transform.localPosition = Vector3.zero;
        ps.soundRadiusVisualizer = sc.AddComponent<SoundRadius>();

        // Camera Follow
        var cam = Camera.main;
        if (cam != null)
        {
            var cf = cam.gameObject.AddComponent<CameraFollow>();
            cf.target = go.transform;
            cf.offset = new Vector3(0, 0, -10);
            cf.smoothSpeed = 5f;
        }
    }

    // ── GUARD ─────────────────────────────────────────────────────────────────
    static void BuildGuard()
    {
        var guardRoot = new GameObject("Guards");

        // Guard 1 (Living Room patrol)
        var g1 = CreateGuard(guardRoot, "Guard1", new Vector3(-15f, -10f, 0));
        g1.GetComponent<GuardPatrol>().waypoints = CreateWaypoints(guardRoot, "WP1", new Vector2[] {
            new Vector2(-15, -10), new Vector2(-15, -5), new Vector2(-10, -5), new Vector2(-10, -10)
        });

        // Guard 2 (Kitchen patrol)
        var g2 = CreateGuard(guardRoot, "Guard2", new Vector3(15f, 10f, 0));
        g2.GetComponent<GuardPatrol>().waypoints = CreateWaypoints(guardRoot, "WP2", new Vector2[] {
            new Vector2(15, 10), new Vector2(20, 10), new Vector2(20, 5), new Vector2(15, 5)
        });
    }

    static Transform[] CreateWaypoints(GameObject parent, string prefix, Vector2[] pts)
    {
        var wpParent = new GameObject(prefix + "_Parent");
        wpParent.transform.SetParent(parent.transform);
        var wps = new Transform[pts.Length];
        for (int i = 0; i < pts.Length; i++)
        {
            var w = new GameObject($"{prefix}_{i}");
            w.transform.SetParent(wpParent.transform);
            w.transform.position = pts[i];
            wps[i] = w.transform;
        }
        return wps;
    }

    static GameObject CreateGuard(GameObject parent, string name, Vector3 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform);
        go.layer = LayerMask.NameToLayer("Guard");
        go.transform.position = pos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = MakeCircle();
        sr.color = new Color(0.90f, 0.20f, 0.20f);
        sr.sortingOrder = 10;
        // Intentionally no UnlitMat() here so it catches light

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0; rb.freezeRotation = true;
        go.AddComponent<CircleCollider2D>().radius = 0.4f;

        go.AddComponent<GuardController>();
        go.AddComponent<GuardPatrol>();
        var vision = go.AddComponent<GuardVision>();
        vision.wallsMask = LayerMask.GetMask("Walls");

        // Flashlight (Freeform Light2D is just Point light with angles)
        var fl = new GameObject("Flashlight");
        fl.transform.SetParent(go.transform);
        fl.transform.localPosition = Vector3.zero;
        var l = fl.AddComponent<Light2D>();
        l.lightType = Light2D.LightType.Point;
        l.color = new Color(1f, 0.96f, 0.80f); // white-yellow
        l.intensity = 1.2f;
        l.pointLightOuterRadius = 6f;
        l.pointLightInnerAngle = 60f;
        l.pointLightOuterAngle = 60f;
        
        return go;
    }

    // ── ITEMS ─────────────────────────────────────────────────────────────────
    static void BuildItems()
    {
        var defs = new (string n, int v, int w, Color c, Vector2 pos)[]
        {
            ("Coin",     1,  1, Color.white, new Vector2( -15f, -11f)),
            ("Ring",     5,  1, Color.white, new Vector2(  15f, -11f)),
            ("Vase",    10,  3, Color.white, new Vector2( -15f,  12f)),
            ("Painting",20,  5, Color.white, new Vector2(  15f,  12f)),
            ("Crown",   50, 10, Color.white, new Vector2(  15f,   6f)),
            ("Necklace",15,  2, Color.white, new Vector2( -23f,  -5f)),
            ("Statue",  30,  7, Color.white, new Vector2( -23f,  -3f)),
            // New Items
            ("Diamond", 40,  1, Color.white, new Vector2(  0f,   0f)),
            ("Emerald", 35,  1, Color.white, new Vector2( -5f,   5f)),
            ("Ruby",    35,  1, Color.white, new Vector2(  5f,   5f)),
            ("LootBag", 25,  6, Color.white, new Vector2(  10f, -5f)),
        };
        foreach (var d in defs)
        {
            var go = new GameObject($"Item_{d.n}");
            go.layer = LayerMask.NameToLayer("Items");
            go.transform.position = d.pos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetKenneySprite(d.n);
            sr.color = Color.white; // Use actual sprite colors
            sr.sortingOrder = 8;
            // Removed UnlitMat so flashlight affects items!

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.6f; col.isTrigger = true;

            var pu = go.AddComponent<ItemPickup>();
            var so = ScriptableObject.CreateInstance<Item>();
            so.itemName = d.n; so.value = d.v; so.weight = d.w;
            pu.itemData = so;

            go.AddComponent<ItemLabel>();


            // Glow light
            var lg = new GameObject("Glow"); lg.transform.SetParent(go.transform);
            lg.transform.localPosition = Vector3.zero;
            var l = lg.AddComponent<Light2D>();
            l.lightType = Light2D.LightType.Point;
            l.color = d.c; l.intensity = 0.3f; l.pointLightOuterRadius = 1.5f;
            lg.AddComponent<PulseLight>();
        }
    }

    // ── EXIT ──────────────────────────────────────────────────────────────────
    static void BuildExit()
    {
        var go = new GameObject("ExitDoor");
        go.transform.position = new Vector3(0, -17.5f, 0);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = MakeRect();
        sr.color = new Color(0.15f, 0.95f, 0.25f);
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = new Vector2(3f, 0.5f);
        sr.sortingOrder = 5;
        sr.material = UnlitMat(); // Exit stays bright

        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(3f, 0.5f); col.isTrigger = true;
        go.AddComponent<ExitTrigger>();

        var lg = new GameObject("ExitGlow"); lg.transform.SetParent(go.transform);
        lg.transform.localPosition = Vector3.zero;
        var l = lg.AddComponent<Light2D>();
        l.lightType = Light2D.LightType.Point;
        l.color = new Color(0.20f, 1f, 0.30f); l.intensity = 1.2f;
        l.pointLightOuterRadius = 2.5f;
    }

    // ── LOCKED DOOR & KEY ─────────────────────────────────────────────────────
    static void BuildKeyAndDoor()
    {
        // 1. The Key
        var keyGO = new GameObject("Key");
        keyGO.transform.position = new Vector3(4.5f, -15.5f, 0f); // In Guest Bed
        var kSr = keyGO.AddComponent<SpriteRenderer>();
        kSr.sprite = GetKenneySprite("Key");
        kSr.color = Color.white;
        kSr.sortingOrder = 8;
        var kCol = keyGO.AddComponent<CircleCollider2D>();
        kCol.radius = 0.5f;
        keyGO.AddComponent<KeyPickup>();
        
        // Key Label
        var label = new GameObject("LabelText");
        label.transform.SetParent(keyGO.transform, false);
        label.transform.localPosition = new Vector3(0, 0.7f, 0);
        var txt = label.AddComponent<TMPro.TextMeshPro>();
        txt.text = "🔑 KEY";
        txt.fontSize = 3f;
        txt.alignment = TMPro.TextAlignmentOptions.Center;
        var mr = txt.GetComponent<MeshRenderer>();
        if (mr) { mr.sortingLayerName = "Default"; mr.sortingOrder = 15; }
        
        // 2. The Locked Door
        var doorGO = new GameObject("LockedDoor");
        doorGO.transform.position = new Vector3(12.5f, -11f, 0f); // Master Suite Entrance
        var dSr = doorGO.AddComponent<SpriteRenderer>();
        dSr.sprite = MakeRect();
        dSr.color = new Color(0.4f, 0.25f, 0.1f); // Brown wood
        dSr.drawMode = SpriteDrawMode.Tiled;
        dSr.size = new Vector2(0.5f, 4f);
        dSr.sortingOrder = 4;
        
        var dCol = doorGO.AddComponent<BoxCollider2D>();
        dCol.size = new Vector2(0.5f, 4f);
        // BoxCollider is solid, so it blocks movement.
        
        doorGO.AddComponent<LockedDoor>();
    }

    // ── MANAGERS ──────────────────────────────────────────────────────────────
    static void BuildManagers(GameObject canvas, UIManager uiMgr)
    {
        new GameObject("GameManager").AddComponent<GameManager>();
        new GameObject("ScoreManager").AddComponent<ScoreManager>();
        new GameObject("AudioManager").AddComponent<AudioManager>();
        canvas.AddComponent<RestartListener>();
    }

    // ── UI ────────────────────────────────────────────────────────────────────
    static void BuildUI()
    {
        var cvGO = new GameObject("UICanvas");
        var cv = cvGO.AddComponent<Canvas>();
        cv.renderMode = RenderMode.ScreenSpaceOverlay;
        var cs = cvGO.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cvGO.AddComponent<GraphicRaycaster>();
        var uiMgr = cvGO.AddComponent<UIManager>();

        // Pickup Flash Image
        var flashGO = new GameObject("FlashOverlay");
        flashGO.transform.SetParent(cvGO.transform, false);
        var flashImg = flashGO.AddComponent<Image>();
        flashImg.color = new Color(1, 1, 1, 0);
        flashImg.raycastTarget = false;
        var flashRT = flashGO.GetComponent<RectTransform>();
        flashRT.anchorMin = Vector2.zero;
        flashRT.anchorMax = Vector2.one;
        flashRT.sizeDelta = Vector2.zero;
        uiMgr.flashImage = flashImg;

        // HUD
        var hud = Panel(cvGO, "HUD", AnchorPreset.TopLeft, new Vector2(10,-10), new Vector2(240,90),
                        new Color(0,0,0,0.6f));
        uiMgr.hudPanel    = hud;
        uiMgr.scoreText   = TMP(hud, "Score",  new Vector2(12,-14), "Score: 0",    18);
        uiMgr.weightText  = TMP(hud, "Weight", new Vector2(12,-38), "Weight: 0",   18);
        uiMgr.itemsText   = TMP(hud, "Items",  new Vector2(12,-62), "Items: 0",    18);

        // Sneak tooltip
        var tip = Panel(cvGO, "SneakTip", AnchorPreset.BottomCenter, new Vector2(0,30),
                        new Vector2(320,36), new Color(0,0,0,0.55f));
        TMP(tip, "TipText", new Vector2(0,-14), "Hold SHIFT to sneak  |  E to pick up", 16);
        uiMgr.sneakTooltip = tip;

        // Win panel
        var win = Panel(cvGO, "WinPanel", AnchorPreset.Center, Vector2.zero,
                        new Vector2(0,0), new Color(0f,0.15f,0f,0.98f));
        var winRT = win.GetComponent<RectTransform>();
        winRT.anchorMin = Vector2.zero; winRT.anchorMax = Vector2.one;
        winRT.sizeDelta = Vector2.zero;
        
        win.SetActive(false); uiMgr.winPanel = win;
        var wt = TMP(win, "WinTitle", new Vector2(0, 180), "YOU ESCAPED!", 72);
        wt.color = new Color(0.4f,1f,0.4f);
        uiMgr.winScoreText = TMP(win, "WinScore", new Vector2(0, 20), "Score: 0", 36);
        TMP(win, "WinBtn", new Vector2(0,-180), "Press R to play again", 24).color = new Color(0.6f,1f,0.6f);

        // Lose panel
        var lose = Panel(cvGO, "LosePanel", AnchorPreset.Center, Vector2.zero,
                         new Vector2(0,0), new Color(0.2f,0f,0f,0.98f));
        var loseRT = lose.GetComponent<RectTransform>();
        loseRT.anchorMin = Vector2.zero; loseRT.anchorMax = Vector2.one;
        loseRT.sizeDelta = Vector2.zero;

        lose.SetActive(false); uiMgr.losePanel = lose;
        var lt = TMP(lose, "LoseTitle", new Vector2(0, 180), "CAUGHT!", 72);
        lt.color = new Color(1f,0.35f,0.35f);
        TMP(lose, "LoseText", new Vector2(0, 20), "The guard spotted you.", 36);
        TMP(lose, "LoseBtn",  new Vector2(0,-180), "Press R to try again", 24).color = new Color(1f,0.6f,0.6f);

        // Pause
        var pause = Panel(cvGO, "PausePanel", AnchorPreset.Center, Vector2.zero,
                          new Vector2(320,160), new Color(0,0,0,0.82f));
        pause.SetActive(false); uiMgr.pausePanel = pause;
        TMP(pause, "PT", new Vector2(0,30), "PAUSED", 28);
        TMP(pause, "PH", new Vector2(0,-10), "ESC to resume", 16);

        BuildManagers(cvGO, uiMgr);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // SPRITE FACTORIES (PERSISTENT ASSETS)
    // ══════════════════════════════════════════════════════════════════════════
    static Material UnlitMat()
    {
        return new Material(Shader.Find("Sprites/Default"));
    }

    static Sprite GetKenneySprite(string name)
    {
        string id = "001";
        switch(name)
        {
            case "Coin":     id = "086"; break;
            case "Ring":     id = "082"; break;
            case "Vase":     id = "032"; break;
            case "Painting": id = "149"; break;
            case "Crown":    id = "020"; break;
            case "Necklace": id = "101"; break;
            case "Statue":   id = "065"; break;
            case "Key":      id = "083"; break;
            case "Diamond":  id = "061"; break;
            case "Emerald":  id = "062"; break;
            case "Ruby":     id = "063"; break;
            case "LootBag":  id = "066"; break;
        }
        string path = $"Assets/Sprites/kenney_generic-items/PNG/Colored/genericItem_color_{id}.png";
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static Sprite GetOrGenerateSprite(string name, System.Func<Texture2D> texGen)
    {
        if (!System.IO.Directory.Exists("Assets/Generated"))
            System.IO.Directory.CreateDirectory("Assets/Generated");

        string path = $"Assets/Generated/{name}.png";
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex == null)
        {
            tex = texGen();
            System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(path);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.filterMode = FilterMode.Point;
                importer.spritePixelsPerUnit = Mathf.Max(tex.width, tex.height);
                importer.SaveAndReimport();
            }
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static Sprite MakeRect()
    {
        return GetOrGenerateSprite("Rect", () => {
            var tex = new Texture2D(32, 32);
            var px = new Color[32 * 32];
            for (int i = 0; i < px.Length; i++) px[i] = Color.white;
            tex.SetPixels(px); tex.Apply(); return tex;
        });
    }

    static Sprite MakeCircle()
    {
        return GetOrGenerateSprite("Circle", () => {
            var tex = new Texture2D(32, 32);
            var px = new Color[32 * 32];
            for (int i = 0; i < px.Length; i++) {
                float d = Vector2.Distance(new Vector2(i % 32, i / 32), new Vector2(16, 16));
                px[i] = d < 14.5f ? Color.white : d < 16f ? new Color(1, 1, 1, 0.5f) : Color.clear;
            }
            tex.SetPixels(px); tex.Apply(); return tex;
        });
    }

    static Sprite MakeDiamond()
    {
        return GetOrGenerateSprite("Diamond", () => {
            var tex = new Texture2D(32, 32);
            var px = new Color[32 * 32];
            for (int i = 0; i < px.Length; i++) {
                int x = i % 32, y = i / 32;
                bool inside = Mathf.Abs(x - 16) + Mathf.Abs(y - 16) < 15;
                bool edge = Mathf.Abs(x - 16) + Mathf.Abs(y - 16) < 16;
                px[i] = inside ? Color.white : edge ? new Color(1, 1, 1, 0.5f) : Color.clear;
            }
            tex.SetPixels(px); tex.Apply(); return tex;
        });
    }

    static UnityEngine.Tilemaps.Tile GetOrGenerateTile(string name, Sprite sprite, Color color)
    {
        string path = $"Assets/Generated/{name}.asset";
        var tile = AssetDatabase.LoadAssetAtPath<UnityEngine.Tilemaps.Tile>(path);
        if (tile == null)
        {
            tile = ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();
            tile.sprite = sprite;
            tile.color = color;
            AssetDatabase.CreateAsset(tile, path);
        }
        else 
        {
            tile.sprite = sprite;
            tile.color = color;
            EditorUtility.SetDirty(tile);
            AssetDatabase.SaveAssets();
        }
        return tile;
    }

    // ══════════════════════════════════════════════════════════════════════════
    // UI HELPERS
    // ══════════════════════════════════════════════════════════════════════════
    enum AnchorPreset { TopLeft, Center, BottomCenter }

    static GameObject Panel(GameObject parent, string name, AnchorPreset anchor,
                            Vector2 anchoredPos, Vector2 size, Color bg)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var img = go.AddComponent<Image>(); img.color = bg;
        var rt  = go.GetComponent<RectTransform>();
        rt.sizeDelta = size; rt.anchoredPosition = anchoredPos;
        switch (anchor)
        {
            case AnchorPreset.TopLeft:
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0,1); break;
            case AnchorPreset.Center:
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f,0.5f); break;
            case AnchorPreset.BottomCenter:
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f,0f); break;
        }
        return go;
    }

    static TMP_Text TMP(GameObject parent, string name, Vector2 pos, string text, int fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = fontSize; t.color = Color.white;
        t.alignment = TextAlignmentOptions.Center;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(400, fontSize + 8);
        return t;
    }

    // ══════════════════════════════════════════════════════════════════════════
    // LAYER UTIL
    // ══════════════════════════════════════════════════════════════════════════
    static void EnsureLayer(string layerName)
    {
        var so = new SerializedObject(
            AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
        var layers = so.FindProperty("layers");
        for (int i = 8; i < layers.arraySize; i++)
        {
            var el = layers.GetArrayElementAtIndex(i);
            if (el.stringValue == layerName) return;
            if (string.IsNullOrEmpty(el.stringValue))
            { el.stringValue = layerName; so.ApplyModifiedProperties(); return; }
        }
    }
}
