using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapLayerController : MonoBehaviour
{
    public static MapLayerController Instance { get; private set; }

    public static readonly Vector2 MapAnchorMin = new Vector2(0f, 0.04f);
    public static readonly Vector2 MapAnchorMax = new Vector2(0.74f, 0.97f);

    RawImage mapImage;
    RectTransform nodeLayer;
    readonly Dictionary<string, RectTransform> nodeRoots = new Dictionary<string, RectTransform>();

    public RectTransform NodeLayer => nodeLayer;
    public RawImage MapImage => mapImage;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    IEnumerator Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "MainMenu" || sceneName.ToLower().Contains("menu"))
        {
            gameObject.SetActive(false);
            yield break;
        }

        Canvas canvas = null;
        while (canvas == null)
        {
            GameObject canvasObject = GameObject.Find("MainCanvas");
            canvas = canvasObject != null ? canvasObject.GetComponent<Canvas>() : null;
            if (canvas == null) yield return null;
        }

        BuildMap(canvas.transform);
    }

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
        node.anchoredPosition = Vector2.zero;
    }

    void BuildMap(Transform canvasTransform)
    {
        Transform old = canvasTransform.Find("MapImage");
        if (old != null)
            Destroy(old.gameObject);

        GameObject imageObject = new GameObject("MapImage", typeof(RectTransform));
        imageObject.transform.SetParent(canvasTransform, false);

        mapImage = imageObject.AddComponent<RawImage>();
        mapImage.raycastTarget = false;
        mapImage.texture = Resources.Load<Texture2D>("EuropeMap");
        if (mapImage.texture == null)
            mapImage.texture = Resources.Load<Texture2D>("Maps/EuropeMap");

        RectTransform mapRect = imageObject.GetComponent<RectTransform>();
        mapRect.anchorMin = MapAnchorMin;
        mapRect.anchorMax = MapAnchorMax;
        mapRect.sizeDelta = Vector2.zero;
        mapRect.anchoredPosition = Vector2.zero;

        GameObject nodeLayerObject = new GameObject("NodeLayer", typeof(RectTransform));
        nodeLayerObject.transform.SetParent(imageObject.transform, false);
        nodeLayerObject.transform.SetAsLastSibling();
        nodeLayer = nodeLayerObject.GetComponent<RectTransform>();
        nodeLayer.anchorMin = MapAnchorMin;
        nodeLayer.anchorMax = MapAnchorMax;
        nodeLayer.sizeDelta = Vector2.zero;
        nodeLayer.anchoredPosition = Vector2.zero;

        BuildNodeRoots();
        PlaceBehindGameplayMap();
    }

    public void PlaceForAttackPhase()
    {
        if (mapImage != null)
            mapImage.transform.SetAsLastSibling();
    }

    public void PlaceBehindGameplayMap()
    {
        if (mapImage == null || mapImage.transform.parent == null) return;

        Transform mapArea = mapImage.transform.parent.Find("MapAreaBg");
        if (mapArea != null)
            mapImage.transform.SetSiblingIndex(mapArea.GetSiblingIndex() + 1);
        else
            mapImage.transform.SetSiblingIndex(0);
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
            node.sizeDelta = Vector2.zero;
            node.anchoredPosition = Vector2.zero;

            nodeRoots[city.name] = node;
        }
    }
}
