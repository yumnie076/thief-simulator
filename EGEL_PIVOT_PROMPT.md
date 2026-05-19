# 🦔 PROJECT PIVOT — Egel op Expeditie (Serious Game)

**MAJOR PIVOT: We are abandoning Weight of Greed (thief sim) entirely.**
**New project: Egel op Expeditie — a serious game about garden sustainability and hedgehog welfare.**
**Connected to:** Tuinen van de Toekomst (Avans Hogeschool Breda + gemeente Altena project).

Read this entire document before writing any code. Then execute step by step. Stop and confirm after each major step.

---

## STEP 0 — DISCIPLINE & CLEANUP

### 0A. Re-read your rules
1. Open `GEMINI.md`. Confirm you've read all 8 rules.
2. Open `BUGLOG.md` and `SESSION_LOG.md`. State the top 3 risk patterns from previous sessions.

### 0B. Document the pivot in SESSION_LOG.md
Append immediately:
```
## YYYY-MM-DD — PIVOT — Weight of Greed → Egel op Expeditie
### Reason for pivot
- Original "Weight of Greed" thief sim no longer aligns with project goals
- New direction: serious game about garden sustainability tied to Tuinen van de Toekomst project (Avans + gemeente Altena)
- Theme "Less Is More" is dropped (no longer relevant constraint)
### Files being archived
- (list what you'll move/delete)
```

### 0C. Clean up thief sim files
Move all thief-sim-specific files to `_Archive/WeightOfGreed/` instead of deleting (in case we need code references):

Move to archive:
- `Assets/Scenes/MainGame.unity` → `_Archive/WeightOfGreed/MainGame.unity`
- `Assets/Scripts/Player/PlayerController.cs` (thief version)
- `Assets/Scripts/Player/PlayerInventory.cs`
- `Assets/Scripts/Player/PlayerSound.cs`
- `Assets/Scripts/Guard/` (entire folder)
- `Assets/Scripts/Items/Item.cs` (thief items)
- `Assets/Scripts/Items/ItemPickup.cs`
- `Assets/Editor/SceneBootstrapper.cs` (thief version)
- `Assets/Generated/` (thief sprites)

**KEEP** (still useful):
- `GEMINI.md`, `BUGLOG.md`, `SESSION_LOG.md`
- `Assets/Fonts/`
- `Assets/Settings/` (URP settings, project settings)
- `Assets/Scripts/Game/GameManager.cs` — but **gut its contents** and rewrite for this new game
- `Assets/Scripts/Game/UIManager.cs` — same: gut and rewrite
- Active Input Handling setting (Both)

Result: cleaner workspace, no leftover thief logic interfering.

Confirm to user: "Cleanup complete. Archive folder created. Ready to build Egel op Expeditie."

---

## STEP 1 — GAME DESIGN OVERVIEW

### Concept
**Egel op Expeditie** is a 3–4 minute serious game that:
1. Asks the player about their own garden (awareness question)
2. Lets them transform a garden into a hedgehog-friendly habitat
3. Shows a hedgehog visiting their garden and how it survives based on their choices
4. Ends with educational summary

### Educational message
A hedgehog-friendly garden is a biodiverse garden. Less concrete + more native plants + water + shelter = healthier ecosystem for hedgehogs, bees, butterflies, birds.

### Genre / View
Top-down 2D. Cartoon flat-color style with clean outlines (no pixel art — use procedural sprites with rounded shapes and bold colors).

### Game Phases (linear flow)
- **Phase 1 — Intro Question** (~15 seconds) — sets garden start state
- **Phase 2 — Garden Build** (~90 seconds) — player transforms garden, learns facts
- **Phase 3 — Hedgehog Visit** (~45 seconds) — hedgehog walks through, succeeds or fails
- **Phase 4 — Result Screen** (open) — score, comparison, tips, replay

---

## STEP 2 — PHASE 1: INTRO QUESTION

### Layout (full-screen UI canvas)
- Top: Title "🦔 EGEL OP EXPEDITIE"
- Subtitle: "Hoe egelvriendelijk is jouw tuin?"
- Center: question text in large font:
  > "Hoe groen is jouw tuin het meest?"
- Below: 3 large vertically-stacked buttons:

