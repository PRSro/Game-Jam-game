using System;
using System.Collections.Generic;
using System.Linq;

public static class TerritoryGraph
{
    private static Dictionary<string, List<string>> adjacency;
    private static Dictionary<string, string> territoryToRegion;
    private static Dictionary<string, List<string>> regionTerritories;
    private static Dictionary<string, int> regionBonus;
    private static bool initialized = false;

    public static void Reset() { initialized = false; }

    static void Initialize()
    {
        if (initialized) return;
        initialized = true;

        adjacency = new Dictionary<string, List<string>>();
        territoryToRegion = new Dictionary<string, string>();
        regionTerritories = new Dictionary<string, List<string>>();
        regionBonus = new Dictionary<string, int>();

        BuildAdjacency();
        BuildRegions();
    }

    static void BuildAdjacency()
    {
        AddEdge("London", "Dublin");
        AddEdge("London", "Amsterdam");
        AddEdge("London", "Paris");
        AddEdge("London", "Brussels");

        AddEdge("Dublin", "London");

        AddEdge("Paris", "London");
        AddEdge("Paris", "Brussels");
        AddEdge("Paris", "Amsterdam");
        AddEdge("Paris", "Lyon");
        AddEdge("Paris", "Geneva");
        AddEdge("Paris", "Madrid");

        AddEdge("Lyon", "Paris");
        AddEdge("Lyon", "Geneva");
        AddEdge("Lyon", "Florence");
        AddEdge("Lyon", "Venice");
        AddEdge("Lyon", "Rome");
        AddEdge("Lyon", "Madrid");

        AddEdge("Madrid", "Lisbon");
        AddEdge("Madrid", "Paris");
        AddEdge("Madrid", "Lyon");
        AddEdge("Madrid", "Toledo");

        AddEdge("Lisbon", "Madrid");
        AddEdge("Lisbon", "Toledo");

        AddEdge("Toledo", "Madrid");
        AddEdge("Toledo", "Lisbon");

        AddEdge("Brussels", "Paris");
        AddEdge("Brussels", "Amsterdam");
        AddEdge("Brussels", "London");

        AddEdge("Amsterdam", "London");
        AddEdge("Amsterdam", "Brussels");
        AddEdge("Amsterdam", "Paris");

        AddEdge("Geneva", "Lyon");
        AddEdge("Geneva", "Paris");
        AddEdge("Geneva", "Venice");
        AddEdge("Geneva", "Florence");
        AddEdge("Geneva", "Vienna");

        AddEdge("Rome", "Florence");
        AddEdge("Rome", "Venice");
        AddEdge("Rome", "Valletta");
        AddEdge("Rome", "Lyon");

        AddEdge("Florence", "Rome");
        AddEdge("Florence", "Venice");
        AddEdge("Florence", "Geneva");
        AddEdge("Florence", "Lyon");

        AddEdge("Venice", "Rome");
        AddEdge("Venice", "Florence");
        AddEdge("Venice", "Vienna");
        AddEdge("Venice", "Lyon");
        AddEdge("Venice", "Geneva");
        AddEdge("Venice", "Dubrovnik");

        AddEdge("Valletta", "Rome");
        AddEdge("Valletta", "Constantinople");

        AddEdge("Vienna", "Prague");
        AddEdge("Vienna", "Venice");
        AddEdge("Vienna", "Constantinople");
        AddEdge("Vienna", "Krakow");
        AddEdge("Vienna", "Geneva");

        AddEdge("Prague", "Vienna");
        AddEdge("Prague", "Krakow");
        AddEdge("Prague", "Venice");

        AddEdge("Krakow", "Vienna");
        AddEdge("Krakow", "Prague");
        AddEdge("Krakow", "Riga");

        AddEdge("Constantinople", "Vienna");
        AddEdge("Constantinople", "Dubrovnik");
        AddEdge("Constantinople", "Valletta");
        AddEdge("Constantinople", "Athens");
        AddEdge("Constantinople", "Bucharest");

        AddEdge("Dubrovnik", "Venice");
        AddEdge("Dubrovnik", "Constantinople");
        AddEdge("Dubrovnik", "Rome");
        AddEdge("Dubrovnik", "Vienna");

        AddEdge("Riga", "Krakow");
        AddEdge("Riga", "Warsaw");
        AddEdge("Riga", "Moscow");
        AddEdge("Riga", "Kyiv");
        AddEdge("Riga", "Stockholm");

        // New territories
        AddEdge("Berlin", "Hamburg");
        AddEdge("Berlin", "Munich");
        AddEdge("Berlin", "Prague");
        AddEdge("Berlin", "Warsaw");
        AddEdge("Berlin", "Copenhagen");

        AddEdge("Munich", "Berlin");
        AddEdge("Munich", "Vienna");
        AddEdge("Munich", "Prague");
        AddEdge("Munich", "Venice");
        AddEdge("Munich", "Geneva");

        AddEdge("Hamburg", "Berlin");
        AddEdge("Hamburg", "Copenhagen");
        AddEdge("Hamburg", "Amsterdam");

        AddEdge("Stockholm", "Copenhagen");
        AddEdge("Stockholm", "Riga");

        AddEdge("Copenhagen", "Hamburg");
        AddEdge("Copenhagen", "Stockholm");
        AddEdge("Copenhagen", "Berlin");

        AddEdge("Warsaw", "Berlin");
        AddEdge("Warsaw", "Krakow");
        AddEdge("Warsaw", "Kyiv");
        AddEdge("Warsaw", "Moscow");
        AddEdge("Warsaw", "Riga");

        AddEdge("Moscow", "Warsaw");
        AddEdge("Moscow", "Kyiv");
        AddEdge("Moscow", "Riga");

        AddEdge("Kyiv", "Warsaw");
        AddEdge("Kyiv", "Moscow");
        AddEdge("Kyiv", "Bucharest");
        AddEdge("Kyiv", "Riga");

        AddEdge("Budapest", "Vienna");
        AddEdge("Budapest", "Belgrade");
        AddEdge("Budapest", "Bucharest");
        AddEdge("Budapest", "Krakow");

        AddEdge("Belgrade", "Budapest");
        AddEdge("Belgrade", "Bucharest");
        AddEdge("Belgrade", "Constantinople");
        AddEdge("Belgrade", "Dubrovnik");

        AddEdge("Athens", "Constantinople");
        AddEdge("Athens", "Valletta");

        AddEdge("Naples", "Rome");
        AddEdge("Naples", "Valletta");
        AddEdge("Naples", "Florence");

        AddEdge("Marseille", "Lyon");
        AddEdge("Marseille", "Madrid");
        AddEdge("Marseille", "Geneva");

        AddEdge("Bucharest", "Belgrade");
        AddEdge("Bucharest", "Constantinople");
        AddEdge("Bucharest", "Kyiv");
        AddEdge("Bucharest", "Budapest");

        // Seville
        AddEdge("Seville", "Madrid");
        AddEdge("Seville", "Lisbon");
        AddEdge("Seville", "Toledo");
        AddEdge("Seville", "Algiers");

        // Algiers
        AddEdge("Algiers", "Seville");
        AddEdge("Algiers", "Tunis");
        AddEdge("Algiers", "Marseille");

        // Tunis
        AddEdge("Tunis", "Algiers");
        AddEdge("Tunis", "Naples");
        AddEdge("Tunis", "Valletta");

        // Ankara
        AddEdge("Ankara", "Constantinople");
        AddEdge("Ankara", "Smyrna");
        AddEdge("Ankara", "Trebizond");

        // Smyrna
        AddEdge("Smyrna", "Constantinople");
        AddEdge("Smyrna", "Ankara");
        AddEdge("Smyrna", "Athens");

        // Trebizond
        AddEdge("Trebizond", "Ankara");
        AddEdge("Trebizond", "Tbilisi");

        // Tbilisi
        AddEdge("Tbilisi", "Trebizond");
        AddEdge("Tbilisi", "Baku");
        AddEdge("Tbilisi", "Kyiv");

        // Baku
        AddEdge("Baku", "Tbilisi");

        // Beirut
        AddEdge("Beirut", "Constantinople");
        AddEdge("Beirut", "Alexandria");

        // Alexandria
        AddEdge("Alexandria", "Beirut");
        AddEdge("Alexandria", "Tunis");

        // Tripoli
        AddEdge("Tripoli", "Tunis");
        AddEdge("Tripoli", "Alexandria");
        AddEdge("Tripoli", "Valletta");

        // Add cross-connections from new territories to existing:
        AddEdge("Constantinople", "Ankara");
        AddEdge("Constantinople", "Smyrna");
        AddEdge("Constantinople", "Beirut");
        AddEdge("Athens", "Smyrna");
        AddEdge("Naples", "Tunis");
        AddEdge("Marseille", "Algiers");
        AddEdge("Tunis", "Alexandria");
        AddEdge("Tunis", "Tripoli");
    }

