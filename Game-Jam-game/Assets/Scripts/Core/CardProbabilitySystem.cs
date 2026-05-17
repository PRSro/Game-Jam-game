using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CardProbabilitySystem
{
    public static Dictionary<int, Dictionary<CardType, float>> BaseWeights = new Dictionary<int, Dictionary<CardType, float>>
    {
        { 0, new Dictionary<CardType, float> { { CardType.ATTACK, 10f }, { CardType.DEFENSE, 12f }, { CardType.INFLUENCE, 30f }, { CardType.SABOTAGE, 18f }, { CardType.FARMING, 10f }, { CardType.BLACK_MARKET, 20f } } },
        { 1, new Dictionary<CardType, float> { { CardType.ATTACK, 35f }, { CardType.DEFENSE, 25f }, { CardType.INFLUENCE, 8f }, { CardType.SABOTAGE, 10f }, { CardType.FARMING, 12f }, { CardType.BLACK_MARKET, 10f } } },
        { 2, new Dictionary<CardType, float> { { CardType.ATTACK, 10f }, { CardType.DEFENSE, 35f }, { CardType.INFLUENCE, 20f }, { CardType.SABOTAGE, 8f }, { CardType.FARMING, 15f }, { CardType.BLACK_MARKET, 12f } } },
        { 3, new Dictionary<CardType, float> { { CardType.ATTACK, 18f }, { CardType.DEFENSE, 8f }, { CardType.INFLUENCE, 15f }, { CardType.SABOTAGE, 35f }, { CardType.FARMING, 12f }, { CardType.BLACK_MARKET, 12f } } }
    };

    public static Dictionary<CardType, float> GetAdjustedWeights(
        FactionData faction, List<FactionData> allFactions,
        List<TerritoryData> territories, int currentTurn)
    {
        if (!BaseWeights.ContainsKey(faction.factionId))
            return new Dictionary<CardType, float>();

        Dictionary<CardType, float> weights = new Dictionary<CardType, float>(BaseWeights[faction.factionId]);

        if (faction.power < 20)
        {
            weights[CardType.DEFENSE] += 20f;
            weights[CardType.FARMING] += 15f;
            weights[CardType.ATTACK] -= 10f;
        }

        if (faction.territories.Count < 3)
        {
            weights[CardType.ATTACK] += 15f;
            weights[CardType.INFLUENCE] += 10f;
        }

        if (faction.influence >= 80)
        {
            weights[CardType.INFLUENCE] += 20f;
            weights[CardType.BLACK_MARKET] += 10f;
        }

        bool underAttack = false;
        foreach (TerritoryData t in faction.territories)
        {
            foreach (string adjName in t.adjacentTerritories)
            {
                TerritoryData adj = territories.Find(td => td.name == adjName);
                if (adj != null && adj.controlledBy >= 0 && adj.controlledBy != faction.factionId)
                {
                    FactionData owner = allFactions.Find(f => f.factionId == adj.controlledBy);
                    if (owner != null && adj.troops > t.troops)
                    {
                        underAttack = true;
                        break;
                    }
                }
            }
            if (underAttack) break;
        }
        if (underAttack)
        {
            weights[CardType.DEFENSE] += 25f;
            weights[CardType.SABOTAGE] += 10f;
        }

        if (faction.secrets >= 40)
        {
            weights[CardType.BLACK_MARKET] += 15f;
            weights[CardType.INFLUENCE] += 10f;
        }

        if (currentTurn > 300)
        {
            weights[CardType.ATTACK] += 10f;
            weights[CardType.SABOTAGE] += 10f;
            weights[CardType.DEFENSE] -= 5f;
        }

        int year = TurnDateManager.GetYear(currentTurn);
        if (year >= 1789 && year <= 1815)
        {
            foreach (CardType t in weights.Keys.ToList())
                if (t == CardType.ATTACK) weights[t] += 8f;
        }
        if (year == 1848)
        {
            foreach (CardType t in weights.Keys.ToList())
                if (t == CardType.SABOTAGE) weights[t] += 12f;
        }
        if (year >= 1914 && year <= 1918)
        {
            foreach (CardType t in weights.Keys.ToList())
            {
                if (t == CardType.ATTACK) weights[t] += 15f;
                if (t == CardType.DEFENSE) weights[t] += 10f;
            }
        }
        if (year >= 1939 && year <= 1945)
        {
            foreach (CardType t in weights.Keys.ToList())
            {
                if (t == CardType.ATTACK) weights[t] += 20f;
                if (t == CardType.DEFENSE) weights[t] += 15f;
            }
        }
        if (year >= 1947 && year <= 1991)
        {
            foreach (CardType t in weights.Keys.ToList())
            {
                if (t == CardType.INFLUENCE) weights[t] += 12f;
                if (t == CardType.BLACK_MARKET) weights[t] += 8f;
            }
        }

        foreach (CardType t in weights.Keys.ToList())
            weights[t] = Mathf.Max(weights[t], 1f);

        float total = weights.Values.Sum();
        if (total <= 0f)
        {
            var keys = weights.Keys.ToList();
            float flat = 100f / keys.Count;
            foreach (var k in keys) weights[k] = flat;
            return weights;
        }
        foreach (CardType t in weights.Keys.ToList())
            weights[t] = weights[t] / total * 100f;

        return weights;
    }

    public static CardData DrawWeightedCard(
        FactionData faction,
        List<FactionData> allFactions,
        List<TerritoryData> territories,
        int currentTurn)
    {
        if (faction.hand.Count >= 7)
            return null;

        if (faction.deck.Count == 0)
        {
            faction.deck.AddRange(faction.discardPile);
            faction.discardPile.Clear();
            DeckManager.Shuffle(faction.deck);
        }

        if (faction.deck.Count == 0)
            return null;

        if (faction.skipDrawNextTurn)
        {
            faction.skipDrawNextTurn = false;
            return null;
        }

        Dictionary<CardType, float> weights = GetAdjustedWeights(faction, allFactions, territories, currentTurn);

        List<CardType> orderedTypes = weights.OrderByDescending(kv => kv.Value).Select(kv => kv.Key).ToList();

        float roll = Random.value * 100f;
        float accumulated = 0f;
        CardType chosenType = orderedTypes[0];

        foreach (CardType t in orderedTypes)
        {
            accumulated += weights[t];
            if (roll <= accumulated)
            {
                chosenType = t;
                break;
            }
        }

        CardData card = FindCardOfType(faction, chosenType, weights);
        if (card != null)
        {
            faction.hand.Add(card);

            if (card.rarity == CardRarity.LEGENDARY)
            {
                GameManager.Instance?.LogMessage($"\u2726 LEGENDARY: {card.cardName} drawn by {faction.factionName}!");
            }

            return card;
        }

        return null;
    }

    static CardData FindCardOfType(FactionData faction, CardType preferredType, Dictionary<CardType, float> weights)
    {
        List<CardType> fallbackOrder = weights.OrderByDescending(kv => kv.Value).Select(kv => kv.Key).ToList();

        foreach (CardType t in fallbackOrder)
        {
            List<CardData> candidates = faction.deck.FindAll(c => c.cardType == t);
            if (candidates.Count == 0) continue;

            CardData best = null;
            float bestRoll = float.MaxValue;
            foreach (CardData c in candidates)
            {
                float multiplier = GetRarityMultiplier(c.rarity);
                float adjusted = weights[t] * multiplier;
                float r = Random.value / Mathf.Max(adjusted, 0.01f);
                if (r < bestRoll)
                {
                    bestRoll = r;
                    best = c;
                }
            }

            if (best != null)
            {
                faction.deck.Remove(best);
                return best;
            }
        }

        if (faction.deck.Count > 0)
        {
            CardData any = faction.deck[0];
            faction.deck.RemoveAt(0);
            return any;
        }

        return null;
    }

    static float GetRarityMultiplier(CardRarity rarity)
    {
        switch (rarity)
        {
            case CardRarity.COMMON: return 1.0f;
            case CardRarity.UNCOMMON: return 0.6f;
            case CardRarity.RARE: return 0.3f;
            case CardRarity.LEGENDARY: return 0.1f;
            default: return 1.0f;
        }
    }
}
