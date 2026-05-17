using UnityEngine;

public static class RoundedRectGenerator
{
    public static Sprite Generate(
        int width, int height,
        int cornerRadius,
        Color fillColor,
        Color borderColor,
        int borderThickness = 3)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;

        int r = Mathf.Min(cornerRadius, Mathf.Min(width, height) / 2);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dist = DistanceToRoundedRect(x, y, width, height, r);

                if (dist <= 0f)
                {
                    bool isBorder = dist > -borderThickness;
                    pixels[y * width + x] = isBorder ? borderColor : fillColor;
                }
                else if (dist < 1f)
                {
                    Color edge = borderColor;
                    edge.a = 1f - dist;
                    pixels[y * width + x] = edge;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        Sprite sprite = Sprite.Create(
            tex,
            new Rect(0, 0, width, height),
            new Vector2(0.5f, 0.5f),
            100f);
        return sprite;
    }

    static float DistanceToRoundedRect(float px, float py, int w, int h, int r)
    {
        float cx = Mathf.Clamp(px, r, w - r);
        float cy = Mathf.Clamp(py, r, h - r);
        float dx = px - cx;
        float dy = py - cy;
        return Mathf.Sqrt(dx * dx + dy * dy) - r;
    }
}
