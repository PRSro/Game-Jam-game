using UnityEngine;

public static class MapProjection
{
    const float MinLongitude = -25f;
    const float MaxLongitude =  60f;
    const float MinLatitude  =  30f;
    const float MaxLatitude  =  72f;

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

    /// <summary>
    /// Given a UV that may be in water, searches in a spiral outward up to maxRadius
    /// to find the nearest land pixel. Returns the adjusted UV.
    /// </summary>
    public static Vector2 NudgeToLand(Vector2 uv, int maxRadiusPx = 50)
    {
        if (MapLayerController.Instance == null) return uv;
        if (MapLayerController.Instance.IsLandAtUV(uv)) return uv;

        for (int r = 1; r <= maxRadiusPx; r++)
        {
            for (int dx = -r; dx <= r; dx++)
            for (int dy = -r; dy <= r; dy++)
            {
                if (Mathf.Abs(dx) != r && Mathf.Abs(dy) != r) continue;
                Texture2D tex = MapLayerController.Instance.MapImage?.texture as Texture2D;
                if (tex == null) return uv;
                float testU = uv.x + (float)dx / tex.width;
                float testV = uv.y + (float)dy / tex.height;
                Vector2 candidate = new Vector2(Mathf.Clamp01(testU), Mathf.Clamp01(testV));
                if (MapLayerController.Instance.IsLandAtUV(candidate))
                    return candidate;
            }
        }
        return uv;
    }
}
