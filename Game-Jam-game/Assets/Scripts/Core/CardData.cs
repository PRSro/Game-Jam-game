using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Secret Societies/Card")]
public class CardData : ScriptableObject
{
    public int cardId;
    public string cardName;
    [TextArea(2, 3)]
    public string description;
    public int power;
    public CardType cardType;
    public int factionId;
    public CardRarity rarity = CardRarity.COMMON;
    [TextArea(1, 2)]
    public string flavorText;
}

public enum CardRarity
{
    COMMON,
    UNCOMMON,
    RARE,
    LEGENDARY
}

public enum CardType
{
    ATTACK,
    DEFENSE,
    INFLUENCE,
    SABOTAGE,
    FARMING,
    BLACK_MARKET
}
