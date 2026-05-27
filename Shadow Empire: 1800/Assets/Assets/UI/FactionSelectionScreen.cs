using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class FactionSelectionScreen : MonoBehaviour
{
    // Palette — matches MainMenuManager
    static readonly Color BG      = new Color(0.08f, 0.05f, 0.03f);
    static readonly Color Panel   = new Color(0.13f, 0.08f, 0.04f);
    static readonly Color Gold    = new Color(0.78f, 0.58f, 0.16f);
    static readonly Color GoldDim = new Color(0.78f, 0.58f, 0.16f, 0.35f);
    static readonly Color Light   = new Color(0.96f, 0.90f, 0.78f);
    static readonly Color Mid     = new Color(0.83f, 0.66f, 0.41f);
    static readonly Color Dark    = new Color(0.55f, 0.41f, 0.08f);
    static readonly Color Ink     = new Color(0.24f, 0.17f, 0.12f);

    // Faction lore
    static readonly string[] Lore = {
        "Founded in Bavaria, 1776. Masters of infiltration, espionage, and the slow conquest of minds. They dismantle tyranny through enlightenment.",
        "Suppressed in 1312 but never destroyed. The Templars command vast hidden wealth, fierce military discipline, and unbreakable oaths.",
        "Rising from medieval stonemason guilds, they wove a network across every court in Europe. Knowledge is their true weapon.",
        "Born in Napoleonic fire, they fight for liberty through conspiracy and revolt. Ruthless, passionate, rooted in the common people."
    };
    static readonly string[] Specialty = {
        "Influence & Secrets",
        "Military Power & Holy War",
        "Brotherhood Networks & Defence",
        "Territory Control & Uprising"
    };
    static readonly string[] Motto = {
        "\"Illumina et Regna\" — Enlighten and Rule",
        "\"Non nobis, Domine\" — Not unto us, O Lord",
        "\"Ordo ab Chao\" — Order from Chaos",
        "\"Libert\u00E0 o Morte\" — Liberty or Death"
    };
    static readonly float[] StatPower     = { 0.50f, 0.85f, 0.60f, 0.70f };
    static readonly float[] StatInfluence = { 0.90f, 0.45f, 0.65f, 0.40f };
    static readonly float[] StatSecrets   = { 0.80f, 0.30f, 0.50f, 0.25f };

    int selectedFaction = 0;
    bool confirmed = false;

    // Card state
    Image[] cardBg;
    Image[] cardBorder;
    Text[] cardSelectLabel;

    // Detail panel texts
    Text detailName, detailSpec, detailLore, detailMotto;
    Text confirmLabel;
    Image confirmBorder, confirmBg;

    List<FactionData> factions;

    // ── Entry ─────────────────────────────────────────────────────────────────
    public void Show()
    {
        factions = FactionDatabase.GetAllFactions();
        BuildUI();
        SelectFaction(0);
        StartCoroutine(FadeIn());
    }

    // ── Build ─────────────────────────────────────────────────────────────────
    void BuildUI()
    {
        // Root — fills the parent canvas
        RectTransform r = gameObject.AddComponent<RectTransform>();
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.sizeDelta = Vector2.zero; r.anchoredPosition = Vector2.zero;
        gameObject.AddComponent<Image>().color = BG;
        gameObject.AddComponent<CanvasGroup>().alpha = 0f;

        // ── Header strip ──────────────────────────────────────────────────────
        MakeRect("TopBar", transform, V(0,0.92f), V(1,1))
            .AddComponent<Image>().color = new Color(Panel.r, Panel.g, Panel.b, 0.95f);
        MakeRect("TopDivider", transform, V(0,0.915f), V(1,0.921f))
            .AddComponent<Image>().color = GoldDim;

        MakeText("Title", transform, V(0.01f,0.93f), V(0.99f,0.998f),
            "CHOOSE YOUR SECRET SOCIETY", 30, FontStyle.Bold, Gold,
            TextAnchor.MiddleCenter);
        MakeText("Subtitle", transform, V(0.01f,0.89f), V(0.99f,0.93f),
            "The invisible war for Europe begins in 1800. Your hand will shape history.",
            13, FontStyle.Italic, Dark, TextAnchor.MiddleCenter);

        // ── Bottom bar ────────────────────────────────────────────────────────
        MakeRect("BottomBar", transform, V(0,0), V(1,0.10f))
            .AddComponent<Image>().color = new Color(Panel.r, Panel.g, Panel.b, 0.95f);
        MakeRect("BottomDiv", transform, V(0,0.099f), V(1,0.101f))
            .AddComponent<Image>().color = GoldDim;

        // Confirm button (centre bottom)
        GameObject confirmGO = MakeRect("Confirm", transform, V(0.28f,0.015f), V(0.72f,0.082f));
        confirmBorder = confirmGO.AddComponent<Image>(); confirmBorder.color = Gold;
        GameObject confirmInner = MakeRect("Inner", confirmGO.transform, V(0,0), V(1,1));
        confirmInner.GetComponent<RectTransform>().sizeDelta = new Vector2(-3,-3);
        confirmBg = confirmInner.AddComponent<Image>(); confirmBg.color = Ink;
        confirmLabel = MakeText("Label", confirmGO.transform, V(0,0), V(1,1),
            "BEGIN", 18, FontStyle.Bold, Light, TextAnchor.MiddleCenter).GetComponent<Text>();
        AddButton(confirmGO, confirmBg,
            () => { confirmBg.color = new Color(0.22f,0.12f,0.04f); confirmBorder.color = Mid; },
            () => { confirmBg.color = Ink; confirmBorder.color = Gold; },
            OnConfirm);

        // Back button (left bottom)
        GameObject backGO = MakeRect("Back", transform, V(0.01f,0.015f), V(0.16f,0.082f));
        Image backBorder = backGO.AddComponent<Image>(); backBorder.color = GoldDim;
        GameObject backInner = MakeRect("Inner", backGO.transform, V(0,0), V(1,1));
        backInner.GetComponent<RectTransform>().sizeDelta = new Vector2(-2,-2);
        Image backBg = backInner.AddComponent<Image>(); backBg.color = Ink;
        MakeText("Label", backGO.transform, V(0,0), V(1,1),
            "\u25C0 BACK", 14, FontStyle.Normal, Dark, TextAnchor.MiddleCenter);
        AddButton(backGO, backBg,
            () => backBg.color = new Color(0.16f,0.08f,0.03f),
            () => backBg.color = Ink,
            OnBack);

        // ── Faction cards (left 60%) ──────────────────────────────────────────
        cardBg = new Image[factions.Count];
        cardBorder = new Image[factions.Count];
        cardSelectLabel = new Text[factions.Count];

        float cardW = 0.148f;
        float gap   = 0.008f;
        float startX = 0.005f;

        for (int i = 0; i < factions.Count; i++)
        {
            float x0 = startX + i * (cardW + gap);
            float x1 = x0 + cardW;
            BuildCard(i, V(x0, 0.11f), V(x1, 0.89f));
        }

        // ── Detail panel (right 38%) ──────────────────────────────────────────
        BuildDetailPanel(V(0.618f, 0.11f), V(0.995f, 0.89f));
    }

    void BuildCard(int index, Vector2 aMin, Vector2 aMax)
    {
        FactionData f = factions[index];
        GameObject card = MakeRect("Card_" + index, transform, aMin, aMax);

        cardBorder[index] = card.AddComponent<Image>();
        cardBorder[index].color = new Color(f.factionColor.r*0.5f, f.factionColor.g*0.5f, f.factionColor.b*0.5f);

        GameObject inner = MakeRect("Inner", card.transform, V(0,0), V(1,1));
        inner.GetComponent<RectTransform>().sizeDelta = new Vector2(-4,-4);
        cardBg[index] = inner.AddComponent<Image>();
        cardBg[index].color = new Color(0.10f, 0.06f, 0.03f);

        // Faction colour top accent
        MakeRect("Accent", inner.transform, V(0,0.88f), V(1,1))
            .AddComponent<Image>().color = new Color(f.factionColor.r, f.factionColor.g, f.factionColor.b, 0.6f);

        // Name
        MakeText("Name", inner.transform, V(0.04f,0.76f), V(0.96f,0.89f),
            f.factionName.ToUpper(), 14, FontStyle.Bold, f.factionColor,
            TextAnchor.MiddleCenter);

        // Divider
        MakeRect("NameDiv", inner.transform, V(0.08f,0.748f), V(0.92f,0.753f))
            .AddComponent<Image>().color = GoldDim;

        // Symbol
        string[] syms = { "\u2726", "\u2720", "\u229E", "\u2692" };
        Text sym = MakeText("Symbol", inner.transform, V(0.1f,0.50f), V(0.9f,0.75f),
            index < syms.Length ? syms[index] : "\u2605",
            44, FontStyle.Normal,
            new Color(f.factionColor.r, f.factionColor.g, f.factionColor.b, 0.25f),
            TextAnchor.MiddleCenter).GetComponent<Text>();

        // Stat bars
        BuildStatBar(inner.transform, "PWR", StatPower[index],
            V(0.06f,0.36f), V(0.94f,0.46f), f.factionColor);
        BuildStatBar(inner.transform, "INF", StatInfluence[index],
            V(0.06f,0.24f), V(0.94f,0.34f), Gold);
        BuildStatBar(inner.transform, "SEC", StatSecrets[index],
            V(0.06f,0.12f), V(0.94f,0.22f), Mid);

        // Select label
        cardSelectLabel[index] = MakeText("SelLabel", inner.transform,
            V(0.02f,0.01f), V(0.98f,0.11f),
            "CLICK TO SELECT", 9, FontStyle.Italic,
            new Color(Dark.r, Dark.g, Dark.b, 0.5f),
            TextAnchor.MiddleCenter).GetComponent<Text>();

        // Click + hover
        int cap = index;
        AddButton(card, cardBg[cap],
            () => { if (cap != selectedFaction) cardBg[cap].color = new Color(0.14f,0.07f,0.03f); },
            () => { if (cap != selectedFaction) cardBg[cap].color = new Color(0.10f,0.06f,0.03f); },
            () => SelectFaction(cap));
    }

    void BuildStatBar(Transform parent, string label, float value,
        Vector2 aMin, Vector2 aMax, Color barColor)
    {
        GameObject c = MakeRect("Stat_" + label, parent, aMin, aMax);
        MakeText("Lbl", c.transform, V(0,0.5f), V(1,1), label, 8,
            FontStyle.Normal, Dark, TextAnchor.MiddleLeft);
        GameObject track = MakeRect("Track", c.transform, V(0,0), V(1,0.5f));
        track.AddComponent<Image>().color = new Color(0.05f,0.03f,0.01f);
        GameObject fill = MakeRect("Fill", track.transform, V(0,0), V(Mathf.Clamp01(value),1));
        fill.AddComponent<Image>().color = barColor;
    }

    void BuildDetailPanel(Vector2 aMin, Vector2 aMax)
    {
        GameObject p = MakeRect("Detail", transform, aMin, aMax);
        p.AddComponent<Image>().color = new Color(0.07f,0.035f,0.015f,0.96f);
        MakeRect("Border", transform, V(aMin.x-0.003f, aMin.y-0.003f),
            V(aMax.x+0.002f, aMax.y+0.002f))
            .AddComponent<Image>().color = GoldDim;

        MakeText("IntelHeader", p.transform, V(0.05f,0.91f), V(0.95f,0.99f),
            "FACTION INTEL", 13, FontStyle.Bold,
            new Color(Gold.r, Gold.g, Gold.b, 0.55f), TextAnchor.MiddleCenter);
        MakeRect("HeaderDiv", p.transform, V(0.05f,0.903f), V(0.95f,0.907f))
            .AddComponent<Image>().color = GoldDim;

        detailName = MakeText("Name", p.transform, V(0.04f,0.80f), V(0.96f,0.905f),
            "", 22, FontStyle.Bold, Gold, TextAnchor.MiddleCenter).GetComponent<Text>();

        detailSpec = MakeText("Spec", p.transform, V(0.04f,0.74f), V(0.96f,0.80f),
            "", 12, FontStyle.Italic, Mid, TextAnchor.MiddleCenter).GetComponent<Text>();

        MakeRect("SpecDiv", p.transform, V(0.05f,0.732f), V(0.95f,0.736f))
            .AddComponent<Image>().color = GoldDim;

        detailLore = MakeText("Lore", p.transform, V(0.05f,0.38f), V(0.95f,0.73f),
            "", 13, FontStyle.Normal, Light, TextAnchor.UpperLeft).GetComponent<Text>();

        MakeRect("MottoDiv", p.transform, V(0.05f,0.30f), V(0.95f,0.304f))
            .AddComponent<Image>().color = GoldDim;

        detailMotto = MakeText("Motto", p.transform, V(0.05f,0.20f), V(0.95f,0.30f),
            "", 12, FontStyle.Italic, Dark, TextAnchor.MiddleCenter).GetComponent<Text>();
    }

    // ── Selection ─────────────────────────────────────────────────────────────
    void SelectFaction(int index)
    {
        selectedFaction = index;
        FactionData f = factions[index];

        detailName.text = f.factionName.ToUpper();
        detailName.color = f.factionColor;
        detailSpec.text = index < Specialty.Length ? Specialty[index] : "";
        detailLore.text = index < Lore.Length ? Lore[index] : "";
        detailMotto.text = index < Motto.Length ? Motto[index] : "";
        confirmLabel.text = "BEGIN AS " + f.factionName.ToUpper() + "  \u25B6";

        for (int i = 0; i < factions.Count; i++)
        {
            bool sel = i == selectedFaction;
            cardBg[i].color = sel
                ? new Color(0.18f, 0.09f, 0.04f)
                : new Color(0.10f, 0.06f, 0.03f);
            cardBorder[i].color = sel
                ? factions[i].factionColor
                : new Color(factions[i].factionColor.r*0.45f,
                            factions[i].factionColor.g*0.45f,
                            factions[i].factionColor.b*0.45f);
            cardSelectLabel[i].text = sel ? "\u2726 SELECTED \u2726" : "CLICK TO SELECT";
        }
    }

    // ── Actions ───────────────────────────────────────────────────────────────
    void OnConfirm()
    {
        if (confirmed) return;
        confirmed = true;
        StartCoroutine(ConfirmTransition());
    }

    void OnBack()
    {
        StartCoroutine(BackTransition());
    }

    IEnumerator ConfirmTransition()
    {
        CanvasGroup cg = GetComponent<CanvasGroup>();
        float t = 0f;
        while (t < 0.5f) { t += Time.deltaTime; cg.alpha = 1f - t/0.5f; yield return null; }
        cg.alpha = 0f;

        GameManager.Instance.playerFactionId = selectedFaction;
        if (GameUIManager.Instance == null)
        {
            new GameObject("GameUIManager").AddComponent<GameUIManager>();
        }
        GameUIManager.Instance.StartGame(selectedFaction);
        Destroy(gameObject);
    }

    IEnumerator BackTransition()
    {
        CanvasGroup cg = GetComponent<CanvasGroup>();
        float t = 0f;
        while (t < 0.35f) { t += Time.deltaTime; cg.alpha = 1f - t/0.35f; yield return null; }
        cg.alpha = 0f;
        if (MainMenuManager.Instance != null)
            MainMenuManager.Instance.RestoreMainMenu();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        Destroy(gameObject);
    }

    IEnumerator FadeIn()
    {
        CanvasGroup cg = GetComponent<CanvasGroup>();
        float t = 0f;
        while (t < 0.6f) { t += Time.deltaTime; cg.alpha = t/0.6f; yield return null; }
        cg.alpha = 1f;
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
        t.fontSize = size; t.fontStyle = style; t.color = color;
        t.alignment = align;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        return go;
    }

    static void AddButton(GameObject go, Image targetGraphic,
        System.Action onEnter, System.Action onExit, System.Action onClick)
    {
        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = targetGraphic;
        btn.navigation = new Navigation { mode = Navigation.Mode.None };
        btn.onClick.AddListener(() => onClick());
        EventTrigger et = go.AddComponent<EventTrigger>();
        var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enter.callback.AddListener(_ => onEnter());
        et.triggers.Add(enter);
        var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exit.callback.AddListener(_ => onExit());
        et.triggers.Add(exit);
    }

    static Font GetFont() => UIFont.Get();
}
