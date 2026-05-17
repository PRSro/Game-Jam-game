using System.Collections.Generic;
using UnityEngine;

public static class DeckManager
{
    public static void Shuffle(List<CardData> deck)
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            CardData temp = deck[i];
            deck[i] = deck[j];
            deck[j] = temp;
        }
    }

    public static CardData DrawCard(FactionData faction)
    {
        if (faction.hand.Count >= 7)
            return null;

        if (faction.deck.Count == 0)
        {
            faction.deck.AddRange(faction.discardPile);
            faction.discardPile.Clear();
            Shuffle(faction.deck);
        }

        if (faction.deck.Count == 0)
            return null;

        CardData card = faction.deck[0];
        faction.deck.RemoveAt(0);
        faction.hand.Add(card);
        return card;
    }

    public static void DiscardCard(FactionData faction, CardData card)
    {
        if (faction.hand.Contains(card))
        {
            faction.hand.Remove(card);
            faction.discardPile.Add(card);
        }
    }

    public static void DiscardRandom(FactionData faction, int count)
    {
        for (int i = 0; i < count && faction.hand.Count > 0; i++)
        {
            int idx = Random.Range(0, faction.hand.Count);
            CardData card = faction.hand[idx];
            faction.hand.RemoveAt(idx);
            faction.discardPile.Add(card);
        }
    }

    public static void DealInitialHand(FactionData faction, int handSize)
    {
        for (int i = 0; i < handSize; i++)
            DrawCard(faction);
    }
}
