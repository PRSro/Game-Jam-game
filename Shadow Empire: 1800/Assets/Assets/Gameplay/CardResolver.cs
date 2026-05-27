using UnityEngine;

public static class CardResolver
{
    public static void Resolve(CardData card, FactionData source, FactionData target)
    {
        if (card == null || source == null)
        {
            GameManager.Instance?.LogMessage("Invalid card resolution skipped.");
            return;
        }

        if (card.goldCost > 0)
        {
            if (source.gold < card.goldCost)
            {
                GameManager.Instance?.LogMessage($"WARNING: {source.factionName} has {source.gold} gold but {card.cardName} costs {card.goldCost}. Resolution proceeds anyway.");
            }
            else
            {
                source.gold -= card.goldCost;
                GameManager.Instance?.LogMessage($"{source.factionName} pays {card.goldCost} gold to play {card.cardName} (remaining: {source.gold}).");
            }
        }

        switch (card.cardType)
        {
            case CardType.ATTACK:
                ResolveAttack(card, source, target);
                break;

            case CardType.DEFENSE:
                ResolveDefense(card, source);
                break;

            case CardType.INFLUENCE:
                ResolveInfluence(card, source);
                break;

            case CardType.SABOTAGE:
                ResolveSabotage(card, source, target);
                break;

            case CardType.FARMING:
                ResolveFarming(card, source);
                break;

            case CardType.BLACK_MARKET:
                ResolveBlackMarket(card, source, target);
                break;
        }

        if (card.rarity == CardRarity.LEGENDARY)
            ResolveLegendary(card, source, target);
    }

    static void ResolveLegendary(CardData card, FactionData source, FactionData target)
    {
        GameManager.Instance?.LogMessage($"\u2726\u2726 LEGENDARY EFFECT: {card.cardName} — {card.description}");

        switch (card.cardName)
        {
            case "Eye of Providence":
                if (target != null)
                {
                    TerritoryData enemyTerr = null;
                    if (GameManager.Instance != null)
                    {
                        foreach (TerritoryData t in GameManager.Instance.territories)
                        {
                            if (t.controlledBy == target.factionId)
                            {
                                enemyTerr = t;
                                break;
                            }
                        }
                    }
                    if (enemyTerr != null)
                    {
                        target.territories.Remove(enemyTerr);
                        enemyTerr.controlledBy = -1;
                        enemyTerr.troops = 0;
                        GameManager.Instance?.LogMessage($"Eye of Providence converts {enemyTerr.name} to neutral!");
                        if (MapSystemController.Instance != null)
                            MapSystemController.Instance.SyncProvinceFromTerritory(enemyTerr.name);
                        if (GameUIManager.Instance != null)
                            GameUIManager.Instance.UpdateTerritoryMap();
                    }
                }
                break;

            case "Templar's Wrath":
                int extraDmg = source.power > 0 ? Mathf.RoundToInt(source.power * 0.3f) : 10;
                foreach (FactionData f in GameManager.Instance.factions)
                {
                    if (f.isEliminated || f == source) continue;
                    f.shieldPoints = 0;
                    f.power = Mathf.Max(0, f.power - extraDmg);
                    GameManager.Instance?.LogMessage($"{f.factionName} takes {extraDmg} damage from Templar's Wrath, shields ignored!");
                }
                GameManager.Instance?.CheckEliminations();
                break;

            case "Grand Lodge Decree":
                foreach (TerritoryData t in GameManager.Instance.territories)
                {
                    if (t.controlledBy == source.factionId)
                        t.AddTroops(5);
                }
                GameManager.Instance?.LogMessage($"All {source.factionName} territories gain +5 troops from Grand Lodge Decree!");
                if (MapSystemController.Instance != null)
                    MapSystemController.Instance.SyncProvincesFromTerritories();
                if (GameUIManager.Instance != null)
                    GameUIManager.Instance.UpdateTerritoryMap();
                break;

            case "The Carbonari Flame":
                foreach (FactionData f in GameManager.Instance.factions)
                {
                    if (f.isEliminated || f == source) continue;
                    DeckManager.DiscardRandom(f, 3);
                }
                GameManager.Instance?.LogMessage("Carbonari Flame forces ALL enemies to discard 3 cards each!");
                if (GameUIManager.Instance != null)
                    GameUIManager.Instance.UpdateAllFactions();
                break;
        }

        if (GameUIManager.Instance != null)
            GameUIManager.Instance.UpdateAllFactions();
    }

