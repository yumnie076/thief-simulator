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