| Button | Background | Text | Sets garden state |
|--------|-----------|------|-------------------|
| 1 | Red `#d94a3d` | 🟥 Vooral tegels en weinig natuur | Hard start (80% paved) |
| 2 | Yellow `#e8c447` | 🟨 Mix van tegels, gras en wat planten | Medium start (50% paved, 30% grass, 20% planted) |
| 3 | Green `#5ab84a` | 🟩 Veel groen, bloemen, bomen en plek voor insecten | Easy start (20% paved, 50% grass, 30% planted) |

- Footer: small text "Een Tuinen van de Toekomst x Avans project"

### Behavior
- Player clicks one → store result in `GameManager.gardenStartState`
- Fade out to Phase 2
- No back button (one-time choice)

---

## STEP 3 — PHASE 2: GARDEN BUILD

### Layout
- Main view (~80% screen): top-down garden grid
- Top-right HUD:
  - Actions remaining: "Acties: 10"
  - Biodiversity score: "🌱 Biodiversiteit: 3"
- Left sidebar: 6 placement tool buttons (icons + labels)
- Bottom-right: "✅ Klaar — Roep de egel!" button (always visible)

### Garden grid
- 12 × 8 = 96 tiles total
- Each tile: 1 unit world size, rounded corners
- Initial state depends on Phase 1 answer (see table above)
- 4 tile states:
  - **Tegel** (paved): gray `#9a9a9a` with darker grout lines
  - **Gras** (grass): bright green `#7bc043`
  - **Geplant** (planted): green tile with a plant sprite overlay (flower / bush / tree / pond / leaf-pile / hedgehog-house)
  - **Water** (pond): blue `#4a9eb8`

### Placement Tools (left sidebar)

| Tool | Icon | Action | Cost | What it does |
|------|------|--------|------|--------------|
| 🔨 Tegel weg | hammer | Click paved tile → remove | 1 action | Tile becomes grass |
| 🌸 Bloem | flower | Click grass tile → plant flower | 1 action | Adds flowers (attracts insects) |
| 🌿 Struik | bush | Click grass tile → plant bush | 1 action | Provides hedgehog shelter |
| 🌳 Boom | tree | Click grass tile → plant tree | 1 action | Air quality, shade, birds |
| 💧 Vijver | water drop | Click grass tile → make pond | 2 actions | Water source (must have escape ramp = pond auto-includes ramp tile) |
| 🍂 Bladhoop | leaves | Click grass tile → leaf pile | 1 action | Hibernation spot |
| 🏠 Egelhuis | small house | Click grass tile → hedgehog house | 2 actions | Safe shelter, big score boost |

Each tool: click on sidebar to "select" → click on garden tile to apply.
Tool button has hover state and a "selected" highlight.

### Budget rules
- Start with 10 actions
- Each placement deducts 1 (or 2 for pond/house)
- If 0 actions left: tools grayed out, only "Klaar" button works
- If "Klaar" pressed early: still works, just with less impact

### Education Popup on each placement
Brief popup appears top-center for 3 seconds with the relevant fact. Use this content:

```csharp
// Place this in Assets/Scripts/Game/EducationContent.cs
public static class EducationContent
{
    public static readonly Dictionary<string, string> Facts = new()
    {
        ["RemoveTile"] = "🔨 Minder verharding = meer ruimte voor planten en regenwater dat de bodem in kan.",
        ["Flower"]     = "🌸 Wilde bloemen trekken bijen, vlinders en kevers — belangrijk voedsel voor egels.",
        ["Bush"]       = "🌿 Struiken bieden schuilplek voor egels, kleine vogels en insecten.",
        ["Tree"]       = "🌳 Bomen verkoelen je tuin, vangen CO₂ op en huisvesten vogels en insecten.",
        ["Pond"]       = "💧 Een vijver met schuine kant geeft egels en vogels drinkwater — zonder dat ze verdrinken.",
        ["LeafPile"]   = "🍂 Een bladhoop onder een struik is dé winterslaap-plek voor de egel.",
        ["House"]      = "🏠 Een egelhuis biedt veiligheid tegen rovers, kou en regen.",
    };
    
    public static readonly Dictionary<string, string> EndTips = new()
    {
        ["FenceGap"]   = "🦔 Tip: maak een gat van 13×13 cm in je schutting — een egelpoort. Zo kunnen egels tussen tuinen reizen.",
        ["NoPellets"]  = "🚫 Gebruik geen slakkenkorrels. Die doden 200.000 egels per jaar in Nederland.",
        ["NoMowing"]   = "🦔 Check altijd je grasveld voordat je maait — vooral 's avonds en in lang gras.",
        ["WaterBowl"]  = "💧 Geen vijver? Een ondiepe waterbak (max 5cm) helpt egels en vogels in droge zomers.",
        ["Native"]     = "🌱 Plant inheemse soorten — die voeden meer insecten dan exotische tuinplanten.",
    };
}
```