    static void ResolveAttack(CardData card, FactionData source, FactionData target)
    {
        if (target == null)
        {
            GameManager.Instance?.LogMessage("Attack card has no valid target.");
            return;
        }

        int combo = GameManager.Instance?.GetComboPower(card) ?? 0;
        int damage = card.power + combo;

        int crusadeBonus = HistoricalEventManager.GetAttackBoostForFaction(source.factionId);
        damage += crusadeBonus;
        if (crusadeBonus > 0)
            GameManager.Instance?.LogMessage($"{source.factionName} rides the Napoleonic tide — +{crusadeBonus} damage!");

        int penalty = HistoricalEventManager.GetPenaltyForFaction(source.factionId);
        damage += penalty;

        if (GameManager.Instance != null && GameManager.Instance.IsFactionDominant(source))
        {
            int bonus = 2;
            damage += bonus;
            GameManager.Instance?.LogMessage($"{source.factionName} is DOMINANT — +{bonus} bonus damage!");
        }

        if (target.shieldPoints > 0)
        {
            int absorbed = Mathf.Min(target.shieldPoints, damage);
            target.shieldPoints -= absorbed;
            damage -= absorbed;
            GameManager.Instance?.LogMessage($"{target.factionName}'s shield absorbed {absorbed} damage.");
        }

        int damageAfterShield = damage;
        target.power = Mathf.Max(0, target.power - damageAfterShield);

        string msg = $"{source.factionName} attacks {target.factionName} with {card.cardName} for {damageAfterShield} damage!" +
            (damage != damageAfterShield ? $" ({damage - damageAfterShield} absorbed by shield)" : "");
        GameManager.Instance?.LogMessage(msg);

        // Flash defender panel
        GameUIManager.Instance?.FlashFactionPanel(target.factionId);

        // Flash damage screen effect if player is the target
        if (target.isPlayerControlled && GameUIManager.Instance != null)
            GameUIManager.Instance.FlashDamageEffect();

        // Check eliminations after every attack
        GameManager.Instance?.CheckEliminations();
    }

    static void ResolveDefense(CardData card, FactionData source)
    {
        int combo = GameManager.Instance?.GetComboPower(card) ?? 0;
        int gain = card.power + combo;
        int before = source.shieldPoints;
        source.shieldPoints = Mathf.Min(20, source.shieldPoints + gain);
        int actual = source.shieldPoints - before;
        GameManager.Instance?.LogMessage($"{source.factionName} gains {actual} shield from {card.cardName}. (Total: {source.shieldPoints})");
    }

