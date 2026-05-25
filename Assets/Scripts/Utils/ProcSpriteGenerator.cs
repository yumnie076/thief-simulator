#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Procedurally generates and saves all sprites for Egel op Expeditie.
/// All textures are saved as PNGs to Assets/Generated/EgelGame/ to survive Play Mode.
/// </summary>
public static class ProcSpriteGenerator
{
    private const string OutputDir = "Assets/Resources/EgelGame";
    private const int TileSize = 64;
    private const int IconSize = 48;
    private const int HedgehogSize = 64;

    // ─── Color palette ───
    private static readonly Color PavedColor   = HexColor("#9a9a9a");
    private static readonly Color GroutColor   = HexColor("#6e6e6e");
    private static readonly Color GrassColor   = HexColor("#7bc043");
    private static readonly Color GrassShade   = HexColor("#5fa033");
    private static readonly Color WaterColor   = HexColor("#4a9eb8");
    private static readonly Color WaterLight   = HexColor("#6cb4cf");
    private static readonly Color FlowerRed    = HexColor("#f04060");
    private static readonly Color FlowerOrange = HexColor("#f0a040");
    private static readonly Color FlowerYellow = HexColor("#f0d040");
    private static readonly Color FlowerPurple = HexColor("#b040f0");
    private static readonly Color FlowerWhite  = Color.white;
    private static readonly Color BushColor    = HexColor("#2d7a2a");
    private static readonly Color TreeTrunk    = HexColor("#6b4226");
    private static readonly Color TreeLeaves   = HexColor("#3a8a3a");
    private static readonly Color LeafPile     = HexColor("#c97338");
    private static readonly Color HouseWall    = HexColor("#8b6239");
    private static readonly Color HouseRoof    = HexColor("#4a3525");
    private static readonly Color HedgehogBody = HexColor("#6b4226");
    private static readonly Color HedgehogSpike= HexColor("#3a2818");
    private static readonly Color HedgehogBelly= HexColor("#c79868");
    private static readonly Color BgColor      = HexColor("#d4e8d4");

    [MenuItem("EgelGame/Generate All Sprites")]
    public static void GenerateAll()
    {
        if (!Directory.Exists(OutputDir))
            Directory.CreateDirectory(OutputDir);

        // Tiles
        GeneratePavedTile();
        GenerateGrassTile();
        GenerateWaterTile();

        // Decorations on grass
        GenerateFlowerTile();
        GenerateBushTile();
        GenerateTreeTile();
        GenerateLeafPileTile();
        GenerateHedgehogHouseTile();

        // Characters & Animals
        GenerateHedgehogSprite();
        GeneratePlayerSprite();
        GenerateButterflySprite();
        GenerateBugSprite();
        GenerateSpiderSprite();
        GenerateFoxSprite();
        GenerateBirdSprite();
        GenerateSnailSprite();
        GenerateFrogSprite();

        // Tool icons
        GenerateToolIcons();

        // Background
        GenerateBackground();

        AssetDatabase.Refresh();
        Debug.Log("[ProcSpriteGenerator] All sprites generated in " + OutputDir);
    }

    // ─── Tile sprites ───

