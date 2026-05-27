using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSystemController : MonoBehaviour
{
    public static MapSystemController Instance { get; private set; }

    public static Dictionary<string, string> territoryToProvince = new Dictionary<string, string>
    {
        { "London", "England" },
        { "Dublin", "Ireland" },
        { "Paris", "France" },
        { "Lyon", "France" },
        { "Madrid", "Spain" },
        { "Lisbon", "Portugal" },
        { "Toledo", "Spain" },
        { "Amsterdam", "Netherlands" },
        { "Brussels", "Belgium" },
        { "Rome", "Papal States" },
        { "Venice", "Venice" },
        { "Florence", "Tuscany" },
        { "Valletta", "Sicily" },
        { "Vienna", "Austria" },
        { "Prague", "Bohemia" },
        { "Geneva", "Switzerland" },
        { "Krakow", "Poland" },
        { "Constantinople", "Ottoman Empire" },
        { "Dubrovnik", "Croatia" },
        { "Riga", "Baltic" },
        { "Berlin", "Prussia" },
        { "Munich", "Bavaria" },
        { "Hamburg", "Hanover" },
        { "Stockholm", "Sweden" },
        { "Copenhagen", "Denmark" },
        { "Warsaw", "Poland" },
        { "Moscow", "Russia" },
        { "Kyiv", "Ukraine" },
        { "Budapest", "Hungary" },
        { "Belgrade", "Serbia" },
        { "Athens", "Greece" },
        { "Naples", "Naples" },
        { "Marseille", "Provence" },
        { "Bucharest", "Wallachia" },
        { "Seville", "Andalusia" },
        { "Algiers", "Maghreb" },
        { "Tunis", "Tunisia" },
        { "Ankara", "Anatolia" },
        { "Smyrna", "Ionia" },
        { "Trebizond", "Pontus" },
        { "Tbilisi", "Georgia" },
        { "Baku",       "Azerbaijan" },
        { "Beirut",     "Levant"     },
        { "Alexandria", "Egypt"      },
        { "Tripoli",    "Libya"      },
    };

    bool _overlayDirty = false;
    Coroutine _overlayCoroutine = null;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        MapLayerController.OnLandTextureReady += OnLandReady;
        MapLayerController.OnMapBuilt += OnMapRebuilt;
    }

    void OnDestroy()
    {
        MapLayerController.OnLandTextureReady -= OnLandReady;
        MapLayerController.OnMapBuilt -= OnMapRebuilt;
    }

    void OnMapRebuilt()
    {
        MapLayerController.OnLandTextureReady -= OnLandReady;
        MapLayerController.OnLandTextureReady += OnLandReady;
    }

    void OnLandReady()
    {
        MapLayerController.OnLandTextureReady -= OnLandReady;
        ProvinceDatabase.NudgeAllToLand();
        SyncProvincesFromTerritories();
    }

    public void SyncProvincesFromTerritories()
    {
        foreach (TerritoryData territory in TerritoryDatabase.GetAllTerritories())
            SyncProvinceFromTerritory(territory.name);
    }

    public void SyncProvinceFromTerritory(string territoryName)
    {
        TerritoryData territory = TerritoryDatabase.GetAllTerritories()
            .Find(t => t.name == territoryName);
        if (territory == null) return;

        if (territoryToProvince.TryGetValue(territoryName, out string provinceName))
        {
            ProvinceData prov = ProvinceDatabase.GetProvince(provinceName);
            if (prov != null)
            {
                prov.controllingFactionId = territory.controlledBy;
            }
        }
        
        RegenerateOverlay();
    }
    
    public void RegenerateOverlay()
    {
        if (MapLayerController.Instance?.OverlayImage == null) return;
        _overlayDirty = true;
        if (_overlayCoroutine == null)
            _overlayCoroutine = StartCoroutine(RegenerateOverlayAsync());
    }

    IEnumerator RegenerateOverlayAsync()
    {
        while (_overlayDirty)
        {
            _overlayDirty = false;
            yield return null;
            Texture2D overlay = FactionOverlayRenderer.GenerateOverlay(
                ProvinceDatabase.GetAllProvinces(), 512, 512);
            FactionOverlayRenderer.ApplyOverlayToRawImage(
                MapLayerController.Instance.OverlayImage, overlay);
        }
        _overlayCoroutine = null;
    }
}
