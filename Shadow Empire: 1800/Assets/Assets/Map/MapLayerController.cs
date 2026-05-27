using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapLayerController : MonoBehaviour
{
    public static MapLayerController Instance { get; private set; }
    public static System.Action OnMapBuilt;
    public bool IsLandTextureReady => readableMapTexture != null;
    public static System.Action OnLandTextureReady;

    public static readonly Vector2 MapAnchorMin = new Vector2(0f, 0.04f);
    public static readonly Vector2 MapAnchorMax = new Vector2(0.733f, 0.98f);

    RawImage mapImage;
    RawImage overlayImage;
    RawImage borderOverlayImage;
    RectTransform nodeLayer;
    readonly Dictionary<string, RectTransform> nodeRoots = new Dictionary<string, RectTransform>();
    Texture2D readableMapTexture;

    public RectTransform NodeLayer => nodeLayer;
    public RawImage MapImage => mapImage;
    public RawImage OverlayImage => overlayImage;
    public RawImage BorderOverlayImage => borderOverlayImage;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Map is built explicitly via BuildMapForGame(canvasTransform)
        // called by GameUIManager.StartGame() once the canvas exists.
        // The old coroutine that polled for "MainCanvas" caused double-build races.
    }

    /// <summary>
    /// Returns true if the map texture pixel at the given UV is land (not sea).
    /// Sea pixels are detected by high blue channel dominance.
    /// Uses a separate readable texture so the display texture is never altered.
    /// </summary>
    public bool IsLandAtUV(Vector2 uv)
    {
        if (readableMapTexture == null) return true;

        int px = Mathf.Clamp(Mathf.RoundToInt(uv.x * readableMapTexture.width),
            0, readableMapTexture.width - 1);
        int py = Mathf.Clamp(
            Mathf.RoundToInt((1f - uv.y) * readableMapTexture.height),
            0, readableMapTexture.height - 1);

        Color pixel;
        try { pixel = readableMapTexture.GetPixel(px, py); }
        catch { return true; }

        bool isSea = pixel.b > 0.50f && pixel.b > pixel.r + 0.20f;
        return !isSea;
    }

    public bool IsLandAtUV(float u, float v) => IsLandAtUV(new Vector2(u, v));

    public RectTransform GetNodeRoot(string cityName)
    {
        return nodeRoots.TryGetValue(cityName, out RectTransform node) ? node : null;
    }

    public void RebuildNodes()
    {
        if (nodeLayer == null) return;
        foreach (Transform child in nodeLayer)
            Destroy(child.gameObject);
        nodeRoots.Clear();
        BuildNodeRoots();
    }

    public void RefreshNodePosition(string territoryName)
    {
        if (!nodeRoots.TryGetValue(territoryName, out RectTransform node)) return;
        TerritoryData city = TerritoryDatabase.GetAllTerritories()
            .Find(t => t.name == territoryName);
        if (city == null) return;

        float canvasX = city.mapPosition.x;
        float canvasY = 1f - city.mapPosition.y;
        node.anchorMin = node.anchorMax = new Vector2(canvasX, canvasY);
        node.pivot = new Vector2(0.5f, 0.5f);
        node.anchoredPosition = Vector2.zero;
    }

    void BuildMap(Transform canvasTransform)
    {
        Transform old = canvasTransform.Find("MapImage");
        if (old != null)
            Destroy(old.gameObject);

        GameObject imageObject = new GameObject("MapImage", typeof(RectTransform));
        imageObject.transform.SetParent(canvasTransform, false);
        // Draw order is managed by GameUIManager.EnforceDrawOrder via OnMapBuilt callback.
        // Do not set a fixed index here; it will race with GameUIManager's sibling assignments.

        mapImage = imageObject.AddComponent<RawImage>();
        mapImage.raycastTarget = false;
        mapImage.texture = Resources.Load<Texture2D>("Maps/EuropeMap");
        if (mapImage.texture == null)
            mapImage.texture = Resources.Load<Texture2D>("EuropeMap");
        if (mapImage.texture == null)
        {
            Texture2D fallback = new Texture2D(4, 4, TextureFormat.RGB24, false);
            Color[] px = new Color[16];
            Color mapTone = new Color(0.15f,0.22f,0.12f); // dark green, visible
            for (int i = 0; i < 16; i++) px[i] = mapTone;
            fallback.SetPixels(px);
            fallback.Apply();
            mapImage.texture = fallback;
            mapImage.color = new Color(1f, 1f, 1f, 1f);
            Debug.LogWarning("[MapLayerController] EuropeMap texture not found - using fallback. Place EuropeMap.png in Assets/Resources/Maps/");
        }

        // Build a readable copy for IsLandAtUV asynchronously — never replace
        // the display texture, which can turn white on Linux/OpenGL when
        // Graphics.Blit.ReadPixels runs outside a valid render context.
        StartCoroutine(BuildReadableTexture(mapImage.texture as Texture2D));

        RectTransform mapRect = imageObject.GetComponent<RectTransform>();
        mapRect.anchorMin = MapAnchorMin;
        mapRect.anchorMax = MapAnchorMax;
        mapRect.sizeDelta = Vector2.zero;
        mapRect.anchoredPosition = Vector2.zero;

        // Add Overlay layer
        GameObject overlayObject = new GameObject("OverlayImage", typeof(RectTransform));
        overlayObject.transform.SetParent(imageObject.transform, false);
        overlayImage = overlayObject.AddComponent<RawImage>();
        overlayImage.raycastTarget = false;
        overlayImage.color = Color.white;
        RectTransform overlayRect = overlayObject.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.sizeDelta = Vector2.zero;
        overlayRect.anchoredPosition = Vector2.zero;

        // Add border overlay layer
        GameObject borderObject = new GameObject("BorderOverlayImage", typeof(RectTransform));
        borderObject.transform.SetParent(imageObject.transform, false);
        borderOverlayImage = borderObject.AddComponent<RawImage>();
        borderOverlayImage.raycastTarget = false;
        Texture2D borderTex = Resources.Load<Texture2D>("Maps/BorderOverlay");
        if (borderTex != null)
        {
            borderOverlayImage.texture = borderTex;
            borderOverlayImage.color = Color.white;
        }
        else
        {
            borderOverlayImage.color = Color.clear;
            Debug.LogWarning("[MapLayerController] BorderOverlay texture not found at Resources/Maps/BorderOverlay — border layer will be invisible.");
        }
        RectTransform borderRect = borderObject.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.sizeDelta = Vector2.zero;
        borderRect.anchoredPosition = Vector2.zero;

        GameObject nodeLayerObject = new GameObject("NodeLayer", typeof(RectTransform));
        nodeLayerObject.transform.SetParent(imageObject.transform, false);
        nodeLayerObject.transform.SetAsLastSibling();
        nodeLayer = nodeLayerObject.GetComponent<RectTransform>();
        nodeLayer.anchorMin = Vector2.zero;
        nodeLayer.anchorMax = Vector2.one;
        nodeLayer.pivot = new Vector2(0.5f, 0.5f);
        nodeLayer.sizeDelta = Vector2.zero;
        nodeLayer.anchoredPosition = Vector2.zero;

        BuildNodeRoots();
        PlaceBehindGameplayMap();
        nodeLayer.gameObject.SetActive(true);
        OnMapBuilt?.Invoke();
    }

    public void SetBorderOverlay(Texture2D tex)
    {
        if (borderOverlayImage == null) return;
        borderOverlayImage.texture = tex;
        borderOverlayImage.color = Color.white;
    }

    public void SetBorderOverlayAlpha(float a)
    {
        if (borderOverlayImage != null)
        {
            Color c = borderOverlayImage.color;
            c.a = a;
            borderOverlayImage.color = c;
        }
    }

    public void PlaceForAttackPhase()
    {
        // Keep the map and NodeLayer fixed; attack UI should not move province nodes.
    }

    /// <summary>
    /// Ensures the map is built when the game starts. Handles the case where
    /// MapLayerController was deactivated during the menu phase, or needs a
    /// clean rebuild after restart. Always destroys any prior MapImage to
    /// avoid duplicate children.
    /// </summary>
    public void BuildMapForGame(Transform canvasTransform)
    {
        Transform old = canvasTransform.Find("MapImage");
        if (old != null) Destroy(old.gameObject);

        mapImage = null;
        overlayImage = null;
        borderOverlayImage = null;
        nodeLayer = null;
        nodeRoots.Clear();

        gameObject.SetActive(true);
        BuildMap(canvasTransform);
    }

    public void PlaceBehindGameplayMap()
    {
        // Intentionally empty — draw order is managed exclusively by
        // GameUIManager.EnforceDrawOrder() to avoid sibling index races.
    }

    IEnumerator BuildReadableTexture(Texture2D src)
    {
        if (src == null) yield break;

        yield return new WaitForEndOfFrame();

        if (src.isReadable)
        {
            readableMapTexture = src;
            OnLandTextureReady?.Invoke();
            yield break;
        }

        try
        {
            RenderTexture rt = RenderTexture.GetTemporary(src.width, src.height, 0,
                RenderTextureFormat.ARGB32);
            Graphics.Blit(src, rt);
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;
            Texture2D copy = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false);
            copy.ReadPixels(new Rect(0, 0, src.width, src.height), 0, 0);
            copy.Apply();
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
            readableMapTexture = copy;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[MapLayerController] Could not build readable texture: " + e.Message +
                " — IsLandAtUV will always return true (all positions treated as land).");
            readableMapTexture = null;
        }

        OnLandTextureReady?.Invoke();
    }

    public void RebuildGridAndOverlay() => StartCoroutine(NudgeAndRebuild());

    IEnumerator NudgeAndRebuild()
    {
        yield return null;
        ProvinceDatabase.NudgeAllToLand();
        OnLandTextureReady?.Invoke();
    }

    void BuildNodeRoots()
    {
        nodeRoots.Clear();
        foreach (TerritoryData city in TerritoryDatabase.GetAllTerritories())
        {
            GameObject nodeObject = new GameObject("Node_" + city.name, typeof(RectTransform));
            nodeObject.transform.SetParent(nodeLayer, false);

            RectTransform node = nodeObject.GetComponent<RectTransform>();
            float canvasX = city.mapPosition.x;
            float canvasY = 1f - city.mapPosition.y;
            node.anchorMin = node.anchorMax = new Vector2(canvasX, canvasY);
            node.pivot = new Vector2(0.5f, 0.5f);
            node.sizeDelta = Vector2.zero;
            node.anchoredPosition = Vector2.zero;

            nodeRoots[city.name] = node;
        }
    }
}
