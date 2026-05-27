using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
[Player taps card in hand]
  → GameUIManager.OnHandCardClicked(card)
  → Shows target popup if card needs a target
  → Player picks target
  → GameManager.PlayCard(card, target)           [for ATTACK/DEFENSE/INFLUENCE/SABOTAGE/FARMING/BLACK_MARKET]
      → TurnActionController.PlayCard(faction, card, target)
          → ValidateCardPlay → faction.hand.Remove(card) → GameManager.QueueCard(card, faction, target)
          → faction.cardsPlayedThisTurn++
          → GameManager.OnCardPlayed event fires → UI refreshes hand

[OR for territory attacks during ATTACK_PHASE — no card required]
  → Player taps a source territory node
  → Dice choice popup shows (1/2/3)
  → GameManager.pendingAttackerDiceCount set
  → Player taps a target territory node
  → AttackPhaseUI.OnNodeClicked → GameManager.QueueTerritoryAttack(null, player, source, target)

[RESOLUTION_PHASE begins]
  → GameManager coroutine dequeues each PlayedCard
  → If sourceTerritory != null → ResolveTerritoryAttack(played)
      → Builds attackBonusDice/defenderBonusDice/reroll/sabotage from consumed queue cards
      → CombatManager.ResolveTerritoryBattle(...)
      → OnCombatResolved → GameUIManager.ShowCombatResultBanner
  → Else → CardResolver.Resolve(card, source, target)
      → Dispatches to ResolveAttack/ResolveDefense/.../ResolveLegendary
      → Effects applied → GameUIManager.UpdateAllFactions()
*/

public static class TurnActionController
{
    static readonly Dictionary<int, List<string>> grandLodgeLinks = new Dictionary<int, List<string>>();

    public static void ResetUniqueActionState()
    {
        grandLodgeLinks.Clear();
    }

    public static bool PlayCard(FactionData faction, CardData card, FactionData target)
    {
        if (!ValidateCardPlay(faction, card)) return false;

        GameManager gm = GameManager.Instance;
        faction.hand.Remove(card);
        gm.QueueCard(card, faction, target);
        if (card != null && gm.currentState == GameState.PLAY_PHASE)
            faction.cardsPlayedThisTurn++;
        gm.allCardsThisResolution.Add(card);
        if (faction.isPlayerControlled) gm.PlayerCardsThisResolution.Add(card);
        gm.OnCardPlayed?.Invoke(card, faction, target);
        gm.LogMessage($"{faction.factionName} plays {card.cardName}" +
            (target != null && target != faction ? $" targeting {target.factionName}" : ""));
        return true;
    }

    public static bool PlayTerritoryAttack(FactionData faction, CardData card,
                                           TerritoryData source, TerritoryData target)
    {
        // BUG FIX 1 — guard: surface null arguments immediately so attacks are never silently dropped
        if (faction == null || source == null || target == null)
        {
            Debug.LogError($"[PlayTerritoryAttack] Null argument — attack NOT queued. " +
                           $"faction={faction}, source={source}, target={target}");
            return false;
        }
        Debug.Log($"[PlayTerritoryAttack] Queuing attack: {faction.factionName} {source.name} -> {target.name}");
        if (card != null && !ValidateCardPlay(faction, card)) return false;
        if (!ValidateTerritoryAttack(faction, card, source, target)) return false;

        GameManager gm = GameManager.Instance;
        if (card != null)
            faction.hand.Remove(card);
        gm.QueueTerritoryAttack(card, faction, source, target);
        faction.cardsPlayedThisTurn++;
        if (card != null)
        {
            gm.allCardsThisResolution.Add(card);
            if (faction.isPlayerControlled) gm.PlayerCardsThisResolution.Add(card);
            gm.LogMessage($"{faction.factionName} plays {card.cardName}: {source.name} attacks {target.name}!");
        }
        else
        {
            gm.allCardsThisResolution.Add(null);
            if (faction.isPlayerControlled) gm.PlayerCardsThisResolution.Add(null);
            gm.LogMessage($"{faction.factionName} attacks from {source.name} to {target.name} without playing a card.");
        }
        return true;
    }

