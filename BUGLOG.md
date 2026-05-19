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

## 2026-05-19 — SerializeField name mismatches between subagent-generated scripts and bootstrapper
**Symptom:** Bootstrapper's SerializedObject.FindProperty() would fail at runtime — wiring `fadeCanvasGroup` when PhaseController uses `fadeOverlay`, `hungerFill` when HedgehogPhaseUI uses `hungerBarFill`, etc.
**Root cause:** Two subagents generated scripts independently — one created the bootstrapper wiring, the other created the MonoBehaviours. Neither could see the other's field names.
**Fix:** Manually audited ALL FindProperty() calls against actual SerializeField names in each script: fadeOverlay, hungerBarFill, headerText, messageText, doneButton, scoreText.
**Lesson:** When multiple subagents generate interdependent code, always do a cross-reference audit of ALL SerializeField names before declaring "done". Use grep to find mismatches.
## 2026-05-19 � Duplicate GardenManager bug
**Symptom:** Scene Bootstrapper created a duplicate singleton.
**Root cause:** I added the component to the managers GameObject, but it was already being added further down in the script.
**Fix:** Removed the duplicate AddComponent call in EgelSceneBootstrapper.cs.
**Lesson:** Always read the full method before adding new components that are singletons.

---
## 2026-05-19 � Compiler Errors
**Symptom:** Unity compiler threw errors about ambiguous Object and missing GardenTile type.
**Root cause:** I removed GardenTile.cs but forgot to update BiodiversityTracker.cs and PlaceableTool.cs which still referenced it. I also used an ambiguous Object reference in PhaseController.cs.
**Fix:** Rewrote BiodiversityTracker to use GardenObject, removed unused methods in PlaceableTool, and used UnityEngine.Object explicitly in PhaseController.
**Lesson:** Always check references when deleting a core class like GardenTile.

---

## 2026-05-20 � BuildPhaseUI OnScoreChanged Compiler Error
**Symptom:** The Build Phase HUD disappeared and the EgelGame menu item was missing in Unity after restarting.
**Root cause:** BuildPhaseUI.cs subscribed to BiodiversityTracker.Instance.OnScoreChanged, but BiodiversityTracker.cs was missing the OnScoreChanged event declaration after the recent gridless overhaul.
**Fix:** Declared public event System.Action<float> OnScoreChanged in BiodiversityTracker.cs, invoked it inside RecalculateScore(), and updated BuildPhaseUI.cs's Show() method to initialize the score correctly.
**Lesson:** Always perform a full compilation check and verify that all events/callbacks referenced by UI classes are fully implemented in the manager classes.

---

## 2026-05-20 - Map Too Big, Placed Items Invisible, HUD Squashed
**Symptom:** The garden map felt too empty and vast, placed objects and the hedgehog were completely invisible upon placement, and the HUD counters in the top-right did not show any text labels.
**Root cause:** 
1. The garden bounds were 16x16 and felt sparse with only 5 initial bushes and invisible placements.
2. Placed items and the hedgehog used Y-based dynamic sorting orders ranging from -160 to 0. Since the tiled grass background was statically sorted at -10, these objects were rendered behind the background.
3. The Counter area's VerticalLayoutGroup in the bootstrapper did not have Child Control Width/Height enabled, squashing TextMeshPro text elements to zero-width.
**Fix:** 
1. Shrunk default garden size to a cozy 12x12, adjusted camera orthographic size to 5.5f, and updated camera/background coordinates.
2. Changed the background's sortingOrder to -1000 to guarantee it is always behind everything, and added dynamic depth sorting to PlayerController.cs.
3. Enabled childControlWidth and childControlHeight on the Counter area layout group.
**Lesson:** Always set background graphics to a very low sorting order (e.g. -1000) when dynamic Y-based sorting is used, and always ensure layout groups controlling custom UI sizes have Control Width/Height flags set to avoid zero-width scaling.

---


## 2026-05-20 � Tool selection stuck on Flowers (Mouse/UI Raycast Block)
**Symptom:** The user could only place flowers and could not select or place other items.
**Root cause:**
1. The fullscreen BuildPanel had a transparent Image component with raycastTarget defaulted to true. This blocked all mouse raycasts from reaching the 2D game world, meaning standard mouse clicks on the grass background were blocked (so they could only place objects using the Spacebar which defaults to the first tool: Flowers).
2. The UI button elements did not have keyboard shortcuts, meaning that if the mouse event system had conflicts or was blocked, the user had no fallback method to select other tools.
3. The selected tool was not visually highlighted by default at phase startup, which made it unclear which tool was currently active.
**Fix:**
1. Set bg.raycastTarget = false on BuildPanel in EgelSceneBootstrapper.cs so clicks pass to the 2D world seamlessly.
2. Implemented keyboard shortcuts [1] through [7] in BuildPhaseUI.cs to allow quick, bulletproof selection of any tool.
3. Updated the button labels on screen to display their hotkeys: [1] to [7].
4. Added an automatic SelectTool(1) call in BuildPhaseUI.Show() to visually highlight the default Flower tool instantly when the build phase starts.
**Lesson:** Fullscreen UI panel overlays must always have raycastTarget = false on their background images to avoid blocking mouse clicks on the game world. Provide keyboard fallback shortcuts (e.g. number keys 1-9) for critical UI toolbar actions to guarantee accessibility and bypass potential EventSystem failures.

---
