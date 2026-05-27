# Current Issues

Last reviewed: 2026-05-23

Scope: static inspection of the Unity C# gameplay/UI code under `Assets/Assets`. Unity Editor validation was not run from this shell because no Unity, `dotnet`, `csc`, or `mcs` CLI is available here.

## Critical

### 1. Built game can show no visible map

Reported symptom: in a build, the main gameplay map was not visible.

Relevant code/assets:
- `Assets/Assets/Map/MapLayerController.cs:36` waits for `MainCanvas`, then builds `MapImage` under that canvas.
- `Assets/Assets/Map/MapLayerController.cs:94` loads `Resources.Load<Texture2D>("Maps/EuropeMap")`.
- `Assets/Assets/Map/MapLayerController.cs:96` falls back to `Resources.Load<Texture2D>("EuropeMap")`.
- `Assets/Assets/Resources/Maps/EuropeMap.png` exists, so the primary resource path appears correct.
- `Assets/Assets/UI/GameUIManager.cs:199` destroys stale `MapCanvas`, but the active map is now built as `MapImage` under `MainCanvas`.
- `Assets/Assets/UI/GameUIManager.cs:573` calls `ApplyPhaseVisibility` immediately after building UI.
- `Assets/Assets/UI/GameUIManager.cs:632` and `Assets/Assets/UI/GameUIManager.cs:926` wait asynchronously for `MapLayerController.Instance.MapImage`.
- `Assets/Assets/UI/GameUIManager.cs:2092` re-parents click zones under `MapImage` when the map is built.

Why this is more likely a runtime/layering/lifecycle issue than a missing asset issue: the map texture is present at the path used by `Resources.Load<Texture2D>("Maps/EuropeMap")`. If the build still shows no map, the failure is probably that `MapLayerController` never builds against the active `MainCanvas`, the `MapImage` is created while the game canvas is hidden, draw order places an opaque UI element above it, or a later UI rebuild destroys/reorders the map unexpectedly.

Likely reproduction:
1. Make a player build or run the same scene flow used by the build.
2. Start from the main menu, pick a faction, and enter gameplay.
3. Observe whether `MainCanvas/MapImage` exists at runtime, whether its `RawImage.texture` is `EuropeMap`, and whether its alpha/rect/sibling order make it visible.

Suggested investigation:
- Log when `MapLayerController.Start`, `BuildMap`, and `GameUIManager.OnMapLayerBuilt` run in a build.
- In the Unity hierarchy during play mode, verify that `MainCanvas/MapImage` exists, is active, has a non-null texture, and is not hidden behind `mapAreaBg`, `mapFrame`, panels, or a disabled parent canvas.
- Confirm the gameplay scene actually contains or receives `MapLayerController` from `Initializer`.
- Confirm `SetGameCanvasVisible(false)` during cutscenes is followed by a visible canvas before/after `GameManager.BeginGame`.

### 2. Cards are not shown in the UI during gameplay

Reported symptom: cards were not shown in the UI.

Relevant code:
- `Assets/Assets/UI/GameUIManager.cs:527` creates `HandArea`.
- `Assets/Assets/UI/GameUIManager.cs:2969` shows `HandArea` only during `DRAW_PHASE` or `PLAY_PHASE`.
- `Assets/Assets/UI/GameUIManager.cs:4138` creates an inactive `cardPrefab`.
- `Assets/Assets/UI/GameUIManager.cs:2281` and `Assets/Assets/UI/GameUIManager.cs:4052` instantiate cards into `HandArea` and then activate them.
- `Assets/Assets/UI/GameUIManager.cs:2284` and `Assets/Assets/UI/GameUIManager.cs:4055` call `CardUIController.BuildCard()` before `SetCard`.
- `Assets/Assets/UI/GameUIManager.cs:599` and `Assets/Assets/UI/GameUIManager.cs:2115` move `HandArea` to the last sibling for visibility.

