using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TerritoryData
{
    public string name;
    public int controlledBy = -1;
    public int farmingValue;
    public int strategicValue;
    public bool isPort;
    public string historicalNote;
    public Vector2 mapPosition;
    public int troops = 0;
    public int farmingBoostTurns = 0;
    public List<string> adjacentTerritories = new List<string>();

    public TerritoryData(string name, int farmingValue, int strategicValue, bool isPort, string historicalNote, int controlledBy = -1)
        : this(name, farmingValue, strategicValue, isPort, historicalNote, Vector2.zero, controlledBy)
    {
    }

    public TerritoryData(string name, int farmingValue, int strategicValue, bool isPort, string historicalNote, Vector2 mapPosition, int controlledBy = -1)
    {
        this.name = name;
        this.farmingValue = farmingValue;
        this.strategicValue = strategicValue;
        this.isPort = isPort;
        this.historicalNote = historicalNote;
        this.mapPosition = mapPosition;
        this.controlledBy = controlledBy;
        this.troops = controlledBy >= 0 ? 5 : 0;
        this.farmingBoostTurns = 0;
    }

    /// <summary>Adds troops to this territory, clamped to minimum 0.</summary>
    public void AddTroops(int count)
    {
        troops = Mathf.Max(0, troops + count);
    }

    /// <summary>Removes troops from this territory, clamped to minimum 0.</summary>
    public void RemoveTroops(int count)
    {
        troops = Mathf.Max(0, troops - count);
    }
}

public static class TerritoryDatabase
{
    private static List<TerritoryData> territories = null;

    public static void Reset()
    {
        territories = null;
    }

    public static List<TerritoryData> GetAllTerritories()
    {
        if (territories == null)
        {
            territories = new List<TerritoryData>();
            CreateTerritories();
        }
        return territories;
    }

    public static List<TerritoryData> GetTerritoriesOwnedBy(int factionId)
    {
        return GetAllTerritories().FindAll(t => t.controlledBy == factionId);
    }

    static void AddAdjacent(TerritoryData t, params string[] neighbors)
    {
        t.adjacentTerritories.AddRange(neighbors);
    }

