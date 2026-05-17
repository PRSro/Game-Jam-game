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
    /// Each province's mapped area is painted with its controlling faction's semi-transparent color.
    /// Neutral provinces receive no overlay.</summary>
    public static Texture2D GenerateOverlay(List<ProvinceData> provinces, int width, int height)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;
        tex.SetPixels(pixels);

        Dictionary<int, List<ProvinceData>> byFaction = new Dictionary<int, List<ProvinceData>>();
        foreach (ProvinceData p in provinces)
        {
            if (p.controllingFactionId < 0) continue;
            if (!byFaction.ContainsKey(p.controllingFactionId))
                byFaction[p.controllingFactionId] = new List<ProvinceData>();
            byFaction[p.controllingFactionId].Add(p);
        }

        foreach (var kvp in byFaction)
        {
            Color overlayColor = GetFactionOverlayColor(kvp.Key);
            if (overlayColor.a <= 0) continue;

            foreach (ProvinceData province in kvp.Value)
            {
                PaintProvince(tex, province, overlayColor, width, height);
            }
        }

        tex.Apply();
        return tex;
    }

    /// <summary>Paints a circular region on the overlay texture at the province's map position
    /// with the given faction color. The circle radius is scaled to the texture dimensions.</summary>
    static void PaintProvince(Texture2D tex, ProvinceData province, Color color, int texWidth, int texHeight)
    {
        int cx = Mathf.RoundToInt(province.mapPosition.x * texWidth);
        int cy = Mathf.RoundToInt((1f - province.mapPosition.y) * texHeight);
        int radius = Mathf.RoundToInt(Mathf.Min(texWidth, texHeight) * 0.025f);

        for (int dy = -radius; dy <= radius; dy++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                if (dx * dx + dy * dy > radius * radius) continue;
                int px = cx + dx;
                int py = cy + dy;
                if (px < 0 || px >= texWidth || py < 0 || py >= texHeight) continue;

                tex.SetPixel(px, py, color);
            }
        }
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