Risk areas:
- `HandArea` is intentionally hidden outside `DRAW_PHASE` and `PLAY_PHASE`; if the build spends little/no visible time in those states, the hand can look missing.
- The prefab starts inactive by design, so every creation path must call `go.SetActive(true)` and `BuildCard()` before the player sees anything.
- The hand is rebuilt from the current faction hand data. If draw/deck setup fails, the UI can be structurally correct but empty.
- Higher-sibling panels are moved above most UI; `HandArea` is later moved to the top, but economy/market/probability overlays can still obscure the lower screen depending on state.
- Card clickability is only enabled during `PLAY_PHASE`; visible-but-disabled cards during `DRAW_PHASE` can be mistaken for broken interaction.

Likely reproduction:
1. Enter gameplay after faction selection.
2. During `DRAW_PHASE` or `PLAY_PHASE`, inspect `MainCanvas/HandArea`.
3. Confirm whether `HandArea` is active, has child card objects, and each child has `CanvasGroup.alpha == 1`.
4. Confirm the player faction hand contains cards after draw.

Suggested investigation:
- Log hand count before `RefreshHandUI`.
- Log each card instantiated into `HandArea`.
- In play mode, verify `HandArea.activeInHierarchy`, child count, child scale, child rect size, and `CanvasGroup.alpha`.
- Check whether phase transitions hide the hand before the first rendered frame in builds.

### 3. Card asset system does not match the current card data/UI system

Reported symptom: the card system does not match.

Relevant code/assets:
- `Assets/Assets/Data/CardDatabase.cs:47` generates `card.cardId` as zero-based integers at runtime.
- `Assets/Assets/Data/CardDatabase.cs` currently creates 36 cards across four factions.
- `Assets/Assets/UI/CardUIController.cs:311` loads card art from `Resources/Cards/Archive/<cardId>`.
- `Assets/Assets/Resources/Cards/` contains `card1.png` through `card24.png`.
- `Assets/Assets/Resources/Cards/Archive/` contains only `1.png`.

Impact: most designed card images cannot be loaded by the current code. Runtime IDs are `0..35`, but the available root assets are named `card1.png..card24.png`, and the archive path only has `1.png`. This means cards will either use a single matching archive image or fall back to procedural placeholder art. Even if cards become visible, the visual card set will not match the intended card art/design.

Likely reproduction:
1. Enter a phase where cards are visible.
2. Inspect a card whose `cardId` is not `1`.
3. Observe placeholder art instead of a matching PNG.

Suggested investigation:
- Decide whether card art should be keyed by stable string IDs, by generated numeric IDs, or by explicit asset path fields on `CardData`.
- Decide whether the intended assets are the root `Resources/Cards/card*.png` files or the `Archive` folder.
- Avoid relying on generated list order for permanent art mapping; adding/reordering cards will otherwise break art assignments.

### 4. Attack target validation allows non-adjacent territory attacks

Affected code:
- `Assets/Assets/UI/AttackPhaseUI.cs:209` accepts any non-player territory after a source is selected.
- `Assets/Assets/UI/GameUIManager.cs:864` does the same for card-driven territory attacks.
- `Assets/Assets/Core/TurnActionController.cs:128` validates owner and troop count, but does not validate adjacency.
- `Assets/Assets/Core/GameManager.cs:613` resolves any queued `PlayedCard` with a `targetTerritory`.

Impact: a player can select a valid source, then click any enemy/neutral territory, including a territory outside the highlighted attack routes. The pending attack is resolved without a shared-border check, so map topology can be bypassed.

Likely reproduction:
1. Enter attack selection with a player territory that has at least one valid adjacent target.
2. Select that source territory.
3. Click a distant non-player territory that is not in `ProvinceGraph.GetAttackNeighbours(source.name)`.
4. The UI queues/resolves the attack anyway.

Suggested fix: make `TurnActionController.ValidateTerritoryAttack` enforce `ProvinceGraph.GetAttackNeighbours(source.name).Contains(target.name)` or a single canonical attack-route helper. Also make `AttackPhaseUI.OnNodeClicked` and `GameUIManager.OnTerritoryClicked` reject non-highlighted targets before queueing.

### 5. Player card combos are cleared before resolution

