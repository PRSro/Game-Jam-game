using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class BlackMarketSystem
{
    public enum MarketItemType { CARD, TROOPS, INTEL, SABOTAGE_KIT }

    public class MarketSlot
    {
        public CardData card;
        public MarketItemType itemType;
        public int secretsCost;
        public int quantity;
        public int factionTarget = -1;
        public bool purchased;
        public bool reserved;
    }

    public static MarketSlot[] GenerateMarket(
        List<FactionData> allFactions,
        List<TerritoryData> territories,
        int currentTurn)
    {
        MarketSlot[] slots = new MarketSlot[6];

        List<CardData> globalPool = CardDatabase.GetAllCards().FindAll(c =>
            c.factionId >= 0 && c.factionId < allFactions.Count
        );
        List<CardData> availableForMarket = globalPool
            .Where(c => !allFactions.Any(f =>
                f.deck.Contains(c) || f.hand.Contains(c)))
            .ToList();
        if (availableForMarket.Count == 0)
            availableForMarket = new List<CardData>(globalPool);
        for (int i = 0; i < 3; i++)
        {
            slots[i] = new MarketSlot();
            slots[i].itemType = MarketItemType.CARD;
            slots[i].card = availableForMarket[Random.Range(0, availableForMarket.Count)];
            slots[i].secretsCost = CalculateCardCost(slots[i].card, currentTurn, allFactions, territories);
            slots[i].purchased = false;
        }

        slots[3] = new MarketSlot();
        slots[3].itemType = MarketItemType.TROOPS;
        slots[3].quantity = 3;
        slots[3].secretsCost = 8;
        slots[3].purchased = false;

        slots[4] = new MarketSlot();
        slots[4].itemType = MarketItemType.INTEL;
        List<FactionData> validTargets = allFactions.FindAll(f => !f.isEliminated);
        if (validTargets.Count > 0)
            slots[4].factionTarget = validTargets[Random.Range(0, validTargets.Count)].factionId;
        slots[4].secretsCost = 12;
        slots[4].purchased = false;

        slots[5] = new MarketSlot();
        slots[5].itemType = MarketItemType.SABOTAGE_KIT;
        slots[5].secretsCost = 20;
        slots[5].purchased = false;

        ApplyPriceModifiers(slots, currentTurn, allFactions, territories);

        return slots;
    }

    static int CalculateCardCost(CardData card, int currentTurn, List<FactionData> allFactions, List<TerritoryData> territories)
    {
        float rarityMultiplier;
        switch (card.rarity)
        {
            case CardRarity.COMMON: rarityMultiplier = 1f; break;
            case CardRarity.UNCOMMON: rarityMultiplier = 1.33f; break;
            case CardRarity.RARE: rarityMultiplier = 2f; break;
            case CardRarity.LEGENDARY: rarityMultiplier = 4f; break;
            default: rarityMultiplier = 1f; break;
        }
        return card.power * 3 + Mathf.RoundToInt(rarityMultiplier * 5f);
    }

    static void ApplyPriceModifiers(MarketSlot[] slots, int currentTurn, List<FactionData> allFactions, List<TerritoryData> territories)
    {
        bool warEconomy = currentTurn >= 100 && currentTurn <= 200 &&
                          allFactions.Where(f => !f.isEliminated).Average(f => f.power) < 40;
        if (warEconomy)
        {
            foreach (MarketSlot s in slots)
            {
                if (s.itemType == MarketItemType.TROOPS) s.secretsCost += 3;
                if (s.itemType == MarketItemType.CARD && s.card != null && s.card.cardType == CardType.ATTACK)
                    s.secretsCost += 4;
            }
        }

        if (currentTurn > 5)
        {
            int totalTerr = territories.Count(t => t.controlledBy >= 0);
            bool noChanges = true;
            foreach (TerritoryData t in territories)
            {
                int prevOwner = t.controlledBy;
                if (prevOwner >= 0) { noChanges = false; break; }
            }
            if (noChanges)
            {
                foreach (MarketSlot s in slots)
                    s.secretsCost = Mathf.Max(1, s.secretsCost - 2);
            }
        }

        int maxTerr = allFactions.Where(f => !f.isEliminated).Max(f => f.territories.Count);
        if (maxTerr > 12)
        {
            foreach (MarketSlot s in slots)
                s.secretsCost += 5;
        }
    }

    public static bool PurchaseMarketSlot(
        FactionData buyer, MarketSlot slot,
        TerritoryData troopTarget = null)
    {
        if (slot.purchased) return false;
        if (buyer.secrets < Mathf.RoundToInt(slot.secretsCost * buyer.marketDiscount)) return false;
        if (slot.itemType == MarketItemType.TROOPS && troopTarget == null) return false;
        if (troopTarget != null && troopTarget.controlledBy != buyer.factionId) return false;

        int cost = Mathf.RoundToInt(slot.secretsCost * buyer.marketDiscount);
        buyer.secrets -= cost;
        slot.purchased = true;

        switch (slot.itemType)
        {
            case MarketItemType.CARD:
                if (buyer.hand.Count < 7)
                {
                    buyer.hand.Add(slot.card);
                    if (slot.card.rarity == CardRarity.LEGENDARY)
                        GameManager.Instance?.LogMessage($"\u2726 LEGENDARY: {slot.card.cardName} purchased from black market by {buyer.factionName}!");
                }
                break;
            case MarketItemType.TROOPS:
                troopTarget.AddTroops(slot.quantity);
                break;
            case MarketItemType.INTEL:
                GameManager.Instance?.LogMessage($"{buyer.factionName} purchases intel on faction {slot.factionTarget} — troop data revealed for 3 turns.");
                break;
            case MarketItemType.SABOTAGE_KIT:
                FactionData target = GameManager.Instance?.factions.Find(f => f.factionId == slot.factionTarget);
                if (target != null)
                    target.skipDrawNextTurn = true;
                else
                {
                    FactionData leading = GameManager.Instance?.factions
                        .Where(f => !f.isEliminated && f != buyer)
                        .OrderByDescending(f => f.power)
                        .FirstOrDefault();
                    if (leading != null)
                        leading.skipDrawNextTurn = true;
                }
                break;
        }

        GameManager.Instance?.LogMessage($"{buyer.factionName} purchases {slot.itemType} from the black market.");
        if (GameUIManager.Instance != null)
            GameUIManager.Instance.RefreshMarketUI();

        return true;
    }

    public static bool HasAffordableItem(FactionData faction, MarketSlot[] market)
    {
        if (market == null) return false;
        foreach (MarketSlot slot in market)
        {
            if (slot == null || slot.purchased) continue;
            int effectiveCost = Mathf.RoundToInt(slot.secretsCost * faction.marketDiscount);
            if (faction.secrets >= effectiveCost)
                return true;
        }
        return false;
    }

    public static bool HasLegendaryAvailable(MarketSlot[] market)
    {
        if (market == null) return false;
        foreach (MarketSlot slot in market)
        {
            if (slot != null && !slot.purchased && slot.card != null && slot.card.rarity == CardRarity.LEGENDARY)
                return true;
        }
        return false;
    }
}