    static void ResolveInfluence(CardData card, FactionData source)
    {
        int combo = GameManager.Instance?.GetComboPower(card) ?? 0;
        int effectivePower = card.power + combo;
        if (source.factionId == 0)
            effectivePower += 1;
        int printingBonus = HistoricalEventManager.GetInfluenceBoost();
        effectivePower += printingBonus;
        int alchemyBonus = HistoricalEventManager.GetInfluenceBoostForFaction(source.factionId);
        effectivePower += alchemyBonus;
        int minigameBonus = GameManager.Instance?.ConsumePendingCardPowerBonus() ?? 0;
        effectivePower += minigameBonus;

        int secretsGain = effectivePower;
        source.secrets = Mathf.Min(50, source.secrets + secretsGain);

        int influenceGain = Mathf.Max(1, effectivePower / 2);
        source.influence = Mathf.Min(100, source.influence + influenceGain);

        string msg = $"{source.factionName} gains {secretsGain} secrets and {influenceGain} influence from {card.cardName}.";

        TerritoryData neutralTarget = null;
        if (GameManager.Instance != null)
        {
            foreach (TerritoryData t in GameManager.Instance.territories)
            {
                if (t.controlledBy == -1 && source.influence >= 25)
                {
                    foreach (string adj in t.adjacentTerritories)
                    {
                        TerritoryData adjT = GameManager.Instance.territories.Find(td => td.name == adj);
                        if (adjT != null && adjT.controlledBy == source.factionId)
                        {
                            if (neutralTarget == null || t.farmingValue > neutralTarget.farmingValue)
                                neutralTarget = t;
                            break;
                        }
                    }
                }
            }

            if (neutralTarget != null && HasAdjacentWithTenTroops(source, neutralTarget))
            {
                int flipCost = 20;
                if (source.influence >= flipCost)
                {
                    source.influence -= flipCost;
                    neutralTarget.controlledBy = source.factionId;
                    neutralTarget.troops = 2;
                    source.territories.Add(neutralTarget);
                    msg += $" Influence flips {neutralTarget.name} to your control!";
                    if (MapSystemController.Instance != null)
                        MapSystemController.Instance.SyncProvinceFromTerritory(neutralTarget.name);
                    if (GameUIManager.Instance != null)
                        GameUIManager.Instance.UpdateTerritoryMap();
                }
            }
        }

        if (printingBonus > 0) msg += " (Congress of Vienna bonus)";
        if (alchemyBonus > 0) msg += " (Russian Revolution bonus)";
        if (minigameBonus != 0) msg += $" (Negotiation {minigameBonus:+#;-#;0})";
        if (GameManager.Instance != null && GameManager.Instance.IsFactionDominant(source))
            msg += " DOMINANT!";
        GameManager.Instance?.LogMessage(msg);
    }

    static void ResolveSabotage(CardData card, FactionData source, FactionData target)
    {
        if (target == null)
        {
            GameManager.Instance?.LogMessage("Sabotage card has no valid target.");
            return;
        }

        int minigameBonus = GameManager.Instance?.ConsumePendingCardPowerBonus() ?? 0;
        int sabotageCount = Mathf.Max(1, 2 + minigameBonus);
        if (HistoricalEventManager.GetSabotageBoost() > 0)
        {
            sabotageCount = 4;
            GameManager.Instance?.LogMessage("The Rise of Fascism empowers sabotage — +2 extra discard!");
        }

        int beforeCount = target.hand.Count;
        DeckManager.DiscardRandom(target, sabotageCount);
        int actualDiscarded = beforeCount - target.hand.Count;
        GameManager.Instance?.LogMessage($"{source.factionName} sabotages {target.factionName} with {card.cardName}, forcing discard of {actualDiscarded} cards.");

        if (target.hand.Count == 0)
            GameManager.Instance?.LogMessage($"{target.factionName} has been completely disrupted — hand wiped!");

        GameUIManager.Instance?.UpdateAllFactions();
    }

    static void ResolveFarming(CardData card, FactionData source)
    {
        int combo = GameManager.Instance?.GetComboPower(card) ?? 0;
        int effectivePower = card.power + combo + HistoricalEventManager.GetFarmingBoost();
        int secretsGain = effectivePower;
        source.secrets = Mathf.Min(50, source.secrets + secretsGain);

        int powerGain = effectivePower / 3;
        source.power = Mathf.Min(100, source.power + powerGain);

        if (GameManager.Instance != null)
        {
            TerritoryData best = null;
            foreach (TerritoryData t in GameManager.Instance.territories)
            {
                if (t.controlledBy != -1) continue;
                bool adjacent = false;
                foreach (string adj in t.adjacentTerritories)
                {
                    TerritoryData adjT = GameManager.Instance.territories.Find(td => td.name == adj);
                    if (adjT != null && adjT.controlledBy == source.factionId) { adjacent = true; break; }
                }
                if (!adjacent) continue;
                if (best == null || t.farmingValue > best.farmingValue)
                    best = t;
            }
            if (best != null && HasAdjacentWithTenTroops(source, best))
            {
                best.controlledBy = source.factionId;
                best.troops = 3;
                source.territories.Add(best);
                if (MapSystemController.Instance != null)
                    MapSystemController.Instance.SyncProvinceFromTerritory(best.name);
                if (GameUIManager.Instance != null)
                    GameUIManager.Instance.UpdateTerritoryMap();
                GameManager.Instance.LogMessage($"{source.factionName} claims {best.name} through agricultural expansion.");
            }

            TerritoryData boostTarget = null;
            int highestFarm = -1;
            foreach (TerritoryData t in source.territories)
            {
                if (t.farmingValue > highestFarm)
                {
                    highestFarm = t.farmingValue;
                    boostTarget = t;
                }
            }
            if (boostTarget != null)
            {
                boostTarget.farmingBoostTurns = 3;
                GameManager.Instance.LogMessage($"{boostTarget.name} gains farming boost for 3 turns (+{boostTarget.farmingValue} troops/turn).");
                GameUIManager.Instance?.UpdateTerritoryMap();
            }
        }

        GameManager.Instance?.LogMessage($"{source.factionName} cultivates the land, gaining {powerGain} power and {secretsGain} secrets.");
        GameUIManager.Instance?.UpdateAllFactions();
    }

