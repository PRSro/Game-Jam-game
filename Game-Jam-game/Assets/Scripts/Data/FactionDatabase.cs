using System.Collections.Generic;
using UnityEngine;

public static class FactionDatabase
{
    private static List<FactionData> factions = null;

    public static List<FactionData> GetAllFactions()
    {
        if (factions == null)
        {
            factions = new List<FactionData>();
            factions.Add(CreateFaction("Illuminati", 0, new Color(1f, 0.843f, 0f)));
            factions.Add(CreateFaction("Knights Templar", 1, new Color(0.8f, 0f, 0f)));
            factions.Add(CreateFaction("Freemasons", 2, new Color(0.102f, 0.227f, 0.541f)));
            factions.Add(CreateFaction("The Carbonari", 3, new Color(0.176f, 0.102f, 0.031f))); // #2D1A08
        }
        return factions;
    }

    public static FactionData GetFaction(int id)
    {
        return GetAllFactions().Find(f => f.factionId == id);
    }

    static FactionData CreateFaction(string name, int id, Color color)
    {
        FactionData f = ScriptableObject.CreateInstance<FactionData>();
        f.factionName = name;
        f.factionId = id;
        f.factionColor = color;
        return f;
    }
}
