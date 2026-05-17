using UnityEngine;

/// <summary>
/// Attach to a scene GameObject. Disable or strip in release builds.
/// Runs all test suites on Awake and prints results to the Unity Console.
/// </summary>
public class TestRunner : MonoBehaviour
{
    [Header("Disable in release builds")]
    public bool runOnAwake = true;

    void Awake()
    {
        if (!runOnAwake) return;

        Debug.Log("=== SECRET SOCIETIES — TEST SUITE ===");
        Run("TestPlayerConquest", TestPlayerConquest.RunAll);
        Run("TestAttackArrows",   TestAttackArrows.RunAll);
        Run("TestTurnLimit",      TestTurnLimit.RunAll);
        Run("TestEventOrder",     TestEventOrder.RunAll);
        Run("TestCardDraw",       TestCardDraw.RunAll);
        Debug.Log("=== ALL SUITES COMPLETE ===");
    }

    static void Run(string name, System.Action suite)
    {
        try   { suite(); }
        catch (System.Exception e) { Debug.LogError($"[EXCEPTION] {name}: {e.Message}\n{e.StackTrace}"); }
    }
}
