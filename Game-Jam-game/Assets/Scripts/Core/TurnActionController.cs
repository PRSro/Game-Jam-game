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
        gm.allCardsThisResolution.Add(null);
        if (faction.isPlayerControlled) gm.PlayerCardsThisResolution.Add(null);
        gm.LogMessage($"{faction.factionName} expands from {source.name} to {target.name}.");
        MapSystemController.Instance?.SyncProvinceFromTerritory(target.name);
        return true;
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
        return true;
    }

    static bool ValidateExpand(FactionData faction, TerritoryData source, TerritoryData target)
    {
        GameManager gm = GameManager.Instance;
        if (gm.currentState != GameState.PLAY_PHASE) return false;
        if (source.controlledBy != faction.factionId) return false;
        if (target.controlledBy != -1) return false;
        if (!TerritoryGraph.AreAdjacent(source.name, target.name)) return false;
        if (source.troops < 2) return false;
        return true;
    }
}
