using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TurnLogUI : MonoBehaviour
{
    public Text logText;
    public ScrollRect scrollRect;

    Queue<string> logEntries = new Queue<string>();
    const int MAX_ENTRIES = 10;

    void Awake()
    {
        if (logText == null)
            logText = GetComponentInChildren<Text>();
        if (scrollRect == null)
            scrollRect = GetComponentInChildren<ScrollRect>();
        if (logText != null)
            logText.supportRichText = true;
    }

    public void AddEntry(string message)
    {
        if (logText == null) return;
        string colored = ApplyColorCoding(message);
        logEntries.Enqueue(colored);
        if (logEntries.Count > MAX_ENTRIES)
            logEntries.Dequeue();

        logText.text = string.Join("\n", logEntries);
        Canvas.ForceUpdateCanvases();
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 0f;
    }

    string ApplyColorCoding(string msg)
    {
        string lower = msg.ToLower();
        if (lower.Contains("attacks") || lower.Contains("damage"))
            return $"<color=#cc4444>{msg}</color>";
        if (lower.Contains("shield"))
            return $"<color=#4488cc>{msg}</color>";
        if (lower.Contains("eliminated"))
            return $"<color=#ff2222>{msg}</color>";
        if (lower.Contains("victory") || lower.Contains("dominant"))
            return $"<color=#ffcc00>{msg}</color>";
        if (lower.Contains("sabotage") || lower.Contains("discard"))
            return $"<color=#aa44cc>{msg}</color>";
        if (lower.Contains("draws") || lower.Contains("drew") || lower.Contains("influence") || lower.Contains("secrets"))
            return $"<color=#44cc88>{msg}</color>";
        return $"<color=#d4c8a8>{msg}</color>";
    }
}
