using UnityEngine;

public static class ColorExtensions
{
    public static Color WithA(this Color c, float a)
        => new Color(c.r, c.g, c.b, a);
}
