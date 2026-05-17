using System.Collections.Generic;
using UnityEngine;

public static class CardDatabase
{
    private static List<CardData> allCards = null;
    static int _nextCardId = 0;

    public static List<CardData> GetAllCards()
    {
        if (allCards == null)
        {
            _nextCardId = 0;
            allCards = new List<CardData>();
            CreateIlluminatiCards();
            CreateTemplarCards();
            CreateFreemasonCards();
            CreateCarbonariCards();
        }
        return allCards;
    }

    public static List<CardData> GetFactionDeck(int factionId, int copiesPerCard = 3)
    {
        List<CardData> deck = new List<CardData>();
        List<CardData> factionCards = GetAllCards().FindAll(c => c.factionId == factionId);
        for (int i = 0; i < factionCards.Count; i++)
        {
            int copies = factionCards[i].rarity == CardRarity.LEGENDARY ? 1 : copiesPerCard;
            for (int j = 0; j < copies; j++)
                deck.Add(factionCards[i]);
        }
        return deck;
    }

    static CardRarity RarityForPower(int power)
    {
        if (power >= 9) return CardRarity.LEGENDARY;
        if (power >= 7) return CardRarity.RARE;
        if (power >= 4) return CardRarity.UNCOMMON;
        return CardRarity.COMMON;
    }

    static CardData MakeCard(string name, string desc, int power, CardType type, int factionId, string flavor, CardRarity? rarityOverride = null)
    {
        CardData card = ScriptableObject.CreateInstance<CardData>();
        card.cardId = _nextCardId++;
        card.cardName = name;
        card.description = desc;
        card.power = power;
        card.cardType = type;
        card.factionId = factionId;
        card.flavorText = flavor;
        card.rarity = rarityOverride ?? RarityForPower(power);
        return card;
    }

    static void CreateIlluminatiCards()
    {
        allCards.Add(MakeCard("All-Seeing Eye", "Reveal hidden truths and gain influence.", 8, CardType.INFLUENCE, 0, "Nothing escapes the illuminated gaze."));
        allCards.Add(MakeCard("Shadow Network", "Deploy agents to disrupt enemy operations.", 6, CardType.SABOTAGE, 0, "Whispers in the dark move mountains."));
        allCards.Add(MakeCard("Pyramid Scheme", "Exploit the masses for military gain.", 5, CardType.ATTACK, 0, "From the top, we control it all."));
        allCards.Add(MakeCard("New World Order", "Shape the future through sheer influence.", 7, CardType.INFLUENCE, 0, "One order to rule them all."));
        allCards.Add(MakeCard("Lucid Dream", "Bend perception to shield your forces.", 4, CardType.DEFENSE, 0, "Is this even real?"));
        allCards.Add(MakeCard("Grain Tithe", "Extract wealth from the peasantry.", 6, CardType.FARMING, 0, "The harvest belongs to the enlightened."));
        allCards.Add(MakeCard("Shadow Harvest", "Reap where others have sown.", 4, CardType.FARMING, 0, "Your labor, our reward."));
        allCards.Add(MakeCard("Invisible Hand", "Market forces bend to your will.", 8, CardType.BLACK_MARKET, 0, "The market is a weapon."));
        allCards.Add(MakeCard("Eye of Providence", "Converts one enemy territory to neutral instantly.", 10, CardType.INFLUENCE, 0, "The pyramid sees all.", CardRarity.LEGENDARY));
    }

    static void CreateTemplarCards()
    {
        allCards.Add(MakeCard("Crusade", "Launch a holy war against your enemies.", 8, CardType.ATTACK, 1, "Deus Vult!"));
        allCards.Add(MakeCard("Holy Relic", "Ancient artifacts provide divine protection.", 7, CardType.DEFENSE, 1, "Blessed by the Pope himself."));
        allCards.Add(MakeCard("Treasure Fleet", "Return with riches from distant lands.", 5, CardType.INFLUENCE, 1, "Gold flows from the East."));
        allCards.Add(MakeCard("Secret Tunnel", "Infiltrate enemy strongholds undetected.", 4, CardType.SABOTAGE, 1, "We know all your passages."));
        allCards.Add(MakeCard("Fortress Walls", "Impenetrable defenses against assault.", 6, CardType.DEFENSE, 1, "None shall pass."));
        allCards.Add(MakeCard("Holy Land Cultivation", "Templar farms in Outremer fed armies.", 7, CardType.FARMING, 1, "The soil is blessed with salt and blood."));
        allCards.Add(MakeCard("Mill Rights", "Control the grain, control the people.", 5, CardType.FARMING, 1, "The miller's toll is our tithe."));
        allCards.Add(MakeCard("Templar Treasury", "Centuries of wealth hidden in plain sight.", 7, CardType.BLACK_MARKET, 1, "Gold endures when kingdoms fall."));
        allCards.Add(MakeCard("Templar's Wrath", "Attacks deal triple damage, ignore shields this turn.", 10, CardType.ATTACK, 1, "The holy flame consumes all.", CardRarity.LEGENDARY));
    }

