using UnityEngine;

public static class UIFont
{
    private static Font _cached;
    private static bool _loggedFailure;

    public static Font Get()
    {
        if (_cached != null) return _cached;

        _cached = LoadBuiltin("LegacyRuntime.ttf");
        if (_cached != null) return _cached;

        _cached = LoadBuiltin("Arial.ttf");
        if (_cached != null) return _cached;

        foreach (string name in new[] { "Arial", "Liberation Sans", "DejaVu Sans", "Helvetica", "sans-serif" })
        {
            _cached = LoadOSFont(name);
            if (_cached != null) return _cached;
        }

        string[] available = GetInstalledFontNames();
        if (available != null && available.Length > 0)
        {
            foreach (string fontName in available)
            {
                _cached = LoadOSFont(fontName);
                if (_cached != null) return _cached;
            }
        }

        if (!_loggedFailure)
        {
            Debug.LogError("[UIFont] No usable Unity or OS font could be loaded. Legacy Text labels may render blank.");
            _loggedFailure = true;
        }

        _cached = new Font("EmergencyFallback");
        return _cached;
    }

    static Font LoadBuiltin(string resourceName)
    {
        try
        {
            return Resources.GetBuiltinResource<Font>(resourceName);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[UIFont] Failed to load built-in font '{resourceName}': {ex.Message}");
            return null;
        }
    }

    static Font LoadOSFont(string fontName)
    {
        if (string.IsNullOrEmpty(fontName)) return null;
        try
        {
            return Font.CreateDynamicFontFromOSFont(fontName, 12);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[UIFont] Failed to load OS font '{fontName}': {ex.Message}");
            return null;
        }
    }

    static string[] GetInstalledFontNames()
    {
        try
        {
            return Font.GetOSInstalledFontNames();
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[UIFont] Failed to enumerate OS fonts: {ex.Message}");
            return null;
        }
    }
}
