using System.Collections.Generic;
using UnityEngine;

public static class FactionDatabase
{
    private static List<FactionData> factions = null;

    public static void Reset() { factions = null; }

    public static List<FactionData> GetAllFactions()
    {
        if (factions == null)
        {
            factions = new List<FactionData>();
            factions.Add(CreateFaction("Illuminati", 0, new Color(0.85f, 0.70f, 0.15f)));
            factions.Add(CreateFaction("Knights Templar", 1, new Color(0.75f, 0.10f, 0.10f)));
            factions.Add(CreateFaction("Freemasons", 2, new Color(0.15f, 0.35f, 0.75f)));
            factions.Add(CreateFaction("The Carbonari", 3, new Color(0.60f, 0.75f, 0.20f)));
            ApplyFactionIdentity(factions);
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

    static void ApplyFactionIdentity(List<FactionData> allFactions)
    {
        foreach (FactionData f in allFactions)
        {
            switch (f.factionId)
            {
                case 0:
                    f.passiveAbilityName = "Enlightened";
                    f.passiveAbilityDesc = "Influence cards cost -1 power to play; gain +2 influence income per turn.";
                    f.uniqueActionName = "Plant Agent";
                    break;
                case 1:
                    f.passiveAbilityName = "Fortified";
                    f.passiveAbilityDesc = "Owned territories gain +1 troop at turn start, capped at 8 per territory.";
                    f.uniqueActionName = "Holy War";
                    break;
                case 2:
                    f.passiveAbilityName = "Lodge Network";
                    f.passiveAbilityDesc = "Gain +1 secret per owned territory each turn.";
                    f.uniqueActionName = "Grand Lodge";
                    break;
                case 3:
                    f.passiveAbilityName = "Revolutionary Cell";
                    f.passiveAbilityDesc = "Gain +5 influence when a historical event fires.";
                    f.uniqueActionName = "Incite Uprising";
                    break;
            }
        }
    }
}
