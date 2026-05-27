# CLAUDE.md — Shadow Empire: 1800

## Project Overview
A Unity-based historical strategy game set in 1800s Europe. Players control secret societies
(Illuminati, Knights Templar, Freemasons, Carbonari) competing for territory, influence, and
resources through card-based actions on a historically-accurate map. Built for a game jam,
now in active post-jam development.

---

## Unity Setup Requirements

- **Unity Version**: 2021 LTS or higher
- **Current package state**: `com.unity.ugui` is already present in `Assets/Packages/manifest.json`.
- **Input handling**: UI code uses legacy `UnityEngine.EventSystems` pointer handlers. If input breaks in Editor, check Edit -> Project Settings -> Player -> Active Input Handling and allow the old Input Manager or Both.
- **Validation note**: this repository has no CLI test/build path confirmed from shell. Validate UI/map/card changes in the Unity Editor and in a player build.
- **Texture Read/Write**: `EuropeMap.png` at `Assets/Resources/Maps/EuropeMap.png` **must** have Read/Write enabled in its import settings for `MapLayerController.IsLandAtUV()` (and therefore the whole province land-detection/nudge system) to work. Without it, `GetPixel` throws and every UV is treated as land (safe fallback — no provinces are rejected, but nudging is skipped). Enable this via the Inspector by selecting the PNG and checking **Read/Write** under Advanced.

---

## Folder Structure

```
Assets/
└── Assets/
    ├── Resources/
    │   ├── Music/          # Loaded via Resources.LoadAll<AudioClip>("Music")
    │   ├── Maps/           # EuropeMap.png is loaded as "Maps/EuropeMap"
    │   └── Cards/          # Current assets mostly card1.png..card24.png
    │       └── Archive/    # Runtime card art lookup currently targets this folder
    ├── StreamingAssets/
    │   └── Cutscenes/      # Faction cutscenes loaded by CutsceneManager
    ├── Core/               # Game state, turn flow, data models
    ├── UI/                 # Code-built UI and screen managers
    ├── Map/                # Map rendering, territory, province systems
    ├── Gameplay/           # AI, combat, card resolution
    ├── Data/               # Runtime card database
    └── Tests/              # Manual test scripts; no automated framework yet
```

---

## Architecture

### Singleton Managers (boot order matters)
1. `Initializer.cs` — `[RuntimeInitializeOnLoadMethod]` creates core singletons before scene load
2. `GameManager` — central game state, turn management, faction initialization
3. `AudioManager` — music playback, loads all clips from `Resources/Music/`
4. `AIController` — AI decision-making, initialized with faction personalities
5. `GameUIManager` — builds all UI in code (no prefabs in scene), manages canvas layers
6. `CutsceneManager` — VideoPlayer-based cutscenes from StreamingAssets
7. `TurnPhaseManager` / `TurnActionController` — turn flow and card effect execution

### Canvas Layer Order (sortingOrder)
- `MenuCanvas` (MainMenuManager) — do not touch from GameUIManager
- `MainCanvas` sortingOrder=10 — primary game UI
- Runtime map image is currently built as `MainCanvas/MapImage` by `MapLayerController`
- `MapCanvas` may exist only as stale/legacy scene state and is destroyed by `GameUIManager.Start()`
- `TimelineCanvas` — historical timeline slider
- `CutsceneCanvas` — rendered on top of everything during cutscenes

### Card System
Cards are built **entirely in code** — no prefabs exist in the scene.
- `GameUIManager.BuildCardPrefab()` creates the base GO
- `CardUIController.BuildCard()` constructs all visual layers as child GameObjects
- Hand is a `HorizontalLayoutGroup` (`HandArea`) anchored at bottom 23% of screen
- **HandArea is only visible during `DRAW_PHASE` and `PLAY_PHASE`** — check game state
  when debugging card visibility
- Card data is generated at runtime in `CardDatabase` with zero-based numeric IDs
- Card art currently loads from `Resources/Cards/Archive/<cardId>` and falls back to procedural
  placeholder art if the texture is not found
- Known mismatch: most existing PNGs are under `Resources/Cards/card1.png` through
  `card24.png`, while the loader expects `Resources/Cards/Archive/<cardId>`. Do not assume
  visible cards are using final art until this mapping is reconciled.

### Key Game States (GameState enum)
```
DRAW_PHASE -> PLAY_PHASE -> RESOLUTION_PHASE -> REINFORCEMENT_PHASE -> ATTACK_PHASE -> FORTIFY_PHASE -> next turn
```
UI visibility is gated on these states in `GameUIManager.ApplyPhaseVisibility()`.

---

## Known Issues & Active Fixes

The detailed active issue list is in `CURRENT_ISSUES.md`. Treat that file as the current bug ledger before making fixes.

