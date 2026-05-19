# Bug Log — Thief Simulator (Weight of Greed)

Format:
## YYYY-MM-DD — Brief description
**Symptom:** 
**Root cause:** 
**Fix:** 
**Lesson:** 

---

## 2026-05-11 — Tilemap tiles invisible under URP 2D
**Symptom:** Floor and wall tiles not visible in Play mode — only dark background showed.
**Root cause:** `ScriptableObject.CreateInstance<Tile>()` tiles get Sprite-Lit-Default material by default. Global ambient light was 0.08 (near black), so tiles rendered black — indistinguishable from background. Also tile color (#1f1812 walls on #0d0d1a background) too similar.
**Fix:** Replaced Tilemap approach with plain GameObjects + SpriteRenderer using MakeRect/MakeCircle procedural sprites. These use Sprites/Default (unlit) material automatically and are always visible regardless of lighting.
**Lesson:** In URP 2D, never rely on tilemaps being visible at low ambient light. Either use unlit sprites for map geometry, OR set global ambient to ≥ 0.4. For jam builds, GameObject quads are more reliable than tilemaps.

---

## 2026-05-11 — Procedural assets disappear in Play Mode
**Symptom:** Scene looks perfectly fine in Scene view, but when hitting Play, all walls, floors, and sprites generated via `SceneBootstrapper` vanish.
**Root cause:** `Sprite.Create` and `ScriptableObject.CreateInstance<Tile>` only create assets in memory. When entering Play Mode, Unity serializes the scene and discards all unreferenced memory objects, causing them to become null.
**Fix:** Wrote a `GetOrGenerateSprite` helper that saves the generated Texture2D as a PNG in `Assets/Generated/` and imports it via `AssetDatabase`. Saved `Tile` objects as `.asset` files.
**Lesson:** Never rely on in-memory `ScriptableObject` or `Sprite` instances in Editor scripts if you want them to survive Play Mode. Always write them to the AssetDatabase. Also removed `UnlitMat` on world objects so they properly react to URP 2D point lights.

## 2026-05-11 — TMP Emoji Missing Risk (Step 1)
**Symptom:** Emojis like 🪙, 💍 might render as squares.
**Root cause:** TextMeshPro requires the Emoji font asset to be imported and assigned as a fallback in TMP Settings.
**Fix:** Pending user verification. If squares appear, go to Window → TextMeshPro → Import TMP Essential Resources, and import Examples and Extras.
**Lesson:** Worldspace UI with emojis requires TMP setup, not just pasting text.

## 2026-05-11 — SceneBootstrapper Syntax Error (Method Nesting)
**Symptom:** "ThiefSim" menu disappeared, "Enter Safe Mode" popup in Unity.
**Root cause:** I accidentally closed the `BuildMap` method early and left `Decor` calls in the class scope while adding `RoomLabel`.
**Fix:** Corrected method braces and moved `RoomLabel` declaration to the class scope.
**Lesson:** Always check method scoping when copy-pasting or generating helper methods inside large methods.

