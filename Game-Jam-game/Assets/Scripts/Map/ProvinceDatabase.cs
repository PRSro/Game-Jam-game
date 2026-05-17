using System.Collections.Generic;
using UnityEngine;

public static class ProvinceDatabase
{
    private static List<ProvinceData> provinces = null;

    /// <summary>Returns all provinces in the database.</summary>
    public static List<ProvinceData> GetAllProvinces()
    {
        if (provinces == null) BuildProvinces();
        return provinces;
    }

    /// <summary>Finds a province by name.</summary>
    public static ProvinceData GetProvince(string name)
    {
        if (provinces == null) BuildProvinces();
        return provinces.Find(p => p.provinceName == name);
    }

    /// <summary>Returns all provinces controlled by the given faction.</summary>
    public static List<ProvinceData> GetProvincesOwnedBy(int factionId)
    {
        if (provinces == null) BuildProvinces();
        return provinces.FindAll(p => p.controllingFactionId == factionId);
    }

    /// <summary>Returns all provinces in a given historical region.</summary>
    public static List<ProvinceData> GetProvincesInRegion(string region)
    {
        if (provinces == null) BuildProvinces();
        return provinces.FindAll(p => p.historicalRegion == region);
    }

    static void BuildProvinces()
    {
        provinces = new List<ProvinceData>();

        // Province UV coords: U = (lon+25)/70  |  V = 1-(lat-34)/38  (V=0 top, V=1 bottom)

        // British Isles
        AddProvince("England",       "British Isles",   new Vector2(0.3553f, 0.5392f)); // London     51.51,  -0.13
        AddProvince("Scotland",      "British Isles",   new Vector2(0.3116f, 0.4224f)); // Edinburgh  55.95,  -3.19
        AddProvince("Ireland",       "British Isles",   new Vector2(0.2679f, 0.4913f)); // Dublin     53.33,  -6.25
        AddProvince("Wales",         "British Isles",   new Vector2(0.3386f, 0.5539f)); // Cardiff    51.48,  -3.18  FIXED

        // France
        AddProvince("France",        "France",          new Vector2(0.3907f, 0.6092f)); // Paris      48.85,   2.35
        AddProvince("Brittany",      "France",          new Vector2(0.3357f, 0.6329f)); // Rennes     48.11,  -1.68  FIXED
        AddProvince("Normandy",      "France",          new Vector2(0.3671f, 0.5987f)); // Rouen      49.44,   1.10  FIXED
        AddProvince("Aquitaine",     "France",          new Vector2(0.3489f, 0.7145f)); // Bordeaux   44.84,  -0.58  FIXED
        AddProvince("Provence",      "France",          new Vector2(0.4339f, 0.7553f)); // Marseille  43.30,   5.37

        // Iberia
        AddProvince("Spain",         "Iberia",          new Vector2(0.3043f, 0.8311f)); // Madrid     40.42,  -3.70
        AddProvince("Portugal",      "Iberia",          new Vector2(0.2266f, 0.8758f)); // Lisbon     38.72,  -9.14
        AddProvince("Catalonia",     "Iberia",          new Vector2(0.3879f, 0.8055f)); // Barcelona  41.39,   2.15

        // Low Countries
        AddProvince("Netherlands",   "Low Countries",   new Vector2(0.4271f, 0.5166f)); // Amsterdam  52.37,   4.90
        AddProvince("Belgium",       "Low Countries",   new Vector2(0.4193f, 0.5566f)); // Brussels   50.85,   4.35

        // German States
        AddProvince("Prussia",       "German States",   new Vector2(0.5487f, 0.5126f)); // Berlin     52.52,  13.41
        AddProvince("Bavaria",       "German States",   new Vector2(0.5479f, 0.6461f)); // Munich     48.14,  11.58  FIXED
        AddProvince("Saxony",        "German States",   new Vector2(0.5607f, 0.5776f)); // Dresden    51.05,  13.74  FIXED
        AddProvince("Hanover",       "German States",   new Vector2(0.5271f, 0.5197f)); // Hanover    52.37,   9.73  FIXED
        AddProvince("Baden",         "German States",   new Vector2(0.5050f, 0.6197f)); // Stuttgart  48.78,   9.18  FIXED
        AddProvince("Westphalia",    "German States",   new Vector2(0.4907f, 0.5224f)); // Dortmund   51.51,   7.47  FIXED

        // Italian States
        AddProvince("Piedmont",      "Italian States",  new Vector2(0.4479f, 0.6803f)); // Turin      45.07,   7.69  FIXED
        AddProvince("Lombardy",      "Italian States",  new Vector2(0.4807f, 0.6671f)); // Milan      45.46,   9.19  FIXED
        AddProvince("Venice",        "Italian States",  new Vector2(0.5300f, 0.6618f)); // Venice     45.44,  12.34  FIXED
        AddProvince("Tuscany",       "Italian States",  new Vector2(0.5214f, 0.7500f)); // Florence   43.77,  11.26  FIXED
        AddProvince("Papal States",  "Italian States",  new Vector2(0.5357f, 0.7921f)); // Rome       41.90,  12.50
        AddProvince("Naples",        "Italian States",  new Vector2(0.5693f, 0.8408f)); // Naples     40.85,  14.27  FIXED
        AddProvince("Sicily",        "Italian States",  new Vector2(0.5543f, 0.8987f)); // Palermo    38.12,  13.36  FIXED

        // Switzerland
        AddProvince("Switzerland",   "Switzerland",     new Vector2(0.4636f, 0.6592f)); // Bern       46.95,   7.45

        // Austrian Empire
        AddProvince("Austria",       "Austrian Empire", new Vector2(0.5910f, 0.6261f)); // Vienna     48.21,  16.37
        AddProvince("Bohemia",       "Austrian Empire", new Vector2(0.5634f, 0.5768f)); // Prague     50.08,  14.44
        AddProvince("Hungary",       "Austrian Empire", new Vector2(0.6291f, 0.6447f)); // Budapest   47.50,  19.04

        // Poland
        AddProvince("Poland",        "Poland",          new Vector2(0.6573f, 0.5203f)); // Warsaw     52.23,  21.01

        // Baltic
        AddProvince("Baltic",        "Russia",          new Vector2(0.7016f, 0.3961f)); // Riga       56.95,  24.11

        // Scandinavia
        AddProvince("Sweden",        "Scandinavia",     new Vector2(0.6153f, 0.3334f)); // Stockholm  59.33,  18.07
        AddProvince("Norway",        "Scandinavia",     new Vector2(0.5107f, 0.3182f)); // Oslo       59.91,  10.75
        AddProvince("Denmark",       "Scandinavia",     new Vector2(0.5367f, 0.4295f)); // Copenhagen 55.68,  12.57

        // Russia
        AddProvince("Russia",        "Russia",          new Vector2(0.8946f, 0.4276f)); // Moscow     55.75,  37.62
        AddProvince("Ukraine",       "Russia",          new Vector2(0.7931f, 0.5671f)); // Kyiv       50.45,  30.52

        // Balkans
        AddProvince("Ottoman Empire","Balkans",         new Vector2(0.7707f, 0.8155f)); // Istanbul   41.01,  28.95
        AddProvince("Serbia",        "Balkans",         new Vector2(0.6494f, 0.7153f)); // Belgrade   44.82,  20.46
        AddProvince("Greece",        "Balkans",         new Vector2(0.6961f, 0.8953f)); // Athens     37.98,  23.73
        AddProvince("Wallachia",     "Balkans",         new Vector2(0.7300f, 0.7255f)); // Bucharest  44.43,  26.10
        AddProvince("Bosnia",        "Balkans",         new Vector2(0.6194f, 0.7408f)); // Sarajevo   43.85,  18.36
        AddProvince("Dalmatia",      "Balkans",         new Vector2(0.6257f, 0.7711f)); // Dubrovnik  42.65,  18.09  FIXED

        // North Africa
        AddProvince("Egypt",         "North Africa",    new Vector2(0.9043f, 0.9539f)); // Cairo      30.06,  31.25  FIXED
        AddProvince("Maghreb",       "North Africa",    new Vector2(0.4009f, 0.9279f)); // Algiers    36.74,   3.06
    }

    static void AddProvince(string name, string region, Vector2 position)
    {
        provinces.Add(new ProvinceData(name, region, position, false));
    }
}