### Biodiversity score formula
Score during Phase 2:
- Each unique type placed = +1 biodiversity
- Same type multiple times = +0.5 each (diminishing returns)
- Removing pavement = +0.25 each

This rewards variety, not quantity.

### Verify STEP 3
- [ ] Question screen leads to garden screen with correct start state
- [ ] Each of 6 tools works (click sidebar → click tile → tile updates)
- [ ] Action counter decreases correctly
- [ ] Pond costs 2 actions, house costs 2 actions, others cost 1
- [ ] Education popup shows for 3 seconds on each placement
- [ ] "Klaar" button transitions to Phase 3

---

## STEP 4 — PHASE 3: HEDGEHOG VISIT

### Setup
- Garden stays visible (player can no longer modify)
- A hedgehog sprite spawns at the bottom-center of the garden
- HUD changes:
  - Top: "🦔 De egel zoekt voedsel en schuilplek..."
  - Timer: 45 seconds countdown
  - Hedgehog stats: Hunger (0-100, decreases over time), Safety (0-100)

### Hedgehog AI behavior (simple state machine)
States in priority order:
1. **SeekFood** — if hunger < 70, walk to nearest food source (flower tile = insects/slugs, leaf pile = beetles)
2. **SeekWater** — if hunger ≥ 70 AND pond exists, walk to pond to drink
3. **SeekShelter** — if 30 seconds passed OR a "danger" appears, walk to nearest bush / leaf pile / hedgehog house
4. **Wander** — random movement if all needs met

### Movement
- Speed: 1.5 units/sec on grass/planted
- Speed: 0.7 units/sec on paved (hedgehogs hate concrete)
- Smooth interpolation between tiles
- Sprite rotates to face movement direction

### Danger system (optional, only if time)
Every 15 seconds, spawn a temporary "cat" or "owl" sprite that moves across the garden
- If cat passes near hedgehog (< 2 units) AND hedgehog is NOT under shelter (bush/house): -20 safety
- Hedgehog must hide to survive

### Outcomes during phase
- Each food source eaten: +15 hunger restore, +5 score
- Drinking water: +10 hunger restore, +3 score
- Reaching shelter at end: +20 score
- Time runs out without shelter: -10 score

### End trigger
Phase 3 ends when EITHER:
- Hedgehog reaches a safe shelter (bush, leaf pile, or hedgehog house) for >3 seconds, OR
- Timer reaches 0

Transition to Phase 4.

### Verify STEP 4
- [ ] Hedgehog spawns and moves naturally
- [ ] Hedgehog avoids paved tiles (moves slower on them)
- [ ] Hedgehog seeks food, then shelter
- [ ] Timer counts down
- [ ] Phase transitions on hedgehog success or timer end

---

## STEP 5 — PHASE 4: RESULT SCREEN

### Layout (full-screen UI)
- Title: based on outcome:
  - All needs met + reached shelter → "🎉 Jouw tuin is een egel-paradijs!"
  - Partial → "👍 Een goed begin voor de egel!"
  - Hedgehog struggled → "😔 De egel had het moeilijk in jouw tuin..."

### Score breakdown box
```
═══════════════════════════════
       EGEL SCORE
═══════════════════════════════

  Biodiversiteit:     12 punten
  Voedsel gevonden:   25 punten  
  Water bereikt:       8 punten
  Veilige schuilplek: 20 punten
  ─────────────────────────────
  TOTAAL:             65 punten
═══════════════════════════════
```

### Garden snapshot
Side-by-side comparison:
- "Start" (gray tile garden based on Phase 1 answer)
- "Jouw tuin" (current state)

