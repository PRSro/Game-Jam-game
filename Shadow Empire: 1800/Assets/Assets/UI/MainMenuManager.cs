using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance { get; private set; }

    public Canvas canvas { get; private set; }
    GameObject mainMenuRoot;

    // Palette — identical to FactionSelectionScreen
    static readonly Color BG      = new Color(0.08f, 0.05f, 0.03f);
    static readonly Color Panel   = new Color(0.13f, 0.08f, 0.04f);
    static readonly Color Gold    = new Color(0.78f, 0.58f, 0.16f);
    static readonly Color GoldDim = new Color(0.78f, 0.58f, 0.16f, 0.35f);
    static readonly Color Light   = new Color(0.96f, 0.90f, 0.78f);
    static readonly Color Mid     = new Color(0.83f, 0.66f, 0.41f);
    static readonly Color Dark    = new Color(0.55f, 0.41f, 0.08f);
    static readonly Color Ink     = new Color(0.24f, 0.17f, 0.12f);

    // Rotating quotes
    static readonly string[] Quotes = {
        "\"The world is governed by very different personages from what is\nimagined by those who are not behind the scenes.\"\n— Benjamin Disraeli, 1844",
        "\"Give me control of a nation's money and I care not who makes its laws.\"\n— Attributed to Mayer Amschel Rothschild, c. 1790",
        "\"In politics, nothing happens by accident.\nIf it happens, you can bet it was planned that way.\"\n— Franklin D. Roosevelt",
        "\"All the world's a stage, and the men and women merely players —\nbut who writes the play?\"\n— Adapted from Shakespeare",
        "\"It is well enough that people of the nation do not understand our\nbanking and monetary system, for if they did, I believe there would be\na revolution before tomorrow morning.\"\n— Henry Ford"
    };

    int quoteIndex = 0;
    float quoteTimer = 0f;
    bool fadingQuote = false;
    bool menuBuilt = false;
    Text quoteText;
    CanvasGroup quoteCG;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (menuBuilt) return;
        menuBuilt = true;

        // Destroy any stale UI canvases from previous game sessions
        var staleUI = GameObject.Find("MainCanvas");
        if (staleUI != null) Destroy(staleUI);
        var staleMap = GameObject.Find("MapCanvas");
        if (staleMap != null) Destroy(staleMap);
        var staleTimeline = GameObject.Find("TimelineCanvas");
        if (staleTimeline != null) Destroy(staleTimeline);

        if (!FindObjectOfType<EventSystem>())
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            try
            {
                es.AddComponent<StandaloneInputModule>();
            }
            catch
            {
                Debug.Log("[MainMenu] StandaloneInputModule unavailable; relying on default input module.");
            }
        }
        if (Camera.main != null)
        {
            Camera.main.backgroundColor = BG;
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
        }
        BuildCanvas();
        if (canvas == null)
        {
            Debug.LogError("[MainMenu] Canvas failed to build.");
            return;
        }
        BuildMainMenu();
        StartCoroutine(FadeIn());
    }

    void Update()
    {
        if (quoteText == null || fadingQuote) return;
        quoteTimer += Time.deltaTime;
        if (quoteTimer >= 7f)
        {
            quoteTimer = 0f;
            StartCoroutine(CycleQuote());
        }
    }

    // ── Canvas ────────────────────────────────────────────────────────────────
    void BuildCanvas()
    {
        GameObject go = new GameObject("MenuCanvas");
        go.transform.SetParent(transform, false);
        canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler s = go.AddComponent<CanvasScaler>();
        s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        s.referenceResolution = new Vector2(1280, 720);  // matches FactionSelectionScreen
        s.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        s.matchWidthOrHeight = 0f;  // scale by width — stable at any aspect ratio
        go.AddComponent<GraphicRaycaster>();
    }

    // ── Main menu ─────────────────────────────────────────────────────────────
    void BuildMainMenu()
    {
        mainMenuRoot = MakeRect("MainMenuRoot", canvas.transform, V(0,0), V(1,1));
        mainMenuRoot.AddComponent<Image>().color = BG;
        mainMenuRoot.GetComponent<Image>().raycastTarget = false;
        mainMenuRoot.AddComponent<CanvasGroup>().alpha = 0f;

        // Left gold accent bar
        var leftAccent = MakeRect("LeftAccent", mainMenuRoot.transform, V(0.035f,0.06f), V(0.040f,0.94f))
            .AddComponent<Image>();
        leftAccent.color = GoldDim;
        leftAccent.raycastTarget = false;

        // Title block (left 60%)
        MakeText("Title", mainMenuRoot.transform, V(0.055f,0.72f), V(0.62f,0.94f),
            "SHADOW EMPIRES", 58, FontStyle.Bold, Gold, TextAnchor.LowerLeft);

        MakeText("Year", mainMenuRoot.transform, V(0.055f,0.64f), V(0.62f,0.73f),
            "1800", 30, FontStyle.Normal, Mid, TextAnchor.UpperLeft);

        MakeText("Tagline", mainMenuRoot.transform, V(0.055f,0.57f), V(0.62f,0.645f),
            "The invisible war for the world", 15, FontStyle.Italic, Dark, TextAnchor.UpperLeft);

        // Gold divider under tagline
        var dividerImg = MakeRect("Divider", mainMenuRoot.transform, V(0.055f,0.558f), V(0.60f,0.563f))
            .AddComponent<Image>();
        dividerImg.color = GoldDim;
        dividerImg.raycastTarget = false;

        // Rotating quote (left 60%, below divider)
        GameObject qGO = MakeRect("Quote", mainMenuRoot.transform, V(0.055f,0.30f), V(0.60f,0.555f));
        quoteCG = qGO.AddComponent<CanvasGroup>();
        quoteText = qGO.AddComponent<Text>();
        quoteText.font = GetFont();
        quoteText.fontSize = 13;
        quoteText.fontStyle = FontStyle.Italic;
        quoteText.color = new Color(Mid.r, Mid.g, Mid.b, 0.85f);
        quoteText.alignment = TextAnchor.UpperLeft;
        quoteText.horizontalOverflow = HorizontalWrapMode.Wrap;
        quoteText.verticalOverflow = VerticalWrapMode.Overflow;
        quoteText.text = Quotes[0];

        // Version line
        MakeText("Version", mainMenuRoot.transform, V(0.055f,0.04f), V(0.55f,0.10f),
            "A Conspiracy Strategy Game  ·  Game Jam 2025",
            10, FontStyle.Italic, new Color(Dark.r, Dark.g, Dark.b, 0.45f), TextAnchor.MiddleLeft);

        // Right button panel (right 30% of screen, vertically centered)
        GameObject btnPanel = MakeRect("BtnPanel", mainMenuRoot.transform,
            V(0.66f, 0.28f), V(0.97f, 0.82f));
        var btnPanelImg = btnPanel.AddComponent<Image>();
        btnPanelImg.color = new Color(Panel.r, Panel.g, Panel.b, 0.85f);
        btnPanelImg.raycastTarget = false;

        // Gold border around button panel
        var btnBorderImg = MakeRect("BtnBorder", mainMenuRoot.transform,
            V(0.657f,0.277f), V(0.973f,0.823f)).AddComponent<Image>();
        btnBorderImg.color = GoldDim;
        btnBorderImg.raycastTarget = false;

        // 2 buttons inside the panel
        string[] labels  = { "SINGLEPLAYER", "QUIT" };
        float[]  yMins   = { 0.60f, 0.20f };
        float[]  yMaxs   = { 0.80f, 0.40f };
        System.Action[] actions = {
            ShowFactionSelectionScreen,
            Application.Quit
        };
        for (int i = 0; i < labels.Length; i++)
        {
            int cap = i;
            MakeButton(btnPanel.transform, labels[i],
                V(0.07f, yMins[i]), V(0.93f, yMaxs[i]),
                () => actions[cap]());
        }

        // Corner ornaments (top-left only, subtle)
        var ornH = MakeRect("OrnH", mainMenuRoot.transform, V(0.01f,0.945f), V(0.055f,0.958f))
            .AddComponent<Image>();
        ornH.color = new Color(Dark.r, Dark.g, Dark.b, 0.4f);
        ornH.raycastTarget = false;
        var ornV = MakeRect("OrnV", mainMenuRoot.transform, V(0.01f,0.945f), V(0.022f,0.990f))
            .AddComponent<Image>();
        ornV.color = new Color(Dark.r, Dark.g, Dark.b, 0.4f);
        ornV.raycastTarget = false;
    }

    void MakeButton(Transform parent, string label, Vector2 aMin, Vector2 aMax,
        System.Action onClick)
    {
        GameObject go = MakeRect("Btn_" + label, parent, aMin, aMax);
        Image bg = go.AddComponent<Image>(); bg.color = Ink;

        Outline outline = go.AddComponent<Outline>();
        outline.effectColor = Gold;
        outline.effectDistance = new Vector2(2, -2);

        GameObject lbl = MakeText("Lbl", go.transform, V(0,0), V(1,1),
            label, 15, FontStyle.Bold, Light, TextAnchor.MiddleCenter);
        lbl.GetComponent<Text>().raycastTarget = false;

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = bg;
        btn.navigation = new Navigation { mode = Navigation.Mode.None };
        btn.onClick.AddListener(() => onClick());

        var hover = go.AddComponent<ButtonHover>();
        hover.onEnter = () => { bg.color = new Color(0.22f,0.12f,0.04f); outline.effectColor = Mid; };
        hover.onExit  = () => { bg.color = Ink; outline.effectColor = Gold; };
    }

    // ── Overlays ──────────────────────────────────────────────────────────────

    // ── Navigation ────────────────────────────────────────────────────────────
    void ShowFactionSelectionScreen()
    {
        mainMenuRoot.SetActive(false);
        GameObject fssGO = new GameObject("FactionSelection");
        fssGO.transform.SetParent(canvas.transform, false);
        fssGO.AddComponent<FactionSelectionScreen>().Show();
    }

    public void RestoreMainMenu()
    {
        mainMenuRoot.SetActive(true);
    }

    // ── Animations ────────────────────────────────────────────────────────────
    IEnumerator FadeIn()
    {
        CanvasGroup cg = mainMenuRoot.GetComponent<CanvasGroup>();
        cg.interactable = false;
        cg.blocksRaycasts = false;
        float t = 0f;
        while (t < 1.0f)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Clamp01(t);
            yield return null;
        }
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    IEnumerator CycleQuote()
    {
        fadingQuote = true;
        float t = 0f;
        while (t < 0.4f) { t += Time.deltaTime; quoteCG.alpha = 1f - t/0.4f; yield return null; }
        quoteCG.alpha = 0f;
        quoteIndex = (quoteIndex + 1) % Quotes.Length;
        quoteText.text = Quotes[quoteIndex];
        t = 0f;
        while (t < 0.4f) { t += Time.deltaTime; quoteCG.alpha = t/0.4f; yield return null; }
        quoteCG.alpha = 1f;
        fadingQuote = false;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    static Vector2 V(float x, float y) => new Vector2(x, y);

    static GameObject MakeRect(string name, Transform parent, Vector2 aMin, Vector2 aMax)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = aMin; r.anchorMax = aMax;
        r.sizeDelta = Vector2.zero; r.anchoredPosition = Vector2.zero;
        return go;
    }

    static GameObject MakeText(string name, Transform parent, Vector2 aMin, Vector2 aMax,
        string txt, int size, FontStyle style, Color color, TextAnchor align)
    {
        GameObject go = MakeRect(name, parent, aMin, aMax);
        Text t = go.AddComponent<Text>();
        t.text = txt;
        t.font = GetFont();
        t.fontSize = size; t.fontStyle = style; t.color = color; t.alignment = align;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return go;
    }

    class ButtonHover : MonoBehaviour,
        UnityEngine.EventSystems.IPointerEnterHandler,
        UnityEngine.EventSystems.IPointerExitHandler
    {
        public System.Action onEnter;
        public System.Action onExit;
        public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData e)
            => onEnter?.Invoke();
        public void OnPointerExit(UnityEngine.EventSystems.PointerEventData e)
            => onExit?.Invoke();
    }

    static Font GetFont() => UIFont.Get();
}
