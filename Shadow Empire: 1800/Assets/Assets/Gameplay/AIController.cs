using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIController : MonoBehaviour
{
    public static AIController Instance { get; private set; }

    struct Personality
    {
        public int attackWeight;
        public int defenseWeight;
        public int influenceWeight;
        public int sabotageWeight;
        public int farmingWeight;
        public int blackMarketWeight;
    }

    Dictionary<int, Personality> personalities = new Dictionary<int, Personality>();
    Dictionary<int, int> lastAttackOnPlayerTurn = new Dictionary<int, int>();
    HashSet<string> queuedAttackTargets = new HashSet<string>();
    const int PLAYER_ATTACK_COOLDOWN = 2;
    const int SPHERE_INFLUENCE_THRESHOLD = 25;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitPersonalities();
    }

    void InitPersonalities()
    {
        personalities[0] = new Personality { attackWeight=12, defenseWeight=8,  influenceWeight=14, sabotageWeight=8,  farmingWeight=7,  blackMarketWeight=9 };
        personalities[1] = new Personality { attackWeight=14, defenseWeight=10, influenceWeight=6,  sabotageWeight=6,  farmingWeight=6,  blackMarketWeight=8 };
        personalities[2] = new Personality { attackWeight=8,  defenseWeight=14, influenceWeight=10, sabotageWeight=6,  farmingWeight=8,  blackMarketWeight=8 };
        personalities[3] = new Personality { attackWeight=10, defenseWeight=6,  influenceWeight=12, sabotageWeight=14, farmingWeight=6,  blackMarketWeight=8 };
    }

    float GetCardTypePriority(FactionData faction, CardType type)
    {
        if (!personalities.ContainsKey(faction.factionId)) return 50f;
        Personality p = personalities[faction.factionId];
        switch (type)
        {
            case CardType.ATTACK: return p.attackWeight * 10f;
            case CardType.DEFENSE: return p.defenseWeight * 10f;
            case CardType.INFLUENCE: return p.influenceWeight * 10f;
            case CardType.SABOTAGE: return p.sabotageWeight * 10f;
            case CardType.FARMING: return p.farmingWeight * 10f;
            case CardType.BLACK_MARKET: return p.blackMarketWeight * 10f;
            default: return 50f;
        }
    }

    void EvaluateMarketPurchases(FactionData faction, List<BlackMarketSystem.MarketSlot> market)
    {
        foreach (BlackMarketSystem.MarketSlot slot in market)
        {
            if (slot == null || slot.purchased) continue;
            if (faction.secrets < slot.secretsCost) continue;

            bool shouldBuy = false;

            if (slot.itemType == BlackMarketSystem.MarketItemType.CARD && slot.card != null)
            {
                float priority = GetCardTypePriority(faction, slot.card.cardType);
                shouldBuy = priority > 60f && faction.hand.Count < 6;
            }
            else if (slot.itemType == BlackMarketSystem.MarketItemType.TROOPS)
            {
                bool underPressure = faction.territories.Any(t =>
                    GameManager.Instance.GetAdjacentEnemyTerritories(t).Any(e =>
                        e.troops > t.troops));
                shouldBuy = underPressure && faction.secrets >= 15;
            }
            else if (slot.itemType == BlackMarketSystem.MarketItemType.INTEL)
            {
                int atk = personalities.ContainsKey(faction.factionId) ? personalities[faction.factionId].attackWeight : 10;
                shouldBuy = atk >= 14 && faction.secrets >= 20;
            }
            else if (slot.itemType == BlackMarketSystem.MarketItemType.SABOTAGE_KIT)
            {
                FactionData player = GameManager.Instance.factions
                    .Find(f => f.isPlayerControlled);
                shouldBuy = player != null && player.hand.Count >= 5
                         && faction.secrets >= 25;
            }

            if (shouldBuy)
            {
                TerritoryData troopDest = slot.itemType == BlackMarketSystem.MarketItemType.TROOPS
                    ? faction.territories.OrderBy(t =>
                        GameManager.Instance.GetAdjacentEnemyTerritories(t).Count)
                      .FirstOrDefault()
                    : null;
                BlackMarketSystem.PurchaseMarketSlot(faction, slot, troopDest);
                break;
            }
        }
    }

    public void QueueCardsForTurn(FactionData faction, List<FactionData> allFactions)
    {
        // Clear queued attack targets at start of each turn
        queuedAttackTargets.Clear();

        if (GameManager.Instance == null || GameManager.Instance.currentState != GameState.RESOLUTION_PHASE)
            return;

        if (faction.territoryCards.Count >= 5)
        {
            List<TerritoryCard> set = FindBestTradeSet(faction.territoryCards);
            if (set != null) GameManager.Instance.TradeCardSet(faction, set);
        }

        if (!lastAttackOnPlayerTurn.ContainsKey(faction.factionId))
            lastAttackOnPlayerTurn[faction.factionId] = -PLAYER_ATTACK_COOLDOWN;

        if (GameManager.Instance.currentMarket != null)
        {
            List<BlackMarketSystem.MarketSlot> marketList = new List<BlackMarketSystem.MarketSlot>(GameManager.Instance.currentMarket);
            EvaluateMarketPurchases(faction, marketList);
        }

        if (Random.value < 0.25f)
            TryUseUniqueAction(faction);

        int expansionsThisTurn = 0;
        while (expansionsThisTurn < 2)
        {
            if (!TryExpandNeutral(faction)) break;
            expansionsThisTurn++;
        }

        int cardsQueued = 0;
        int safetyLimit = 0;
        while (cardsQueued < GameManager.MAX_CARDS_PER_TURN
               && faction.hand.Count > 0
               && faction.cardsPlayedThisTurn < GameManager.MAX_CARDS_PER_TURN
               && safetyLimit < 20)
        {
            safetyLimit++;

            FactionData primaryTarget = FindPrimaryTarget(faction, allFactions);
            if (primaryTarget == null) break;

            CardData bestCard = EvaluateBestCard(faction, allFactions);
            if (bestCard == null) break;

            if (bestCard.cardType == CardType.ATTACK && HistoricalEventManager.IsAttacksBlocked())
            {
                bestCard = FindAlternativeToAttack(faction, allFactions);
                if (bestCard == null) break;
            }

            if (bestCard.cardType == CardType.ATTACK)
            {
                if (TryQueueTerritoryAttack(faction, bestCard, allFactions))
                {
                    cardsQueued++;
                    continue;
                }

                FactionData target = GetTargetForCard(bestCard, faction, allFactions);
                if (target == null) { break; }

                if (target.isPlayerControlled)
                {
                    int last = lastAttackOnPlayerTurn.ContainsKey(faction.factionId)
                        ? lastAttackOnPlayerTurn[faction.factionId] : -PLAYER_ATTACK_COOLDOWN;
                    if (GameManager.Instance.currentTurn - last < PLAYER_ATTACK_COOLDOWN)
                    {
                        bestCard = FindAlternativeToAttack(faction, allFactions);
                        if (bestCard == null) break;
                        target = GetTargetForCard(bestCard, faction, allFactions);
                        if (target == null) break;
                    }
                    else
                    {
                        lastAttackOnPlayerTurn[faction.factionId] = GameManager.Instance.currentTurn;
                    }
                }
                else if (!HasSpheresOverlap(faction, target))
                {
                    bestCard = FindAlternativeToAttack(faction, allFactions);
                    if (bestCard == null) break;
                    target = GetTargetForCard(bestCard, faction, allFactions);
                    if (target == null) break;
                }

                if (TurnActionController.PlayCard(faction, bestCard, target))
                    cardsQueued++;
                continue;
            }

            FactionData nonAttackTarget = GetTargetForCard(bestCard, faction, allFactions);
            if (nonAttackTarget == null) { break; }

            if (TurnActionController.PlayCard(faction, bestCard, nonAttackTarget))
                cardsQueued++;
        }

        // Guaranteed territory attack pass (no card needed)
        int turn = GameManager.Instance != null ? GameManager.Instance.currentTurn : 1;
        int minAttacks = turn > 80 ? 3 : 1;
        int maxAttacks = turn > 80 && faction.territoryAttackWins >= 2 ? 4 : 3;
        int territoryAttacks = 0;
        int safety = 0;
        bool anyNeutralsOnMap = GameManager.Instance.territories
            .Any(t => t.controlledBy == -1);

        while (territoryAttacks < maxAttacks && safety < 30)
        {
            safety++;
            if (anyNeutralsOnMap)
            {
                if (TryExpandNeutral(faction)) { territoryAttacks++; anyNeutralsOnMap = GameManager.Instance.territories.Any(t => t.controlledBy == -1); continue; }
                anyNeutralsOnMap = false;
            }
            if (TryConquerEnemy(faction)) { territoryAttacks++; continue; }
            break;
        }
    }

    bool TryUseUniqueAction(FactionData faction)
    {
        if (faction == null || GameManager.Instance == null) return false;
        List<TerritoryData> enemyTerritories = GameManager.Instance.territories
            .Where(t => t.controlledBy >= 0 && t.controlledBy != faction.factionId)
            .OrderByDescending(t => t.strategicValue + t.troops)
            .ToList();

        switch (faction.factionId)
        {
            case 0:
                return enemyTerritories.Count > 0 && TurnActionController.PlantAgent(faction, enemyTerritories[0]);
            case 1:
                return enemyTerritories.Count > 0 && TurnActionController.HolyWar(faction, enemyTerritories[0]);
            case 2:
                List<TerritoryData> owned = faction.territories
                    .Where(t => t.controlledBy == faction.factionId)
                    .ToList();
                for (int i = 0; i < owned.Count; i++)
                    for (int j = i + 1; j < owned.Count; j++)
                        if (TurnActionController.GrandLodge(faction, owned[i], owned[j]))
                            return true;
                return false;
            case 3:
                return enemyTerritories.Count > 0 && TurnActionController.InciteUprising(faction, enemyTerritories[0]);
        }
        return false;
    }

    bool TryQueueTerritoryAttack(FactionData faction, CardData card, List<FactionData> allFactions)
    {

        List<TerritoryData> ownedWithTroops = faction.territories
            .Where(t => t.troops >= 10)
            .OrderByDescending(t => t.troops)
            .ToList();

        foreach (TerritoryData source in ownedWithTroops)
        {
            List<TerritoryData> targets = GameManager.Instance.GetAdjacentEnemyTerritories(source);
            if (targets.Count == 0) continue;

            TerritoryData bestTarget = null;
            int bestScore = -999;

            foreach (TerritoryData t in targets)
            {
                int score = 0;
                score -= t.troops * 3;
                score += t.strategicValue * 4;
                score += t.farmingValue * 2;

                FactionData owner = allFactions.Find(f => f.factionId == t.controlledBy);
                if (owner != null && owner.isPlayerControlled)
                    score += 15;
                if (owner != null && owner.territories.Count <= 2)
                    score += 20;

                string region = TerritoryGraph.GetRegion(t.name);
                if (region != null && TerritoryGraph.OwnsEntireRegion(region, faction.territories))
                    score += 10;
                if (source.troops > t.troops * 2)
                    score += 15;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = t;
                }
            }

            if (bestTarget != null)
            {
                if (TurnActionController.PlayTerritoryAttack(faction, card, source, bestTarget))
                    return true;
            }
        }
        return false;
    }

    bool TryConquerEnemy(FactionData faction)
    {
        List<TerritoryData> ownedWithTroops = faction.territories
            .Where(t => t.troops >= 5)
            .OrderByDescending(t => t.troops)
            .ToList();

        foreach (TerritoryData source in ownedWithTroops)
        {
            foreach (string adjName in ProvinceGraph.GetAttackNeighbours(source.name))
            {
                TerritoryData t = GameManager.Instance.territories.Find(td => td.name == adjName);
                if (t != null && t.controlledBy >= 0 && t.controlledBy != faction.factionId)
                {
                    if (queuedAttackTargets.Contains(t.name)) continue;
                    if (source.troops <= t.troops + 2) continue;
                    queuedAttackTargets.Add(t.name);
                    if (TurnActionController.PlayTerritoryAttack(faction, null, source, t))
                        return true;
                }
            }
        }
        return false;
    }

    bool TryExpandNeutral(FactionData faction)
    {
        List<TerritoryData> ownedWithTroops = faction.territories
            .Where(t => t.troops >= 2)
            .OrderByDescending(t => t.troops)
            .ToList();

        foreach (TerritoryData source in ownedWithTroops)
        {
            foreach (string adjName in ProvinceGraph.GetAttackNeighbours(source.name))
            {
                TerritoryData t = GameManager.Instance.territories.Find(td => td.name == adjName);
                if (t != null && t.controlledBy == -1)
                {
                    // Prevent duplicate attacks on same target in same turn
                    if (queuedAttackTargets.Contains(t.name)) continue;
                    queuedAttackTargets.Add(t.name);

                    bool queued = TurnActionController.Expand(faction, source, t);
                    if (queued)
                    {
                        return true;
                    }
                    // If Expand rejected the action, continue searching for another target.
                }
            }
        }
        return false;
    }

    bool HasSpheresOverlap(FactionData a, FactionData b)
    {
        return a.influence >= SPHERE_INFLUENCE_THRESHOLD && b.influence >= SPHERE_INFLUENCE_THRESHOLD;
    }

    FactionData GetTargetForCard(CardData card, FactionData self, List<FactionData> allFactions)
    {
        if (card.cardType == CardType.ATTACK || card.cardType == CardType.SABOTAGE || card.cardType == CardType.BLACK_MARKET)
            return FindPrimaryTarget(self, allFactions);
        return self;
    }

    CardData FindAlternativeToAttack(FactionData faction, List<FactionData> allFactions)
    {
        CardData best = null;
        int bestScore = -999;
        foreach (CardData card in faction.hand)
        {
            if (card.cardType == CardType.ATTACK) continue;
            int score = ScoreCard(card, faction, allFactions);
            if (score > bestScore)
            {
                bestScore = score;
                best = card;
            }
        }
        return best;
    }

    FactionData FindPrimaryTarget(FactionData self, List<FactionData> allFactions)
    {
        FactionData player = allFactions.Find(f => f.isPlayerControlled && !f.isEliminated);

        bool canAttackPlayer = player != null;
        if (canAttackPlayer)
        {
            int last = lastAttackOnPlayerTurn.ContainsKey(self.factionId) ? lastAttackOnPlayerTurn[self.factionId] : -PLAYER_ATTACK_COOLDOWN;
            if (GameManager.Instance.currentTurn - last < PLAYER_ATTACK_COOLDOWN)
                canAttackPlayer = false;
        }

        // Check if player is a valid target
        if (canAttackPlayer && player.influence >= 80)
            return player;

        FactionData target = FindWeakestBorderFaction(self, allFactions);
        if (target != null) return target;

        target = null;
        int highestThreat = -1;

        foreach (FactionData f in allFactions)
        {
            if (f.isEliminated || f == self) continue;
            if (f.isPlayerControlled && !canAttackPlayer) continue;
            if (!HasSpheresOverlap(self, f) && !f.isPlayerControlled) continue;

            int threat = f.power + f.influence + f.territories.Count * 3;
            if (threat > highestThreat)
            {
                highestThreat = threat;
                target = f;
            }
        }

        return target;
    }

    FactionData FindWeakestBorderFaction(FactionData self, List<FactionData> allFactions)
    {
        Dictionary<FactionData, int> borderTroopCounts = new Dictionary<FactionData, int>();

        foreach (TerritoryData t in self.territories)
        {
            foreach (string adjName in t.adjacentTerritories)
            {
                TerritoryData adj = GameManager.Instance.territories.Find(td => td.name == adjName);
                if (adj != null && adj.controlledBy >= 0 && adj.controlledBy != self.factionId)
                {
                    FactionData owner = allFactions.Find(f => f.factionId == adj.controlledBy);
                    if (owner != null && !owner.isEliminated)
                    {
                        if (!borderTroopCounts.ContainsKey(owner))
                            borderTroopCounts[owner] = 0;
                        borderTroopCounts[owner] += adj.troops;
                    }
                }
            }
        }

        if (borderTroopCounts.Count == 0) return null;

        FactionData weakest = null;
        int lowestTroops = int.MaxValue;
        foreach (var kvp in borderTroopCounts)
        {
            if (kvp.Value < lowestTroops)
            {
                lowestTroops = kvp.Value;
                weakest = kvp.Key;
            }
        }
        return weakest;
    }

    CardData EvaluateBestCard(FactionData faction, List<FactionData> allFactions)
    {
        FactionData player = allFactions.Find(f => f.isPlayerControlled && !f.isEliminated);

        if (faction.secrets < 15)
        {
            CardData bm = faction.hand.Find(c => c.cardType == CardType.BLACK_MARKET);
            if (bm != null) return bm;
        }

        if (faction.power < 25)
        {
            CardData farm = faction.hand.Find(c => c.cardType == CardType.FARMING);
            if (farm != null) return farm;
        }

        if (faction.power < 20)
        {
            CardData def = faction.hand.Find(c => c.cardType == CardType.DEFENSE);
            if (def != null) return def;
        }

        if (player != null && player.hand.Count > 4)
        {
            CardData sab = faction.hand.Find(c => c.cardType == CardType.SABOTAGE);
            if (sab != null) return sab;
        }

        int territoryAttackScore = EvaluateTerritoryAttackOpportunity(faction);
        if (territoryAttackScore > 30)
        {
            CardData atk = faction.hand.Find(c => c.cardType == CardType.ATTACK);
            if (atk != null) return atk;
        }

        CardData bestCard = null;
        int bestScore = -999;

        foreach (CardData card in faction.hand)
        {
            int score = ScoreCard(card, faction, allFactions);
            if (score > bestScore)
            {
                bestScore = score;
                bestCard = card;
            }
        }

        if (bestCard != null)
        {
            FactionData testTarget = GetTargetForCard(bestCard, faction, allFactions);
            if (testTarget == null && bestCard.cardType != CardType.DEFENSE && bestCard.cardType != CardType.FARMING)
            {
                return null;
            }
        }
        return bestCard;
    }

    int EvaluateTerritoryAttackOpportunity(FactionData faction)
    {
        if (GameManager.Instance.currentTurn < 10) return 0;
        int score = 0;
        foreach (TerritoryData t in faction.territories)
        {
            if (t.troops < 10) continue;
            foreach (string adjName in t.adjacentTerritories)
            {
                TerritoryData adj = GameManager.Instance.territories.Find(td => td.name == adjName);
                if (adj != null && adj.controlledBy != faction.factionId && adj.controlledBy >= 0)
                {
                    if (t.troops > adj.troops * 1.5f)
                        score += 20;
                    score += adj.strategicValue * 3;
                }
            }
        }
        return score;
    }

    int ScoreCard(CardData card, FactionData faction, List<FactionData> allFactions)
    {
        Personality p = personalities.ContainsKey(faction.factionId) ? personalities[faction.factionId] : new Personality { attackWeight = 10, defenseWeight = 10, influenceWeight = 10, sabotageWeight = 10, farmingWeight = 10, blackMarketWeight = 10 };

        int score = 0;
        FactionData player = allFactions.Find(f => f.isPlayerControlled && !f.isEliminated);

        switch (card.cardType)
        {
            case CardType.ATTACK:
                score = card.power * p.attackWeight;
                if (player != null && player.power < 20)
                    score += 30;
                score += EvaluateTerritoryAttackOpportunity(faction) * 2;
                break;

            case CardType.DEFENSE:
                score = card.power * p.defenseWeight;
                if (faction.power < 30)
                    score += 60;

                int borderExposure = 0;
                foreach (TerritoryData t in faction.territories)
                {
                    foreach (string adjName in t.adjacentTerritories)
                    {
                        TerritoryData adj = GameManager.Instance.territories.Find(td => td.name == adjName);
                        if (adj != null && adj.controlledBy >= 0 && adj.controlledBy != faction.factionId && adj.troops > t.troops)
                            borderExposure += 5;
                    }
                }
                score += borderExposure;
                break;

            case CardType.INFLUENCE:
                score = card.power * p.influenceWeight;
                int neutralAdjCount = 0;
                foreach (TerritoryData t in faction.territories)
                {
                    foreach (string adjName in t.adjacentTerritories)
                    {
                        TerritoryData adj = GameManager.Instance.territories.Find(td => td.name == adjName);
                        if (adj != null && adj.controlledBy == -1)
                            neutralAdjCount += 3;
                    }
                }
                score += neutralAdjCount;
                break;

            case CardType.SABOTAGE:
                score = card.power * p.sabotageWeight;
                if (player != null && player.hand.Count > 4)
                    score += 40;
                break;

            case CardType.FARMING:
                score = card.power * p.farmingWeight;
                if (faction.power < 25)
                    score += 50;
                if (faction.secrets < 15)
                    score += 30;
                if (faction.territories.Count > 0)
                    score += 10;
                break;

            case CardType.BLACK_MARKET:
                score = card.power * p.blackMarketWeight;
                if (faction.secrets < 15)
                    score += 60;
                if (faction.secretCards < 10)
                    score += 20;
                break;
        }

        return score;
    }

    List<TerritoryCard> FindBestTradeSet(List<TerritoryCard> cards)
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

    public static string GetPersonalityLabel(int factionId)
    {
        if (Instance == null || !Instance.personalities.ContainsKey(factionId))
            return "Balanced";
        var p = Instance.personalities[factionId];
        var weights = new Dictionary<string, int>
        {
            { "Aggressor", p.attackWeight },
            { "Fortifier", p.defenseWeight },
            { "Influencer", p.influenceWeight },
            { "Saboteur", p.sabotageWeight },
            { "Farmer", p.farmingWeight },
            { "Marketeer", p.blackMarketWeight }
        };
        string best = "Balanced";
        int bestVal = 0;
        foreach (var kvp in weights)
            if (kvp.Value > bestVal) { bestVal = kvp.Value; best = kvp.Key; }
        return best;
    }

    public void ExecuteFortify(FactionData faction)
    {
        TerritoryData bestDest = null;
        int bestThreat = 0;
        foreach (TerritoryData t in faction.territories)
        {
            int enemyCount = GameManager.Instance.GetAdjacentEnemyTerritories(t).Count;
            if (enemyCount == 0) continue;
            int threat = enemyCount * 5 - t.troops;
            if (threat > bestThreat) { bestThreat = threat; bestDest = t; }
        }
        if (bestDest == null) return;

        TerritoryData bestSource = null;
        int bestExcess = 0;
        foreach (TerritoryData t in faction.territories)
        {
            bool isBorder = GameManager.Instance.GetAdjacentEnemyTerritories(t).Count > 0;
            if (isBorder) continue;
            if (t.troops <= 3) continue;
            int excess = t.troops - 3;
            if (excess > bestExcess) { bestExcess = excess; bestSource = t; }
        }
        if (bestSource == null || bestExcess == 0) return;

        if (!GameManager.Instance.AreTerritoriesConnected(bestSource, bestDest, faction.factionId))
            return;

        bestSource.troops -= bestExcess;
        bestDest.troops += bestExcess;
        GameManager.Instance.LogMessage($"{faction.factionName} fortifies {bestDest.name} (+{bestExcess} from {bestSource.name}).");
    }
}
