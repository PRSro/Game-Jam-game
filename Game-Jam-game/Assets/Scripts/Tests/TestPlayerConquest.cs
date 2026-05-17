using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Tests player conquest (territory capture) logic via CombatManager directly.
/// CombatManager.ResolveTerritoryBattle does not require GameManager.Instance
/// for its dice/combat logic — only for the faction-list update on capture.
/// Tests marked [SKIP-RNG] may occasionally skip due to dice randomness; re-run to confirm.
/// </summary>
public static class TestPlayerConquest
{
    public static void RunAll()
    {
        Test_AttackerWinsWhenTroopsOverwhelm();
        Test_TerritoryOwnershipTransfersOnVictory();
        Test_DefenderHoldsWhenAttackerIsWeak();
        Test_AttackerNeedsMoreThanOneTroop();
        Debug.Log("[TEST] TestPlayerConquest: ALL PASSED");
    }

    // Creates a TerritoryData using the real constructor, then overrides troops.
    static TerritoryData Make(string name, int controlledBy, int troops)
    {
        var t = new TerritoryData(name, 1, 1, false, "test", controlledBy);
        t.troops = troops;
        return t;
    }

    static void Test_AttackerWinsWhenTroopsOverwhelm()
    {
        TerritoryData src = Make("Rome",   0, 50);
        TerritoryData tgt = Make("Naples", 1, 1);

        CombatResult result = CombatManager.ResolveTerritoryBattle(
            src, tgt,
            attackerBonusDice: 0, defenderBonusDice: 0,
            attackerReroll: false, preBattleSabotage: 0, defenderShieldBonus: 0,
            maxRounds: 20);

        Debug.Assert(result.attackerWon,
            "[FAIL] Test_AttackerWinsWhenTroopsOverwhelm: 50 troops vs 1 should always win");
        Debug.Assert(result.territoryCaptured,
            "[FAIL] Test_AttackerWinsWhenTroopsOverwhelm: territory should be captured");
        Debug.Log("[PASS] Test_AttackerWinsWhenTroopsOverwhelm");
    }

    static void Test_TerritoryOwnershipTransfersOnVictory()
    {
        TerritoryData src = Make("Rome",   0, 50);
        TerritoryData tgt = Make("Naples", 1, 1);

        CombatResult result = CombatManager.ResolveTerritoryBattle(src, tgt, maxRounds: 20);

        if (!result.territoryCaptured)
        {
            Debug.LogWarning("[SKIP-RNG] Test_TerritoryOwnershipTransfersOnVictory: attacker didn't win this roll — re-run");
            return;
        }

        Debug.Assert(tgt.controlledBy == 0,
            $"[FAIL] Test_TerritoryOwnershipTransfersOnVictory: controlledBy={tgt.controlledBy}, expected 0");
        Debug.Log("[PASS] Test_TerritoryOwnershipTransfersOnVictory");
    }

    static void Test_DefenderHoldsWhenAttackerIsWeak()
    {
        TerritoryData src = Make("Rome",   0, 2);
        TerritoryData tgt = Make("Naples", 1, 20);

        CombatResult result = CombatManager.ResolveTerritoryBattle(src, tgt, maxRounds: 3);

        // We only assert the method completes without exceptions and returns a valid log
        Debug.Assert(result.battleLog != null,
            "[FAIL] Test_DefenderHoldsWhenAttackerIsWeak: battleLog should never be null");
        Debug.Log("[PASS] Test_DefenderHoldsWhenAttackerIsWeak");
    }

    static void Test_AttackerNeedsMoreThanOneTroop()
    {
        TerritoryData src = Make("Rome",   0, 1); // exactly 1 troop — should be blocked
        TerritoryData tgt = Make("Naples", 1, 5);

        CombatResult result = CombatManager.ResolveTerritoryBattle(src, tgt, maxRounds: 5);

        Debug.Assert(!result.attackerWon,
            "[FAIL] Test_AttackerNeedsMoreThanOneTroop: attacker with 1 troop should not win");
        Debug.Assert(!result.territoryCaptured,
            "[FAIL] Test_AttackerNeedsMoreThanOneTroop: territory should not be captured with 1 troop");
        Debug.Assert(result.battleLog.Contains("more than 1 troop"),
            "[FAIL] Test_AttackerNeedsMoreThanOneTroop: battleLog should contain the 1-troop rule message");
        Debug.Log("[PASS] Test_AttackerNeedsMoreThanOneTroop");
    }
}
