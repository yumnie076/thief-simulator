using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Runtime procedural map builder.
/// Creates a 4-room house layout using solid-color tiles as a fallback
/// (replace TileBase references with real Kenney tiles when imported).
///
/// IMPORTANT: Attach this to an empty GameObject named "MapBuilder" in the scene.
/// The script self-destructs after building so there's no overhead in play mode.
/// </summary>
public class MapBuilder : MonoBehaviour
{
    [Header("Tilemaps (assign in Inspector)")]
    public Tilemap floorMap;
    public Tilemap wallMap;

    [Header("Tiles (assign Kenney tiles or leave null for solid-color fallback)")]
    public TileBase floorTile;
    public TileBase wallTile;

    private void Awake()
    {
        if (floorMap == null || wallMap == null)
        {
            Debug.LogWarning("MapBuilder: Tilemaps not assigned — skipping procedural build.");
            return;
        }
        BuildMap();
        // Destroy self after building
        Destroy(this);
    }

    private void BuildMap()
    {
        // House: 30 x 20 units, bottom-left at (-15, -10)
        // Coordinate system: tile (0,0) = world (-15,-10)
        const int W = 30, H = 20;

        // ── Floor (everything inside) ─────────────────────────────────────────
        FillRect(floorMap, floorTile, 1, 1, W - 2, H - 2);

        // ── Outer walls ───────────────────────────────────────────────────────
        FillRect(wallMap, wallTile, 0, 0, W, 1);          // bottom wall
        FillRect(wallMap, wallTile, 0, H - 1, W, 1);      // top wall
        FillRect(wallMap, wallTile, 0, 0, 1, H);          // left wall
        FillRect(wallMap, wallTile, W - 1, 0, 1, H);      // right wall

        // ── Front door opening (exit) — center of bottom wall ─────────────────
        ClearRect(wallMap, 13, 0, 4, 1);                  // 4-tile gap

        // ── Interior horizontal divider (top half vs bottom half) ─────────────
        // y = 10 → separates Living Room (y<10) from top rooms (y>10)
        FillRect(wallMap, wallTile, 1, 10, W - 2, 1);
        // Hallway gap: centered
        ClearRect(wallMap, 13, 10, 4, 1);

        // ── Interior vertical dividers (top rooms) ────────────────────────────
        // Kitchen | Bedroom  divider at x=10
        FillRect(wallMap, wallTile, 10, 11, 1, H - 12);
        ClearRect(wallMap, 10, 14, 1, 3);                 // door gap

        // Bedroom | Master Bedroom  at x=20
        FillRect(wallMap, wallTile, 20, 11, 1, H - 12);
        ClearRect(wallMap, 20, 14, 1, 3);                 // door gap
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private void FillRect(Tilemap map, TileBase tile, int x, int y, int w, int h)
    {
        if (tile == null) return;
        for (int ix = x; ix < x + w; ix++)
            for (int iy = y; iy < y + h; iy++)
                map.SetTile(new Vector3Int(ix, iy, 0), tile);
    }

    private void ClearRect(Tilemap map, int x, int y, int w, int h)
    {
        for (int ix = x; ix < x + w; ix++)
            for (int iy = y; iy < y + h; iy++)
                map.SetTile(new Vector3Int(ix, iy, 0), null);
    }
}
