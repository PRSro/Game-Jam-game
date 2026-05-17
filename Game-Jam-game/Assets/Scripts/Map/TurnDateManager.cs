using UnityEngine;

public static class TurnDateManager
{
    static readonly string[] MonthNames = { "January", "July" };
    const int START_YEAR = 1790;
    const int END_YEAR = 1990;

    /// <summary>Returns the year for a given turn. Turn 0 = January 1790.</summary>
    public static int GetYear(int turn)
    {
        return Mathf.Min(END_YEAR, START_YEAR + turn / 2);
    }

    /// <summary>Returns the month name for a given turn: January (even turns) or July (odd turns).</summary>
    public static string GetMonth(int turn)
    {
        return MonthNames[turn % 2];
    }

    /// <summary>Returns a formatted date string like "January 1790" for the given turn.</summary>
    public static string GetDateString(int turn)
    {
        return $"{GetMonth(turn)} {GetYear(turn)}";
    }

    /// <summary>Returns true if the year changes between the previous turn and the current turn.</summary>
    public static bool DidYearChange(int previousTurn, int currentTurn)
    {
        return GetYear(previousTurn) != GetYear(currentTurn);
    }

    /// <summary>Returns 0 for January (even turns) and 1 for July (odd turns).</summary>
    public static int GetMonthIndex(int turn)
    {
        return turn % 2;
    }

    /// <summary>Total number of turns: 400, covering 1790-1990.</summary>
    public const int TOTAL_TURNS = 400;
}