    static void AddEdge(string a, string b)
    {
        if (!adjacency.ContainsKey(a)) adjacency[a] = new List<string>();
        if (!adjacency.ContainsKey(b)) adjacency[b] = new List<string>();
        if (!adjacency[a].Contains(b)) adjacency[a].Add(b);
        if (!adjacency[b].Contains(a)) adjacency[b].Add(a);
    }

    static void BuildRegions()
    {
        AddRegion("British Isles", new List<string> { "London", "Dublin" }, 2);
        AddRegion("Iberia", new List<string> { "Madrid", "Lisbon", "Toledo", "Seville" }, 3);
        AddRegion("France", new List<string> { "Paris", "Lyon", "Brussels", "Amsterdam", "Marseille" }, 4);
        AddRegion("Italy", new List<string> { "Rome", "Venice", "Florence", "Valletta", "Naples", "Tunis" }, 4);
        AddRegion("Central Europe", new List<string> { "Vienna", "Prague", "Geneva", "Krakow", "Munich", "Budapest" }, 4);
        AddRegion("German States", new List<string> { "Berlin", "Hamburg" }, 2);
        AddRegion("Scandinavia", new List<string> { "Stockholm", "Copenhagen" }, 2);
        AddRegion("Eastern Europe", new List<string> { "Warsaw", "Moscow", "Kyiv", "Riga", "Tbilisi", "Baku" }, 3);
        AddRegion("Balkans", new List<string> { "Constantinople", "Dubrovnik", "Belgrade", "Athens", "Bucharest", "Ankara", "Smyrna", "Trebizond" }, 3);
        AddRegion("North Africa", new List<string> { "Algiers", "Tripoli" }, 2);
        AddRegion("Near East", new List<string> { "Beirut", "Alexandria" }, 1);
    }