    static void CreateTerritories()
    {
        territories = new List<TerritoryData>();

        TerritoryData t;

        t = new TerritoryData("Rome", 3, 5, false, "Rome: Capital of unified Italy after 1870. Heart of Papal power until the Lateran Treaty of 1929.");
        AddAdjacent(t, "Florence", "Venice", "Valletta", "Lyon");
        territories.Add(t);

        t = new TerritoryData("Venice", 2, 4, true, "Venice: Fell to Napoleon in 1797, ending the Venetian Republic. Later part of Austrian and then unified Italy.");
        AddAdjacent(t, "Rome", "Florence", "Vienna", "Lyon", "Geneva", "Dubrovnik");
        territories.Add(t);

        t = new TerritoryData("Paris", 4, 5, false, "Paris: Epicenter of revolution — 1789, 1830, 1848, 1871. Commune, empire, and republic by turns.");
        AddAdjacent(t, "London", "Brussels", "Amsterdam", "Lyon", "Geneva", "Madrid");
        territories.Add(t);

        t = new TerritoryData("London", 3, 4, true, "London: Capital of the British Empire at its zenith. Center of global finance throughout the 19th century.");
        AddAdjacent(t, "Dublin", "Amsterdam", "Paris", "Brussels");
        territories.Add(t);

        t = new TerritoryData("Constantinople", 3, 5, true, "Constantinople: Ottoman capital until 1922. Renamed Istanbul under the Turkish Republic.");
        AddAdjacent(t, "Vienna", "Dubrovnik", "Valletta", "Athens", "Bucharest");
        territories.Add(t);

        t = new TerritoryData("Vienna", 4, 4, false, "Vienna: Capital of the Austro-Hungarian Empire. Congress of Vienna 1815 reshaped Europe.");
        AddAdjacent(t, "Prague", "Venice", "Constantinople", "Krakow", "Geneva");
        territories.Add(t);

        t = new TerritoryData("Prague", 3, 3, false, "Prague: Industrial and cultural hub of the Bohemian Crown. Center of Czech nationalism in the 19th century.");
        AddAdjacent(t, "Vienna", "Krakow", "Venice");
        territories.Add(t);

        t = new TerritoryData("Madrid", 3, 3, false, "Madrid: Capital of Spain through war, revolution, and civil war. Center of the Spanish Golden Age legacy.");
        AddAdjacent(t, "Lisbon", "Paris", "Lyon", "Toledo");
        territories.Add(t);

        t = new TerritoryData("Lisbon", 2, 3, true, "Lisbon: Capital of Portugal, neutral in WWII. Survived the 1755 earthquake to remain a global port.");
        AddAdjacent(t, "Madrid", "Toledo");
        territories.Add(t);

        t = new TerritoryData("Amsterdam", 3, 4, true, "Amsterdam: Global financial capital through the 19th century. Center of diamond trade and banking.");
        AddAdjacent(t, "London", "Brussels", "Paris");
        territories.Add(t);

        t = new TerritoryData("Brussels", 2, 2, false, "Brussels: Capital of Belgium after 1830 independence. Seat of European power in the modern era.");
        AddAdjacent(t, "Paris", "Amsterdam", "London");
        territories.Add(t);

        t = new TerritoryData("Geneva", 1, 2, false, "Geneva: Refuge for revolutionaries and thinkers. Host of the International Red Cross founded 1863.");
        AddAdjacent(t, "Lyon", "Paris", "Venice", "Florence", "Vienna");
        territories.Add(t);

        t = new TerritoryData("Florence", 2, 3, false, "Florence: Briefly capital of Italy 1865–1871. Center of art and culture through the 19th century.");
        AddAdjacent(t, "Rome", "Venice", "Geneva", "Lyon");
        territories.Add(t);

        t = new TerritoryData("Toledo", 2, 3, false, "Toledo: Ancient capital of Spain, center of the Spanish Civil War's iconic siege at the Alcázar.");
        AddAdjacent(t, "Madrid", "Lisbon");
        territories.Add(t);

        t = new TerritoryData("Krakow", 3, 3, false, "Krakow: Polish cultural capital under Austrian partition. Center of 19th-century Polish nationalism.");
        AddAdjacent(t, "Vienna", "Prague", "Riga");
        territories.Add(t);

        t = new TerritoryData("Riga", 2, 2, true, "Riga: Major Baltic port of the Russian Empire. Center of Latvian national awakening in the 1800s.");
        AddAdjacent(t, "Krakow", "Warsaw", "Moscow", "Kyiv", "Stockholm");
        territories.Add(t);

        t = new TerritoryData("Dubrovnik", 1, 3, true, "Dubrovnik: The Ragusan Republic ended in 1808 under Napoleon. A Habsburg port thereafter.");
        AddAdjacent(t, "Venice", "Constantinople", "Rome", "Vienna");
        territories.Add(t);

        t = new TerritoryData("Valletta", 1, 4, true, "Valletta: Fell to Napoleon in 1798 then to Britain. Strategic Mediterranean naval base for a century.");
        AddAdjacent(t, "Rome", "Constantinople");
        territories.Add(t);

        t = new TerritoryData("Dublin", 2, 1, true, "Dublin: Heart of Irish nationalism and the 1916 Easter Rising. Capital of the Irish Free State from 1922.");
        AddAdjacent(t, "London");
        territories.Add(t);

        t = new TerritoryData("Lyon", 3, 2, false, "Lyon: Silk capital of Europe, center of the French Resistance during WWII.");
        AddAdjacent(t, "Paris", "Geneva", "Florence", "Venice", "Rome", "Madrid");
        territories.Add(t);

        t = new TerritoryData("Berlin", 3, 4, false, "Berlin: Capital of Prussia and later unified Germany. Center of the Industrial Revolution in Central Europe.");
        AddAdjacent(t, "Hamburg", "Munich", "Prague", "Warsaw", "Copenhagen");
        territories.Add(t);

        t = new TerritoryData("Munich", 2, 2, false, "Munich: Bavarian capital, center of the 1918 revolution and the Nazi movement's birthplace.");
        AddAdjacent(t, "Berlin", "Vienna", "Prague", "Venice", "Geneva");
        territories.Add(t);

        t = new TerritoryData("Hamburg", 3, 3, true, "Hamburg: Major Hanseatic port, gateway to the Atlantic for Central Europe. Devastated in WWII.");
        AddAdjacent(t, "Berlin", "Copenhagen", "Amsterdam");
        territories.Add(t);

        t = new TerritoryData("Stockholm", 2, 2, true, "Stockholm: Capital of Sweden, neutral in both world wars. Center of Scandinavian culture.");
        AddAdjacent(t, "Copenhagen", "Riga");
        territories.Add(t);

        t = new TerritoryData("Copenhagen", 2, 3, true, "Copenhagen: Danish capital, home of Tivoli Gardens. Fell to Prussia in 1864 losing Schleswig-Holstein.");
        AddAdjacent(t, "Hamburg", "Stockholm", "Berlin");
        territories.Add(t);

        t = new TerritoryData("Warsaw", 3, 3, false, "Warsaw: Polish capital, partitioned between Russia, Prussia, and Austria through the 19th century.");
        AddAdjacent(t, "Berlin", "Krakow", "Kyiv", "Moscow", "Riga");
        territories.Add(t);

        t = new TerritoryData("Moscow", 4, 5, false, "Moscow: Ancient capital of Russia, burned during Napoleon's 1812 invasion. Survived to become Soviet capital.");
        AddAdjacent(t, "Warsaw", "Kyiv", "Riga");
        territories.Add(t);

        t = new TerritoryData("Kyiv", 3, 3, false, "Kyiv: Heartland of the Russian Empire's grain wealth. Major railway hub by the late 19th century.");
        AddAdjacent(t, "Warsaw", "Moscow", "Bucharest", "Riga");
        territories.Add(t);

        t = new TerritoryData("Budapest", 3, 3, false, "Budapest: Twin city of Buda and Pest, capital of the Kingdom of Hungary within the Habsburg Empire.");
        AddAdjacent(t, "Vienna", "Belgrade", "Bucharest", "Krakow");
        territories.Add(t);

        t = new TerritoryData("Belgrade", 2, 3, false, "Belgrade: Strategic fortress city at the confluence of the Danube and Sava. Key to Balkan control.");
        AddAdjacent(t, "Budapest", "Bucharest", "Constantinople", "Dubrovnik");
        territories.Add(t);

        t = new TerritoryData("Athens", 2, 3, true, "Athens: Birthplace of democracy, became independent from the Ottoman Empire in 1830.");
        AddAdjacent(t, "Constantinople", "Valletta");
        territories.Add(t);

        t = new TerritoryData("Naples", 2, 3, true, "Naples: Capital of the Kingdom of the Two Sicilies until Garibaldi's unification of Italy in 1860.");
        AddAdjacent(t, "Rome", "Valletta", "Florence");
        territories.Add(t);

        t = new TerritoryData("Marseille", 3, 3, true, "Marseille: Major Mediterranean port, gateway to the French Empire's colonial holdings in North Africa.");
        AddAdjacent(t, "Lyon", "Madrid", "Geneva");
        territories.Add(t);

        t = new TerritoryData("Bucharest", 2, 2, false, "Bucharest: Capital of the Romanian Principalities, unified in 1859. Known as the Little Paris of the East.");
        AddAdjacent(t, "Belgrade", "Constantinople", "Kyiv", "Budapest");
        territories.Add(t);

        ApplyMapPositions();
    }