### Build-visible map missing
Reported build symptom: the gameplay map was not seen. `EuropeMap.png` exists at the path used by `Resources.Load<Texture2D>("Maps/EuropeMap")`, so current investigation should focus on lifecycle and layering:
- `MapLayerController` waits for `MainCanvas` and builds `MainCanvas/MapImage`.
- `GameUIManager` hides the game canvas during the faction cutscene and later makes it visible.
- `MapImage` is inserted under `MainCanvas`, then draw order is adjusted by both `PlaceBehindGameplayMap()` and `EnforceDrawOrder()`.
- In a build, verify `MainCanvas/MapImage` exists, is active, has a texture, has a visible rect, and is not covered by opaque UI.

### Cards missing from UI
Reported build symptom: cards were not shown. The hand is intentionally state-gated:
1. Confirm the current `GameState` is `DRAW_PHASE` or `PLAY_PHASE`.
2. Confirm `MainCanvas/HandArea` is active and has instantiated child cards.
3. Confirm each child card is active, has `CanvasGroup.alpha == 1`, and has a nonzero `RectTransform`.
4. Confirm the player faction hand is populated before `RefreshHandUI`.
5. Check overlays such as market/economy/probability panels if the cards exist but are covered.

### Card system mismatch
The current runtime card system and card assets are not aligned:
- `CardDatabase` generates 36 cards and assigns numeric IDs from runtime list order.
- `CardUIController` loads art from `Resources/Cards/Archive/<cardId>`.
- The asset folder mostly contains `Resources/Cards/card1.png` through `card24.png`; `Archive` currently contains only `1.png`.
- Result: most cards will use placeholder art even if the hand UI is visible.

Before implementing card fixes, choose one stable art contract: explicit art path on `CardData`, stable string IDs, or a renamed/reorganized asset set that matches numeric IDs. Avoid relying on generated list order for permanent art mapping.

---

## Systems Reference

### AudioManager
- Loads all AudioClips from `Resources/Music/` on Awake
- Plays random tracks, chains to next on completion
- No external dependencies

### CutsceneManager
- Videos must live in `Application.streamingAssetsPath + "/Cutscenes/"`
- Filenames must match `cutscenePaths[]` array exactly (case-sensitive on Linux)
- Missing video silently skips to `onComplete` callback — game continues
- VideoPlayer requires H.264 MP4; other codecs may fail silently on some platforms

### MapSystem
- `MapLayerController` / `MapLayerManager` — layer toggling and RawImage display
- `FactionOverlayRenderer` — colors territories by controlling faction
- `TerritoryGraphRenderer` — renders territory connections
- `ProvinceData` / `ProvinceGraph` — granular province-level data (deeper than territory)
- `MapAPIFetcher` — generates map textures procedurally per historical year
- `MapTimelineUI` — Slider-driven year selection

### Economy & Cards
- `BlackMarketSystem` — secret resource trading between factions
- `CardMarket` / `CardMarketEntry` — public card economy
- `CardProbabilitySystem` — weights card draws and AI decisions
- `DeckManager` — handles draw/discard, per-faction decks
- Card types: ATTACK, DEFENSE, INFLUENCE, SABOTAGE, FARMING, BLACK_MARKET

### AI
- `AIController.InitPersonalities()` — defines per-faction behavioral weights
- AI evaluates cards each turn based on faction personality + game state
- AI plays through the same `TurnActionController` as the human player

---

## Development Guidelines

### Adding a New Card Type
1. Add enum value to `CardType` in CardData.cs
2. Add color constant in CardUIController.cs
3. Add case to `GeneratePlaceholderArt()` in CardUIController.cs
4. Add evaluation logic in AIController.cs
5. Add resolution logic in CardResolver.cs / TurnActionController.cs

### Adding a New UI Screen
1. Create a new Manager script following the singleton pattern
2. Build UI entirely in code in a `BuildUI()` method called from `Awake()`
3. Assign a Canvas with a unique `sortingOrder`
4. Reference the color palette constants from GameUIManager for visual consistency
5. Do NOT add UI GameObjects directly in the scene — everything is code-built

### Modifying AI Behavior
- Faction personalities are in `AIController.InitPersonalities()`
- Each personality has weighted preferences per card type and target selection
- Test AI changes by running a full game with the human player spectating (pick any faction,
  skip all input)

### Map Changes
- Province-level changes: `ProvinceData.cs`, `ProvinceDatabase.cs`
- Territory-level changes: `TerritoryData.cs`, `TerritoryGraph.cs`
- Visual changes: renderers in `Map/` folder
- Always test map changes at multiple timeline years — textures are year-specific

---

## Post-Jam Roadmap (priority order)
1. Verify build-visible map lifecycle and draw order.
2. Make cards visible and interactive end-to-end in Editor and player builds.
3. Reconcile card data IDs with the actual card art asset layout.
4. Fix attack adjacency validation and attack-phase option detection.
5. Fix player card combo resolution timing.
6. Deepen economy — resources should constrain military options.
7. Add diplomatic relations system (numerical faction relationships).
8. Weight cards as historical events, not just combat modifiers.
9. Fog of war / intelligence layer (expand CardProbabilitySystem).
10. Province-level granularity in gameplay (population, terrain, stability).
