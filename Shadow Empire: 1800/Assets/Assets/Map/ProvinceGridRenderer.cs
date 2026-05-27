using System.Collections.Generic;
using UnityEngine;

public static class ProvinceGridRenderer
{
    public static Texture2D GenerateGridTexture(List<ProvinceData> provinces, int width, int height)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;

        Color dotColor = new Color(1f, 1f, 1f, 0.9f);
        Color gridLineColor = new Color(1f, 1f, 1f, 0.10f);

        for (float frac = 0.1f; frac < 1f; frac += 0.1f)
        {
            int lx = Mathf.RoundToInt(frac * width);
            for (int y = 0; y < height; y++)
                pixels[y * width + lx] = gridLineColor;

            int ly = Mathf.RoundToInt(frac * height);
            for (int x = 0; x < width; x++)
                pixels[ly * width + x] = gridLineColor;
        }

        foreach (ProvinceData p in provinces)
        {
            int cx = Mathf.RoundToInt(p.mapPosition.x * width);
            int cy = Mathf.RoundToInt((1f - p.mapPosition.y) * height);
            int r = 6;

            for (int dy = -r; dy <= r; dy++)
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    if (dx * dx + dy * dy > r * r) continue;
                    int px = cx + dx;
                    int py = cy + dy;
                    if (px < 0 || px >= width || py < 0 || py >= height) continue;
                    pixels[py * width + px] = dotColor;
                }
            }

            Color ringColor = Color.grey;
            if (p.controllingFactionId >= 0)
            {
                FactionData fd = FactionDatabase.GetFaction(p.controllingFactionId);
                if (fd != null)
                    ringColor = fd.factionColor;
            }

            int innerR = r - 2;
            for (int dy = -r; dy <= r; dy++)
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    int d2 = dx * dx + dy * dy;
                    if (d2 <= innerR * innerR || d2 > r * r) continue;
                    int px = cx + dx;
                    int py = cy + dy;
                    if (px < 0 || px >= width || py < 0 || py >= height) continue;
                    pixels[py * width + px] = ringColor;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        tex.name = "ProvinceGrid";
        return tex;
    }
}
