using System.Collections.Generic;
using System.Linq;

public static class CardMarket
{
    static List<CardMarketEntry> listings = new List<CardMarketEntry>();

    public static List<CardMarketEntry> GetListings() => listings;

    public static void Refresh(GameManager gm)
    {
        listings.Clear();
        foreach (FactionData f in gm.factions)
        {
            if (f.isEliminated) continue;
            var sellable = f.hand.Where(c => true).Take(2).ToList();
            foreach (CardData card in sellable)
            {
                int price = 5 + card.power;
                listings.Add(new CardMarketEntry(card, price, f.factionId));
            }
        }
    }

    public static bool BuyCard(GameManager gm, FactionData buyer, CardMarketEntry entry)
    {
        if (buyer.gold < entry.price) return false;
        FactionData seller = gm.factions.Find(f => f.factionId == entry.sellerFactionId);
        if (seller == null || seller.isEliminated) return false;
        if (!seller.hand.Contains(entry.card)) return false;

        buyer.gold -= entry.price;
        seller.gold += entry.price;
        seller.hand.Remove(entry.card);
        buyer.hand.Add(entry.card);
        listings.Remove(entry);

        gm.LogMessage($"{buyer.factionName} buys {entry.card.cardName} from market for {entry.price} gold.");
        return true;
    }
}
