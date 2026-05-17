using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public struct CombatResult
{
    public bool attackerWon;
    public int attackerTroopsLost;
    public int defenderTroopsLost;
    public bool territoryCaptured;
    public int[] attackerDice;
    public int[] defenderDice;
    public string battleLog;
}

public static class CombatManager
{
    public static void DealDamage(FactionData target, int amount)
    {
        int remaining = amount;

        if (target.shieldPoints > 0)
        {
            int absorbed = Mathf.Min(target.shieldPoints, remaining);
            target.shieldPoints -= absorbed;
            remaining -= absorbed;
        }

        target.power = Mathf.Max(0, target.power - remaining);

        if (GameUIManager.Instance != null)
            GameUIManager.Instance.FlashFactionPanel(target.factionId);

        if (target.power <= 0)
        {
            GameManager.Instance?.CheckEliminations();
        }
    }

    public static void AddShield(FactionData target, int amount)
    {
        target.shieldPoints += amount;
    }

    public static void AddSecrets(FactionData target, int amount)
    {
        target.secrets = Mathf.Min(50, target.secrets + amount);
    }

    public static bool IsEliminated(FactionData faction)
    {
        return faction.isEliminated || faction.power <= 0;
    }

    static int RollDie()
    {
        return Random.Range(1, 7);
    }

    static int[] RollDice(int count)
    {
        int[] results = new int[count];
        for (int i = 0; i < count; i++)
            results[i] = RollDie();
        System.Array.Sort(results);
        System.Array.Reverse(results);
        return results;
    }

