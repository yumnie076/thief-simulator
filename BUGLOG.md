# Bug Log ‚Äî Thief Simulator (Weight of Greed)

Format:
## YYYY-MM-DD ‚Äî Brief description
**Symptom:** 
**Root cause:** 
**Fix:** 
**Lesson:** 

---

## 2026-05-11 ‚Äî Tilemap tiles invisible under URP 2D
**Symptom:** Floor and wall tiles not visible in Play mode ‚Äî only dark background showed.
**Root cause:** `ScriptableObject.CreateInstance<Tile>()` tiles get Sprite-Lit-Default material by default. Global ambient light was 0.08 (near black), so tiles rendered black ‚Äî indistinguishable from background. Also tile color (#1f1812 walls on #0d0d1a background) too similar.
**Fix:** Replaced Tilemap approach with plain GameObjects + SpriteRenderer using MakeRect/MakeCircle procedural sprites. These use Sprites/Default (unlit) material automatically and are always visible regardless of lighting.
**Lesson:** In URP 2D, never rely on tilemaps being visible at low ambient light. Either use unlit sprites for map geometry, OR set global ambient to ‚â• 0.4. For jam builds, GameObject quads are more reliable than tilemaps.

---

## 2026-05-11 ‚Äî Procedural assets disappear in Play Mode
**Symptom:** Scene looks perfectly fine in Scene view, but when hitting Play, all walls, floors, and sprites generated via `SceneBootstrapper` vanish.
**Root cause:** `Sprite.Create` and `ScriptableObject.CreateInstance<Tile>` only create assets in memory. When entering Play Mode, Unity serializes the scene and discards all unreferenced memory objects, causing them to become null.
**Fix:** Wrote a `GetOrGenerateSprite` helper that saves the generated Texture2D as a PNG in `Assets/Generated/` and imports it via `AssetDatabase`. Saved `Tile` objects as `.asset` files.
**Lesson:** Never rely on in-memory `ScriptableObject` or `Sprite` instances in Editor scripts if you want them to survive Play Mode. Always write them to the AssetDatabase. Also removed `UnlitMat` on world objects so they properly react to URP 2D point lights.

## 2026-05-11 ‚Äî TMP Emoji Missing Risk (Step 1)
**Symptom:** Emojis like ü™ô, üíç might render as squares.
**Root cause:** TextMeshPro requires the Emoji font asset to be imported and assigned as a fallback in TMP Settings.
**Fix:** Pending user verification. If squares appear, go to Window ‚Üí TextMeshPro ‚Üí Import TMP Essential Resources, and import Examples and Extras.
**Lesson:** Worldspace UI with emojis requires TMP setup, not just pasting text.

## 2026-05-11 ‚Äî SceneBootstrapper Syntax Error (Method Nesting)
**Symptom:** "ThiefSim" menu disappeared, "Enter Safe Mode" popup in Unity.
**Root cause:** I accidentally closed the `BuildMap` method early and left `Decor` calls in the class scope while adding `RoomLabel`.
**Fix:** Corrected method braces and moved `RoomLabel` declaration to the class scope.
**Lesson:** Always check method scoping when copy-pasting or generating helper methods inside large methods.

## 2026-05-19 ‚Äî SerializeField name mismatches between subagent-generated scripts and bootstrapper
**Symptom:** Bootstrapper's SerializedObject.FindProperty() would fail at runtime ‚Äî wiring `fadeCanvasGroup` when PhaseController uses `fadeOverlay`, `hungerFill` when HedgehogPhaseUI uses `hungerBarFill`, etc.
**Root cause:** Two subagents generated scripts independently ‚Äî one created the bootstrapper wiring, the other created the MonoBehaviours. Neither could see the other's field names.
**Fix:** Manually audited ALL FindProperty() calls against actual SerializeField names in each script: fadeOverlay, hungerBarFill, headerText, messageText, doneButton, scoreText.
**Lesson:** When multiple subagents generate interdependent code, always do a cross-reference audit of ALL SerializeField names before declaring "done". Use grep to find mismatches.
## 2026-05-19 ó Duplicate GardenManager bug
**Symptom:** Scene Bootstrapper created a duplicate singleton.
**Root cause:** I added the component to the managers GameObject, but it was already being added further down in the script.
**Fix:** Removed the duplicate AddComponent call in EgelSceneBootstrapper.cs.
**Lesson:** Always read the full method before adding new components that are singletons.

---
## 2026-05-19 ó Compiler Errors
**Symptom:** Unity compiler threw errors about ambiguous Object and missing GardenTile type.
**Root cause:** I removed GardenTile.cs but forgot to update BiodiversityTracker.cs and PlaceableTool.cs which still referenced it. I also used an ambiguous Object reference in PhaseController.cs.
**Fix:** Rewrote BiodiversityTracker to use GardenObject, removed unused methods in PlaceableTool, and used UnityEngine.Object explicitly in PhaseController.
**Lesson:** Always check references when deleting a core class like GardenTile.

---

## 2026-05-20 ó BuildPhaseUI OnScoreChanged Compiler Error
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


## 2026-05-20 ó Tool selection stuck on Flowers (Mouse/UI Raycast Block)
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


## 2026-05-25 ó Missing TMPro namespace compiler error in HedgehogAI.cs
**Symptom:** Unity compilation threw error CS0246: The type or namespace name 'TMP_Text' could not be found.
**Root cause:** HedgehogAI.cs used a TMP_Text variable reference for updating the game's header text, but did not have the using TMPro; import at the top of the file.
**Fix:** Added using TMPro; at the top of HedgehogAI.cs.
**Lesson:** Always remember to import TMPro when declaring TMP_Text or TextMeshProUGUI variables inside C# scripts.

---


## 2026-05-25 ó HedgehogNeeds.Safety set accessor inaccessible compiler error
**Symptom:** Unity compilation threw error CS0272: The property or indexer 'HedgehogNeeds.Safety' cannot be used in this context because the set accessor is inaccessible.
**Root cause:** HedgehogAI.cs directly modified 
eeds.Safety to decay safety during a chase and recover it during peace, but the Safety set accessor in HedgehogNeeds.cs was declared as private set.
**Fix:** Changed private set to set (public set) in HedgehogNeeds.cs.
**Lesson:** Always check the accessor permissions (public/private) of properties in manager/data classes before directly assigning or incrementing them from separate components.

---

## 2026-05-25 ó UI Raycast Block and Vertical Text
**Symptom:** The user could not click on the toolbar buttons to select tools, and the text on the buttons was rendering letter-by-letter vertically.
**Root cause:** The BuildPanel had an Image with raycastTarget = true which blocked clicks from reaching the toolbar. The text was vertical because TextMeshProUGUI had enableWordWrapping = true by default inside a narrow button container.
**Fix:** Set img.raycastTarget = false for background panels and txt.enableWordWrapping = false for tool buttons in EgelSceneBootstrapper.cs.
**Lesson:** Transparent UI overlays still block raycasts if raycastTarget is not explicitly disabled. Always verify word wrapping behavior on dynamically created text objects within small layout groups.

---

## 2026-05-25 ó AddBiodiversity Compiler Error
**Symptom:** Unity compiler error CS1061: 'ScoreManager' does not contain a definition for 'AddBiodiversity'.
**Root cause:** I used ScoreManager.Instance.AddBiodiversity(2f) in Insect.cs, but the ScoreManager class only had SetBiodiversity and no AddBiodiversity method.
**Fix:** Added public void AddBiodiversity(float points) to ScoreManager.cs.
**Lesson:** Always check if a method exists in the target class before calling it, especially when making assumptions based on similar methods like AddFood or AddWater.

---

## 2026-05-25 ó Map overlapping and UI not updating
**Symptom:** The map background covered placed objects and player, fox was still too fast, and inventory was still broken.
**Root cause:** Background tiles generated by GardenManager didn't have scale correction, causing large sprites to overlap massively. Fox speeds were still too high and no spawn delay existed. The inventory UI changes were made in a Bootstrapper script, meaning the user had to manually regenerate the scene, but didn't.
**Fix:** Forced background tiles to scale exactly to 1x1 using bounds.size in GardenManager.cs. Reduced fox speeds to 0.8f/1.5f and added waitTime = 6f to FoxAI.cs Start(). Instructed user to run EgelGame > Build Scene.
**Lesson:** Always remember to scale raw sprites if they have large bounds and use 100 PPU, and always remind the user to rebuild Editor-generated scenes when modifying bootstrapper logic.

---

## 2026-05-25 ó Map Z-sorting and duplicate layers
**Symptom:** Player was stuck 'under the map', and there were two background layers overlapping.
**Root cause:** GardenManager's GenerateBackgroundGrid didn't delete the fallback 'Background' created by the Bootstrapper, nor did it delete the old 'BackgroundGrid' when resetting. Furthermore, its sortingOrder of -10 was being out-prioritized by GardenObject which subtracts y*10 from its sorting order (resulting in values like -150).
**Fix:** Set grid sortingOrder to -1000, and explicitly destroyed 'Background' and old 'BackgroundGrid' before creating a new grid in GardenManager.cs.
**Lesson:** Always make sure dynamically spawned grids delete old fallback layers, and ensure background sorting orders are extremely negative (e.g. -1000) to account for Y-based fake depth sorting in top-down 2D games.

---

## 2026-05-25 ó Map size, hunger balancing, and inventory layout
**Symptom:** The game ended in 10 seconds because the hedgehog starved too quickly, maps were considered too small, and the inventory was just a blank white bar.
**Root cause:** HungerDecayRate was set to 8f (drained 50 hunger in 6s). The map width/height constraints were conservative. Removing the TextMeshPro component from the inventory buttons broke the VerticalLayoutGroup, causing the icon to not render correctly.
**Fix:** Decreased HungerDecayRate to 1.2f. Increased map sizes to 14, 18, 24. Re-added the text label to the inventory buttons but only containing the shortcut key (e.g. '[1]') to restore proper layout constraints while remaining minimal.
**Lesson:** Always test UI layout changes locally; removing elements from an active LayoutGroup can break remaining elements. Ensure gameplay decay rates match intended session lengths.

---

## 2026-05-25 ó Result Screen Text Glitches
**Symptom:** Text on the Result screen and education popups appeared 'glitchy'.
**Root cause:** The text strings in ResultScreenUI and EducationContent contained emojis. Unity's TextMeshPro default font does not contain glyphs for these emojis, causing missing character fallback squares or corrupted rendering.
**Fix:** Removed all emojis from ResultScreenUI and EducationContent string dictionaries.
**Lesson:** Never use emojis in Unity TextMeshPro strings unless a dedicated emoji fallback font is explicitly loaded and assigned.

---