    static void AddRegion(string name, List<string> territories, int bonus)
    {
        regionTerritories[name] = territories;
        regionBonus[name] = bonus;
        foreach (string t in territories)
            territoryToRegion[t] = name;
    }

    public static List<string> GetAdjacentTerritories(string territoryName)
    {
        Initialize();
        if (adjacency.ContainsKey(territoryName))
            return new List<string>(adjacency[territoryName]);
        return new List<string>();
    }

    /// <summary>Checks if two territories share a border.</summary>
    public static bool AreAdjacent(string a, string b)
    {
        Initialize();
        return adjacency.ContainsKey(a) && adjacency[a].Contains(b);
    }

    /// <summary>Gets the region name a territory belongs to.</summary>
    public static string GetRegion(string territoryName)
    {
        Initialize();
        if (territoryToRegion.ContainsKey(territoryName))
            return territoryToRegion[territoryName];
        return null;
    }

    /// <summary>Gets the bonus troops awarded for controlling an entire region.</summary>
    public static int GetRegionBonus(string regionName)
    {
        Initialize();
        if (regionBonus.ContainsKey(regionName))
            return regionBonus[regionName];
        return 0;
    }

    /// <summary>Gets the list of territory names that belong to a region.</summary>
    public static List<string> GetRegionTerritories(string regionName)
    {
        Initialize();
        if (regionTerritories.ContainsKey(regionName))
            return new List<string>(regionTerritories[regionName]);
        return new List<string>();
    }

    /// <summary>Gets all defined region names.</summary>
    public static List<string> GetAllRegionNames()
    {
        Initialize();
        return new List<string>(regionTerritories.Keys);
    }

    /// <summary>Returns all territory names in the graph.</summary>
    public static List<string> GetAllTerritoryNames()
    {
        Initialize();
        return new List<string>(adjacency.Keys);
    }

    /// <summary>Calculates reinforcement troops for a faction based on owned territories and region bonuses.</summary>
    public static int CalculateReinforcements(List<TerritoryData> ownedTerritories)
    {
        Initialize();
        int territoryCount = ownedTerritories.Count;
        int baseTroops = Math.Max(3, territoryCount / 3);

        Dictionary<string, int> regionOwned = new Dictionary<string, int>();
        foreach (TerritoryData t in ownedTerritories)
        {
            string region = GetRegion(t.name);
            if (region != null)
            {
                if (!regionOwned.ContainsKey(region))
                    regionOwned[region] = 0;
                regionOwned[region]++;
            }
        }

        int bonusTroops = 0;
        foreach (string region in regionTerritories.Keys)
        {
            int needed = regionTerritories[region].Count;
            if (regionOwned.ContainsKey(region) && regionOwned[region] >= needed)
            {
                bonusTroops += regionBonus[region];
            }
        }

        return baseTroops + bonusTroops;
    }

    /// <summary>Checks if a faction owns every territory in the specified region.</summary>
    public static bool OwnsEntireRegion(string regionName, List<TerritoryData> ownedTerritories)
    {
        Initialize();
        if (!regionTerritories.ContainsKey(regionName))
            return false;
        List<string> needed = regionTerritories[regionName];
        int count = 0;
        foreach (TerritoryData t in ownedTerritories)
            if (needed.Contains(t.name))
                count++;
        return count >= needed.Count;
    }
}
