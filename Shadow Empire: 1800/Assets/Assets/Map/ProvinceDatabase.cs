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

    /// <summary>Nudges all provinces that landed in water onto the nearest land pixel.</summary>
    public static void NudgeAllToLand()
    {
        if (provinces == null) BuildProvinces();
        if (MapLayerController.Instance == null) return;
        foreach (ProvinceData p in provinces)
            p.mapPosition = MapProjection.NudgeToLand(p.mapPosition);
    }

    static void BuildProvinces()
    {
        provinces = new List<ProvinceData>();

        // ── BRITISH ISLES ──────────────────────────────────────────────────────
        AddProvince("England",        "British Isles",   MapProjection.GeoToUV(51.51f,  -0.13f)); // London
        AddProvince("Scotland",       "British Isles",   MapProjection.GeoToUV(55.95f,  -3.19f)); // Edinburgh
        AddProvince("Ireland",        "British Isles",   MapProjection.GeoToUV(53.33f,  -6.25f)); // Dublin
        AddProvince("Wales",          "British Isles",   MapProjection.GeoToUV(51.48f,  -3.18f)); // Cardiff

        // ── FRANCE ─────────────────────────────────────────────────────────────
        AddProvince("France",         "France",          MapProjection.GeoToUV(48.85f,   2.35f)); // Paris
        AddProvince("Brittany",       "France",          MapProjection.GeoToUV(48.11f,  -1.68f)); // Rennes
        AddProvince("Normandy",       "France",          MapProjection.GeoToUV(49.44f,   1.10f)); // Rouen
        AddProvince("Aquitaine",      "France",          MapProjection.GeoToUV(44.84f,  -0.58f)); // Bordeaux
        AddProvince("Provence",       "France",          MapProjection.GeoToUV(43.30f,   5.37f)); // Marseille

        // ── IBERIA ─────────────────────────────────────────────────────────────
        AddProvince("Spain",          "Iberia",          MapProjection.GeoToUV(40.42f,  -3.70f)); // Madrid
        AddProvince("Portugal",       "Iberia",          MapProjection.GeoToUV(38.72f,  -9.14f)); // Lisbon
        AddProvince("Catalonia",      "Iberia",          MapProjection.GeoToUV(41.39f,   2.15f)); // Barcelona
        AddProvince("Andalusia",      "Iberia",          MapProjection.GeoToUV(37.38f,  -5.99f)); // Seville

        // ── LOW COUNTRIES ──────────────────────────────────────────────────────
        AddProvince("Netherlands",    "Low Countries",   MapProjection.GeoToUV(52.37f,   4.90f)); // Amsterdam
        AddProvince("Belgium",        "Low Countries",   MapProjection.GeoToUV(50.85f,   4.35f)); // Brussels

        // ── GERMAN STATES ──────────────────────────────────────────────────────
        AddProvince("Prussia",        "German States",   MapProjection.GeoToUV(52.52f,  13.41f)); // Berlin
        AddProvince("Bavaria",        "German States",   MapProjection.GeoToUV(48.14f,  11.58f)); // Munich
        AddProvince("Saxony",         "German States",   MapProjection.GeoToUV(51.05f,  13.74f)); // Dresden
        AddProvince("Hanover",        "German States",   MapProjection.GeoToUV(53.55f,   9.99f)); // Hamburg
        AddProvince("Baden",          "German States",   MapProjection.GeoToUV(48.99f,   8.40f)); // Karlsruhe
        AddProvince("Westphalia",     "German States",   MapProjection.GeoToUV(51.51f,   7.47f)); // Dortmund
        AddProvince("Rhineland",      "German States",   MapProjection.GeoToUV(50.94f,   6.96f)); // Cologne

        // ── ITALIAN STATES ─────────────────────────────────────────────────────
        AddProvince("Piedmont",       "Italian States",  MapProjection.GeoToUV(45.07f,   7.68f)); // Turin
        AddProvince("Lombardy",       "Italian States",  MapProjection.GeoToUV(45.46f,   9.19f)); // Milan
        AddProvince("Venice",         "Italian States",  MapProjection.GeoToUV(45.44f,  12.34f)); // Venice
        AddProvince("Tuscany",        "Italian States",  MapProjection.GeoToUV(43.77f,  11.26f)); // Florence
        AddProvince("Papal States",   "Italian States",  MapProjection.GeoToUV(41.90f,  12.50f)); // Rome
        AddProvince("Naples",         "Italian States",  MapProjection.GeoToUV(40.85f,  14.27f)); // Naples
        AddProvince("Sicily",         "Italian States",  MapProjection.GeoToUV(37.50f,  14.00f)); // Palermo interior

        // ── SWITZERLAND & ALPINE ───────────────────────────────────────────────
        AddProvince("Switzerland",    "Switzerland",     MapProjection.GeoToUV(46.20f,   6.14f)); // Geneva

        // ── AUSTRIAN EMPIRE ────────────────────────────────────────────────────
        AddProvince("Austria",        "Austrian Empire", MapProjection.GeoToUV(48.21f,  16.37f)); // Vienna
        AddProvince("Bohemia",        "Austrian Empire", MapProjection.GeoToUV(50.08f,  14.44f)); // Prague
        AddProvince("Hungary",        "Austrian Empire", MapProjection.GeoToUV(47.50f,  19.04f)); // Budapest
        AddProvince("Croatia",        "Austrian Empire", MapProjection.GeoToUV(45.81f,  15.98f)); // Zagreb
        AddProvince("Galicia",        "Austrian Empire", MapProjection.GeoToUV(50.06f,  19.94f)); // Krakow

        // ── POLAND ─────────────────────────────────────────────────────────────
        AddProvince("Poland",         "Poland",          MapProjection.GeoToUV(52.23f,  21.01f)); // Warsaw
        AddProvince("Lithuania",      "Poland",          MapProjection.GeoToUV(54.69f,  25.28f)); // Vilnius

        // ── SCANDINAVIA ────────────────────────────────────────────────────────
        AddProvince("Sweden",         "Scandinavia",     MapProjection.GeoToUV(59.33f,  18.07f)); // Stockholm
        AddProvince("Norway",         "Scandinavia",     MapProjection.GeoToUV(59.91f,  10.75f)); // Oslo
        AddProvince("Denmark",        "Scandinavia",     MapProjection.GeoToUV(55.68f,  12.57f)); // Copenhagen
        AddProvince("Finland",        "Scandinavia",     MapProjection.GeoToUV(60.17f,  24.94f)); // Helsinki
        AddProvince("Helsinki",       "Russia",          MapProjection.GeoToUV(60.17f,  24.94f));

        // ── RUSSIA ─────────────────────────────────────────────────────────────
        AddProvince("Russia",         "Russia",          MapProjection.GeoToUV(55.75f,  37.62f)); // Moscow
        AddProvince("St. Petersburg", "Russia",          MapProjection.GeoToUV(59.95f,  30.32f)); // St. Petersburg
        AddProvince("Baltic",         "Russia",          MapProjection.GeoToUV(56.95f,  24.11f)); // Riga
        AddProvince("Ukraine",        "Russia",          MapProjection.GeoToUV(50.45f,  30.52f)); // Kyiv
        AddProvince("Crimea",         "Russia",          MapProjection.GeoToUV(45.03f,  33.99f)); // Simferopol
        AddProvince("Volga",          "Russia",          MapProjection.GeoToUV(53.20f,  50.15f)); // Samara
        AddProvince("Don",            "Russia",          MapProjection.GeoToUV(47.23f,  39.72f)); // Rostov-on-Don

        // ── BALKANS ────────────────────────────────────────────────────────────
        AddProvince("Ottoman Empire", "Balkans",         MapProjection.GeoToUV(41.01f,  28.95f)); // Constantinople
        AddProvince("Serbia",         "Balkans",         MapProjection.GeoToUV(44.82f,  20.46f)); // Belgrade
        AddProvince("Greece",         "Balkans",         MapProjection.GeoToUV(37.98f,  23.73f)); // Athens
        AddProvince("Wallachia",      "Balkans",         MapProjection.GeoToUV(44.43f,  26.10f)); // Bucharest
        AddProvince("Moldavia",       "Balkans",         MapProjection.GeoToUV(47.00f,  28.83f)); // Chisinau
        AddProvince("Bosnia",         "Balkans",         MapProjection.GeoToUV(43.84f,  18.36f)); // Sarajevo
        AddProvince("Bulgaria",       "Balkans",         MapProjection.GeoToUV(42.70f,  23.32f)); // Sofia
        AddProvince("Albania",        "Balkans",         MapProjection.GeoToUV(41.33f,  19.83f)); // Tirana

        // ── OTTOMAN EMPIRE ─────────────────────────────────────────────────────
        AddProvince("Anatolia",       "Ottoman Empire",  MapProjection.GeoToUV(39.93f,  32.86f)); // Ankara
        AddProvince("Ionia",          "Ottoman Empire",  MapProjection.GeoToUV(38.42f,  27.14f)); // Smyrna (Izmir)
        AddProvince("Pontus",         "Ottoman Empire",  MapProjection.GeoToUV(41.29f,  36.33f)); // Samsun
        AddProvince("Syria",          "Near East",       MapProjection.GeoToUV(33.51f,  36.29f)); // Damascus
        AddProvince("Levant",         "Near East",       MapProjection.GeoToUV(33.89f,  35.50f)); // Beirut
        AddProvince("Mesopotamia",    "Near East",       MapProjection.GeoToUV(33.34f,  44.40f)); // Baghdad

        // ── CAUCASUS ───────────────────────────────────────────────────────────
        AddProvince("Georgia",        "Caucasus",        MapProjection.GeoToUV(41.69f,  44.83f)); // Tbilisi
        AddProvince("Azerbaijan",     "Caucasus",        MapProjection.GeoToUV(40.41f,  49.87f)); // Baku
        AddProvince("Armenia",        "Caucasus",        MapProjection.GeoToUV(40.18f,  44.50f)); // Yerevan

        // ── NORTH AFRICA ───────────────────────────────────────────────────────
        AddProvince("Egypt",          "North Africa",    MapProjection.GeoToUV(30.06f,  31.25f)); // Cairo
        AddProvince("Maghreb",        "North Africa",    MapProjection.GeoToUV(34.85f,   1.65f)); // Tlemcen
        AddProvince("Tunisia",        "North Africa",    MapProjection.GeoToUV(36.00f,   9.37f)); // Kairouan
        AddProvince("Libya",          "North Africa",    MapProjection.GeoToUV(32.90f,  13.18f)); // Tripoli
        AddProvince("Morocco",        "North Africa",    MapProjection.GeoToUV(33.99f,  -5.00f)); // Fez
    }

    static void AddProvince(string name, string region, Vector2 position)
    {
        provinces.Add(new ProvinceData(name, region, position, false));
    }
}
