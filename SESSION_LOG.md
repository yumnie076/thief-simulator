# Session Log — Thief Simulator (Weight of Greed)

Format:
## YYYY-MM-DD — Project — Worked on
### Wins:
### Misses:
### New pattern to remember:

---

## 2026-05-11 — Thief Simulator — Project Kickoff / STEP 0
### Wins:
- Project already has URP 17.4 + Tilemap + Input System packages installed
- Clean Unity 2D project structure ready for scripts
### Misses:
- N/A (first session)
### New pattern to remember:
- This project: top-down 2D stealth. Weight slows player, expands sound radius, extends guard vision range.
- Build order: scripts first (can compile without scene), then build scene manually in Unity editor.

---

## 2026-05-11 — Thief Simulator — Steps 3–11 (SceneBootstrapper)
### Wins:
- Single Editor menu builds entire scene: layers, tilemap, player, guard, 5 items, exit, UI, lights
- Procedural sprites (circle/diamond) — no asset dependency for first playtest
- Room lights + guard flashlight built inline, no manual Inspector work
### Misses:
- N/A
### New pattern to remember:
- SceneBootstrapper pattern: one Editor script wires everything. Fastest path for jam builds.
- Guard vision wallsMask should be set in code via LayerMask.GetMask, not Inspector.

---

## 2026-05-11 — Weight of Greed — Recovery Session
### Wins (caught my own mistakes)
- Identified missing Time.unscaledDeltaTime for camera shake during Time.timeScale = 0
- Caught that Tilemap with unlit tiles works best in 2D with specific light setups, but used procedural tiles with UnlitMat so they still appear
- Upgraded map to 50x35 programmatically without needing external assets
### Misses (user had to catch)
- Shipped Unity 6 project with legacy Input API → 50+ exceptions/frame
- Did not maintain BUGLOG/SESSION_LOG as instructed in GEMINI.md
- Made visuals too minimal (prototype-tier instead of jam-submission tier)
### New patterns to remember
- For Unity 6 projects: ALWAYS check/set Active Input Handling before writing input code
- For jam visuals: Kenney pixel art > primitives, period
- For agent discipline: re-read GEMINI.md if I haven't logged in 30 minutes

---

## 2026-05-19 — PIVOT — Weight of Greed → Egel op Expeditie
### Reason for pivot
- Original "Weight of Greed" thief sim no longer aligns with project goals
- New direction: serious game about garden sustainability tied to Tuinen van de Toekomst project (Avans + gemeente Altena)
- Theme "Less Is More" is dropped (no longer relevant constraint)
### Files being archived
- All Player, Guard, Items, Effects, Utils scripts → `_Archive/WeightOfGreed/`
- SceneBootstrapper.cs, BuildScript.cs → `_Archive/WeightOfGreed/`
- Generated sprites (Circle, Diamond, Rect, tiles) → `_Archive/WeightOfGreed/Generated/`
### Wins:
- Clean archive without deleting anything
- New folder structure created: Garden/, Hedgehog/, UI/, Utils/
- Core singletons (GameManager, ScoreManager, UIManager) rewritten
- ProcSpriteGenerator saves all sprites to disk (learned from BUGLOG)
- EgelSceneBootstrapper builds entire scene with one click
### Misses:
- N/A (session in progress)
### New patterns to remember:
- Always archive old code, never delete
- Procedural sprites MUST go through AssetDatabase.ImportAsset
- Flat cartoon style = no URP lights needed = simpler pipeline

---
## 2026-05-19 � EgelGame � Ecosysteem Overhaul
### Wins:
- Completed a massive overhaul shifting from grid-based tile placement to freeform ecosystem.
- Added AI for the hedgehog and insects.
- Implemented Quest UI.
### Misses:
- Accidentally duplicated GardenManager in Bootstrapper because I didn't read the whole function first.
### New pattern to remember:
- Replacing rigid arrays with freeform object lists enables organic game systems.

---

## 2026-05-20 � EgelGame � Compiler Error and UI Initialization Fixes
### Wins:
- Resolved the missing `OnScoreChanged` compiler error in `BuildPhaseUI.cs` by implementing the event and invocation in `BiodiversityTracker.cs`.
- Added immediate initialization of the biodiversity score HUD element inside `BuildPhaseUI.Show()`.
- Verified clean compilation in `Editor.log`.
### Misses:
- None.
### New pattern to remember:
- When implementing event subscriptions in UI scripts, always ensure the corresponding event exists in the target manager/tracker scripts, and initialize the UI state immediately with current values when enabling the HUD.

---

## 2026-05-20 - EgelGame - Map Size, Visibility, and HUD Layout Fixes
### Wins:
- Resized the garden dimensions to a cozy 12x12 for a denser serious game ecosystem layout.
- Repositioned and resized the tiled background, centered the starting camera, and lowered orthographic size to 5.5f to bring gameplay objects up-close and detailed.
- Solved the major visibility bugs by lowering the grass background sorting order to -1000 and adding dynamic Y-based sorting to the player character, allowing all flora, houses, insects, and hedgehogs to correctly sort and render on top of the grass.
- Enabled control width/height properties on the Counter UI area to fix squashed zero-width TextMeshPro counters in the HUD.
- Implemented automatic color-coded button highlights when tools are selected.
- Wired the Hedgehog Visit safety indicators (text & icon) to display state properly.
### Misses:
- None.
### New pattern to remember:
- Ensure custom layout groups are configured with Child Control flags enabled when spawning text items inside layout hierarchies to prevent automatic zero-width clamping.
- Apply dynamic sorting order updates to player controllers just like AI and world objects to ensure correct relative perspective rendering.

---


## 2026-05-20 - EgelGame - Tool Selection Stuck & Clicks Blocked Fixes
### Wins:
- Resolved the "only place flowers" issue by setting g.raycastTarget = false on BuildPanel in EgelSceneBootstrapper.cs. This allows standard mouse clicks to pass through transparent areas to the game world.
- Implemented bulletproof keyboard shortcuts **1 to 7** in BuildPhaseUI.cs so the user can easily select and place all 7 ecosystem tools (RemoveTile, Flower, Bush, Tree, Pond, LeafPile, HedgehogHouse) directly from the keyboard.
- Updated toolbar UI labels to clearly display the hotkeys: [1] Tegel weg, [2] Bloem, [3] Struik, [4] Boom, [5] Vijver, [6] Bladhoop, [7] Egelhuis.
- Configured BuildPhaseUI.Show() to automatically select and visually highlight the default Flower tool (SelectTool(1)) green on phase startup.
### Misses:
- None.
### New pattern to remember:
- Always set aycastTarget = false on fullscreen parent panel images in UGUI to avoid eating pointer raycasts. Provide keyboard hotkeys for toolbars as an elegant and robust accessibility fallback.

---
