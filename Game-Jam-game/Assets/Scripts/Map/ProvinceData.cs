using UnityEngine;

[System.Serializable]
public class ProvinceData
{
    public string provinceName;
    public string historicalRegion;
    public int controllingFactionId = -1;
    public Color factionOverlayColor = Color.clear;
    public bool isFactionControlStatic = false;
    public Vector2 mapPosition;

    /// <summary>Creates a new province with the given parameters.</summary>
    public ProvinceData(string name, string region, Vector2 position, bool isStatic = false)
    {
        provinceName = name;
        historicalRegion = region;
        mapPosition = position;
        isFactionControlStatic = isStatic;
        controllingFactionId = -1;
        factionOverlayColor = Color.clear;
    }

    /// <summary>Returns the faction color for overlay rendering, or Color.clear if neutral.</summary>
    public Color GetOverlayColor()
    {
        return isFactionControlStatic ? factionOverlayColor : Color.clear;
    }
}
