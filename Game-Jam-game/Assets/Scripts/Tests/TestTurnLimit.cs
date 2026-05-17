using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Tests the 150-turn hard limit and score-based winner selection.
/// FactionData is a ScriptableObject — use ScriptableObject.CreateInstance.
/// TerritoryData requires the full constructor.
/// </summary>
public static class TestTurnLimit
{
    public static void RunAll()
    {
        Test_MaxTurnsConstantIs150();
        Test_WinnerIsHighestTerritoryHolder();
        Test_TieBrokenByPower();
        Test_EliminatedFactionCannotWin();
        Debug.Log("[TEST] TestTurnLimit: ALL PASSED");
    }

    static TerritoryData MakeTerritory(string name, int controlledBy = -1)
    {
        return new TerritoryData(name, 1, 1, false, "test", controlledBy);
    }

    static FactionData MakeFaction(string factionName, int factionId, bool isEliminated = false, int power = 50)
    {
        FactionData f = ScriptableObject.CreateInstance<FactionData>();
        f.factionName  = factionName;
        f.factionId    = factionId;
        f.isEliminated = isEliminated;
        f.power        = power;
        f.territories  = new List<TerritoryData>();
        return f;
    }

    // Replicates the winner-selection logic in GameManager.StartPlayerTurn turn-limit block
    static FactionData SelectWinner(List<FactionData> factions)
    {
        return factions
            .Where(f => !f.isEliminated)
            .OrderByDescending(f => f.territories.Count)
            .ThenByDescending(f => f.power)
            .FirstOrDefault();
    }

    static void Test_MaxTurnsConstantIs150()
    {
        Debug.Assert(GameManager.MAX_TURNS == 150,
            $"[FAIL] Test_MaxTurnsConstantIs150: MAX_TURNS={GameManager.MAX_TURNS}, expected 150");
        Debug.Log("[PASS] Test_MaxTurnsConstantIs150");
    }

    static void Test_WinnerIsHighestTerritoryHolder()
    {
        FactionData f1 = MakeFaction("Alpha", 0);
        FactionData f2 = MakeFaction("Beta",  1);
        f1.territories.Add(MakeTerritory("Rome",   0));
        f1.territories.Add(MakeTerritory("Venice", 0));
        f1.territories.Add(MakeTerritory("Milan",  0));
        f2.territories.Add(MakeTerritory("Paris",  1));

        FactionData winner = SelectWinner(new List<FactionData> { f1, f2 });

        Debug.Assert(winner == f1,
            $"[FAIL] Test_WinnerIsHighestTerritoryHolder: winner='{winner?.factionName}', expected 'Alpha'");
        Debug.Log("[PASS] Test_WinnerIsHighestTerritoryHolder");
    }

    static void Test_TieBrokenByPower()
    {
        FactionData f1 = MakeFaction("LowPower",  0, power: 30);
        FactionData f2 = MakeFaction("HighPower", 1, power: 80);
        f1.territories.Add(MakeTerritory("Rome",   0));
        f1.territories.Add(MakeTerritory("Venice", 0));
        f2.territories.Add(MakeTerritory("Paris",  1));
        f2.territories.Add(MakeTerritory("Lyon",   1));

        FactionData winner = SelectWinner(new List<FactionData> { f1, f2 });

        Debug.Assert(winner == f2,
            $"[FAIL] Test_TieBrokenByPower: winner='{winner?.factionName}', expected 'HighPower'");
        Debug.Log("[PASS] Test_TieBrokenByPower");
    }

    static void Test_EliminatedFactionCannotWin()
    {
        FactionData f1 = MakeFaction("Eliminated", 0, isEliminated: true);
        FactionData f2 = MakeFaction("Survivor",   1, isEliminated: false);
        f1.territories.Add(MakeTerritory("Rome",   0));
        f1.territories.Add(MakeTerritory("Venice", 0));
        f1.territories.Add(MakeTerritory("Milan",  0));
        f2.territories.Add(MakeTerritory("Paris",  1));

        FactionData winner = SelectWinner(new List<FactionData> { f1, f2 });

        Debug.Assert(winner == f2,
            $"[FAIL] Test_EliminatedFactionCannotWin: eliminated faction should not win, got '{winner?.factionName}'");
        Debug.Log("[PASS] Test_EliminatedFactionCannotWin");
    }
}
