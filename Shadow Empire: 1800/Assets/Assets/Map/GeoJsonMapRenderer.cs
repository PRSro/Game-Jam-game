using System.Collections.Generic;
using UnityEngine;

// NOTE: This class is unused — kept for future live API integration.
// MapAPIFetcher.cs handles all map generation currently.
public static class GeoJsonMapRenderer
{
    static readonly Color BorderColor = new Color(0.231f, 0.125f, 0.031f);
    static readonly Color MaskColor = new Color(0.78f, 0.68f, 0.52f);

    static readonly double LonMin = -15.0;
    static readonly double LonMax = 60.0;
    static readonly double LatMin = 30.0;
    static readonly double LatMax = 75.0;

    static readonly int TexWidth = 1920;
    static readonly int TexHeight = 1080;

    /// <summary>Parses a GeoJSON text and renders it to a 1920x1080 Texture2D focused on Europe.</summary>
    public static Texture2D RenderGeoJsonToTexture(string geoJsonText)
    {
        Texture2D tex = new Texture2D(TexWidth, TexHeight, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[TexWidth * TexHeight];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = MaskColor;

        List<ParsedFeature> features = ParseFeatures(geoJsonText);
        Debug.Log("[GeoJson] Rendering, features count: " + features.Count);

        foreach (ParsedFeature feature in features)
        {
            Color fillColor = GetColorForName(feature.name);

            foreach (var polygonGroup in feature.polygonGroups)
            {
                List<List<Vector2Int>> rings = new List<List<Vector2Int>>();
                foreach (var ringCoords in polygonGroup)
                {
                    List<Vector2Int> pixelRing = new List<Vector2Int>();
                    foreach (Vector2d geo in ringCoords)
                    {
                        Vector2Int px = GeoToPixel(geo);
                        pixelRing.Add(px);
                    }
                    if (pixelRing.Count >= 3)
                        rings.Add(pixelRing);
                }
                if (rings.Count > 0)
                {
                    FillPolygon(pixels, rings, fillColor);
                    DrawBorders(pixels, rings);
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        Debug.Log("[GeoJson] Texture applied: " + tex.width + "x" + tex.height + " pixels");
        return tex;
    }

    static List<ParsedFeature> ParseFeatures(string json)
    {
        List<ParsedFeature> features = new List<ParsedFeature>();

        int featIdx = json.IndexOf("\"features\"");
        if (featIdx < 0) return features;

        int arrayStart = json.IndexOf('[', featIdx);
        if (arrayStart < 0) return features;

        int braceDepth = 0;
        int featureStart = -1;

        for (int i = arrayStart; i < json.Length; i++)
        {
            char c = json[i];
            if (c == '{')
            {
                if (braceDepth == 0)
                    featureStart = i;
                braceDepth++;
            }
            else if (c == '}')
            {
                braceDepth--;
                if (braceDepth == 0 && featureStart >= 0)
                {
                    string featureBlock = json.Substring(featureStart, i - featureStart + 1);
                    ParsedFeature pf = ParseSingleFeature(featureBlock);
                    if (pf.name != null)
                        features.Add(pf);
                    featureStart = -1;
                }
            }
        }

        return features;
    }

    static ParsedFeature ParseSingleFeature(string block)
    {
        ParsedFeature pf = ParsedFeature.Create();
        pf.name = ExtractJsonString(block, "name");
        if (pf.name == null) pf.name = ExtractJsonString(block, "NAME");
        if (pf.name == null) pf.name = "Unknown";

        string geomType = ExtractJsonString(block, "type", "geometry");
        if (geomType == null) return pf;
        if (geomType != "Polygon" && geomType != "MultiPolygon")
            return pf;

        string coordsJson = ExtractCoordinatesJson(block);
        if (coordsJson == null) return pf;

        if (geomType == "Polygon")
        {
            var groups = ParsePolygonCoordinates(coordsJson);
            if (groups.Count > 0)
                pf.polygonGroups.Add(groups);
        }
        else if (geomType == "MultiPolygon")
        {
            var multi = ParseMultiPolygonCoordinates(coordsJson);
            pf.polygonGroups.AddRange(multi);
        }

        return pf;
    }

    static string ExtractJsonString(string json, string key, string parentKey = null)
    {
        string searchArea = json;
        if (parentKey != null)
        {
            int parentIdx = json.IndexOf("\"" + parentKey + "\"");
            if (parentIdx < 0) return null;
            int braceStart = json.IndexOf('{', parentIdx);
            if (braceStart < 0) return null;
            int braceEnd = FindMatchingBrace(json, braceStart);
            if (braceEnd < 0) return null;
            searchArea = json.Substring(braceStart, braceEnd - braceStart + 1);
        }

        string search = "\"" + key + "\"";
        int idx = searchArea.IndexOf(search);
        if (idx < 0) return null;

        int colon = searchArea.IndexOf(':', idx);
        if (colon < 0) return null;

        int start = colon + 1;
        while (start < searchArea.Length && char.IsWhiteSpace(searchArea[start]))
            start++;

        if (start >= searchArea.Length) return null;

        if (searchArea[start] == '"')
        {
            start++;
            int end = start;
            while (end < searchArea.Length && searchArea[end] != '"')
            {
                if (searchArea[end] == '\\') end++;
                end++;
            }
            return searchArea.Substring(start, end - start);
        }

        return null;
    }

    static string ExtractCoordinatesJson(string block)
    {
        int coordIdx = block.IndexOf("\"coordinates\"");
        if (coordIdx < 0) return null;
        int colon = block.IndexOf(':', coordIdx);
        if (colon < 0) return null;

        int start = colon + 1;
        while (start < block.Length && char.IsWhiteSpace(block[start]))
            start++;

        if (start >= block.Length) return null;

        int depth = 0;
        int end = start;
        for (int i = start; i < block.Length; i++)
        {
            char c = block[i];
            if (c == '[') depth++;
            else if (c == ']')
            {
                depth--;
                if (depth == 0) { end = i + 1; break; }
            }
        }

        return block.Substring(start, end - start);
    }

    static List<List<Vector2d>> ParsePolygonCoordinates(string json)
    {
        List<List<Vector2d>> rings = new List<List<Vector2d>>();
        int i = json.IndexOf('[');
        if (i < 0) return rings;
        i++;
        while (i < json.Length)
        {
            while (i < json.Length && (json[i] == ',' || char.IsWhiteSpace(json[i]))) i++;
            if (i >= json.Length || json[i] == ']') break;
            if (json[i] != '[') { i++; continue; }
            var ring = ParsePointArray(json, ref i);
            if (ring.Count >= 3) rings.Add(ring);
        }
        return rings;
    }

    static List<List<List<Vector2d>>> ParseMultiPolygonCoordinates(string json)
    {
        var result = new List<List<List<Vector2d>>>();
        int i = json.IndexOf('[');
        if (i < 0) return result;
        i++;
        while (i < json.Length)
        {
            while (i < json.Length && (json[i] == ',' || char.IsWhiteSpace(json[i]))) i++;
            if (i >= json.Length || json[i] == ']') break;
            if (json[i] != '[') { i++; continue; }
            var polygon = ParseRingArray(json, ref i);
            if (polygon.Count > 0) result.Add(polygon);
        }
        return result;
    }

    static List<List<Vector2d>> ParseRingArray(string json, ref int i)
    {
        var rings = new List<List<Vector2d>>();
        if (i >= json.Length || json[i] != '[') return rings;
        i++;
        while (i < json.Length)
        {
            while (i < json.Length && (json[i] == ',' || char.IsWhiteSpace(json[i]))) i++;
            if (i >= json.Length || json[i] == ']') break;
            if (json[i] != '[') { i++; continue; }
            var ring = ParsePointArray(json, ref i);
            if (ring.Count >= 3) rings.Add(ring);
        }
        if (i < json.Length && json[i] == ']') i++;
        return rings;
    }

    static List<Vector2d> ParsePointArray(string json, ref int i)
    {
        var points = new List<Vector2d>();
        if (i >= json.Length || json[i] != '[') return points;
        i++;
        while (i < json.Length)
        {
            while (i < json.Length && (json[i] == ',' || char.IsWhiteSpace(json[i]))) i++;
            if (i >= json.Length || json[i] == ']') break;
            if (json[i] != '[') { i++; continue; }
            i++;
            double lon = 0, lat = 0;
            ParseNumber(json, ref i, out lon);
            while (i < json.Length && (json[i] == ',' || char.IsWhiteSpace(json[i]))) i++;
            ParseNumber(json, ref i, out lat);
            while (i < json.Length && json[i] != ']') i++;
            if (i < json.Length) i++;
            points.Add(new Vector2d(lon, lat));
        }
        if (i < json.Length && json[i] == ']') i++;
        return points;
    }

    static void ParseNumber(string json, ref int i, out double value)
    {
        int start = i;
        if (i < json.Length && (json[i] == '-' || json[i] == '+')) i++;
        while (i < json.Length && (char.IsDigit(json[i]) || json[i] == '.' ||
               json[i] == 'e' || json[i] == 'E' || json[i] == '-' || json[i] == '+'))
            i++;
        double.TryParse(json.Substring(start, i - start).Trim(),
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out value);
    }

    static Color GetColorForName(string name)
    {
        int hash = name.GetHashCode();
        float hue = (Mathf.Abs(hash) % 360) / 360f;
        float sat = 0.55f + (Mathf.Abs(hash >> 8) % 5) * 0.03f;
        float val = 0.60f + (Mathf.Abs(hash >> 16) % 5) * 0.03f;
        Color col = Color.HSVToRGB(hue, sat, val);
        return col;
    }

    static Vector2Int GeoToPixel(Vector2d geo)
    {
        double x = (geo.x - LonMin) / (LonMax - LonMin) * TexWidth;
        double y = (geo.y - LatMin) / (LatMax - LatMin) * TexHeight;
        return new Vector2Int(Mathf.Clamp((int)x, 0, TexWidth - 1),
                              Mathf.Clamp((int)y, 0, TexHeight - 1));
    }

    static void FillPolygon(Color[] pixels, List<List<Vector2Int>> rings, Color fillColor)
    {
        if (rings.Count == 0) return;
        List<Vector2Int> exterior = rings[0];
        if (exterior.Count < 3) return;

        int minY = TexHeight, maxY = 0;
        foreach (Vector2Int p in exterior)
        {
            if (p.y < minY) minY = p.y;
            if (p.y > maxY) maxY = p.y;
        }
        minY = Mathf.Max(0, minY);
        maxY = Mathf.Min(TexHeight - 1, maxY);

        List<List<Vector2Int>> holes = new List<List<Vector2Int>>();
        for (int i = 1; i < rings.Count; i++)
            holes.Add(rings[i]);

        for (int y = minY; y <= maxY; y++)
        {
            List<int> intersections = GetIntersections(y, exterior);
            foreach (var hole in holes)
                intersections.AddRange(GetIntersections(y, hole));

            intersections.Sort();

            for (int i = 0; i + 1 < intersections.Count; i += 2)
            {
                int xStart = Mathf.Max(0, intersections[i]);
                int xEnd = Mathf.Min(TexWidth - 1, intersections[i + 1]);
                for (int x = xStart; x <= xEnd; x++)
                    pixels[y * TexWidth + x] = fillColor;
            }
        }
    }

    static List<int> GetIntersections(int scanY, List<Vector2Int> verts)
    {
        List<int> xs = new List<int>();
        int n = verts.Count;
        for (int i = 0; i < n; i++)
        {
            Vector2Int a = verts[i];
            Vector2Int b = verts[(i + 1) % n];

            if (a.y == b.y) continue;

            int yMin = Mathf.Min(a.y, b.y);
            int yMax = Mathf.Max(a.y, b.y);

            if (scanY < yMin || scanY >= yMax) continue;

            double t = (double)(scanY - a.y) / (b.y - a.y);
            double x = a.x + t * (b.x - a.x);
            xs.Add((int)x);
        }
        return xs;
    }

    static void DrawBorders(Color[] pixels, List<List<Vector2Int>> rings)
    {
        int stroke = 2;
        foreach (var ring in rings)
        {
            for (int i = 0; i < ring.Count; i++)
            {
                Vector2Int a = ring[i];
                Vector2Int b = ring[(i + 1) % ring.Count];
                DrawLine(pixels, a.x, a.y, b.x, b.y, stroke);
            }
        }
    }

    static void DrawLine(Color[] pixels, int x0, int y0, int x1, int y1, int thickness)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = -Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx + dy;

        while (true)
        {
            for (int ty = -thickness; ty <= thickness; ty++)
            {
                for (int tx = -thickness; tx <= thickness; tx++)
                {
                    int px = x0 + tx;
                    int py = y0 + ty;
                    if (px >= 0 && px < TexWidth && py >= 0 && py < TexHeight)
                        pixels[py * TexWidth + px] = BorderColor;
                }
            }

            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 >= dy) { err += dy; x0 += sx; }
            if (e2 <= dx) { err += dx; y0 += sy; }
        }
    }

    static int FindMatchingBrace(string json, int openIdx)
    {
        if (openIdx < 0 || json[openIdx] != '{') return -1;
        int depth = 0;
        for (int i = openIdx; i < json.Length; i++)
        {
            if (json[i] == '{') depth++;
            else if (json[i] == '}')
            {
                depth--;
                if (depth == 0) return i;
            }
        }
        return -1;
    }

    struct ParsedFeature
    {
        public string name;
        public List<List<List<Vector2d>>> polygonGroups;

        public static ParsedFeature Create()
        {
            return new ParsedFeature
            {
                name = null,
                polygonGroups = new List<List<List<Vector2d>>>()
            };
        }
    }

    public struct Vector2d
    {
        public double x;
        public double y;
        public Vector2d(double x, double y) { this.x = x; this.y = y; }
    }
}