Affected code:
- `Assets/Assets/Core/GameManager.cs:584` clears `playerCardsThisResolution`.
- `Assets/Assets/Core/GameManager.cs:602` calls `CheckCardCombos`.
- `Assets/Assets/Core/GameManager.cs:913` exits early when `playerCardsThisResolution.Count < 2`.

Impact: player cards are added to `playerCardsThisResolution` when played, but the list is cleared at the start of `ResolutionAndAIPhase` before combo detection. Player combo bonuses and combo log messages probably never trigger.

Likely reproduction:
1. Play two cards of the same type in one turn.
2. End the turn.
3. Observe that no combo log appears and no combo bonus applies.

Suggested fix: do not clear `playerCardsThisResolution` at the start of resolution. Clear it only after resolution completes or at the start of the next player input phase before any player cards are accepted.

## High

### 6. Attack phase can trigger even when there are no valid routes

Affected code:
- `Assets/Assets/Core/GameManager.cs:1154` checks only that some non-owned territory exists and some player territory has more than one troop.
- `Assets/Assets/UI/AttackPhaseUI.cs:184` uses the stricter `GetValidTargets(territory).Count > 0` check.

Impact: `AttackPhaseThenResolution` can enter `ATTACK_PHASE` and wait out the timer even when no source has a valid adjacent attack target. The UI will tell the player to choose a valid source, but none exists.

Likely reproduction: create a state where the player owns a territory with 2+ troops, but all valid attack neighbours are also player-owned or no attack neighbours exist. End turn; the attack phase can still open and stall until timeout.

Suggested fix: rewrite `HasAttackPhaseOptions` to scan player-controlled territories with `troops > 1` and require `GetAdjacentEnemyTerritories(source).Count > 0` or the same valid-target helper used by attack UI.

### 7. AI neutral expansion path reports success even when no attack is queued

Affected code:
- `Assets/Assets/Gameplay/AIController.cs:128` calls `TryExpandNeutral` up to twice during `RESOLUTION_PHASE`.
- `Assets/Assets/Gameplay/AIController.cs:273` calls `TurnActionController.PlayTerritoryAttack(faction, null, source, t)` but ignores its return value.
- `Assets/Assets/Core/TurnActionController.cs:141` rejects free territory attacks outside `ATTACK_PHASE`.

Impact: AI neutral expansion attempts during resolution are rejected by validation, but `TryExpandNeutral` still returns `true`, so the AI believes it used expansion attempts while no expansion or queued attack happened.

Suggested fix: either call a real expansion API that is valid during AI resolution, or allow a separate AI expansion action during `RESOLUTION_PHASE`. In either case, return the result of the action instead of unconditional `true`.

## Medium

### 8. Reinforcement assignment can claim neutral territory during reinforcement phase

Affected code:
- `Assets/Assets/UI/GameUIManager.cs:745` allows reinforcing player-owned or neutral territories.
- `Assets/Assets/UI/GameUIManager.cs:749` immediately converts a neutral territory to the player's faction.
- `Assets/Assets/Core/GameManager.cs:399` does not validate ownership or game state in `AssignReinforcementToTerritory`.

Impact: during reinforcement placement, clicking a neutral territory converts it to the player and spends one reinforcement. This bypasses the normal neutral-expansion rules and adjacency/cost checks unless this is an intentional design rule.

Suggested fix: if this is not intended, restrict reinforcement placement to already-owned territories and add ownership/state/count validation inside `GameManager.AssignReinforcementToTerritory`.

### 9. Card-limit UI message says 3 while rules allow 2

Affected code:
- `Assets/Assets/Core/GameManager.cs:29` sets `MAX_CARDS_PER_TURN = 2`.
- `Assets/Assets/UI/GameUIManager.cs:2300` displays `Maximum 3 cards per turn.`

Impact: the UI gives the wrong rule when the player hits the limit.

Suggested fix: use `GameManager.Instance.MaxCardsPerTurn` in the message.

## Documentation Notes

`CLAUDE.md` has been rewritten to remove stale setup/path items from the active-fix list. It now points future work at the current build-facing issues: missing visible map, cards not showing in the UI, and the card data/art mapping mismatch.
