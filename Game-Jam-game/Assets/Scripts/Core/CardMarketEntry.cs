[System.Serializable]
public class CardMarketEntry
{
    public CardData card;
    public int price;
    public int sellerFactionId;

    public CardMarketEntry(CardData card, int price, int sellerFactionId)
    {
        this.card = card;
        this.price = price;
        this.sellerFactionId = sellerFactionId;
    }
}
