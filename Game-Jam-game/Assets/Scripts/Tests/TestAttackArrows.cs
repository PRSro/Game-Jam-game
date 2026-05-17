using UnityEngine;

/// <summary>
/// Tests the arrow drawing system in TerritoryGraphRenderer.
/// Requires Play Mode — tests are skipped (with a warning) if the instance is absent.
/// </summary>
public static class TestAttackArrows
{
    public static void RunAll()
    {
        Test_HighlightAttackOptionsDoesNotThrow();
        Test_ShowPendingAttackDoesNotThrow();
        Test_ClearPendingAttackDoesNotThrow();
        Test_RedrawArrowsDoesNotThrow();
        Debug.Log("[TEST] TestAttackArrows: ALL PASSED");
    }

    // Creates a TerritoryData using the real constructor.
    static TerritoryData Make(string name, int controlledBy, int troops)
    {
        var t = new TerritoryData(name, 1, 1, false, "test", controlledBy);
        t.troops = troops;
        return t;
    }

    static void Test_HighlightAttackOptionsDoesNotThrow()
    {
        if (TerritoryGraphRenderer.Instance == null)
        {
            Debug.LogWarning("[SKIP] Test_HighlightAttackOptionsDoesNotThrow: no TerritoryGraphRenderer in scene — run in Play Mode");
            return;
        }
        TerritoryData territory = Make("Rome", 0, 10);
        TerritoryGraphRenderer.Instance.HighlightAttackOptions(territory);
        Debug.Log("[PASS] Test_HighlightAttackOptionsDoesNotThrow");
    }

    static void Test_ShowPendingAttackDoesNotThrow()
    {
        if (TerritoryGraphRenderer.Instance == null)
        {
            Debug.LogWarning("[SKIP] Test_ShowPendingAttackDoesNotThrow: no instance in scene");
            return;
        }
        TerritoryGraphRenderer.Instance.ShowPendingAttack("Rome", "Naples");
        Debug.Log("[PASS] Test_ShowPendingAttackDoesNotThrow");
    }

    static void Test_ClearPendingAttackDoesNotThrow()
    {
        if (TerritoryGraphRenderer.Instance == null)
        {
            Debug.LogWarning("[SKIP] Test_ClearPendingAttackDoesNotThrow: no instance in scene");
            return;
        }
        TerritoryGraphRenderer.Instance.ClearPendingAttack();
        Debug.Log("[PASS] Test_ClearPendingAttackDoesNotThrow");
    }

    static void Test_RedrawArrowsDoesNotThrow()
    {
        if (TerritoryGraphRenderer.Instance == null)
        {
            Debug.LogWarning("[SKIP] Test_RedrawArrowsDoesNotThrow: no instance in scene");
            return;
        }
        TerritoryGraphRenderer.Instance.RedrawArrows();
        Debug.Log("[PASS] Test_RedrawArrowsDoesNotThrow");
    }
}