    private static void GeneratePavedTile()
    {
        var tex = new Texture2D(TileSize, TileSize);
        FillRect(tex, 0, 0, TileSize, TileSize, PavedColor);
        // Grout lines
        FillRect(tex, 0, 0, TileSize, 2, GroutColor);
        FillRect(tex, 0, 0, 2, TileSize, GroutColor);
        FillRect(tex, TileSize / 2 - 1, 0, 2, TileSize, GroutColor);
        FillRect(tex, 0, TileSize / 2 - 1, TileSize, 2, GroutColor);
        // Rounded corner simulation (darken corners)
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3 - i; j++)
            {
                tex.SetPixel(i, j, GroutColor);
                tex.SetPixel(TileSize - 1 - i, j, GroutColor);
                tex.SetPixel(i, TileSize - 1 - j, GroutColor);
                tex.SetPixel(TileSize - 1 - i, TileSize - 1 - j, GroutColor);
            }
        SaveSprite(tex, "tile_paved");
    }

    private static void GenerateGrassTile()
    {
        var tex = new Texture2D(TileSize, TileSize);
        // Use Perlin noise to make a nicer grass texture
        float noiseScale = 0.2f;
        float offsetX = Random.Range(0f, 100f);
        float offsetY = Random.Range(0f, 100f);
        for (int y = 0; y < TileSize; y++)
        {
            for (int x = 0; x < TileSize; x++)
            {
                float noise = Mathf.PerlinNoise(x * noiseScale + offsetX, y * noiseScale + offsetY);
                Color c = Color.Lerp(GrassColor, GrassShade, noise * 0.5f); // subtle variation
                
                // Add some distinct grass blades occasionally
                if (Random.value > 0.95f) c = GrassShade;
                
                tex.SetPixel(x, y, c);
            }
        }
        SaveSprite(tex, "tile_grass");
    }

    private static void GenerateWaterTile()
    {
        var tex = new Texture2D(TileSize, TileSize);
        // Gradient top to bottom
        for (int y = 0; y < TileSize; y++)
        {
            float t = (float)y / TileSize;
            Color c = Color.Lerp(WaterColor, WaterLight, t);
            for (int x = 0; x < TileSize; x++)
                tex.SetPixel(x, y, c);
        }
        // Wave highlights
        for (int x = 4; x < TileSize - 4; x += 8)
        {
            int wy = TileSize / 3 + (x % 16 < 8 ? 2 : -2);
            FillRect(tex, x, wy, 6, 2, new Color(1, 1, 1, 0.3f));
        }
        SaveSprite(tex, "tile_water");
    }

    // ─── Decoration tiles (grass base + decoration) ───

    private static void GenerateFlowerTile()
    {
        var tex = CloneTex(LoadGenerated("tile_grass"));
        if (tex == null) { tex = new Texture2D(TileSize, TileSize); FillRect(tex, 0, 0, TileSize, TileSize, GrassColor); }
        // Draw 3 small flowers
        Color[] flowerColors = { FlowerRed, FlowerOrange, FlowerPurple };
        int[] fx = { 16, 40, 28 };
        int[] fy = { 42, 38, 18 };
        for (int i = 0; i < 3; i++)
            DrawFlower(tex, fx[i], fy[i], 5, flowerColors[i]);
        SaveSprite(tex, "tile_flower");
    }

    private static void GenerateBushTile()
    {
        var tex = CloneTex(LoadGenerated("tile_grass"));
        if (tex == null) { tex = new Texture2D(TileSize, TileSize); FillRect(tex, 0, 0, TileSize, TileSize, GrassColor); }
        // Draw rounded bush shape
        DrawFilledCircle(tex, 32, 32, 18, BushColor);
        DrawFilledCircle(tex, 22, 36, 12, BushColor);
        DrawFilledCircle(tex, 42, 36, 12, BushColor);
        // Highlight
        DrawFilledCircle(tex, 30, 38, 6, Color.Lerp(BushColor, Color.white, 0.2f));
        SaveSprite(tex, "tile_bush");
    }

    private static void GenerateTreeTile()
    {
        var tex = CloneTex(LoadGenerated("tile_grass"));
        if (tex == null) { tex = new Texture2D(TileSize, TileSize); FillRect(tex, 0, 0, TileSize, TileSize, GrassColor); }
        // Trunk
        FillRect(tex, 28, 4, 8, 24, TreeTrunk);
        // Canopy
        DrawFilledCircle(tex, 32, 42, 20, TreeLeaves);
        DrawFilledCircle(tex, 22, 38, 14, TreeLeaves);
        DrawFilledCircle(tex, 42, 38, 14, TreeLeaves);
        // Highlight
        DrawFilledCircle(tex, 28, 46, 8, Color.Lerp(TreeLeaves, Color.white, 0.15f));
        SaveSprite(tex, "tile_tree");
    }

    private static void GenerateLeafPileTile()
    {
        var tex = CloneTex(LoadGenerated("tile_grass"));
        if (tex == null) { tex = new Texture2D(TileSize, TileSize); FillRect(tex, 0, 0, TileSize, TileSize, GrassColor); }
        // Oval leaf pile
        DrawFilledEllipse(tex, 32, 28, 22, 14, LeafPile);
        DrawFilledEllipse(tex, 32, 32, 18, 10, Color.Lerp(LeafPile, Color.white, 0.15f));
        // Leaf details
        var rng = new System.Random(77);
        for (int i = 0; i < 6; i++)
        {
            int lx = 18 + rng.Next(28);
            int ly = 20 + rng.Next(20);
            FillRect(tex, lx, ly, 4, 2, Color.Lerp(LeafPile, GrassShade, 0.3f));
        }
        SaveSprite(tex, "tile_leafpile");
    }

    private static void GenerateHedgehogHouseTile()
    {
        var tex = CloneTex(LoadGenerated("tile_grass"));
        if (tex == null) { tex = new Texture2D(TileSize, TileSize); FillRect(tex, 0, 0, TileSize, TileSize, GrassColor); }
        // House body
        FillRect(tex, 14, 10, 36, 28, HouseWall);
        // Roof (triangle-ish)
        for (int y = 0; y < 16; y++)
        {
            int halfW = 18 + y;
            if (halfW > 30) halfW = 30;
            FillRect(tex, 32 - halfW / 2, 38 + y, halfW, 1, HouseRoof);
        }
        // Door opening
        FillRect(tex, 26, 10, 12, 14, new Color(0.1f, 0.08f, 0.06f));
        // Outline
        DrawRect(tex, 14, 10, 36, 28, HouseRoof);
        SaveSprite(tex, "tile_hedgehoghouse");
    }

    // ─── Hedgehog character ───

    private static void GenerateHedgehogSprite()
    {
        var tex = new Texture2D(HedgehogSize, HedgehogSize);
        // Clear to transparent
        Color clear = new Color(0, 0, 0, 0);
        FillRect(tex, 0, 0, HedgehogSize, HedgehogSize, clear);

        // Body (ellipse)
        DrawFilledEllipse(tex, 32, 28, 22, 16, HedgehogBody);
        // Belly
        DrawFilledEllipse(tex, 32, 24, 14, 10, HedgehogBelly);
        // Spikes on top
        for (int i = 0; i < 12; i++)
        {
            int sx = 16 + i * 3;
            int sy = 38 + (i % 2 == 0 ? 4 : 2);
            FillRect(tex, sx, sy, 2, 6, HedgehogSpike);
        }
        // Eyes
        FillRect(tex, 38, 30, 3, 3, Color.black);
        // Nose
        FillRect(tex, 44, 27, 3, 3, new Color(0.2f, 0.1f, 0.1f));
        // Ear
        DrawFilledCircle(tex, 24, 40, 4, Color.Lerp(HedgehogBody, Color.white, 0.2f));

        SaveSprite(tex, "hedgehog");
    }

    private static void GeneratePlayerSprite()
    {
        var tex = new Texture2D(HedgehogSize, HedgehogSize);
        Color clear = new Color(0, 0, 0, 0);
        FillRect(tex, 0, 0, HedgehogSize, HedgehogSize, clear);

        // Body (Shoulders/Shirt)
        Color shirtColor = HexColor("#3d78d4"); // Blue shirt
        DrawFilledEllipse(tex, 32, 24, 20, 12, shirtColor);
        
        // Head
        Color skinColor = HexColor("#f2ccaa");
        DrawFilledCircle(tex, 32, 36, 12, skinColor);
        
        // Straw Hat
        Color hatColor = HexColor("#e6cf73");
        DrawFilledEllipse(tex, 32, 42, 22, 10, hatColor);
        DrawFilledEllipse(tex, 32, 46, 14, 6, HexColor("#c2ab51")); // hat top
        
        // Hands
        DrawFilledCircle(tex, 16, 20, 6, skinColor); // Left hand
        DrawFilledCircle(tex, 48, 20, 6, skinColor); // Right hand

        SaveSprite(tex, "player_gardener");
    }

    private static void GenerateButterflySprite()
    {
        var tex = new Texture2D(32, 32);
        FillRect(tex, 0, 0, 32, 32, new Color(0,0,0,0));
        // Wings
        Color wingColor = HexColor("#e645e6");
        DrawFilledEllipse(tex, 10, 16, 8, 12, wingColor);
        DrawFilledEllipse(tex, 22, 16, 8, 12, wingColor);
        // Body
        FillRect(tex, 15, 8, 2, 16, Color.black);
        SaveSprite(tex, "insect_butterfly");
    }

    private static void GenerateBugSprite()
    {
        var tex = new Texture2D(16, 16);
        FillRect(tex, 0, 0, 16, 16, new Color(0,0,0,0));
        Color shell = HexColor("#e62222"); // ladybug red
        DrawFilledCircle(tex, 8, 8, 6, shell);
        FillRect(tex, 8, 2, 1, 12, Color.black); // stripe
        FillRect(tex, 8, 12, 4, 4, Color.black); // head
        SaveSprite(tex, "insect_bug");
    }

    private static void GenerateSpiderSprite()
    {
        var tex = new Texture2D(24, 24);
        FillRect(tex, 0, 0, 24, 24, new Color(0,0,0,0));
        Color body = HexColor("#222222");
        DrawFilledCircle(tex, 12, 12, 5, body);
        // Legs
        DrawRect(tex, 2, 14, 20, 1, body);
        DrawRect(tex, 4, 10, 16, 1, body);
        DrawRect(tex, 6, 6, 12, 1, body);
        SaveSprite(tex, "insect_spider");
    }

    // ─── Tool icons ───

    private static void GenerateToolIcons()
    {
        GenerateHammerIcon();
        GenerateFlowerIcon();
        GenerateBushIcon();
        GenerateTreeIcon();
        GenerateWaterIcon();
        GenerateLeafIcon();
        GenerateHouseIcon();
    }

    private static void GenerateHammerIcon()
    {
        var tex = new Texture2D(IconSize, IconSize);
        Color clear = new Color(0, 0, 0, 0);
        FillRect(tex, 0, 0, IconSize, IconSize, clear);
        // Handle
        FillRect(tex, 10, 8, 6, 28, TreeTrunk);
        // Head
        FillRect(tex, 4, 32, 18, 10, HexColor("#888888"));
        SaveSprite(tex, "icon_hammer");
    }

    private static void GenerateFlowerIcon()
    {
        var tex = new Texture2D(IconSize, IconSize);
        Color clear = new Color(0, 0, 0, 0);
        FillRect(tex, 0, 0, IconSize, IconSize, clear);
        // Stem
        FillRect(tex, 22, 4, 4, 22, GrassColor);
        // Petals
        DrawFlower(tex, 24, 34, 8, FlowerRed);
        SaveSprite(tex, "icon_flower");
    }

    private static void GenerateBushIcon()
    {
        var tex = new Texture2D(IconSize, IconSize);
        Color clear = new Color(0, 0, 0, 0);
        FillRect(tex, 0, 0, IconSize, IconSize, clear);
        DrawFilledCircle(tex, 24, 28, 16, BushColor);
        DrawFilledCircle(tex, 16, 24, 10, BushColor);
        DrawFilledCircle(tex, 32, 24, 10, BushColor);
        SaveSprite(tex, "icon_bush");
    }

    private static void GenerateTreeIcon()
    {
        var tex = new Texture2D(IconSize, IconSize);
        Color clear = new Color(0, 0, 0, 0);
        FillRect(tex, 0, 0, IconSize, IconSize, clear);
        FillRect(tex, 20, 4, 8, 18, TreeTrunk);
        DrawFilledCircle(tex, 24, 34, 14, TreeLeaves);
        SaveSprite(tex, "icon_tree");
    }

    private static void GenerateWaterIcon()
    {
        var tex = new Texture2D(IconSize, IconSize);
        Color clear = new Color(0, 0, 0, 0);
        FillRect(tex, 0, 0, IconSize, IconSize, clear);
        // Water drop shape
        DrawFilledCircle(tex, 24, 20, 12, WaterColor);
        // Top point
        for (int y = 0; y < 14; y++)
        {
            int w = y;
            if (w > 0) FillRect(tex, 24 - w / 2, 32 + y, w, 1, WaterColor);
        }
        SaveSprite(tex, "icon_water");
    }

    private static void GenerateLeafIcon()
    {
        var tex = new Texture2D(IconSize, IconSize);
        Color clear = new Color(0, 0, 0, 0);
        FillRect(tex, 0, 0, IconSize, IconSize, clear);
        DrawFilledEllipse(tex, 24, 28, 16, 10, LeafPile);
        DrawFilledEllipse(tex, 24, 24, 12, 6, Color.Lerp(LeafPile, Color.white, 0.2f));
        SaveSprite(tex, "icon_leaf");
    }

    private static void GenerateHouseIcon()
    {
        var tex = new Texture2D(IconSize, IconSize);
        Color clear = new Color(0, 0, 0, 0);
        FillRect(tex, 0, 0, IconSize, IconSize, clear);
        FillRect(tex, 10, 6, 28, 22, HouseWall);
        // Roof
        for (int y = 0; y < 12; y++)
        {
            int w = 14 + y * 2;
            if (w > 36) w = 36;
            FillRect(tex, 24 - w / 2, 28 + y, w, 1, HouseRoof);
        }
        // Door
        FillRect(tex, 18, 6, 12, 12, new Color(0.1f, 0.08f, 0.06f));
        SaveSprite(tex, "icon_house");
    }

    // ─── Background ───

    private static void GenerateBackground()
    {
        var tex = new Texture2D(4, 4);
        FillRect(tex, 0, 0, 4, 4, BgColor);
        SaveSprite(tex, "background");
    }

    // ═══════════════════════════════════════
    // Drawing helpers
    // ═══════════════════════════════════════

    private static void FillRect(Texture2D tex, int x, int y, int w, int h, Color c)
    {
        for (int py = y; py < y + h && py < tex.height; py++)
            for (int px = x; px < x + w && px < tex.width; px++)
                if (px >= 0 && py >= 0)
                    tex.SetPixel(px, py, c);
    }

    private static void DrawRect(Texture2D tex, int x, int y, int w, int h, Color c)
    {
        FillRect(tex, x, y, w, 1, c);
        FillRect(tex, x, y + h - 1, w, 1, c);
        FillRect(tex, x, y, 1, h, c);
        FillRect(tex, x + w - 1, y, 1, h, c);
    }

    private static void DrawFilledCircle(Texture2D tex, int cx, int cy, int r, Color c)
    {
        for (int y = -r; y <= r; y++)
            for (int x = -r; x <= r; x++)
                if (x * x + y * y <= r * r)
                {
                    int px = cx + x;
                    int py = cy + y;
                    if (px >= 0 && px < tex.width && py >= 0 && py < tex.height)
                        tex.SetPixel(px, py, c);
                }
    }

    private static void DrawFilledEllipse(Texture2D tex, int cx, int cy, int rx, int ry, Color c)
    {
        for (int y = -ry; y <= ry; y++)
            for (int x = -rx; x <= rx; x++)
            {
                float nx = (float)x / rx;
                float ny = (float)y / ry;
                if (nx * nx + ny * ny <= 1f)
                {
                    int px = cx + x;
                    int py = cy + y;
                    if (px >= 0 && px < tex.width && py >= 0 && py < tex.height)
                        tex.SetPixel(px, py, c);
                }
            }
    }

    private static void DrawFlower(Texture2D tex, int cx, int cy, int r, Color c)
    {
        // 5 petals around center
        DrawFilledCircle(tex, cx, cy + r, r / 2, c);
        DrawFilledCircle(tex, cx + r, cy, r / 2, c);
        DrawFilledCircle(tex, cx - r, cy, r / 2, c);
        DrawFilledCircle(tex, cx, cy - r, r / 2, c);
        DrawFilledCircle(tex, cx + r / 2, cy + r / 2, r / 2, c);
        // Center
        DrawFilledCircle(tex, cx, cy, r / 2, FlowerYellow);
    }

    // ─── File I/O ───

    private static void SaveSprite(Texture2D tex, string name)
    {
        tex.Apply();
        byte[] png = tex.EncodeToPNG();
        string path = $"{OutputDir}/{name}.png";
        File.WriteAllBytes(path, png);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        // Set importer to Sprite mode
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = TileSize;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
        Object.DestroyImmediate(tex);
    }

    private static Texture2D LoadGenerated(string name)
    {
        string path = $"{OutputDir}/{name}.png";
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    private static Texture2D CloneTex(Texture2D src)
    {
        if (src == null) return null;
        // We need to read pixels, so create a readable copy
        var rt = RenderTexture.GetTemporary(src.width, src.height);
        Graphics.Blit(src, rt);
        var prev = RenderTexture.active;
        RenderTexture.active = rt;
        var copy = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false);
        copy.ReadPixels(new Rect(0, 0, src.width, src.height), 0, 0);
        copy.Apply();
        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);
        return copy;
    }

    private static Color HexColor(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }

    private static void GenerateFoxSprite()
    {
        var tex = new Texture2D(TileSize, TileSize);
        Color clear = new Color(0, 0, 0, 0);
        FillRect(tex, 0, 0, TileSize, TileSize, clear);

        Color foxOrange = HexColor("#e66e25"); // Fox orange
        Color foxWhite = Color.white;
        Color foxDark = HexColor("#2b1b11");

        // Tail
        DrawFilledEllipse(tex, 16, 20, 10, 14, foxOrange);
        DrawFilledCircle(tex, 12, 10, 5, foxWhite); // tail white tip

        // Body
        DrawFilledEllipse(tex, 32, 24, 18, 12, foxOrange);
        // Chest (white belly)
        DrawFilledEllipse(tex, 34, 22, 10, 8, foxWhite);

        // Legs (dark paws)
        FillRect(tex, 20, 4, 4, 10, foxDark);
        FillRect(tex, 28, 4, 4, 10, foxDark);
        FillRect(tex, 36, 4, 4, 10, foxDark);
        FillRect(tex, 44, 4, 4, 10, foxDark);

        // Head
        DrawFilledCircle(tex, 38, 36, 10, foxOrange);
        // Snout
        DrawFilledEllipse(tex, 44, 34, 6, 4, foxOrange);
        FillRect(tex, 49, 33, 2, 2, foxDark); // black nose tip

        // Ears (pointed)
        for (int y = 0; y < 6; y++)
            FillRect(tex, 32 + y/2, 40 + y, 4 - y/2, 1, foxOrange);
        for (int y = 0; y < 6; y++)
            FillRect(tex, 40 + y/2, 40 + y, 4 - y/2, 1, foxOrange);

        // Eye
        FillRect(tex, 41, 37, 2, 2, foxDark);

        SaveSprite(tex, "fox");
    }

    private static void GenerateBirdSprite()
    {
        var tex = new Texture2D(32, 32);
        FillRect(tex, 0, 0, 32, 32, new Color(0,0,0,0));
        Color blueBird = HexColor("#3d9be6");
        Color bellyYellow = HexColor("#e6cf45");

        // Body
        DrawFilledEllipse(tex, 16, 16, 10, 7, blueBird);
        // Yellow belly
        DrawFilledEllipse(tex, 16, 13, 7, 4, bellyYellow);
        // Head
        DrawFilledCircle(tex, 22, 20, 5, blueBird);
        // Beak (yellow/orange)
        FillRect(tex, 26, 19, 3, 2, HexColor("#e69b25"));
        // Wing
        DrawFilledEllipse(tex, 12, 17, 5, 3, HexColor("#227cb5"));
        // Tail
        FillRect(tex, 4, 15, 4, 3, blueBird);

        SaveSprite(tex, "insect_bird");
    }

    private static void GenerateSnailSprite()
    {
        var tex = new Texture2D(24, 24);
        FillRect(tex, 0, 0, 24, 24, new Color(0,0,0,0));
        Color snailBody = HexColor("#e0c7a6");
        Color snailShell = HexColor("#b88554");

        // Body
        DrawFilledEllipse(tex, 12, 6, 10, 3, snailBody);
        // Neck/Head
        FillRect(tex, 18, 8, 3, 6, snailBody);
        // Shell
        DrawFilledCircle(tex, 10, 12, 6, snailShell);
        DrawFilledCircle(tex, 10, 12, 3, HexColor("#8c5e32")); // inner spiral

        SaveSprite(tex, "insect_snail");
    }

    private static void GenerateFrogSprite()
    {
        var tex = new Texture2D(24, 24);
        FillRect(tex, 0, 0, 24, 24, new Color(0,0,0,0));
        Color frogGreen = HexColor("#4bb55c");
        Color frogLight = HexColor("#77d487");

        // Body
        DrawFilledEllipse(tex, 12, 10, 9, 6, frogGreen);
        // Head / Eyes
        DrawFilledCircle(tex, 8, 15, 3, frogGreen);
        DrawFilledCircle(tex, 16, 15, 3, frogGreen);
        // White eyes
        FillRect(tex, 8, 15, 1, 1, Color.black);
        FillRect(tex, 16, 15, 1, 1, Color.black);
        // Belly
        DrawFilledEllipse(tex, 12, 8, 5, 3, frogLight);

        SaveSprite(tex, "insect_frog");
    }
}
#endif
