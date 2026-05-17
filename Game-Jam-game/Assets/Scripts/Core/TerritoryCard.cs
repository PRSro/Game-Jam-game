using UnityEngine;

public enum TerritoryCardType { INFANTRY, CAVALRY, ARTILLERY, WILD }

[System.Serializable]
public class TerritoryCard
{
    public string territoryName;
    public TerritoryCardType cardType;

    public TerritoryCard(string territoryName, TerritoryCardType cardType)
    {
        this.territoryName = territoryName;
        this.cardType = cardType;
    }
}
