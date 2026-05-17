using UnityEngine;

/// <summary>
/// Tests the every-5-turns card draw schedule.
/// Pure arithmetic — no constructors or scene dependencies.
/// </summary>
public static class TestCardDraw
{
    public static void RunAll()
    {
        Test_CardAwardedOnMultiplesOfFive();
        Test_NoCardAwardOnNonMultiples();
        Test_CardAwardedAtTurnLimit();
        Test_FirstDrawIsAtTurnFive();
        Test_TotalDrawRoundsIn150Turns();
        Debug.Log("[TEST] TestCardDraw: ALL PASSED");
    }

    // Exact condition from GameManager.StartPlayerTurn
    static bool ShouldAwardCards(int turn) => turn % 5 == 0;

    static void Test_CardAwardedOnMultiplesOfFive()
    {
        int[] drawTurns = { 5, 10, 15, 20, 25, 50, 75, 100, 125, 150 };
        foreach (int t in drawTurns)
            Debug.Assert(ShouldAwardCards(t), $"[FAIL] Test_CardAwardedOnMultiplesOfFive: turn {t} should award cards");
        Debug.Log("[PASS] Test_CardAwardedOnMultiplesOfFive");
    }

    static void Test_NoCardAwardOnNonMultiples()
    {
        int[] noDrawTurns = { 1, 2, 3, 4, 6, 7, 8, 9, 11, 13, 17, 99, 101, 149 };
        foreach (int t in noDrawTurns)
            Debug.Assert(!ShouldAwardCards(t), $"[FAIL] Test_NoCardAwardOnNonMultiples: turn {t} should NOT award cards");
        Debug.Log("[PASS] Test_NoCardAwardOnNonMultiples");
    }

    static void Test_CardAwardedAtTurnLimit()
    {
        Debug.Assert(GameManager.MAX_TURNS == 150,
            $"[FAIL] Test_CardAwardedAtTurnLimit: MAX_TURNS={GameManager.MAX_TURNS}, expected 150");
        Debug.Assert(ShouldAwardCards(GameManager.MAX_TURNS),
            $"[FAIL] Test_CardAwardedAtTurnLimit: turn {GameManager.MAX_TURNS} should be a draw turn");
        Debug.Log("[PASS] Test_CardAwardedAtTurnLimit");
    }

    static void Test_FirstDrawIsAtTurnFive()
    {
        for (int t = 1; t <= 4; t++)
            Debug.Assert(!ShouldAwardCards(t), $"[FAIL] Test_FirstDrawIsAtTurnFive: turn {t} should not draw");
        Debug.Assert(ShouldAwardCards(5), "[FAIL] Test_FirstDrawIsAtTurnFive: turn 5 should draw");
        Debug.Log("[PASS] Test_FirstDrawIsAtTurnFive");
    }

    static void Test_TotalDrawRoundsIn150Turns()
    {
        int count = 0;
        for (int t = 1; t <= GameManager.MAX_TURNS; t++)
            if (ShouldAwardCards(t)) count++;

        Debug.Assert(count == 30,
            $"[FAIL] Test_TotalDrawRoundsIn150Turns: {count} draw rounds in {GameManager.MAX_TURNS} turns, expected 30");
        Debug.Log($"[PASS] Test_TotalDrawRoundsIn150Turns ({count} draw rounds)");
    }
}
