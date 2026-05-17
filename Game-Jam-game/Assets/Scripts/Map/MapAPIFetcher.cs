using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapAPIFetcher : MonoBehaviour
{
    public static MapAPIFetcher Instance { get; private set; }

    public Dictionary<int, Texture2D> FetchedMaps { get; private set; }
    Dictionary<int, Texture2D> mapCache = new Dictionary<int, Texture2D>();

    static readonly Color SeaColor = new Color(0.45f, 0.62f, 0.75f);
    static readonly Color LandColor = new Color(0.88f, 0.78f, 0.58f);
    static readonly Color BorderColor = new Color(0.15f, 0.08f, 0.02f);
    static readonly int TexWidth = 1920;
    static readonly int TexHeight = 1080;
    static readonly int BorderPixels = 2;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        FetchedMaps = mapCache;
    }

    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName.ToLower().Contains("menu"))
        {
            gameObject.SetActive(false);
            return;
        }
        StartCoroutine(GenerateAllMapTexturesAsync());
    }

    IEnumerator GenerateAllMapTexturesAsync()
    {
        List<MapSnapshot> snapshots = HistoricalMapDatabase.GetAllSnapshots();
        foreach (MapSnapshot snap in snapshots)
        {
            mapCache[snap.year] = GenerateEuropeMapTexture(snap.year);
            yield return null;
        }

        if (MapLayerManager.Instance != null)
        {
            MapLayerManager.Instance.RefreshCachedMaps(mapCache);
            Debug.Log("[MapFetch] Cache applied to MapLayerManager.");
        }
        else
        {
            Debug.LogError("[MapFetch] MapLayerManager.Instance is null after generation.");
        }
    }

    Texture2D GenerateEuropeMapTexture(int year)
    {
        Texture2D tex = new Texture2D(TexWidth, TexHeight, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[TexWidth * TexHeight];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = SeaColor;

        int w = TexWidth, h = TexHeight, b = BorderPixels;

        var rects = new Dictionary<string, Rect>
        {
            { "London",         new Rect(0.119f, 0.372f, 0.06f, 0.05f) },
            { "Dublin",         new Rect(0.034f, 0.413f, 0.06f, 0.05f) },
            { "Paris",          new Rect(0.151f, 0.313f, 0.06f, 0.05f) },
            { "Lyon",           new Rect(0.185f, 0.240f, 0.06f, 0.05f) },
            { "Madrid",         new Rect(0.069f, 0.120f, 0.06f, 0.05f) },
            { "Lisbon",         new Rect(0.001f, 0.082f, 0.06f, 0.05f) },
            { "Toledo",         new Rect(0.065f, 0.109f, 0.06f, 0.05f) },
            { "Amsterdam",      new Rect(0.186f, 0.392f, 0.06f, 0.05f) },
            { "Brussels",       new Rect(0.180f, 0.356f, 0.06f, 0.05f) },
            { "Rome",           new Rect(0.289f, 0.154f, 0.06f, 0.05f) },
            { "Venice",         new Rect(0.286f, 0.234f, 0.06f, 0.05f) },
            { "Florence",       new Rect(0.272f, 0.197f, 0.06f, 0.05f) },
            { "Valletta",       new Rect(0.316f, 0.018f, 0.06f, 0.05f) },
            { "Vienna",         new Rect(0.342f, 0.298f, 0.06f, 0.05f) },
            { "Prague",         new Rect(0.315f, 0.340f, 0.06f, 0.05f) },
            { "Geneva",         new Rect(0.202f, 0.252f, 0.06f, 0.05f) },
            { "Krakow",         new Rect(0.390f, 0.340f, 0.06f, 0.05f) },
            { "Constantinople", new Rect(0.514f, 0.134f, 0.06f, 0.05f) },
            { "Dubrovnik",      new Rect(0.365f, 0.175f, 0.06f, 0.05f) },
            { "Riga",           new Rect(0.447f, 0.495f, 0.06f, 0.05f) },
            { "Berlin",         new Rect(0.301f, 0.395f, 0.06f, 0.05f) },
            { "Munich",         new Rect(0.277f, 0.295f, 0.06f, 0.05f) },
            { "Hamburg",        new Rect(0.255f, 0.420f, 0.06f, 0.05f) },
            { "Stockholm",      new Rect(0.365f, 0.551f, 0.06f, 0.05f) },
            { "Copenhagen",     new Rect(0.290f, 0.466f, 0.06f, 0.05f) },
            { "Warsaw",         new Rect(0.405f, 0.388f, 0.06f, 0.05f) },
            { "Moscow",         new Rect(0.630f, 0.467f, 0.06f, 0.05f) },
            { "Kyiv",           new Rect(0.534f, 0.350f, 0.06f, 0.05f) },
            { "Budapest",       new Rect(0.378f, 0.282f, 0.06f, 0.05f) },
            { "Belgrade",       new Rect(0.398f, 0.222f, 0.06f, 0.05f) },
            { "Athens",         new Rect(0.441f, 0.066f, 0.06f, 0.05f) },
            { "Naples",         new Rect(0.314f, 0.129f, 0.06f, 0.05f) },
            { "Marseille",      new Rect(0.192f, 0.186f, 0.06f, 0.05f) },
            { "Bucharest",      new Rect(0.474f, 0.211f, 0.06f, 0.05f) },
        };

        foreach (Rect r in rects.Values)
        {
            int x1 = Mathf.RoundToInt(r.xMin * w) - b;
            int y1 = Mathf.RoundToInt(r.yMin * h) - b;
            int x2 = Mathf.RoundToInt(r.xMax * w) + b;
            int y2 = Mathf.RoundToInt(r.yMax * h) + b;

            for (int y = y1; y <= y2; y++)
            {
                if (y < 0 || y >= h) continue;
                for (int x = x1; x <= x2; x++)
                {
                    if (x < 0 || x >= w) continue;
                    pixels[y * w + x] = BorderColor;
                }
            }

            x1 += b; y1 += b; x2 -= b; y2 -= b;
            for (int y = y1; y <= y2; y++)
            {
                if (y < 0 || y >= h) continue;
                for (int x = x1; x <= x2; x++)
                {
                    if (x < 0 || x >= w) continue;
                    pixels[y * w + x] = LandColor;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        tex.name = $"Synthetic_{year}";
        return tex;
    }
}
