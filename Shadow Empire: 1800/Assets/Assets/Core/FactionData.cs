using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewFaction", menuName = "Secret Societies/Faction")]
public class FactionData : ScriptableObject
{
    public string factionName;
    public int factionId;
    public Color factionColor;
    public int power = 50;
    public int influence = 50;
    public int secrets = 20;
    public int shieldPoints = 0;
    public int gold = 25;
    public int goldIncome = 0;
    public int goldIncomeBonus = 0;
    public string passiveAbilityName;
    public string passiveAbilityDesc;
    public string uniqueActionName;
    public bool isEliminated = false;
    public bool isPlayerControlled = false;
    public float legitimacy = 50f;        // 0-100, starts at 50
    public float publicExposure = 0f;     // 0-100, starts at 0
    public List<CardData> hand = new List<CardData>();
    public List<CardData> deck = new List<CardData>();
    public List<CardData> discardPile = new List<CardData>();
    public List<TerritoryData> territories = new List<TerritoryData>();
    public int pendingReinforcements = 0;
    public int secretCards = 0;
    public List<TerritoryCard> territoryCards = new List<TerritoryCard>();
    public int cardTradeCount = 0;
    public float marketDiscount = 1f;
    public bool skipDrawNextTurn = false;
    [System.NonSerialized] public int cardsPlayedThisTurn = 0;
    [System.NonSerialized] public int uniqueActionLastTurn = -1;
    [System.NonSerialized] public int territoryAttackWins = 0;

    public void ResetForNewGame()
    {
        power = 50;
        influence = 50;
        secrets = 20;
        shieldPoints = 0;
        gold = 25;
        goldIncome = 0;
        goldIncomeBonus = 0;
        isEliminated = false;
        isPlayerControlled = false;
        legitimacy = 50f;
        publicExposure = 0f;
        hand.Clear();
        deck.Clear();
        discardPile.Clear();
        territories.Clear();
        pendingReinforcements = 0;
        secretCards = 0;
        territoryCards.Clear();
        cardTradeCount = 0;
        marketDiscount = 1f;
        skipDrawNextTurn = false;
        uniqueActionLastTurn = -1;
        territoryAttackWins = 0;
    }
}