    // FIXED: BUG 1 - territory ownership transfer happens here (not duplicated in caller)
    // ADDED: UPGRADE 2 - attackerChosenDice parameter lets player override auto dice count
    /// <summary>Resolves a territory attack using multi-round RISK-style combat. Returns detailed combat results.</summary>
    public static CombatResult ResolveTerritoryBattle(
        TerritoryData attackerTerritory,
        TerritoryData defenderTerritory,
        int attackerBonusDice = 0,
        int defenderBonusDice = 0,
        bool attackerReroll = false,
        int preBattleSabotage = 0,
        int defenderShieldBonus = 0,
        int maxRounds = 5,
        int attackerChosenDice = 0)
    {
        CombatResult result = new CombatResult();
        result.attackerDice = new int[0];
        result.defenderDice = new int[0];
        result.battleLog = "";

        if (attackerTerritory == null || defenderTerritory == null || GameManager.Instance == null)
        {
            result.battleLog = "Invalid territory battle.";
            return result;
        }

        FactionData defenderFaction = GameManager.Instance.factions.Find(f => f.factionId == defenderTerritory.controlledBy);
        int originalDefenderShield = defenderFaction != null ? defenderFaction.shieldPoints : 0;
        if (defenderFaction != null && defenderShieldBonus > 0)
        {
            defenderFaction.shieldPoints += defenderShieldBonus;
        }

        if (attackerTerritory.troops <= 1)
        {
            result.battleLog = "Attacker needs more than 1 troop to attack.";
            if (defenderFaction != null)
                defenderFaction.shieldPoints = originalDefenderShield;
            return result;
        }

        defenderTerritory.troops = Mathf.Max(1, defenderTerritory.troops - preBattleSabotage);

        int roundCount = 0;
        int totalAttackerLosses = 0;
        int totalDefenderLosses = 0;
        int[] lastAttackDice = new int[0];
        int[] lastDefenseDice = new int[0];
        int lastAttackerDiceCount = 0;
        var logBuilder = new System.Text.StringBuilder();

        while (attackerTerritory.troops > 1 && defenderTerritory.troops > 0 && roundCount < maxRounds)
        {
            roundCount++;

            // ADDED: UPGRADE 2 - use attackerChosenDice when explicitly provided
            int attackerDiceCount;
            if (attackerChosenDice > 0)
                attackerDiceCount = Mathf.Clamp(attackerChosenDice + attackerBonusDice, 1, 5);
            else
                attackerDiceCount = Mathf.Min(3, attackerTerritory.troops - 1) + attackerBonusDice;
            attackerDiceCount = Mathf.Clamp(attackerDiceCount, 1, 5);
            int defenderDiceCount = Mathf.Min(2, defenderTerritory.troops) + defenderBonusDice;
            defenderDiceCount = Mathf.Clamp(defenderDiceCount, 1, 4);

            int[] attackDice = RollDice(attackerDiceCount);
            if (attackerReroll && attackDice.Length > 0)
            {
                attackDice[attackDice.Length - 1] = RollDie();
                System.Array.Sort(attackDice);
                System.Array.Reverse(attackDice);
            }
            int[] defenseDice = RollDice(defenderDiceCount);

            lastAttackDice = attackDice;
            lastDefenseDice = defenseDice;
            lastAttackerDiceCount = attackerDiceCount;

            int comparisons = Mathf.Min(attackDice.Length, defenseDice.Length);
            int attackerLosses = 0;
            int defenderLosses = 0;

            for (int i = 0; i < comparisons; i++)
            {
                if (attackDice[i] > defenseDice[i])
                    defenderLosses++;
                else
                    attackerLosses++;
            }

            defenderLosses = Mathf.Min(defenderLosses, defenderTerritory.troops);
            attackerLosses = Mathf.Min(attackerLosses, attackerTerritory.troops - 1);

            totalAttackerLosses += attackerLosses;
            totalDefenderLosses += defenderLosses;

            attackerTerritory.RemoveTroops(attackerLosses);
            defenderTerritory.RemoveTroops(defenderLosses);

            logBuilder.AppendLine($"Round {roundCount}: A({string.Join(",", attackDice)}) vs D({string.Join(",", defenseDice)}) — Attacker loses {attackerLosses}, Defender loses {defenderLosses}");
        }

        result.attackerDice = lastAttackDice;
        result.defenderDice = lastDefenseDice;
        result.attackerTroopsLost = totalAttackerLosses;
        result.defenderTroopsLost = totalDefenderLosses;

        if (defenderTerritory.troops <= 0)
        {
            result.territoryCaptured = true;
            result.attackerWon = true;

            FactionData attackerFaction = GameManager.Instance.factions.Find(f => f.factionId == attackerTerritory.controlledBy);
            defenderFaction = GameManager.Instance.factions.Find(f => f.factionId == defenderTerritory.controlledBy);

            int availableToMove = attackerTerritory.troops - 1; // must leave 1 behind
            int movingTroops = Mathf.Clamp(lastAttackerDiceCount, 1, Mathf.Max(1, availableToMove));
            if (availableToMove < 1)
            {
                // Edge case: attacker somehow has exactly 1 troop left — move 0, do not crash
                movingTroops = 0;
            }
            if (movingTroops > 0)
                attackerTerritory.RemoveTroops(movingTroops);
            defenderTerritory.troops = Mathf.Max(1, movingTroops);

            defenderTerritory.controlledBy = attackerTerritory.controlledBy;
            MapSystemController.Instance?.SyncProvinceFromTerritory(defenderTerritory.name);

            if (defenderFaction != null)
            {
                defenderFaction.territories.Remove(defenderTerritory);
            }
            if (attackerFaction != null && !attackerFaction.territories.Contains(defenderTerritory))
                attackerFaction.territories.Add(defenderTerritory);

            logBuilder.AppendLine($"Territory captured! Attacker moves {movingTroops} troops in.");
        }
        else
        {
            result.attackerWon = false;
            if (roundCount >= maxRounds)
                logBuilder.AppendLine($"Battle ended after {maxRounds} rounds (max rounds reached). No territory captured.");
            else
                logBuilder.AppendLine("Attack repelled. Defender holds.");
        }

        result.battleLog = logBuilder.ToString().TrimEnd();
        if (defenderFaction != null)
            defenderFaction.shieldPoints = originalDefenderShield;

        return result;
    }

    /// <summary>Calculates total reinforcement troops for a faction including region bonuses.</summary>
    public static int CalculateReinforcementsForFaction(FactionData faction)
    {
        List<TerritoryData> owned = faction.territories;
        if (owned.Count == 0) return 0;
        return TerritoryGraph.CalculateReinforcements(owned);
    }
}
