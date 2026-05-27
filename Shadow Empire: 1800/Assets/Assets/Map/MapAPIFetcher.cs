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
            { "London",         new Rect(0.118f, 0.373f, 0.05f, 0.04f) },
            { "Dublin",         new Rect(0.039f, 0.416f, 0.05f, 0.04f) },
            { "Paris",          new Rect(0.155f, 0.313f, 0.05f, 0.04f) },
            { "Lyon",           new Rect(0.189f, 0.242f, 0.05f, 0.04f) },
            { "Madrid",         new Rect(0.074f, 0.115f, 0.05f, 0.04f) },
            { "Lisbon",         new Rect(0.000f, 0.077f, 0.05f, 0.04f) },
            { "Toledo",         new Rect(0.069f, 0.103f, 0.05f, 0.04f) },
            { "Amsterdam",      new Rect(0.190f, 0.396f, 0.05f, 0.04f) },
            { "Brussels",       new Rect(0.182f, 0.361f, 0.05f, 0.04f) },
            { "Rome",           new Rect(0.293f, 0.150f, 0.05f, 0.04f) },
            { "Venice",         new Rect(0.290f, 0.230f, 0.05f, 0.04f) },
            { "Florence",       new Rect(0.276f, 0.192f, 0.05f, 0.04f) },
            { "Valletta",       new Rect(0.320f, 0.023f, 0.05f, 0.04f) },
            { "Vienna",         new Rect(0.345f, 0.283f, 0.05f, 0.04f) },
            { "Prague",         new Rect(0.319f, 0.335f, 0.05f, 0.04f) },
            { "Geneva",         new Rect(0.207f, 0.253f, 0.05f, 0.04f) },
            { "Krakow",         new Rect(0.393f, 0.335f, 0.05f, 0.04f) },
            { "Constantinople", new Rect(0.515f, 0.121f, 0.05f, 0.04f) },
            { "Dubrovnik",      new Rect(0.368f, 0.177f, 0.05f, 0.04f) },
            { "Riga",           new Rect(0.452f, 0.502f, 0.05f, 0.04f) },
            { "Berlin",         new Rect(0.305f, 0.401f, 0.05f, 0.04f) },
            { "Munich",         new Rect(0.280f, 0.301f, 0.05f, 0.04f) },
            { "Hamburg",        new Rect(0.259f, 0.426f, 0.05f, 0.04f) },
            { "Stockholm",      new Rect(0.364f, 0.557f, 0.05f, 0.04f) },
            { "Copenhagen",     new Rect(0.261f, 0.487f, 0.05f, 0.04f) },
            { "Warsaw",         new Rect(0.408f, 0.396f, 0.05f, 0.04f) },
            { "Moscow",         new Rect(0.632f, 0.472f, 0.05f, 0.04f) },
            { "Kyiv",           new Rect(0.536f, 0.376f, 0.05f, 0.04f) },
            { "Budapest",       new Rect(0.381f, 0.273f, 0.05f, 0.04f) },
            { "Belgrade",       new Rect(0.400f, 0.226f, 0.05f, 0.04f) },
            { "Athens",         new Rect(0.444f, 0.070f, 0.05f, 0.04f) },
            { "Naples",         new Rect(0.316f, 0.126f, 0.05f, 0.04f) },
            { "Marseille",      new Rect(0.196f, 0.181f, 0.05f, 0.04f) },
            { "Bucharest",      new Rect(0.476f, 0.213f, 0.05f, 0.04f) },
            { "Seville",        new Rect(0.043f, 0.057f, 0.05f, 0.04f) },
            { "Ankara",         new Rect(0.568f, 0.115f, 0.05f, 0.04f) },
            { "Smyrna",         new Rect(0.490f, 0.080f, 0.05f, 0.04f) },
            { "Trebizond",      new Rect(0.660f, 0.119f, 0.05f, 0.04f) },
            { "Tbilisi",        new Rect(0.729f, 0.155f, 0.05f, 0.04f) },
            { "Tunis",          new Rect(0.261f, 0.044f, 0.05f, 0.04f) },
            { "Algiers",        new Rect(0.165f, 0.042f, 0.05f, 0.04f) },
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
