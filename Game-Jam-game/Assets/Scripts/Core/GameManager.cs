using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum GameState
{
    DRAW_PHASE,
    PLAY_PHASE,
    RESOLUTION_PHASE,
    REINFORCEMENT_PHASE,
    ATTACK_PHASE,
    FORTIFY_PHASE,
    GAME_OVER
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState currentState = GameState.DRAW_PHASE;
    public int currentTurn = 1;
    public int playerFactionId = 0;

    public List<FactionData> factions = new List<FactionData>();

    Queue<PlayedCard> resolutionQueue = new Queue<PlayedCard>();
    public const int MAX_CARDS_PER_TURN = 3;
    List<CardData> playerCardsThisResolution = new List<CardData>();
    public List<CardData> allCardsThisResolution = new List<CardData>();
    public List<CardData> PlayerCardsThisResolution => playerCardsThisResolution;
    // ADDED: UPGRADE 2 - stores attacker's chosen dice count per pending attack
    public int pendingAttackerDiceCount = 0;

    public List<TerritoryData> territories = new List<TerritoryData>();

    public System.Action<CardData, FactionData, FactionData> OnCardPlayed;
    public System.Action<FactionData> OnFactionEliminated;
    public System.Action<int> OnTurnChanged;
    public System.Action<GameState> OnStateChanged;
    public System.Action<string> OnLogMessage;
    public System.Action<bool> OnGameOver;
    public System.Action<int> OnYearChanged;
    public System.Action<int> OnSubTurnChanged;
    public System.Action<string, string> OnHistoricalEventTriggered;
    public System.Action<int> OnReinforcementPhase;
    public System.Action<CombatResult, TerritoryData, TerritoryData> OnCombatResolved;
    public System.Action<float> OnPhaseTimerUpdate;
    public System.Action OnMarketRefresh;
    public const float PHASE_TIME_LIMIT = 30f;
    public const float FORTIFY_TIME_LIMIT = 20f;
    Coroutine phaseTimerCoroutine;
    bool isResolutionRunning = false;
    Dictionary<string, int> comboPowerBoost = new Dictionary<string, int>();

    public const int MAX_TURNS = 150;
    public const int END_YEAR = 1945;
    public BlackMarketSystem.MarketSlot[] currentMarket;
    public int CurrentYear => Mathf.Min(END_YEAR, 1790 + currentTurn);

    public int currentSubTurn = 0;
    public const int MAX_SUB_TURNS = 180;
    public bool IsWaitingForEventDismiss { get; set; }
    public bool IsWaitingForReinforcements { get; set; }
    public bool IsWaitingForAttackSelection { get; set; }
    public bool IsWaitingForFortify { get; set; }

    public TerritoryData pendingAttackSource = null;
    public TerritoryData pendingAttackTarget = null;
    Coroutine subTurnCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void SetGameState(GameState nextState)
    {
        if (currentState == nextState)
            return;

        currentState = nextState;
        OnStateChanged?.Invoke(currentState);
    }

    public void BeginGame()
    {
        InitializeGame();
    }

    void InitializeGame()
    {
        factions = FactionDatabase.GetAllFactions();
        factions[playerFactionId].isPlayerControlled = true;

        territories = TerritoryDatabase.GetAllTerritories();
        OnYearChanged?.Invoke(CurrentYear);

        foreach (FactionData faction in factions)
        {
            faction.ResetForNewGame();
            faction.deck = CardDatabase.GetFactionDeck(faction.factionId);
            DeckManager.Shuffle(faction.deck);
            DeckManager.DealInitialHand(faction, 5);
        }

        AssignStartingTerritories();

        if (GameUIManager.Instance != null)
            GameUIManager.Instance.InitializeUI(factions);

        LogMessage("=== Secret Societies Battle for Europe ===");
        LogMessage("Four factions enter. One leaves.");
        LogMessage("Control territories, assign troops, and conquer Europe!");

        StartCoroutine(StartPlayerTurn());
    }

    void AssignStartingTerritories()
    {
        var startAssignments = new Dictionary<int, List<string>>
        {
            { 0, new List<string> { "Rome",           "Florence"    } },
            { 1, new List<string> { "Paris",          "Lyon"        } },
            { 2, new List<string> { "London",         "Amsterdam"   } },
            { 3, new List<string> { "Constantinople", "Dubrovnik"   } }
        };

        foreach (var kvp in startAssignments)
        {
            int fId = kvp.Key;
            foreach (string tName in kvp.Value)
            {
                TerritoryData t = territories.Find(x => x.name == tName);
                if (t == null) continue;
                t.controlledBy = fId;
                t.troops = 5;
                FactionData f = factions.Find(x => x.factionId == fId);
                if (f != null && !f.territories.Contains(t))
                    f.territories.Add(t);
            }
        }
        MapSystemController.Instance?.SyncProvincesFromTerritories();
    }

    IEnumerator StartPlayerTurn()
    {
        if (currentTurn > MAX_TURNS)
        {
            FactionData winner = factions
                .Where(f => !f.isEliminated)
                .OrderByDescending(f => f.territories.Count)
                .ThenByDescending(f => f.power)
                .FirstOrDefault();

            SetGameState(GameState.GAME_OVER);
            if (winner != null)
            {
                LogMessage($"=== TURN LIMIT REACHED (Turn {MAX_TURNS}) ===");
                LogMessage($"Victory by territorial control: {winner.factionName} dominates with {winner.territories.Count} provinces!");
                OnGameOver?.Invoke(winner.isPlayerControlled);
            }
            yield break;
        }

        SetGameState(GameState.REINFORCEMENT_PHASE);

        currentSubTurn = 0;
        OnSubTurnChanged?.Invoke(0);

        foreach (FactionData f in factions)
        {
            f.territories = territories
                .Where(t => t.controlledBy == f.factionId)
                .ToList();
        }

        if (TerritoryGraphRenderer.Instance != null)
            TerritoryGraphRenderer.Instance.RedrawArrows();

        LogMessage($"--- Turn {currentTurn} ---");

        if (HistoricalEventManager.TryTriggerEvent(this))
        {
            OnHistoricalEventTriggered?.Invoke(
                HistoricalEventManager.ActiveEvent.eventName,
                HistoricalEventManager.ActiveEvent.description);
            IsWaitingForEventDismiss = true;
            float timeout = 30f;
            float elapsed = 0f;
            while (IsWaitingForEventDismiss && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            if (IsWaitingForEventDismiss)
            {
                LogMessage("Event auto-dismissed (timeout).");
                IsWaitingForEventDismiss = false;
            }
        }

        foreach (FactionData f in factions)
        {
            if (!f.isEliminated)
            {
                ApplyTerritoryIncome(f);
                ApplyPassiveEffects(f);
                ApplyFarmingBoosts(f);
            }
        }

        CardMarket.Refresh(this);
        foreach (FactionData f in factions)
        {
            if (f.isEliminated) continue;
            LogMessage($"{f.factionName} earns {f.goldIncome} gold (total: {f.gold}).");
        }

        if (currentTurn % 10 == 0)
        {
            var borderProvinces = territories
                .Where(t => t.controlledBy != playerFactionId)
                .OrderBy(_ => Random.value)
                .Take(2);
            foreach (var t in borderProvinces)
            {
                int change = Random.value > 0.5f ? 2 : -2;
                t.troops = Mathf.Max(1, t.troops + change);
                LogMessage($"Unrest in {t.name}: troops {(change > 0 ? "increased" : "decreased")} by {Mathf.Abs(change)}.");
            }
        }

        currentMarket = BlackMarketSystem.GenerateMarket(factions, territories, currentTurn);
        if (OnMarketRefresh != null) OnMarketRefresh();

        foreach (FactionData f in factions)
        {
            if (f.isEliminated) continue;
            int reinforcements = CombatManager.CalculateReinforcementsForFaction(f);
            f.pendingReinforcements = reinforcements;
            LogMessage($"{f.factionName} receives {reinforcements} reinforcements.");
        }

        FactionData player = factions[playerFactionId];
        if (!player.isEliminated)
        {
            OnReinforcementPhase?.Invoke(player.pendingReinforcements);
            IsWaitingForReinforcements = true;
            bool timedOut = false;
            if (TurnPhaseManager.Instance != null)
                TurnPhaseManager.Instance.StartPhaseTimer(() => timedOut = true);
            float timeout = TurnPhaseManager.PHASE_TIME_LIMIT;
            float elapsed = 0f;
            while (IsWaitingForReinforcements && elapsed < timeout && !timedOut)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            if (TurnPhaseManager.Instance != null)
                TurnPhaseManager.Instance.StopPhaseTimer();
            if (IsWaitingForReinforcements)
            {
                AutoAssignReinforcements(player);
                IsWaitingForReinforcements = false;
                LogMessage("Reinforcements auto-assigned (timeout).");
            }
        }
        else
        {
            AutoAssignReinforcements(player);
        }

        foreach (FactionData f in factions)
        {
            if (f.isEliminated || f.isPlayerControlled) continue;
            AutoAssignReinforcements(f);
        }

        // ADDED: UPGRADE 1 - mandatory territory card trade at 5+ cards (mirrors AI behavior)
        FactionData playerFaction = factions[playerFactionId];
        if (!playerFaction.isEliminated && playerFaction.territoryCards.Count >= 5)
        {
            List<TerritoryCard> set = FindBestTradeSet(playerFaction.territoryCards);
            if (set != null)
            {
                int bonus = TradeCardSet(playerFaction, set);
                LogMessage($"You must trade territory cards — gained {bonus} reinforcements.");
            }
        }

        SetGameState(GameState.DRAW_PHASE);

        bool awardCardsThisRound = currentTurn % 5 == 0;
        if (awardCardsThisRound)
        {
            LogMessage($"=== Turn {currentTurn}: Card Draw Round ===");
            foreach (FactionData f in factions)
            {
                if (!f.isEliminated && !f.isPlayerControlled)
                {
                    if (f.skipDrawNextTurn)
                    {
                        f.skipDrawNextTurn = false;
                        LogMessage($"{f.factionName}'s draw was sabotaged.");
                        continue;
                    }
                    var drawn = CardProbabilitySystem.DrawWeightedCard(f, factions, territories, currentTurn);
                    if (drawn != null) LogMessage($"{f.factionName} draws: {drawn.cardName} ({drawn.cardType})");
                }
            }

            bool playerSkipped = player.skipDrawNextTurn;
            if (playerSkipped)
            {
                player.skipDrawNextTurn = false;
                LogMessage("Your draw phase is skipped (sabotaged by black market)!");
            }
            else
            {
                CardData playerDrawn = CardProbabilitySystem.DrawWeightedCard(player, factions, territories, currentTurn);
                if (playerDrawn != null)
                {
                    LogMessage($"You drew: {playerDrawn.cardName}");
                    if (playerDrawn.rarity == CardRarity.LEGENDARY && GameUIManager.Instance != null)
                        GameUIManager.Instance.FlashLegendaryDraw();
                }
                else
                    LogMessage("Your hand is full (max 7).");
            }
        }
        else
        {
            int nextDrawTurn = ((currentTurn / 5) + 1) * 5;
            if (nextDrawTurn - currentTurn == 1)
                LogMessage($"Card draw next turn (Turn {nextDrawTurn}).");
        }

        if (currentTurn == 1)
        {
            LogMessage("=== Turn 1: Initial Card Draw ===");
            for (int i = 0; i < 3; i++)
            {
                CardData initialDrawn = CardProbabilitySystem.DrawWeightedCard(player, factions, territories, currentTurn);
                if (initialDrawn != null)
                {
                    LogMessage($"You drew: {initialDrawn.cardName}");
                }
            }
        }

        if (GameUIManager.Instance != null)
            GameUIManager.Instance.UpdatePlayerHand(player.hand);

        yield return new WaitForSeconds(0.5f);

        SetGameState(GameState.PLAY_PHASE);
        foreach (FactionData f in factions)
            f.cardsPlayedThisTurn = 0;

        if (GameUIManager.Instance != null)
            GameUIManager.Instance.EnableCardInteractions(true);

        LogMessage($"Your turn — play up to 3 cards, then press END TURN. ({PHASE_TIME_LIMIT}s timer)");

        subTurnCoroutine = StartCoroutine(SubTurnCoroutine());
        phaseTimerCoroutine = StartCoroutine(PlayPhaseTimerCoroutine());
    }

    void AutoAssignReinforcements(FactionData faction)
    {
        if (faction.territories.Count == 0 || faction.pendingReinforcements <= 0) return;
        int perTerritory = faction.pendingReinforcements / faction.territories.Count;
        int remainder = faction.pendingReinforcements % faction.territories.Count;
        foreach (TerritoryData t in faction.territories)
        {
            t.AddTroops(perTerritory);
            if (remainder > 0)
            {
                t.AddTroops(1);
                remainder--;
            }
        }
        faction.pendingReinforcements = 0;
    }

    // FIXED: BUG 7 - guard at top of AutoAssignReinforcements prevents wasted work when 0 pending
    /// <summary>Assigns reinforcement troops to a specific territory during the reinforcement phase.</summary>
    public void AssignReinforcementToTerritory(TerritoryData territory, int count)
    {
        FactionData player = factions[playerFactionId];
        if (player.pendingReinforcements < count) count = player.pendingReinforcements;
        territory.AddTroops(count);
        player.pendingReinforcements -= count;
        OnReinforcementPhase?.Invoke(player.pendingReinforcements);

        if (player.pendingReinforcements <= 0)
            IsWaitingForReinforcements = false;
    }

    public int CardsPlayedThisTurn => GetPlayerFaction()?.cardsPlayedThisTurn ?? 0;
    public int MaxCardsPerTurn => MAX_CARDS_PER_TURN;

    public bool CanPlayMoreCards()
    {
        return factions[playerFactionId].cardsPlayedThisTurn < MAX_CARDS_PER_TURN && currentState == GameState.PLAY_PHASE;
    }

    public bool IsFactionDominant(FactionData f) => f.influence >= 100;

    public void PlayCard(CardData card, FactionData target)
    {
        TurnActionController.PlayCard(factions[playerFactionId], card, target);

        if (GameUIManager.Instance != null)
            GameUIManager.Instance.UpdatePlayerHand(factions[playerFactionId].hand);

        if (!CanPlayMoreCards())
            LogMessage("You've played the maximum of 3 cards this turn.");
    }

    /// <summary>Plays a card as a territory attack between adjacent territories.</summary>
    public void PlayTerritoryAttackCard(CardData card, TerritoryData sourceTerritory, TerritoryData targetTerritory)
    {
        TurnActionController.PlayTerritoryAttack(factions[playerFactionId], card, sourceTerritory, targetTerritory);

        if (GameUIManager.Instance != null)
            GameUIManager.Instance.UpdatePlayerHand(factions[playerFactionId].hand);

        if (!CanPlayMoreCards())
            LogMessage("You've played the maximum of 3 cards this turn.");
    }

    public void QueueCard(CardData card, FactionData source, FactionData target)
    {
        PlayedCard played = new PlayedCard(card, source, target);
        resolutionQueue.Enqueue(played);
    }

    public void QueueTerritoryAttack(CardData card, FactionData source, TerritoryData sourceTerritory, TerritoryData targetTerritory)
    {
        PlayedCard played = new PlayedCard(card, source, null, sourceTerritory, targetTerritory);
        resolutionQueue.Enqueue(played);
    }

    // FIXED: BUG 8 - isResolutionRunning guard prevents double-fire of ResolutionAndAIPhase
    public void EndPlayerTurn()
    {
        if (isResolutionRunning) return;
        if (currentState == GameState.FORTIFY_PHASE)
        {
            IsWaitingForFortify = false;
            return;
        }
        if (currentState != GameState.PLAY_PHASE) return;

        if (subTurnCoroutine != null)
        {
            StopCoroutine(subTurnCoroutine);
            subTurnCoroutine = null;
        }
        if (phaseTimerCoroutine != null)
        {
            StopCoroutine(phaseTimerCoroutine);
            phaseTimerCoroutine = null;
        }
        OnPhaseTimerUpdate?.Invoke(0f);

        if (GameUIManager.Instance != null)
            GameUIManager.Instance.EnableCardInteractions(false);

        StartCoroutine(AttackPhaseThenResolution());
    }

    IEnumerator AttackPhaseThenResolution()
    {
        FactionData player = GetPlayerFaction();
        int attacksThisPhase = 0;

        while (attacksThisPhase < 3 && player != null && !player.isEliminated && HasAttackPhaseOptions(player))
        {
            SetGameState(GameState.ATTACK_PHASE);
            LogMessage($"ATTACK: Choose territory attack route, or wait to skip. (Attack {attacksThisPhase + 1}/3)");
            IsWaitingForAttackSelection = true;
            pendingAttackSource = null;
            pendingAttackTarget = null;

            bool timedOut = false;
            if (TurnPhaseManager.Instance != null)
                TurnPhaseManager.Instance.StartPhaseTimer(() => timedOut = true);

            float elapsed = 0f;
            while (IsWaitingForAttackSelection && elapsed < TurnPhaseManager.PHASE_TIME_LIMIT && !timedOut)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (TurnPhaseManager.Instance != null)
                TurnPhaseManager.Instance.StopPhaseTimer();

            if (pendingAttackSource != null && pendingAttackTarget != null)
            {
                var attackEntry = new PlayedCard
                {
                    source = GetPlayerFaction(),
                    card = null,
                    sourceTerritory = pendingAttackSource,
                    targetTerritory = pendingAttackTarget
                };
                resolutionQueue.Enqueue(attackEntry);
                Debug.Log($"[AttackPhase] Player attack queued: {pendingAttackSource.name} -> {pendingAttackTarget.name}");
                attacksThisPhase++;
                IsWaitingForAttackSelection = false;
            }
            else
            {
                IsWaitingForAttackSelection = false;
                break;
            }
        }

        if (attacksThisPhase == 0)
        {
            LogMessage("Attack phase skipped: no valid attack routes or skipped by player.");
        }

        StartCoroutine(ResolutionAndAIPhase());
    }

    IEnumerator PlayPhaseTimerCoroutine()
    {
        float timeLeft = PHASE_TIME_LIMIT;
        while (timeLeft > 0 && currentState == GameState.PLAY_PHASE)
        {
            timeLeft -= Time.deltaTime;
            OnPhaseTimerUpdate?.Invoke(timeLeft);
            yield return null;
        }
        if (currentState == GameState.PLAY_PHASE)
        {
            OnPhaseTimerUpdate?.Invoke(0f);
            LogMessage("Time's up! Auto-ending turn.");
            EndPlayerTurn();
        }
    }

    IEnumerator SubTurnCoroutine()
    {
        while (currentState == GameState.PLAY_PHASE && currentSubTurn < MAX_SUB_TURNS)
        {
            yield return new WaitForSeconds(0.06f);
            if (currentState != GameState.PLAY_PHASE) break;
            currentSubTurn++;
            OnSubTurnChanged?.Invoke(currentSubTurn);
        }
    }

    IEnumerator ResolutionAndAIPhase()
    {
        isResolutionRunning = true;
        SetGameState(GameState.RESOLUTION_PHASE);

        LogMessage("=== Simultaneous Resolution ===");
        playerCardsThisResolution.Clear();
        allCardsThisResolution.Clear();

        foreach (FactionData faction in factions)
        {
            if (faction.isEliminated || faction.isPlayerControlled) continue;
            AIController.Instance.QueueCardsForTurn(faction, factions);
        }

        var allCards = resolutionQueue.ToList();
        for (int i = allCards.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (allCards[i], allCards[j]) = (allCards[j], allCards[i]);
        }
        resolutionQueue.Clear();
        foreach (var pc in allCards) resolutionQueue.Enqueue(pc);

        CheckCardCombos();

        while (resolutionQueue.Count > 0)
        {
            PlayedCard played = resolutionQueue.Dequeue();

            if (played.source == null) { yield return null; continue; }

            if (!played.source.isPlayerControlled && played.card != null)
                LogMessage($"\u25BA {played.source.factionName} plays {played.card.cardName} ({played.card.cardType})");

            if (played.targetTerritory != null)
            {
                yield return StartCoroutine(ResolveTerritoryAttack(played));
            }
            else if (played.card == null)
            {
                yield return null;
                continue;
            }
            else
            {
                CardResolver.Resolve(played.card, played.source, played.target);
            }

            CheckEliminations();
            if (currentState == GameState.GAME_OVER) yield break;
            yield return new WaitForSeconds(0.6f);
        }

        if (CheckWinCondition())
            yield break;

        SetGameState(GameState.FORTIFY_PHASE);
        LogMessage("FORTIFY: Move troops between adjacent owned territories.");

        IsWaitingForFortify = true;
        bool fortifyTimedOut = false;
        if (TurnPhaseManager.Instance != null)
            TurnPhaseManager.Instance.StartPhaseTimer(() => fortifyTimedOut = true);
        float fortifyTimeout = FORTIFY_TIME_LIMIT;
        float fortifyElapsed = 0f;
        while (IsWaitingForFortify && fortifyElapsed < fortifyTimeout && !fortifyTimedOut)
        {
            fortifyElapsed += Time.deltaTime;
            yield return null;
        }
        if (TurnPhaseManager.Instance != null)
            TurnPhaseManager.Instance.StopPhaseTimer();
        if (IsWaitingForFortify)
        {
            LogMessage("Fortify skipped (timeout).");
            IsWaitingForFortify = false;
        }

        foreach (FactionData f in factions)
        {
            if (f.isEliminated || f.isPlayerControlled) continue;
            AIController.Instance.ExecuteFortify(f);
        }

        foreach (FactionData f in factions)
        {
            if (!f.isEliminated)
                f.marketDiscount = 1f;
        }

        HistoricalEventManager.ClearActiveEvent(this);
        currentTurn++;
        OnYearChanged?.Invoke(CurrentYear);
        OnTurnChanged?.Invoke(currentTurn);

        yield return new WaitForSeconds(0.5f);
        isResolutionRunning = false;
        StartCoroutine(StartPlayerTurn());
    }

    // FIXED: BUG 1 - ResolveTerritoryAttack no longer duplicates territory transfer.
    // CombatManager.ResolveTerritoryBattle() handles all ownership changes internally.
    // Only territory card award and log remain here.
    IEnumerator ResolveTerritoryAttack(PlayedCard played)
    {
        TerritoryData source = played.sourceTerritory;
        TerritoryData target = played.targetTerritory;
        CardData card = played.card;

        if (played.source == null || source == null || target == null)
        {
            LogMessage("Invalid territory attack skipped.");
            pendingAttackerDiceCount = 0;
            yield break;
        }

        int attackBonusDice = 0;
        int defenseBonusDice = 0;
        bool attackerReroll = false;
        int preBattleSabotage = 0;
        int defenderShield = 0;

        if (card != null && card.cardType == CardType.ATTACK)
        {
            attackBonusDice = 1;
            attackerReroll = card.power >= 7;
        }

        FactionData targetFaction = target.controlledBy >= 0 ? factions.Find(f => f.factionId == target.controlledBy) : null;

        Queue<PlayedCard> remainingQueue = new Queue<PlayedCard>();
        if (targetFaction != null)
        {
            bool foundDefense = false;
            bool foundSabotage = false;
            foreach (PlayedCard pc in resolutionQueue)
            {
                bool consume = false;
                if (pc.card == null) { remainingQueue.Enqueue(pc); continue; }
                if (!foundDefense && pc.card.cardType == CardType.DEFENSE && pc.source == targetFaction)
                {
                    defenseBonusDice = pc.card.power >= 6 ? 1 : 0;
                    defenderShield = pc.card.power >= 4 ? 2 : 0;
                    consume = true;
                    foundDefense = true;
                    LogMessage($"{targetFaction.factionName} fortifies defenses with {pc.card.cardName}!");
                }
                if (!foundSabotage && pc.card.cardType == CardType.SABOTAGE && pc.target == played.source)
                {
                    preBattleSabotage = pc.card.power;
                    consume = true;
                    foundSabotage = true;
                    LogMessage($"Sabotage reduces {target.name} troops before battle!");
                }
                if (!consume)
                    remainingQueue.Enqueue(pc);
            }
            resolutionQueue.Clear();
            while (remainingQueue.Count > 0)
                resolutionQueue.Enqueue(remainingQueue.Dequeue());
        }

        int crusadeBonus = HistoricalEventManager.GetAttackBoostForFaction(played.source.factionId);
        if (crusadeBonus > 0)
            LogMessage($"{played.source.factionName} rides the Napoleonic tide — +{crusadeBonus} to combat!");

        int comboBonus = card != null ? GetComboPower(card) : 0;
        // ADDED: UPGRADE 2 - pass player-chosen dice count if set
        int chosenDice = pendingAttackerDiceCount > 0 ? pendingAttackerDiceCount : 0;
        pendingAttackerDiceCount = 0;
        CombatResult result = CombatManager.ResolveTerritoryBattle(
            source, target, attackBonusDice, defenseBonusDice,
            attackerReroll, preBattleSabotage, defenderShield,
            maxRounds: card != null ? card.power + comboBonus : 3, attackerChosenDice: chosenDice);

        OnCombatResolved?.Invoke(result, source, target);

        LogMessage($"\u2694 {source.name} ({source.troops} troops) attacks {target.name} ({target.troops} troops)");
        LogMessage($"Battle: {source.name} -> {target.name}. {result.battleLog}");
        LogMessage($"Result: attacker lost {result.attackerTroopsLost}, defender lost {result.defenderTroopsLost}");

        if (result.attackerWon && result.territoryCaptured)
        {
            LogMessage($"{played.source.factionName} captured {target.name}!");

            TerritoryCardType[] nonWild = { TerritoryCardType.INFANTRY, TerritoryCardType.CAVALRY, TerritoryCardType.ARTILLERY };
            TerritoryCardType awarded = Random.value < 0.125f
                ? TerritoryCardType.WILD
                : nonWild[Random.Range(0, nonWild.Length)];
            played.source.territoryCards.Add(new TerritoryCard(target.name, awarded));
            LogMessage($"{played.source.factionName} earns a {awarded} territory card.");
        }

        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.UpdateTerritoryMap();
            GameUIManager.Instance.UpdateAllFactions();
        }

        yield break;
    }

    public void CheckEliminations()
    {
        foreach (FactionData faction in factions)
        {
            if (faction.isEliminated) continue;
            bool noTerritories = faction.territories.Count == 0;
            bool powerDepleted = faction.power <= 0;

            if (noTerritories || powerDepleted)
            {
                faction.isEliminated = true;
                faction.power = 0;
                string reason = noTerritories
                    ? $"{faction.factionName} has lost all territories and is ELIMINATED!"
                    : $"{faction.factionName}'s power has collapsed \u2014 ELIMINATED!";
                LogMessage(reason);
                OnFactionEliminated?.Invoke(faction);
            }
        }

        if (GameUIManager.Instance != null)
            GameUIManager.Instance.UpdateAllFactions();
    }

    // FIXED: BUG 4 - uses global territories list (via t.controlledBy) not faction.territories list
    bool CheckWinCondition()
    {
        if (currentTurn < 5) return false;

        int aliveCount = 0;
        bool playerAlive = false;

        foreach (FactionData faction in factions)
        {
            if (!faction.isEliminated)
            {
                aliveCount++;
                if (faction.isPlayerControlled)
                    playerAlive = true;

                int totalTerritories = 0;
                foreach (TerritoryData t in territories)
                    if (t.controlledBy == faction.factionId)
                        totalTerritories++;

                if (totalTerritories >= territories.Count)
                {
                    SetGameState(GameState.GAME_OVER);
                    LogMessage($"WORLD DOMINATION: {faction.factionName} controls all of Europe!");
                    OnGameOver?.Invoke(faction.isPlayerControlled);
                    return true;
                }

                if (faction.influence >= 100 && faction.power >= 90 && faction.territories.Count >= 8)
                {
                    SetGameState(GameState.GAME_OVER);
                    LogMessage($"DOMINANCE VICTORY: {faction.factionName} controls the shadows of Europe!");
                    OnGameOver?.Invoke(faction.isPlayerControlled);
                    return true;
                }

                if (faction.influence >= 100 && currentTurn >= 60 && faction.territories.Count >= 6)
                {
                    SetGameState(GameState.GAME_OVER);
                    LogMessage($"INFLUENCE VICTORY: {faction.factionName}'s influence is absolute!");
                    OnGameOver?.Invoke(faction.isPlayerControlled);
                    return true;
                }

                if (faction.secrets >= 45 && faction.territories.Count >= 5 && currentTurn >= 30)
                {
                    SetGameState(GameState.GAME_OVER);
                    LogMessage($"SHADOW COUP: {faction.factionName} executes a devastating secret coup!");
                    OnGameOver?.Invoke(faction.isPlayerControlled);
                    return true;
                }

                if (faction.secrets >= 50)
                {
                    int elimCount = 0;
                    foreach (FactionData f in factions)
                        if (f.isEliminated) elimCount++;
                    if (elimCount >= 2)
                    {
                        SetGameState(GameState.GAME_OVER);
                        LogMessage($"CONSPIRACY VICTORY: {faction.factionName}'s secrets are absolute!");
                        OnGameOver?.Invoke(faction.isPlayerControlled);
                        return true;
                    }
                }
            }
        }

        if (aliveCount <= 1)
        {
            SetGameState(GameState.GAME_OVER);

            if (playerAlive)
            {
                LogMessage("VICTORY! Your faction is the last one standing!");
                OnGameOver?.Invoke(true);
            }
            else
            {
                LogMessage("DEFEAT! Your faction has been eliminated.");
                OnGameOver?.Invoke(false);
            }
            return true;
        }

        return false;
    }

    public void LogMessage(string message)
    {
        OnLogMessage?.Invoke($"[T{currentTurn}] {message}");
    }

    // FIXED: BUG 6 - combo bonuses stored in comboPowerBoost dict (does NOT mutate CardData.power)
    void CheckCardCombos()
    {
        comboPowerBoost.Clear();
        if (playerCardsThisResolution.Count < 2) return;

        var groups = playerCardsThisResolution.Where(c => c != null).GroupBy(c => c.cardType).ToList();
        foreach (var grp in groups)
        {
            if (grp.Count() >= 2)
            {
                CardType t = grp.Key;
                string bonus = "";
                switch (t)
                {
                    case CardType.ATTACK:
                        bonus = "+2 damage per ATTACK card";
                        foreach (var card in grp)
                            comboPowerBoost[card.cardName] = comboPowerBoost.GetValueOrDefault(card.cardName) + 2;
                        break;
                    case CardType.DEFENSE:
                        bonus = "+1 shield per DEFENSE card";
                        foreach (FactionData f in factions)
                            if (!f.isEliminated) f.shieldPoints += 3;
                        break;
                    case CardType.INFLUENCE:
                        bonus = "+3 secrets per INFLUENCE card";
                        foreach (var card in grp)
                            comboPowerBoost[card.cardName] = comboPowerBoost.GetValueOrDefault(card.cardName) + 3;
                        break;
                    case CardType.SABOTAGE:
                        bonus = "+1 discard per SABOTAGE card";
                        foreach (var card in grp)
                            comboPowerBoost[card.cardName] = comboPowerBoost.GetValueOrDefault(card.cardName) + 1;
                        break;
                    case CardType.FARMING:
                        bonus = "+1 power per FARMING card";
                        foreach (var card in grp)
                            comboPowerBoost[card.cardName] = comboPowerBoost.GetValueOrDefault(card.cardName) + 1;
                        break;
                    case CardType.BLACK_MARKET:
                        bonus = "+1 secret stolen per BLACK_MARKET card";
                        foreach (var card in grp)
                            comboPowerBoost[card.cardName] = comboPowerBoost.GetValueOrDefault(card.cardName) + 1;
                        break;
                }
                if (!string.IsNullOrEmpty(bonus))
                    LogMessage($"COMBO: {grp.Count()}x {t} cards! {bonus}.");
            }
        }

        if (groups.Count(g => g.Any()) >= 4)
        {
            LogMessage("COMBO: One of each type! Source factions gain +10 power.");
            foreach (var pc in resolutionQueue)
                pc.source.power = Mathf.Min(100, pc.source.power + 10);
        }
    }

    public int GetComboPower(CardData card)
    {
        return comboPowerBoost.TryGetValue(card.cardName, out int bonus) ? bonus : 0;
    }

    public void RestartGame()
    {
        StopAllCoroutines();
        if (AIController.Instance != null)
            AIController.Instance.StopAllCoroutines();
        subTurnCoroutine = null;
        resolutionQueue.Clear();
        currentTurn = 1;
        pendingAttackSource = null;
        pendingAttackTarget = null;
        IsWaitingForReinforcements = false;
        IsWaitingForEventDismiss = false;
        IsWaitingForAttackSelection = false;
        IsWaitingForFortify = false;

        isResolutionRunning = false;
        comboPowerBoost.Clear();
        TerritoryDatabase.Reset();
        territories = TerritoryDatabase.GetAllTerritories();
        OnYearChanged?.Invoke(CurrentYear);

        foreach (FactionData faction in factions)
        {
            faction.ResetForNewGame();
            faction.deck = CardDatabase.GetFactionDeck(faction.factionId);
            DeckManager.Shuffle(faction.deck);
            DeckManager.DealInitialHand(faction, 5);
        }

        AssignStartingTerritories();

        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.InitializeUI(factions);
            GameUIManager.Instance.UpdatePlayerHand(factions[playerFactionId].hand);
        }

        LogMessage("=== New Game Started ===");
        LogMessage("Four factions enter. One leaves.");

        StartCoroutine(StartPlayerTurn());
    }

    public FactionData GetPlayerFaction()
    {
        if (factions == null || factions.Count == 0 ||
            playerFactionId >= factions.Count)
            return null;
        return factions[playerFactionId];
    }

    void ApplyTerritoryIncome(FactionData faction)
    {
        int totalFarming = 0;
        int totalInfluence = 0;
        int totalPower = 0;

        foreach (TerritoryData t in territories)
        {
            if (t.controlledBy != faction.factionId) continue;
            totalFarming += t.farmingValue;
            if (t.isPort)
                totalInfluence += 1;
            if (t.strategicValue >= 4)
                totalPower += 2;
        }

        faction.secrets = Mathf.Min(50, faction.secrets + totalFarming);
        faction.influence = Mathf.Min(100, faction.influence + totalInfluence);
        faction.power = Mathf.Min(100, faction.power + totalPower);

        int ownedCount = 0;
        foreach (TerritoryData t in territories)
            if (t.controlledBy == faction.factionId) ownedCount++;
        faction.goldIncome = ownedCount;
        faction.gold += faction.goldIncome;
    }

    void ApplyPassiveEffects(FactionData faction)
    {
        int shieldFromSecrets = faction.secrets / 10;
        if (shieldFromSecrets > 0)
        {
            faction.shieldPoints += shieldFromSecrets;
            LogMessage($"{faction.factionName}'s secrets generate {shieldFromSecrets} shield.");
        }

        if (faction.influence >= 100)
            LogMessage($"{faction.factionName} is DOMINANT — attacks deal +2 damage!");
    }

    void ApplyFarmingBoosts(FactionData faction)
    {
        foreach (TerritoryData t in faction.territories)
        {
            if (t.farmingBoostTurns > 0)
            {
                int bonus = t.farmingValue;
                t.AddTroops(bonus);
                LogMessage($"{t.name} generates {bonus} bonus troops from farming boost ({t.farmingBoostTurns} turns remaining).");
                t.farmingBoostTurns--;
            }
        }
    }

    public List<TerritoryCard> FindBestTradeSet(List<TerritoryCard> cards)
    {
        foreach (TerritoryCardType type in System.Enum.GetValues(typeof(TerritoryCardType)))
        {
            if (type == TerritoryCardType.WILD) continue;
            var matching = cards.FindAll(c => c.cardType == type || c.cardType == TerritoryCardType.WILD);
            if (matching.Count >= 3) return matching.GetRange(0, 3);
        }
        var inf = cards.Find(c => c.cardType == TerritoryCardType.INFANTRY);
        var cav = cards.Find(c => c.cardType == TerritoryCardType.CAVALRY);
        var art = cards.Find(c => c.cardType == TerritoryCardType.ARTILLERY);
        if (inf != null && cav != null && art != null)
            return new List<TerritoryCard> { inf, cav, art };
        return null;
    }

    /// <summary>Trades 3 matching territory cards for escalating bonus troops. Returns bonus amount.</summary>
    public int TradeCardSet(FactionData faction, List<TerritoryCard> cards)
    {
        if (cards.Count != 3) return 0;

        var nonWilds = cards.Where(c => c.cardType != TerritoryCardType.WILD).ToList();
        int wildCount = cards.Count - nonWilds.Count;

        bool valid = false;

        if (nonWilds.Count == 0 || nonWilds.All(c => c.cardType == nonWilds[0].cardType))
            valid = true;

        if (!valid)
        {
            var types = new HashSet<TerritoryCardType>(nonWilds.Select(c => c.cardType));
            if (types.Count + wildCount >= 3 && types.Count <= 3)
                valid = true;
        }

        if (!valid) return 0;

        int[] bonusTable = { 4, 6, 8, 10, 12, 15, 20, 25, 30 };
        int idx = Mathf.Clamp(faction.cardTradeCount, 0, bonusTable.Length - 1);
        int bonus = faction.cardTradeCount >= bonusTable.Length
            ? bonusTable[bonusTable.Length - 1] + (faction.cardTradeCount - bonusTable.Length + 1) * 5
            : bonusTable[idx];

        foreach (TerritoryCard c in cards)
        {
            faction.territoryCards.Remove(c);
            TerritoryData matchedTerritory = territories.Find(t => t.name == c.territoryName && t.controlledBy == faction.factionId);
            if (matchedTerritory != null)
            {
                matchedTerritory.AddTroops(2);
                LogMessage($"Territorial bonus: {matchedTerritory.name} +2 troops.");
            }
        }

        faction.pendingReinforcements += bonus;
        faction.cardTradeCount++;
        LogMessage($"{faction.factionName} trades a card set for {bonus} bonus troops!");
        OnReinforcementPhase?.Invoke(faction.pendingReinforcements);
        return bonus;
    }

    /// <summary>Gets all enemy-controlled territories adjacent to the given territory.</summary>
    public List<TerritoryData> GetAdjacentEnemyTerritories(TerritoryData source)
    {
        List<TerritoryData> result = new List<TerritoryData>();
        foreach (string adjName in ProvinceGraph.GetAttackNeighbours(source.name))
        {
            TerritoryData t = territories.Find(td => td.name == adjName);
            if (t != null && t.controlledBy != source.controlledBy && t.controlledBy >= 0)
                result.Add(t);
        }
        return result;
    }

    bool HasAttackPhaseOptions(FactionData faction)
    {
        if (faction == null || territories == null) return false;
        
        bool hasTarget = false;
        foreach (var t in territories)
        {
            if (t.controlledBy != faction.factionId) { hasTarget = true; break; }
        }
        if (!hasTarget) return false;

        foreach (TerritoryData source in territories)
        {
            if (source.controlledBy == faction.factionId && source.troops > 1) return true;
        }
        return false;
    }

    /// <summary>Gets all friendly-controlled territories adjacent to the given territory.</summary>
    public List<TerritoryData> GetAdjacentFriendlyTerritories(TerritoryData target)
    {
        List<TerritoryData> result = new List<TerritoryData>();
        foreach (string adjName in target.adjacentTerritories)
        {
            TerritoryData t = territories.Find(td => td.name == adjName);
            if (t != null && t.controlledBy == target.controlledBy)
                result.Add(t);
        }
        return result;
    }

    /// <summary>Expands from a source territory to an adjacent neutral territory, costing 10 troops.</summary>
    public bool ExpandToNeutral(TerritoryData source, TerritoryData target)
    {
        FactionData player = factions[playerFactionId];
        if (TurnActionController.Expand(player, source, target))
        {
            LogMessage($"Expanded from {source.name} to {target.name} (cost: 10 troops, card slot consumed).");
            if (MapSystemController.Instance != null)
                MapSystemController.Instance.SyncProvinceFromTerritory(target.name);
            return true;
        }
        return false;
    }

    public bool BuyCardFromMarket(CardMarketEntry entry)
    {
        FactionData player = factions[playerFactionId];
        return CardMarket.BuyCard(this, player, entry);
    }

    public bool FortifyTroops(TerritoryData source, TerritoryData destination, int count)
    {
        if (currentState != GameState.FORTIFY_PHASE) return false;
        if (source.controlledBy != playerFactionId || destination.controlledBy != playerFactionId) return false;
        if (!IsConnectedThroughFriendlyTerritories(source, destination)) return false;
        if (source.troops - count < 1) count = source.troops - 1;
        if (count <= 0) return false;

        source.troops -= count;
        destination.troops += count;
        LogMessage($"Fortified: moved {count} troops from {source.name} to {destination.name}.");
        IsWaitingForFortify = false;
        return true;
    }

    bool IsConnectedThroughFriendlyTerritories(TerritoryData a, TerritoryData b)
    {
        return IsConnectedThroughFriendlyTerritories(a, b, playerFactionId);
    }

    bool IsConnectedThroughFriendlyTerritories(TerritoryData a, TerritoryData b, int factionId)
    {
        if (a == b) return false;
        var visited = new HashSet<string>();
        var queue = new Queue<string>();
        queue.Enqueue(a.name);
        visited.Add(a.name);
        while (queue.Count > 0)
        {
            string current = queue.Dequeue();
            TerritoryData currentTerritory = territories.Find(t => t.name == current);
            if (currentTerritory == null) continue;
            foreach (string adj in currentTerritory.adjacentTerritories)
            {
                if (adj == b.name) return true;
                if (visited.Contains(adj)) continue;
                TerritoryData adjT = territories.Find(t => t.name == adj);
                if (adjT != null && adjT.controlledBy == factionId)
                {
                    visited.Add(adj);
                    queue.Enqueue(adj);
                }
            }
        }
        return false;
    }

    /// <summary>Public wrapper for BFS connectivity check. Use for UI queries.</summary>
    public bool AreTerritoriesConnected(TerritoryData a, TerritoryData b, int factionId)
    {
        return IsConnectedThroughFriendlyTerritories(a, b, factionId);
    }
}

[System.Serializable]
public struct PlayedCard
{
    public CardData card;
    public FactionData source;
    public FactionData target;
    public TerritoryData sourceTerritory;
    public TerritoryData targetTerritory;

    public PlayedCard(CardData card, FactionData source, FactionData target)
    {
        this.card = card;
        this.source = source;
        this.target = target;
        this.sourceTerritory = null;
        this.targetTerritory = null;
    }

    public PlayedCard(CardData card, FactionData source, FactionData target, TerritoryData sourceTerritory, TerritoryData targetTerritory)
    {
        this.card = card;
        this.source = source;
        this.target = target;
        this.sourceTerritory = sourceTerritory;
        this.targetTerritory = targetTerritory;
    }
}
