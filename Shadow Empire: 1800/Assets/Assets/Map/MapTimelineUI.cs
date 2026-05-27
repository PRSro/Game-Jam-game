using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MapTimelineUI : MonoBehaviour
{
    Slider timelineSlider;
    Text yearLabel;
    Text dateLabel;

    static readonly Color DarkMahogany = new Color(0.20f, 0.08f, 0.02f);
    static readonly Color Gold = new Color(0.83f, 0.65f, 0.22f);
    static readonly Color Parchment = new Color(0.91f, 0.86f, 0.78f);

    const int START_YEAR = 1790;
    const int END_YEAR = 1990;
    const int TOTAL_YEARS = END_YEAR - START_YEAR;

    void Awake()
    {
        BuildTimeline();
    }

    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "MainMenu" || sceneName.ToLower().Contains("menu"))
        {
            gameObject.SetActive(false);
            return;
        }
        // Hide timeline until GameManager signals the game has begun
        Transform timelineCanvas = transform.Find("TimelineCanvas");
        if (timelineCanvas != null)
            timelineCanvas.gameObject.SetActive(false);

        if (GameManager.Instance != null)
        {
            int turn = GameManager.Instance.currentTurn;
            UpdatePosition(turn);
        }
    }

    public void ShowTimeline()
    {
        Transform timelineCanvas = transform.Find("TimelineCanvas");
        if (timelineCanvas != null)
            timelineCanvas.gameObject.SetActive(true);
    }

    void BuildTimeline()
    {
        GameObject co = new GameObject("TimelineCanvas", typeof(RectTransform));
        co.transform.SetParent(transform, false);
        Canvas canvas = co.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 3;

        CanvasScaler scaler = co.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Timeline is display-only — no raycaster needed
        // co.AddComponent<GraphicRaycaster>();

        Transform ct = co.transform;

        GameObject panel = new GameObject("TimelinePanel", typeof(RectTransform));
        panel.transform.SetParent(ct, false);
        Image panelBg = panel.AddComponent<Image>();
        panelBg.color = DarkMahogany;

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.01f, 0.89f);
        panelRect.anchorMax = new Vector2(0.72f, 0.93f);
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;

        MakeHRule(panel.transform, new Vector2(0.01f, 0.02f), new Vector2(0.99f, 0.03f), new Color(Gold.r, Gold.g, Gold.b, 0.25f));
        MakeHRule(panel.transform, new Vector2(0.01f, 0.97f), new Vector2(0.99f, 0.98f), new Color(Gold.r, Gold.g, Gold.b, 0.25f));

        dateLabel = CreateText(panel.transform, "DateLabel", new Vector2(0.005f, 0.05f), new Vector2(0.15f, 0.95f),
                               12, FontStyle.Bold, Gold, TextAnchor.MiddleLeft);
        dateLabel.text = "January 1790";

        timelineSlider = CreateSlider(panel.transform, "TimelineSlider",
                                       new Vector2(0.17f, 0.10f), new Vector2(0.95f, 0.90f));

        yearLabel = CreateText(panel.transform, "YearLabel", new Vector2(0.88f, 0.05f), new Vector2(0.995f, 0.95f),
                               11, FontStyle.Normal, new Color(Parchment.r, Parchment.g, Parchment.b, 0.70f), TextAnchor.MiddleRight);
        yearLabel.text = "1990";

        CreateYearMarker(panel.transform, 0.022f, "1790");
        CreateYearMarker(panel.transform, 0.155f, "1800");
        CreateYearMarker(panel.transform, 0.333f, "1850");
        CreateYearMarker(panel.transform, 0.511f, "1900");
        CreateYearMarker(panel.transform, 0.689f, "1950");
        CreateYearMarker(panel.transform, 0.978f, "1990");
    }

    Slider CreateSlider(Transform parent, string name, Vector2 aMin, Vector2 aMax)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        Slider slider = go.AddComponent<Slider>();
        slider.interactable = false;
        slider.minValue = 0;
        slider.maxValue = TOTAL_YEARS;
        slider.value = 0;
        slider.direction = Slider.Direction.LeftToRight;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;

        GameObject bgGO = new GameObject("Background", typeof(RectTransform));
        bgGO.transform.SetParent(go.transform, false);
        Image bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.12f, 0.06f, 0.03f);
        RectTransform bgRt = bgGO.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = new Vector2(0, 4);

        GameObject fillGO = new GameObject("FillArea", typeof(RectTransform));
        fillGO.transform.SetParent(go.transform, false);
        RectTransform fillRt = fillGO.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.sizeDelta = Vector2.zero;

        GameObject fillImgGO = new GameObject("Fill", typeof(RectTransform));
        fillImgGO.transform.SetParent(fillGO.transform, false);
        Image fillImg = fillImgGO.AddComponent<Image>();
        fillImg.color = Gold;
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        RectTransform fillImgRt = fillImgGO.GetComponent<RectTransform>();
        fillImgRt.anchorMin = Vector2.zero;
        fillImgRt.anchorMax = Vector2.one;
        fillImgRt.sizeDelta = Vector2.zero;

        slider.fillRect = fillImgRt;
        slider.targetGraphic = bgImg;

        GameObject handleGO = new GameObject("Handle", typeof(RectTransform));
        handleGO.transform.SetParent(go.transform, false);
        Image handleImg = handleGO.AddComponent<Image>();
        handleImg.color = Gold;
        RectTransform handleRt = handleGO.GetComponent<RectTransform>();
        handleRt.anchorMin = new Vector2(0, 0);
        handleRt.anchorMax = new Vector2(0, 1);
        handleRt.sizeDelta = new Vector2(6, 0);
        handleRt.anchoredPosition = Vector2.zero;

        slider.handleRect = handleRt;

        Navigation nav = new Navigation();
        nav.mode = Navigation.Mode.None;
        slider.navigation = nav;

        return slider;
    }

    void CreateYearMarker(Transform parent, float position, string label)
    {
        GameObject marker = new GameObject("Marker_" + label, typeof(RectTransform));
        marker.transform.SetParent(parent, false);
        RectTransform mRt = marker.GetComponent<RectTransform>();
        mRt.anchorMin = new Vector2(position, 0.03f);
        mRt.anchorMax = new Vector2(position, 0.97f);
        mRt.sizeDelta = new Vector2(1, 0);
        mRt.anchoredPosition = Vector2.zero;
        Image mImg = marker.AddComponent<Image>();
        mImg.color = new Color(Gold.r, Gold.g, Gold.b, 0.20f);
        mImg.raycastTarget = false;

        Text labelText = CreateText(parent, "Label_" + label,
                                     new Vector2(position - 0.03f, 0f),
                                     new Vector2(position + 0.03f, 0.18f),
                                     7, FontStyle.Normal, new Color(Parchment.r, Parchment.g, Parchment.b, 0.45f), TextAnchor.UpperCenter);
        labelText.text = label;
    }

    /// <summary>Updates the timeline slider and date label based on the current turn number.</summary>
    public void UpdatePosition(int turn)
    {
        int year = TurnDateManager.GetYear(turn);
        int elapsed = year - START_YEAR;
        float t = Mathf.Clamp01((float)elapsed / TOTAL_YEARS);

        if (timelineSlider != null)
            timelineSlider.value = elapsed;

        if (dateLabel != null)
            dateLabel.text = TurnDateManager.GetDateString(turn);

        if (yearLabel != null)
            yearLabel.text = year.ToString();
    }

    Text CreateText(Transform parent, string name, Vector2 aMin, Vector2 aMax,
                    int size, FontStyle style, Color color, TextAnchor align)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Text t = go.AddComponent<Text>();
        t.font = GetFont();
        t.fontSize = size;
        t.fontStyle = style;
        t.alignment = align;
        t.color = color;
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = aMin;
        r.anchorMax = aMax;
        r.sizeDelta = Vector2.zero;
        r.anchoredPosition = Vector2.zero;
        return t;
    }

    GameObject MakeHRule(Transform parent, Vector2 aMin, Vector2 aMax, Color color)
    {
        GameObject go = new GameObject("HRule", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = color;
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = aMin;
        r.anchorMax = aMax;
        r.sizeDelta = Vector2.zero;
        r.anchoredPosition = Vector2.zero;
        return go;
    }

    static Font GetFont() => UIFont.Get();
}

