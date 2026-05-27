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
    public int defensePenalty = 0;
    public int holyWarMarkedByFaction = -1;
    public int ambushAttackBonus = 0;
    public bool suppressAttackNextTurn = false;
    public bool skipReinforceNextTurn = false;
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

        t = new TerritoryData("Seville", 3, 2, true, "Seville: Major port for trade with the Americas. Center of Andalusian culture.");
        AddAdjacent(t, "Madrid", "Lisbon", "Toledo");
        territories.Add(t);

        t = new TerritoryData("Algiers", 2, 2, true, "Algiers: Center of the Barbary Coast, subject to French invasion in 1830.");
        AddAdjacent(t, "Marseille", "Tunis", "Seville");
        territories.Add(t);

        t = new TerritoryData("Tunis", 2, 3, true, "Tunis: Capital of the Tunisian Regency under Ottoman suzerainty until French protectorate in 1881.");
        AddAdjacent(t, "Naples", "Valletta", "Algiers");
        territories.Add(t);

        t = new TerritoryData("Ankara", 2, 3, false, "Ankara: Ottoman provincial capital, later capital of the Turkish Republic under Atatürk.");
        AddAdjacent(t, "Constantinople", "Smyrna", "Trebizond");
        territories.Add(t);

        t = new TerritoryData("Smyrna", 2, 3, true, "Smyrna: Major Aegean port of the Ottoman Empire, center of the 1919 Greco-Turkish war.");
        AddAdjacent(t, "Constantinople", "Ankara", "Athens");
        territories.Add(t);

        t = new TerritoryData("Trebizond", 2, 2, true, "Trebizond: Black Sea port, last Greek-speaking empire fell to the Ottomans in 1461.");
        AddAdjacent(t, "Ankara", "Tbilisi");
        territories.Add(t);

        t = new TerritoryData("Tbilisi", 2, 3, false, "Tbilisi: Capital of Georgia, contested between Russian and Ottoman empires in the 19th century.");
        AddAdjacent(t, "Trebizond", "Baku", "Kyiv");
        territories.Add(t);

        t = new TerritoryData("Baku", 1, 3, true, "Baku: Major oil port on the Caspian Sea, center of the 19th-century oil boom.");
        AddAdjacent(t, "Tbilisi");
        territories.Add(t);

        t = new TerritoryData("Beirut", 2, 3, true, "Beirut: Major port of the Ottoman Empire's Levant region, center of 19th-century silk trade.");
        AddAdjacent(t, "Constantinople", "Alexandria");
        territories.Add(t);

        t = new TerritoryData("Alexandria", 2, 2, true, "Alexandria: Egypt's Mediterranean port, strategic link between Europe and the Near East.");
        AddAdjacent(t, "Beirut", "Tunis");
        territories.Add(t);

        t = new TerritoryData("Tripoli", 2, 2, true, "Tripoli: Ottoman-controlled North African port, gateway to the Sahara.");
        AddAdjacent(t, "Tunis", "Alexandria", "Valletta");
        territories.Add(t);

        ApplyMapPositions();
    }

    static void ApplyMapPositions()
    {
        var positions = new Dictionary<string, Vector2>
        {
            // British Isles
            { "London",          MapProjection.GeoToUV(51.51f,  -0.13f) },
            { "England",         MapProjection.GeoToUV(51.51f,  -0.13f) },
            { "Edinburgh",       MapProjection.GeoToUV(55.95f,  -3.19f) },
            { "Scotland",        MapProjection.GeoToUV(55.95f,  -3.19f) },
            { "Dublin",          MapProjection.GeoToUV(53.33f,  -6.25f) },
            { "Ireland",         MapProjection.GeoToUV(53.33f,  -6.25f) },
            { "Wales",           MapProjection.GeoToUV(51.48f,  -3.18f) },
            // France
            { "Paris",           MapProjection.GeoToUV(48.85f,   2.35f) },
            { "France",          MapProjection.GeoToUV(48.85f,   2.35f) },
            { "Lyon",            MapProjection.GeoToUV(45.75f,   4.83f) },
            { "Bordeaux",        MapProjection.GeoToUV(44.84f,  -0.58f) },
            { "Aquitaine",       MapProjection.GeoToUV(44.84f,  -0.58f) },
            { "Marseille",       MapProjection.GeoToUV(43.30f,   5.37f) },
            { "Provence",        MapProjection.GeoToUV(43.30f,   5.37f) },
            { "Toulouse",        MapProjection.GeoToUV(43.60f,   1.44f) },
            { "Normandy",        MapProjection.GeoToUV(49.44f,   1.10f) },
            { "Brittany",        MapProjection.GeoToUV(48.11f,  -1.68f) },
            // Iberia
            { "Madrid",          MapProjection.GeoToUV(40.42f,  -3.70f) },
            { "Spain",           MapProjection.GeoToUV(40.42f,  -3.70f) },
            { "Toledo",          MapProjection.GeoToUV(39.86f,  -4.03f) },
            { "Lisbon",          MapProjection.GeoToUV(38.72f,  -9.14f) },
            { "Portugal",        MapProjection.GeoToUV(38.72f,  -9.14f) },
            { "Barcelona",       MapProjection.GeoToUV(41.39f,   2.15f) },
            { "Catalonia",       MapProjection.GeoToUV(41.39f,   2.15f) },
            { "Sevilla",         MapProjection.GeoToUV(37.38f,  -5.99f) },
            { "Seville",         MapProjection.GeoToUV(37.38f,  -5.99f) },
            { "Andalusia",       MapProjection.GeoToUV(37.38f,  -5.99f) },
            // Low Countries
            { "Amsterdam",       MapProjection.GeoToUV(52.37f,   4.90f) },
            { "Netherlands",     MapProjection.GeoToUV(52.37f,   4.90f) },
            { "Brussels",        MapProjection.GeoToUV(50.85f,   4.35f) },
            { "Belgium",         MapProjection.GeoToUV(50.85f,   4.35f) },
            { "Geneva",          MapProjection.GeoToUV(46.20f,   6.14f) },
            { "Switzerland",     MapProjection.GeoToUV(46.20f,   6.14f) },
            // German States
            { "Berlin",          MapProjection.GeoToUV(52.52f,  13.41f) },
            { "Prussia",         MapProjection.GeoToUV(52.52f,  13.41f) },
            { "Munich",          MapProjection.GeoToUV(48.14f,  11.58f) },
            { "Bavaria",         MapProjection.GeoToUV(48.14f,  11.58f) },
            { "Hamburg",         MapProjection.GeoToUV(53.55f,   9.99f) },
            { "Hanover",         MapProjection.GeoToUV(53.55f,   9.99f) },
            { "Dresden",         MapProjection.GeoToUV(51.05f,  13.74f) },
            { "Saxony",          MapProjection.GeoToUV(51.05f,  13.74f) },
            { "Westphalia",      MapProjection.GeoToUV(51.51f,   7.47f) },
            { "Baden",           MapProjection.GeoToUV(48.99f,   8.40f) },
            { "Rhineland",       MapProjection.GeoToUV(50.94f,   6.96f) },
            // Italian States
            { "Rome",            MapProjection.GeoToUV(41.90f,  12.50f) },
            { "Papal States",    MapProjection.GeoToUV(41.90f,  12.50f) },
            { "Venice",          MapProjection.GeoToUV(45.44f,  12.34f) },
            { "Venetia",         MapProjection.GeoToUV(45.44f,  12.34f) },
            { "Florence",        MapProjection.GeoToUV(43.77f,  11.26f) },
            { "Tuscany",         MapProjection.GeoToUV(43.77f,  11.26f) },
            { "Naples",          MapProjection.GeoToUV(40.85f,  14.27f) },
            { "SouthItaly",      MapProjection.GeoToUV(40.85f,  14.27f) },
            { "Piedmont",        MapProjection.GeoToUV(45.07f,   7.68f) },
            { "Lombardy",        MapProjection.GeoToUV(45.46f,   9.19f) },
            { "Milan",           MapProjection.GeoToUV(45.46f,   9.19f) },
            { "Palermo",         MapProjection.GeoToUV(37.50f,  14.00f) },
            { "Sicily",          MapProjection.GeoToUV(37.50f,  14.00f) },
            { "Valletta",        MapProjection.GeoToUV(35.90f,  14.51f) },
            // Central Europe
            { "Vienna",          MapProjection.GeoToUV(48.21f,  16.37f) },
            { "Austria",         MapProjection.GeoToUV(48.21f,  16.37f) },
            { "Prague",          MapProjection.GeoToUV(50.08f,  14.44f) },
            { "Bohemia",         MapProjection.GeoToUV(50.08f,  14.44f) },
            { "Warsaw",          MapProjection.GeoToUV(52.23f,  21.01f) },
            { "Poland",          MapProjection.GeoToUV(52.23f,  21.01f) },
            { "Krakow",          MapProjection.GeoToUV(50.06f,  19.94f) },
            { "Galicia",         MapProjection.GeoToUV(50.06f,  19.94f) },
            { "Budapest",        MapProjection.GeoToUV(47.50f,  19.04f) },
            { "Hungary",         MapProjection.GeoToUV(47.50f,  19.04f) },
            { "Zagreb",          MapProjection.GeoToUV(45.81f,  15.98f) },
            { "Croatia",         MapProjection.GeoToUV(45.81f,  15.98f) },
            // Balkans
            { "Belgrade",        MapProjection.GeoToUV(44.82f,  20.46f) },
            { "Serbia",          MapProjection.GeoToUV(44.82f,  20.46f) },
            { "Dubrovnik",       MapProjection.GeoToUV(42.65f,  18.09f) },
            { "Dalmatia",        MapProjection.GeoToUV(42.65f,  18.09f) },
            { "Bucharest",       MapProjection.GeoToUV(44.43f,  26.10f) },
            { "Romania",         MapProjection.GeoToUV(44.43f,  26.10f) },
            { "Wallachia",       MapProjection.GeoToUV(44.43f,  26.10f) },
            { "Sofia",           MapProjection.GeoToUV(42.70f,  23.32f) },
            { "Bulgaria",        MapProjection.GeoToUV(42.70f,  23.32f) },
            { "Athens",          MapProjection.GeoToUV(37.98f,  23.73f) },
            { "Greece",          MapProjection.GeoToUV(37.98f,  23.73f) },
            { "Thessaloniki",    MapProjection.GeoToUV(40.64f,  22.94f) },
            { "Bosnia",          MapProjection.GeoToUV(43.84f,  18.36f) },
            { "Sarajevo",        MapProjection.GeoToUV(43.84f,  18.36f) },
            { "Albania",         MapProjection.GeoToUV(41.33f,  19.83f) },
            { "Moldavia",        MapProjection.GeoToUV(47.00f,  28.83f) },
            // Ottoman
            { "Constantinople",  MapProjection.GeoToUV(41.01f,  28.95f) },
            { "Ottoman",         MapProjection.GeoToUV(41.01f,  28.95f) },
            { "Anatolia",        MapProjection.GeoToUV(39.93f,  32.86f) },
            { "Ankara",          MapProjection.GeoToUV(39.93f,  32.86f) },
            { "Ionia",           MapProjection.GeoToUV(38.42f,  27.14f) },
            { "Smyrna",          MapProjection.GeoToUV(38.42f,  27.14f) },
            { "Pontus",          MapProjection.GeoToUV(41.29f,  36.33f) },
            { "Trebizond",       MapProjection.GeoToUV(41.00f,  39.72f) },
            // Russia
            { "Moscow",          MapProjection.GeoToUV(55.75f,  37.62f) },
            { "Russia",          MapProjection.GeoToUV(55.75f,  37.62f) },
            { "St. Petersburg",  MapProjection.GeoToUV(59.95f,  30.32f) },
            { "Kyiv",            MapProjection.GeoToUV(50.45f,  30.52f) },
            { "Ukraine",         MapProjection.GeoToUV(50.45f,  30.52f) },
            { "Odessa",          MapProjection.GeoToUV(46.47f,  30.73f) },
            { "Riga",            MapProjection.GeoToUV(56.95f,  24.11f) },
            { "Baltic",          MapProjection.GeoToUV(56.95f,  24.11f) },
            { "Vilnius",         MapProjection.GeoToUV(54.69f,  25.28f) },
            { "Lithuania",       MapProjection.GeoToUV(54.69f,  25.28f) },
            { "Minsk",           MapProjection.GeoToUV(53.90f,  27.57f) },
            { "Crimea",          MapProjection.GeoToUV(45.03f,  33.99f) },
            { "Don",             MapProjection.GeoToUV(47.23f,  39.72f) },
            { "Volga",           MapProjection.GeoToUV(53.20f,  50.15f) },
            // Scandinavia
            { "Stockholm",       MapProjection.GeoToUV(59.33f,  18.07f) },
            { "Sweden",          MapProjection.GeoToUV(59.33f,  18.07f) },
            { "Scandinavia",     MapProjection.GeoToUV(59.33f,  18.07f) },
            { "Copenhagen",      MapProjection.GeoToUV(55.68f,  12.57f) },
            { "Denmark",         MapProjection.GeoToUV(55.68f,  12.57f) },
            { "Oslo",            MapProjection.GeoToUV(59.91f,  10.75f) },
            { "Norway",          MapProjection.GeoToUV(59.91f,  10.75f) },
            { "Helsinki",        MapProjection.GeoToUV(60.17f,  24.94f) },
            { "Finland",         MapProjection.GeoToUV(60.17f,  24.94f) },
            // Caucasus
            { "Tbilisi",         MapProjection.GeoToUV(41.69f,  44.83f) },
            { "Georgia",         MapProjection.GeoToUV(41.69f,  44.83f) },
            { "Baku",            MapProjection.GeoToUV(40.41f,  49.87f) },
            { "Azerbaijan",      MapProjection.GeoToUV(40.41f,  49.87f) },
            { "Yerevan",         MapProjection.GeoToUV(40.18f,  44.50f) },
            { "Armenia",         MapProjection.GeoToUV(40.18f,  44.50f) },
            // Near East
            { "Damascus",        MapProjection.GeoToUV(33.51f,  36.29f) },
            { "Syria",           MapProjection.GeoToUV(33.51f,  36.29f) },
            { "Beirut",          MapProjection.GeoToUV(33.89f,  35.50f) },
            { "Levant",          MapProjection.GeoToUV(33.89f,  35.50f) },
            { "Baghdad",         MapProjection.GeoToUV(33.34f,  44.40f) },
            { "Mesopotamia",     MapProjection.GeoToUV(33.34f,  44.40f) },
            // North Africa
            { "Cairo",           MapProjection.GeoToUV(30.06f,  31.25f) },
            { "Egypt",           MapProjection.GeoToUV(30.06f,  31.25f) },
            { "Alexandria",      MapProjection.GeoToUV(31.20f,  29.92f) },
            { "Algiers",         MapProjection.GeoToUV(36.74f,   3.06f) },
            { "Maghreb",         MapProjection.GeoToUV(34.85f,   1.65f) },
            { "Tunis",           MapProjection.GeoToUV(36.82f,  10.18f) },
            { "Tunisia",         MapProjection.GeoToUV(36.00f,   9.37f) },
            { "Tripoli",         MapProjection.GeoToUV(32.90f,  13.18f) },
            { "Libya",           MapProjection.GeoToUV(32.90f,  13.18f) },
            { "Fez",             MapProjection.GeoToUV(34.04f,  -5.00f) },
            { "Morocco",         MapProjection.GeoToUV(33.99f,  -5.00f) },
        };

        foreach (TerritoryData territory in territories)
        {
            if (positions.TryGetValue(territory.name, out Vector2 pos))
                territory.mapPosition = pos;
            else
            {
                Debug.LogWarning($"[TerritoryData] No position for '{territory.name}' — defaulting to center. Add it to ApplyMapPositions().");
                territory.mapPosition = new Vector2(0.5f, 0.5f);
            }
        }
    }
}
