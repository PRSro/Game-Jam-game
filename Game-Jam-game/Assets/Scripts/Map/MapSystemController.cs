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
        { "Dubrovnik", "Dalmatia" },
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
        { "Bucharest", "Wallachia" }
    };

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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
        if (MapLayerController.Instance != null && MapLayerController.Instance.MapImage != null)
        {
            Texture2D overlay = FactionOverlayRenderer.GenerateOverlay(ProvinceDatabase.GetAllProvinces(), 1024, 1024);
            FactionOverlayRenderer.ApplyOverlayToRawImage(MapLayerController.Instance.MapImage, overlay);
        }
    }
}
