# TODO — Thief Simulator (Weight of Greed)

## STEP 0 — Project Setup & Log Files
- [x] Create BUGLOG.md
- [x] Create SESSION_LOG.md
- [x] Create TODO.md
- [x] Verify Unity project has URP 2D + Tilemap + InputSystem

## STEP 1 — Script Suite (all .cs files)
- [x] Scripts/Game/GameManager.cs
- [x] Scripts/Game/ScoreManager.cs
- [x] Scripts/Game/UIManager.cs
- [x] Scripts/Game/ExitTrigger.cs
- [x] Scripts/Game/MapBuilder.cs
- [x] Scripts/Game/RestartListener.cs
- [x] Scripts/Player/PlayerController.cs
- [x] Scripts/Player/PlayerInventory.cs
- [x] Scripts/Player/PlayerSound.cs
- [x] Scripts/Guard/GuardController.cs
- [x] Scripts/Guard/GuardVision.cs
- [x] Scripts/Guard/GuardPatrol.cs
- [x] Scripts/Items/Item.cs
- [x] Scripts/Items/ItemPickup.cs
- [x] Scripts/Utils/SoundRadius.cs

## STEP 2-11 — Scene built via SceneBootstrapper
- [x] Tilemap floor + walls (4 rooms + hallway + doorways)
- [x] Wall TilemapCollider2D + CompositeCollider2D (Static)
- [x] Physics layers: Walls, Player, Guard, Items
- [x] Player: circle sprite, Rigidbody2D, all scripts, SoundRadius child
- [x] Guard: red sprite, 6 waypoints, GuardPatrol + GuardVision + GuardController
- [x] Guard flashlight Light2D
- [x] 5 Items placed: Coin/Ring/Vase/Painting/Crown with glow lights
- [x] Exit door trigger (green, bottom wall gap)
- [x] 4 room warm point lights
- [x] Global dark blue ambient light
- [x] HUD: Score / Weight / Items
- [x] Win panel, Lose panel, Pause panel
- [x] Sneak tooltip (10s)
- [x] RestartListener (R key)

## STEP 12 — Polish + audio
- [x] AudioManager.cs (procedural pickup/footstep/alarm sounds)

## STEP 13 — Build .exe
- [ ] Standalone Windows build in /Build/