    static void ApplyMapPositions()
    {
        foreach (TerritoryData territory in territories)
        {
            territory.mapPosition = territory.name switch
            {
                "London" => new Vector2(0.3553f, 0.5392f),
                "Paris" => new Vector2(0.3907f, 0.6092f),
                "Madrid" => new Vector2(0.3043f, 0.8311f),
                "Rome" => new Vector2(0.5357f, 0.7921f),
                "Berlin" => new Vector2(0.5487f, 0.5126f),
                "Vienna" => new Vector2(0.5910f, 0.6261f),
                "Amsterdam" => new Vector2(0.4271f, 0.5166f),
                "Brussels" => new Vector2(0.4193f, 0.5566f),
                "Lisbon" => new Vector2(0.2266f, 0.8758f),
                "Dublin" => new Vector2(0.2679f, 0.4913f),
                "Toledo" => new Vector2(0.3170f, 0.8447f),
                "Venice" => new Vector2(0.5300f, 0.6618f),
                "Florence" => new Vector2(0.5214f, 0.7500f),
                "Valletta" => new Vector2(0.5644f, 0.9500f),
                "Geneva" => new Vector2(0.4564f, 0.6724f),
                "Krakow" => new Vector2(0.6420f, 0.5774f),
                "Dubrovnik" => new Vector2(0.6257f, 0.7711f),
                "Riga" => new Vector2(0.7016f, 0.3961f),
                "Stockholm" => new Vector2(0.6153f, 0.3334f),
                "Copenhagen" => new Vector2(0.5367f, 0.4295f),
                "Moscow" => new Vector2(0.8946f, 0.4276f),
                "Kyiv" => new Vector2(0.7931f, 0.5671f),
                "Warsaw" => new Vector2(0.6573f, 0.5203f),
                "Prague" => new Vector2(0.5634f, 0.5768f),
                "Budapest" => new Vector2(0.6291f, 0.6447f),
                "Bucharest" => new Vector2(0.7300f, 0.7255f),
                "Athens" => new Vector2(0.6961f, 0.8953f),
                "Constantinople" => new Vector2(0.7707f, 0.8155f),
                "Marseille" => new Vector2(0.4339f, 0.7553f),
                "Lyon" => new Vector2(0.4261f, 0.6908f),
                "Munich" => new Vector2(0.5479f, 0.6461f),
                "Hamburg" => new Vector2(0.5271f, 0.4724f),
                "Belgrade" => new Vector2(0.6494f, 0.7153f),
                "Naples" => new Vector2(0.5693f, 0.8408f),
                _ => Vector2.zero
            };
        }
    }
}