    public static bool Expand(FactionData faction, TerritoryData source, TerritoryData target)
    {
        if (!ValidateExpand(faction, source, target)) return false;

        source.troops = Mathf.Max(1, source.troops - 10);
        target.controlledBy = faction.factionId;
        MapSystemController.Instance?.SyncProvinceFromTerritory(target.name);
        target.troops = 5;
        if (!faction.territories.Contains(target)) faction.territories.Add(target);
        faction.cardsPlayedThisTurn++;
        GameManager gm = GameManager.Instance;
        if (faction.isPlayerControlled)
            gm.lastPlayerNeutralExpansionTurn = gm.currentTurn;
        gm.allCardsThisResolution.Add(null);
        if (faction.isPlayerControlled) gm.PlayerCardsThisResolution.Add(null);
        gm.LogMessage($"{faction.factionName} expands from {source.name} to {target.name}.");
        return true;
    }

    public static bool PlantAgent(FactionData faction, TerritoryData target)
    {
        GameManager gm = GameManager.Instance;
        if (!CanUseUniqueAction(faction, 0, gm) || target == null || target.controlledBy == faction.factionId) return false;
        if (faction.secrets < 5)
        {
            gm.LogMessage("Plant Agent requires 5 secrets.");
            return false;
        }

        faction.secrets -= 5;
        target.defensePenalty = Mathf.Max(-3, target.defensePenalty - 1);
        faction.uniqueActionLastTurn = gm.currentTurn;
        gm.LogMessage($"{faction.factionName} plants an agent in {target.name}. Defense penalty is now {target.defensePenalty}.");
        return true;
    }

    public static bool HolyWar(FactionData faction, TerritoryData target)
    {
        GameManager gm = GameManager.Instance;
        if (!CanUseUniqueAction(faction, 1, gm) || target == null || target.controlledBy == faction.factionId) return false;
        target.holyWarMarkedByFaction = faction.factionId;
        faction.uniqueActionLastTurn = gm.currentTurn;
        gm.LogMessage($"{faction.factionName} declares Holy War on {target.name}.");
        return true;
    }

    public static bool GrandLodge(FactionData faction, TerritoryData a, TerritoryData b)
    {
        GameManager gm = GameManager.Instance;
        if (!CanUseUniqueAction(faction, 2, gm) || a == null || b == null || a == b) return false;
        if (a.controlledBy != faction.factionId || b.controlledBy != faction.factionId) return false;
        if (ProvinceGraph.GetAttackNeighbours(a.name).Contains(b.name)) return false;

        if (!grandLodgeLinks.ContainsKey(faction.factionId))
            grandLodgeLinks[faction.factionId] = new List<string>();
        if (grandLodgeLinks[faction.factionId].Count >= 3)
        {
            gm.LogMessage("Grand Lodge already has 3 permanent links.");
            return false;
        }

        string key = LodgeKey(a.name, b.name);
        if (grandLodgeLinks[faction.factionId].Contains(key)) return false;
        grandLodgeLinks[faction.factionId].Add(key);
        faction.uniqueActionLastTurn = gm.currentTurn;
        gm.LogMessage($"{faction.factionName} links {a.name} and {b.name} through the Grand Lodge.");
        return true;
    }

    public static bool InciteUprising(FactionData faction, TerritoryData target)
    {
        GameManager gm = GameManager.Instance;
        if (!CanUseUniqueAction(faction, 3, gm) || target == null || target.controlledBy == faction.factionId) return false;
        target.suppressAttackNextTurn = true;
        faction.uniqueActionLastTurn = gm.currentTurn;
        gm.LogMessage($"{faction.factionName} incites an uprising in {target.name}; it cannot send attacks next turn.");
        return true;
    }

    public static bool IsGrandLodgeLinked(int factionId, string a, string b)
    {
        return grandLodgeLinks.ContainsKey(factionId) &&
               grandLodgeLinks[factionId].Contains(LodgeKey(a, b));
    }

    static bool CanUseUniqueAction(FactionData faction, int requiredFactionId, GameManager gm)
    {
        if (gm == null || faction == null || faction.isEliminated) return false;
        if (faction.factionId != requiredFactionId) return false;
        if (faction.uniqueActionLastTurn == gm.currentTurn) return false;
        return true;
    }

