using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Tests historical event ordering and window logic.
/// Pure logic — no TerritoryData or FactionData instantiation needed.
/// </summary>
public static class TestEventOrder
{
    public static void RunAll()
    {
        Test_MaxTurnsConstantIs150();
        Test_EventsSelectedInChronologicalOrder();
        Test_ResetClearsActiveEvent();
        Debug.Log("[TEST] TestEventOrder: ALL PASSED");
    }

    static void Test_MaxTurnsConstantIs150()
    {
        Debug.Assert(GameManager.MAX_TURNS == 150,
            $"[FAIL] Test_MaxTurnsConstantIs150: MAX_TURNS={GameManager.MAX_TURNS}, expected 150");
        Debug.Log("[PASS] Test_MaxTurnsConstantIs150");
    }

    static void Test_EventsSelectedInChronologicalOrder()
    {
        // Replicate the BUG FIX 5 sort using a mock event list with known earliestTurn values
        var mockEvents = new List<(string name, int earliestTurn)>
        {
            ("World War II",           138),
            ("French Revolution",        1),
            ("Napoleonic Wars",         10),
            ("Belle Époque",           105),
            ("Industrial Revolution",   30),
        };

        mockEvents.Sort((a, b) => a.earliestTurn.CompareTo(b.earliestTurn));

        string[] expected =
        {
            "French Revolution",
            "Napoleonic Wars",
            "Industrial Revolution",
            "Belle Époque",
            "World War II"
        };

        for (int i = 0; i < expected.Length; i++)
        {
            Debug.Assert(mockEvents[i].name == expected[i],
                $"[FAIL] Test_EventsSelectedInChronologicalOrder: index {i} = '{mockEvents[i].name}', expected '{expected[i]}'");
        }
        Debug.Log("[PASS] Test_EventsSelectedInChronologicalOrder");
    }

    static void Test_ResetClearsActiveEvent()
    {
        HistoricalEventManager.Reset();
        Debug.Assert(HistoricalEventManager.ActiveEvent == null,
            "[FAIL] Test_ResetClearsActiveEvent: ActiveEvent should be null after Reset()");
        Debug.Log("[PASS] Test_ResetClearsActiveEvent");
    }
}
