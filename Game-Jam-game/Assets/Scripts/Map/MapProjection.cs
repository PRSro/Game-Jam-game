using UnityEngine;

public static class MapProjection
{
    const float MinLongitude = -25f;
    const float MaxLongitude = 45f;
    const float MinLatitude = 34f;
    const float MaxLatitude = 72f;

    public static Vector2 GeoToUV(float latitude, float longitude)
    {
        float u = Mathf.InverseLerp(MinLongitude, MaxLongitude, longitude);
        float v = 1f - Mathf.InverseLerp(MinLatitude, MaxLatitude, latitude);
        return new Vector2(Mathf.Clamp01(u), Mathf.Clamp01(v));
    }

    public static Vector2 UVToCanvasAnchor(Vector2 uv)
    {
        return new Vector2(
            Mathf.Lerp(MapLayerController.MapAnchorMin.x, MapLayerController.MapAnchorMax.x, uv.x),
            Mathf.Lerp(MapLayerController.MapAnchorMin.y, MapLayerController.MapAnchorMax.y, 1f - uv.y));
    }
}
