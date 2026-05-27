using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapLayerManager : MonoBehaviour
{
    public static MapLayerManager Instance { get; private set; }

    public Canvas mapCanvas;
    public RawImage layer4Grid;

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
        mapCanvas = GameObject.Find("MainCanvas")?.GetComponent<Canvas>();
    }

    public void RefreshCachedMaps(Dictionary<int, Texture2D> cache) { }
    public void RefreshPoliticalMap(int year) { }
    public void RegenerateOverlay() { }
    public void SetOverlayTexture(Texture2D overlay) { }
    public void SetGridTexture(Texture2D grid) { }
}