    static string LodgeKey(string a, string b)
    {
        return string.CompareOrdinal(a, b) < 0 ? $"{a}|{b}" : $"{b}|{a}";
    }

    static bool ValidateCardPlay(FactionData faction, CardData card)
    {
        GameManager gm = GameManager.Instance;
        if (faction == null || card == null || gm == null) return false;
        if (faction.isEliminated) return false;
        if (faction.cardsPlayedThisTurn >= GameManager.MAX_CARDS_PER_TURN) return false;
        bool validState = faction.isPlayerControlled
            ? gm.currentState == GameState.PLAY_PHASE
            : gm.currentState == GameState.RESOLUTION_PHASE;
        if (!validState) return false;
        if (!faction.hand.Contains(card)) return false;
        if (card.factionId != faction.factionId)
        {
            gm.LogMessage($"[CardValidation] {faction.factionName} attempted to play {card.cardName} " +
                          $"(owned by faction {card.factionId}) — rejected.");
            return false;
        }
        if (card.cardType == CardType.ATTACK && HistoricalEventManager.IsAttacksBlocked())
        {
            gm.LogMessage("Attacks blocked this turn by historical event.");
            return false;
        }
        return true;
    }

    static bool ValidateTerritoryAttack(FactionData faction, CardData card,
                                        TerritoryData source, TerritoryData target)
    {
        GameManager gm = GameManager.Instance;
        if (faction == null || source == null || target == null || gm == null)
        {
            gm?.LogMessage("Invalid territory attack selection.");
            return false;
        }
        if (faction.isEliminated) return false;
        if (card != null && faction.cardsPlayedThisTurn >= GameManager.MAX_CARDS_PER_TURN) return false;
        if (gm.currentState != GameState.PLAY_PHASE &&
            gm.currentState != GameState.ATTACK_PHASE &&
            gm.currentState != GameState.RESOLUTION_PHASE) return false;
        if (card != null && card.cardType != CardType.ATTACK) return false;
        if (card == null && gm.currentState != GameState.ATTACK_PHASE)
        {
            gm.LogMessage("Free territory attacks can only be declared during the attack phase.");
            return false;
        }
        if (HistoricalEventManager.IsAttacksBlocked())
        {
            gm.LogMessage("Attacks blocked this turn by historical event.");
            return false;
        }

        if (source.controlledBy != faction.factionId)
        {
            gm.LogMessage("Source territory does not belong to you.");
            return false;
        }
        if (target.controlledBy == faction.factionId)
        {
            gm.LogMessage("You cannot attack your own territory.");
            return false;
        }

        if (source.troops < 2)
        {
            gm.LogMessage("Source territory needs at least 2 troops.");
            return false;
        }
        if (source.suppressAttackNextTurn)
        {
            gm.LogMessage($"{source.name} is disrupted by an uprising and cannot attack this turn.");
            return false;
        }

        // Adjacency check: target must be reachable from source via the province graph.
        List<string> neighbours = ProvinceGraph.GetAttackNeighbours(source.name);
        if (!neighbours.Contains(target.name) && !IsGrandLodgeLinked(faction.factionId, source.name, target.name))
        {
            gm.LogMessage($"{target.name} is not adjacent to {source.name}. Choose a neighbouring territory.");
            return false;
        }
        return true;
    }

    static bool ValidateExpand(FactionData faction, TerritoryData source, TerritoryData target)
    {
        GameManager gm = GameManager.Instance;
        if (gm.currentState != GameState.PLAY_PHASE && gm.currentState != GameState.RESOLUTION_PHASE) return false;
        if (source.controlledBy != faction.factionId) return false;
        if (target.controlledBy != -1) return false;
        if (!ProvinceGraph.CanAttack(source.name, target.name)) return false;
        if (source.troops < 2) return false;
        if (faction.isPlayerControlled)
        {
            if (gm.currentTurn - gm.lastPlayerNeutralExpansionTurn < 2)
            {
                gm.LogMessage($"You can expand into neutral territory again in {2 - (gm.currentTurn - gm.lastPlayerNeutralExpansionTurn)} turn(s).");
                return false;
            }
        }
        return true;
    }
}
