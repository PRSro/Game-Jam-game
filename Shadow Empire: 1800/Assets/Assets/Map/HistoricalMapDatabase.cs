using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MapSnapshot
{
    public int year;
    public string resourcePath;
}

public static class HistoricalMapDatabase
{
    static List<MapSnapshot> snapshots;

    /// <summary>Returns the full list of map snapshot entries covering 1790-1990.</summary>
    public static List<MapSnapshot> GetAllSnapshots()
    {
        if (snapshots == null)
            BuildSnapshots();
        return snapshots;
    }

    /// <summary>Finds the snapshot whose year is closest to the given year.</summary>
    public static MapSnapshot GetClosestSnapshot(int year)
    {
        if (snapshots == null) BuildSnapshots();

        MapSnapshot best = snapshots[0];
        int bestDiff = Mathf.Abs(snapshots[0].year - year);

        for (int i = 1; i < snapshots.Count; i++)
        {
            int diff = Mathf.Abs(snapshots[i].year - year);
            if (diff < bestDiff)
            {
                bestDiff = diff;
                best = snapshots[i];
            }
        }
        return best;
    }

    /// <summary>Generates a placeholder Texture2D for a given year as a fallback until real map assets are imported.</summary>
    public static Texture2D GeneratePlaceholderMap(int year)
    {
        Texture2D tex = new Texture2D(256, 144, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        Color seaColor = new Color(0.30f, 0.38f, 0.48f, 0.45f);
        Color[] pixels = new Color[256 * 144];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = seaColor;
        tex.SetPixels(pixels);
        tex.Apply();
        tex.name = $"Placeholder_{year}";
        return tex;
    }

    public static Color GetEraColor(int year)
    {
        if (year <= 1799) return new Color(0.65f, 0.55f, 0.40f);
        if (year <= 1815) return new Color(0.55f, 0.40f, 0.30f);
        if (year <= 1830) return new Color(0.60f, 0.50f, 0.35f);
        if (year <= 1848) return new Color(0.58f, 0.48f, 0.32f);
        if (year <= 1860) return new Color(0.50f, 0.42f, 0.28f);
        if (year <= 1878) return new Color(0.55f, 0.47f, 0.30f);
        if (year <= 1890) return new Color(0.53f, 0.45f, 0.33f);
        if (year <= 1900) return new Color(0.56f, 0.44f, 0.31f);
        if (year <= 1918) return new Color(0.45f, 0.35f, 0.25f);
        if (year <= 1939) return new Color(0.48f, 0.38f, 0.27f);
        if (year <= 1945) return new Color(0.40f, 0.30f, 0.20f);
        if (year <= 1960) return new Color(0.50f, 0.40f, 0.30f);
        if (year <= 1975) return new Color(0.52f, 0.43f, 0.32f);
        return new Color(0.54f, 0.45f, 0.34f);
    }

    static string GetEraLabel(int year)
    {
        if (year <= 1799) return "Revolutionary Era";
        if (year <= 1815) return "Napoleonic Wars";
        if (year <= 1830) return "Restoration";
        if (year <= 1848) return "Pre-March / Vormärz";
        if (year <= 1860) return "Revolutions & Unification";
        if (year <= 1878) return "Imperial Age";
        if (year <= 1890) return "Belle Époque";
        if (year <= 1900) return "Fin de Siècle";
        if (year <= 1918) return "World War I";
        if (year <= 1939) return "Interwar Period";
        if (year <= 1945) return "World War II";
        if (year <= 1960) return "Post-War / Cold War";
        if (year <= 1975) return "Cold War Détente";
        return "Late Cold War";
    }

    static void BuildSnapshots()
    {
        snapshots = new List<MapSnapshot>();

        AddSnapshot(1790, "Maps/Europe_1790");
        AddSnapshot(1800, "Maps/Europe_1800");
        AddSnapshot(1803, "Maps/Europe_1803");
        AddSnapshot(1806, "Maps/Europe_1806");
        AddSnapshot(1809, "Maps/Europe_1809");
        AddSnapshot(1812, "Maps/Europe_1812");
        AddSnapshot(1815, "Maps/Europe_1815");
        AddSnapshot(1820, "Maps/Europe_1820");
        AddSnapshot(1830, "Maps/Europe_1830");
        AddSnapshot(1848, "Maps/Europe_1848");
        AddSnapshot(1860, "Maps/Europe_1860");
        AddSnapshot(1870, "Maps/Europe_1870");
        AddSnapshot(1878, "Maps/Europe_1878");
        AddSnapshot(1890, "Maps/Europe_1890");
        AddSnapshot(1900, "Maps/Europe_1900");
        AddSnapshot(1914, "Maps/Europe_1914");
        AddSnapshot(1918, "Maps/Europe_1918");
        AddSnapshot(1920, "Maps/Europe_1920");
        AddSnapshot(1938, "Maps/Europe_1938");
        AddSnapshot(1939, "Maps/Europe_1939");
        AddSnapshot(1942, "Maps/Europe_1942");
        AddSnapshot(1945, "Maps/Europe_1945");
        AddSnapshot(1950, "Maps/Europe_1950");
        AddSnapshot(1960, "Maps/Europe_1960");
        AddSnapshot(1975, "Maps/Europe_1975");
        AddSnapshot(1990, "Maps/Europe_1990");
    }

    static void AddSnapshot(int year, string path)
    {
        snapshots.Add(new MapSnapshot { year = year, resourcePath = path });
    }
}