### Educational summary (3 random tips from EndTips dictionary)
Pull 3 random tips from `EducationContent.EndTips`. Show as a styled list.

Add this important final message:
> 💡 "Elke kleine verandering helpt. Eén tuin is al een verschil."

### Buttons
- "🔄 Speel opnieuw" → reload scene
- "❌ Sluiten" → Application.Quit()

---

## STEP 6 — VISUAL STYLE (CARTOON FLAT)

NO pixel art. NO sketch lines. Use **procedural rounded sprites** with:
- Bold flat colors (no gradients except for water + sky)
- Subtle drop shadow on placeable items (5px offset, 30% alpha)
- Thick outline on player-interactive elements (2px, dark green/brown)
- Smooth rounded corners on all UI

### Color palette
```
Sky/background:    #d4e8d4 (very pale green-blue)
Grass:             #7bc043 (bright fresh green)
Grass shade:       #5fa033 (under-bush darker)
Paved tile:        #9a9a9a (gray)
Tile grout:        #6e6e6e
Water:             #4a9eb8 → #6cb4cf (gradient, light to lighter)
Flower colors:     #f04060, #f0a040, #f0d040, #b040f0, #ffffff
Bush:              #2d7a2a
Tree trunk:        #6b4226
Tree leaves:       #3a8a3a
Leaf pile:         #c97338
Hedgehog house:    #8b6239 with dark roof #4a3525
Hedgehog body:     #6b4226 (warm brown)
Hedgehog spikes:   #3a2818 (dark brown)
Hedgehog belly:    #c79868 (light cream)
Sad face:          #555555
Happy face:        #2d2d2d
UI panel:          #ffffff with 90% alpha + shadow
UI accent:         #5ab84a
Button hover:      lighten by 15%
```

### Sprite generation strategy
Use `SceneBootstrapper.cs` to procedurally generate sprites at build time AND save them to `Assets/Generated/EgelGame/` as PNGs (DO NOT rely on in-memory sprites — they vanish on Play. This was a bug last time. See BUGLOG.md.)

Generate these sprites programmatically:
- Tile sprites (4 variants): paved, grass, grass-with-flower, water
- Decoration sprites: 6 different flower colors, 3 bush sizes, 2 tree variants, leaf pile, hedgehog house
- Character: hedgehog (compose from oval body + dark spike texture + small ears + nose + eyes)
- UI icons: 7 tool icons (use Unicode emoji rendered to texture, or simple shape compositions)

