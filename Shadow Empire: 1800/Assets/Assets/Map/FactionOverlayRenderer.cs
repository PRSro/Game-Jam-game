using System.Collections.Generic;
using UnityEngine;

public static class FactionOverlayRenderer
{
    static readonly Color IlluminatiGold   = new Color(0.788f, 0.659f, 0.298f, 0.75f);
    static readonly Color TemplarRed       = new Color(0.545f, 0.000f, 0.000f, 0.75f);
    static readonly Color FreemasonBlue    = new Color(0.106f, 0.227f, 0.420f, 0.75f);
    static readonly Color CarbonariGreen   = new Color(0.176f, 0.353f, 0.153f, 0.75f);

    static Color GetFactionOverlayColor(int factionId)
    {
        switch (factionId)
        {
            case 0: return IlluminatiGold;
            case 1: return TemplarRed;
            case 2: return FreemasonBlue;
            case 3: return CarbonariGreen;
            default: return Color.clear;
        }
    }

    /// <summary>Generates a runtime Texture2D overlay showing faction control of provinces.
    /// Uses Voronoi-style nearest-provinces flood fill so each province appears as a
    /// contiguous region rather than a tiny circle. Only land pixels are painted.</summary>
    public static Texture2D GenerateOverlay(List<ProvinceData> provinces, int width, int height)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

        var controlled = new List<(int cx, int cy, Color col)>();
        foreach (ProvinceData p in provinces)
        {
            if (p.controllingFactionId < 0) continue;
            Color col = GetFactionOverlayColor(p.controllingFactionId);
            if (col.a <= 0) continue;
            int cx = Mathf.Clamp(Mathf.RoundToInt(p.mapPosition.x * width), 0, width - 1);
            int cy = Mathf.Clamp(Mathf.RoundToInt((1f - p.mapPosition.y) * height), 0, height - 1);
            controlled.Add((cx, cy, col));
        }

        if (controlled.Count == 0) { tex.Apply(); return tex; }

        bool[] landMask = new bool[width * height];
        if (MapLayerController.Instance != null)
        {
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    landMask[y * width + x] = MapLayerController.Instance.IsLandAtUV(
                        (float)x / width, 1f - (float)y / height);
        }
        else
        {
            for (int i = 0; i < landMask.Length; i++) landMask[i] = true;
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (!landMask[y * width + x]) continue;

                float minDistSq = float.MaxValue;
                Color bestCol = Color.clear;
                foreach (var (cx, cy, col) in controlled)
                {
                    float dx = x - cx, dy = y - cy;
                    float d = dx * dx + dy * dy;
                    if (d < minDistSq) { minDistSq = d; bestCol = col; }
                }

                pixels[y * width + x] = bestCol;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    /// <summary>Applies the generated overlay texture to a RawImage component.</summary>
    public static void ApplyOverlayToRawImage(UnityEngine.UI.RawImage rawImage, Texture2D overlay)
    {
        if (rawImage == null) return;

        if (rawImage.texture != null && rawImage.texture != overlay)
        {
            Texture2D old = rawImage.texture as Texture2D;
            if (old != null)
            {
#if UNITY_EDITOR
                if (!UnityEditor.EditorUtility.IsPersistent(old))
                    Object.DestroyImmediate(old);
#else
                Object.Destroy(old);
#endif
            }
        }

        rawImage.texture = overlay;
        rawImage.SetMaterialDirty();
    }
}