    static void ResolveBlackMarket(CardData card, FactionData source, FactionData target)
    {
        int combo = GameManager.Instance?.GetComboPower(card) ?? 0;
        int effectivePower = card.power + combo;
        float multiplier = HistoricalEventManager.GetBlackMarketMultiplier();
        if (multiplier > 1f)
            GameManager.Instance?.LogMessage("Banking Innovation doubles black market effects!");

        FactionData richest = null;
        int maxSecrets = -1;
        foreach (FactionData f in GameManager.Instance.factions)
        {
            if (f.isEliminated || f == source) continue;
            if (f.secrets > maxSecrets)
            {
                maxSecrets = f.secrets;
                richest = f;
            }
        }

        if (richest != null)
        {
            int stolen = Mathf.Min(Mathf.RoundToInt(effectivePower / 2f * multiplier), richest.secrets);
            richest.secrets -= stolen;
            source.secrets = Mathf.Min(50, source.secrets + stolen);

            if (richest.isPlayerControlled && GameUIManager.Instance != null)
                GameUIManager.Instance.FlashDamageEffect();

            GameManager.Instance?.LogMessage($"{source.factionName} conducts black market dealings, stealing {stolen} secrets from {richest.factionName}.");
        }

        int influenceGain = Mathf.RoundToInt(3 * multiplier);
        source.influence = Mathf.Min(100, source.influence + influenceGain);

        source.secretCards++;

        if (GameManager.Instance != null)
        {
            TerritoryData bestTerritory = null;
            int maxTroops = -1;
            foreach (TerritoryData t in source.territories)
            {
                if (t.troops > maxTroops)
                {
                    maxTroops = t.troops;
                    bestTerritory = t;
                }
            }

            if (bestTerritory != null && target != null && target.isPlayerControlled)
            {
                int enemyTroops = 0;
                string enemyTerritoryInfo = "";
                foreach (TerritoryData t in GameManager.Instance.territories)
                {
                    if (t.controlledBy == target.factionId)
                    {
                        enemyTroops += t.troops;
                        enemyTerritoryInfo += $"{t.name}({t.troops}) ";
                    }
                }
                GameManager.Instance.LogMessage($"Intel reveals {target.factionName} troop deployments: {enemyTerritoryInfo}");
            }

            int bonusTroops = Mathf.RoundToInt(effectivePower * multiplier);
            if (bestTerritory != null)
            {
                bestTerritory.AddTroops(bonusTroops);
                GameManager.Instance.LogMessage($"Black market supplies {bonusTroops} bonus troops to {bestTerritory.name}.");
            }
        }

        if (source.secretCards >= 10)
            GameManager.Instance?.LogMessage($"{source.factionName} has {source.secretCards} secret cards — ready for Shadow Coup!");

        GameUIManager.Instance?.UpdateAllFactions();
    }

    static bool HasAdjacentWithTenTroops(FactionData source, TerritoryData target)
    {
        foreach (string adj in target.adjacentTerritories)
        {
            TerritoryData adjT = GameManager.Instance.territories.Find(t => t.name == adj);
            if (adjT != null && adjT.controlledBy == source.factionId && adjT.troops >= 10)
                return true;
        }
        return false;
    }
}