If procedural gets too complex, fallback: use **Kenney "Tiny Town" or "Nature Kit"** sprites (https://kenney.nl/assets/nature-kit). Free, CC0.

---

## STEP 7 — FILE STRUCTURE

```
Assets/
├── Editor/
│   └── EgelSceneBootstrapper.cs       ← One-click scene build menu
├── Scenes/
│   └── EgelExpeditie.unity            ← Main scene (all 4 phases in one scene)
├── Scripts/
│   ├── Game/
│   │   ├── GameManager.cs             ← Singleton, holds gardenStartState
│   │   ├── PhaseController.cs         ← State machine for phases 1-4
│   │   ├── ScoreManager.cs            ← Tracks all scoring
│   │   └── EducationContent.cs        ← Static facts dictionary
│   ├── Garden/
│   │   ├── GardenManager.cs           ← Grid of tiles
│   │   ├── GardenTile.cs              ← Single tile data + visual
│   │   ├── PlaceableTool.cs           ← Tool selection logic
│   │   └── BiodiversityTracker.cs     ← Counts unique types
│   ├── Hedgehog/
│   │   ├── HedgehogAI.cs              ← State machine: SeekFood/Water/Shelter/Wander
│   │   ├── HedgehogNeeds.cs           ← Hunger, safety stats
│   │   └── HedgehogVisual.cs          ← Sprite, direction
│   ├── UI/
│   │   ├── IntroScreenUI.cs           ← Phase 1
│   │   ├── BuildPhaseUI.cs            ← Phase 2 HUD + sidebar
│   │   ├── EducationPopup.cs          ← Top popup (3s display)
│   │   ├── HedgehogPhaseUI.cs         ← Phase 3 timer + stats
│   │   └── ResultScreenUI.cs          ← Phase 4 breakdown
│   └── Utils/
│       └── ProcSpriteGenerator.cs     ← Generates and saves sprites
├── Generated/
│   └── EgelGame/                      ← All procedural sprites land here
├── Resources/
│   └── (any runtime-loaded assets)
└── _Archive/
    └── WeightOfGreed/                 ← Old thief sim files (kept for reference)
```

---

## STEP 8 — BUILD ORDER (incremental)

| # | Step                                | Est time | Done when |
|---|-------------------------------------|----------|-----------|
| 1 | Cleanup + archive thief sim         | 15 min   | _Archive folder exists, no thief code in active Scripts/ |
| 2 | Sprite generator + procedural assets| 30 min   | All sprites generated and saved to Assets/Generated/EgelGame/ |
| 3 | Scene + GardenManager grid (12×8)   | 20 min   | Grid renders correctly with mixed tiles |
| 4 | IntroScreenUI (Phase 1)             | 20 min   | 3 buttons work, sets gardenStartState, transitions out |
| 5 | PlaceableTool + tile transformation | 30 min   | Click sidebar tool → click tile → tile updates with sprite |
| 6 | Education popups + biodiversity tracker | 20 min | Each placement shows correct popup; HUD updates score |
| 7 | Phase 2 → Phase 3 transition (Klaar button) | 10 min | UI cleanly switches |
| 8 | HedgehogAI state machine + movement | 40 min   | Hedgehog spawns, seeks food, slower on paved, smooth movement |
| 9 | Phase 3 outcome tracking            | 15 min   | Score updates from hedgehog behavior |
| 10| ResultScreenUI (Phase 4)            | 25 min   | Final score, 3 random tips, replay button works |
| 11| Polish — animations, sound, popups  | 30 min   | Tile placement has scale-punch, popups fade smoothly |
| 12| Final playtest + build .exe         | 15 min   | Complete game loop works, .exe runs standalone |

**Total estimated: ~4.5 hours**

After each step: PRESS PLAY, verify, fix errors, ONLY THEN move on. Report status to user.

---

## STEP 9 — CRITICAL TRAPS (from previous BUGLOG)

These are KNOWN issues from past sessions. Avoid them:

1. **Procedural sprites vanish on Play**: ALWAYS save generated sprites to disk via AssetDatabase, not just in memory.
2. **Input System mismatch**: Unity 6 needs Active Input Handling = "Both" OR use new Input System. Already set to "Both" from previous session.
3. **Unlit material kills 2D lights**: For this game we don't need URP 2D lights (cartoon style works fine with default sprites lit by ambient).
4. **TilemapCollider2D + CompositeCollider2D gotchas**: We're not using Tilemap this time — using GameObject grid instead. Simpler.
5. **TimeScale + Coroutines**: If pausing, use `WaitForSecondsRealtime` not `WaitForSeconds`.
6. **Singleton survival on scene reload**: Use `[RuntimeInitializeOnLoadMethod]` or check `Instance != null` in Awake.

---

## STEP 10 — LOG DISCIPLINE (NON-NEGOTIABLE)

Throughout the build:
- **Every bug** → append to BUGLOG.md immediately, format from GEMINI.md
- **End of build** → append to SESSION_LOG.md with wins/misses/lessons
- **If stuck twice on same problem** → STOP, invoke escalation (Rule 8)
- **Every 200 lines uncommitted** → STOP, build check, fix errors

---

## OUTPUT

When complete, deliver:
1. ✅ Working Unity scene `EgelExpeditie.unity` — open + Play → full game loop
2. ✅ Standalone Windows .exe in `Build/`
3. ✅ Updated BUGLOG.md with new entries
4. ✅ Updated SESSION_LOG.md
5. ✅ `_Archive/WeightOfGreed/` containing old files

---

## EXECUTION RULES

- Start with **STEP 0 (discipline + cleanup)**. Confirm cleanup done before any code.
- After each numbered step in STEP 8, **STOP and report** progress before continuing.
- If a step takes >2× estimated time, **invoke escalation** — don't grind.
- Visual quality matters: not raw primitives, not pixel art — clean cartoon shapes.
- Educational content must use EXACT Dutch text from `EducationContent.cs` above. These facts are accurate and tied to the Tuinen van de Toekomst project.

**Start now with STEP 0.**