    static void CreateFreemasonCards()
    {
        allCards.Add(MakeCard("Gavel of Justice", "Strike down those who defy order.", 6, CardType.ATTACK, 2, "Order must be maintained."));
        allCards.Add(MakeCard("Sacred Geometry", "The perfect form deflects all harm.", 5, CardType.DEFENSE, 2, "The universe has a blueprint."));
        allCards.Add(MakeCard("Lodge Meeting", "Decisions made behind closed doors.", 7, CardType.INFLUENCE, 2, "Brotherhood prevails."));
        allCards.Add(MakeCard("Hidden Symbol", "Subtle signs that confuse enemies.", 5, CardType.SABOTAGE, 2, "You missed the signs."));
        allCards.Add(MakeCard("Cornerstone", "Built to last through the ages.", 4, CardType.DEFENSE, 2, "From humble beginnings."));
        allCards.Add(MakeCard("Grand Lodge Granary", "Brotherhood ensures no one starves.", 5, CardType.FARMING, 2, "From the lodge, bread for all."));
        allCards.Add(MakeCard("Artisan Guild", "Master craftsmen till the land.", 6, CardType.FARMING, 2, "Built by hand, blessed by craft."));
        allCards.Add(MakeCard("Lodge Coffers", "Membership fees from across the continent.", 6, CardType.BLACK_MARKET, 2, "Brotherhood has its price."));
        allCards.Add(MakeCard("Grand Lodge Decree", "All Freemason territories gain +5 troops this turn.", 10, CardType.DEFENSE, 2, "The lodge commands it.", CardRarity.LEGENDARY));
    }

    static void CreateCarbonariCards()
    {
        allCards.Add(MakeCard("Popular Uprising", "The people rise. Enemy power crumbles as soldiers defect to the revolution.", 9, CardType.ATTACK, 3, "Naples, 1820. The Bourbon king fled overnight."));
        allCards.Add(MakeCard("Carbonari Ambush", "A coordinated cell strike eliminates key military officers simultaneously.", 7, CardType.ATTACK, 3, "They struck in seven cities on the same night."));
        allCards.Add(MakeCard("Agent Provocateur", "Plant a Carbonari spy in enemy ranks. Discard 2 cards from target hand as their plans unravel from within.", 6, CardType.SABOTAGE, 3, "Their most trusted general was ours for three years."));
        allCards.Add(MakeCard("Revolutionary Pamphlet", "Flood the target region with seditious literature. Gain Influence as populations question their rulers.", 5, CardType.INFLUENCE, 3, "The pen has started more fires than the torch."));
        allCards.Add(MakeCard("Underground Network", "The cell structure makes the Carbonari impossible to fully destroy. Absorbs damage.", 8, CardType.DEFENSE, 3, "Cut one branch. Ten grow back."));
        allCards.Add(MakeCard("Cell Sustenance", "Revolutionary cells cultivate their own supply lines across Southern Italy.", 7, CardType.FARMING, 3, "Every vendita is a farm; every farm is a fortress."));
        allCards.Add(MakeCard("Mountain Passage", "Smuggle goods and grain across the Apennines to fund the revolution.", 5, CardType.FARMING, 3, "The charcoal burner's path is known only to the brotherhood."));
        allCards.Add(MakeCard("Charcoal Ledger", "The Carbonari's charcoal trade hides a vast network of bribes and arms deals.", 8, CardType.BLACK_MARKET, 3, "Every sack of charcoal carries a secret."));
        allCards.Add(MakeCard("The Carbonari Flame", "Forces ALL enemy factions to discard 3 cards each.", 10, CardType.SABOTAGE, 3, "The flame spreads to all corners.", CardRarity.LEGENDARY));
    }

    public static List<CardData> GetLegendaryCards()
    {
        return GetAllCards().FindAll(c => c.rarity == CardRarity.LEGENDARY);
    }
}
