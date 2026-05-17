using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EconomyUI : MonoBehaviour
{
    public static EconomyUI Instance { get; private set; }

    struct EconomyCard
    {
        public string cardName;
        public ResourceType costType;
        public int costAmount;
        public int powerGain;
    }

    enum ResourceType { INFLUENCE, SECRETS }

    static readonly Dictionary<int, List<EconomyCard>> factionCards = new Dictionary<int, List<EconomyCard>>
    {
        { 0, new List<EconomyCard> {
            new EconomyCard { cardName = "Eyes of the World", costType = ResourceType.INFLUENCE, costAmount = 10, powerGain = 15 },
            new EconomyCard { cardName = "Knowledge is Power", costType = ResourceType.SECRETS, costAmount = 15, powerGain = 20 }
        }},
        { 1, new List<EconomyCard> {
            new EconomyCard { cardName = "Divine Mandate", costType = ResourceType.INFLUENCE, costAmount = 10, powerGain = 15 },
            new EconomyCard { cardName = "Sacred Treasury", costType = ResourceType.SECRETS, costAmount = 15, powerGain = 20 }
        }},
        { 2, new List<EconomyCard> {
            new EconomyCard { cardName = "Grand Architecture", costType = ResourceType.INFLUENCE, costAmount = 10, powerGain = 15 },
            new EconomyCard { cardName = "Lodge Connections", costType = ResourceType.SECRETS, costAmount = 15, powerGain = 20 }
        }},
        { 3, new List<EconomyCard> {
            new EconomyCard { cardName = "Revolutionary Zeal", costType = ResourceType.INFLUENCE, costAmount = 10, powerGain = 15 },
            new EconomyCard { cardName = "Underground Network", costType = ResourceType.SECRETS, costAmount = 15, powerGain = 20 }
        }}
    };

    GameObject backdrop;
    GameObject panel;
    GameObject tableBody;
    List<GameObject> rowGOs = new List<GameObject>();
    bool isVisible = false;

    static readonly Color Dark = new Color(0.16f, 0.08f, 0.02f);
    static readonly Color Gold = new Color(0.83f, 0.65f, 0.22f);
    static readonly Color Parchment = new Color(0.91f, 0.86f, 0.78f);

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Build(Transform parent)
    {
        if (parent == null)
        {
            Debug.LogError("[EconomyUI] Cannot build without a parent transform.");
            return;
        }

        if (panel != null)
        {
            if (backdrop != null) Destroy(backdrop);
            Destroy(panel);
            backdrop = null;
            rowGOs.Clear();
            tableBody = null;
            isVisible = false;
        }

        backdrop = new GameObject("EconomyBackdrop", typeof(RectTransform));
        backdrop.transform.SetParent(parent, false);
        Image backdropImg = backdrop.AddComponent<Image>();
        backdropImg.color = new Color(0f, 0f, 0f, 0.20f);
        backdropImg.raycastTarget = true;
        FullStretch(backdrop.GetComponent<RectTransform>());
        backdrop.SetActive(false);

        panel = new GameObject("EconomyPanel", typeof(RectTransform));
        panel.transform.SetParent(parent, false);

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.06f, 0.03f, 0.01f, 0.95f);
        bg.raycastTarget = true;
        Anchor(panel.GetComponent<RectTransform>(), 0.00f, 0.06f, 0.73f, 0.34f);

        GameObject border = new GameObject("Border", typeof(RectTransform));
        border.transform.SetParent(panel.transform, false);
        Image borderImg = border.AddComponent<Image>();
        borderImg.color = Gold;
        borderImg.raycastTarget = false;
        FullStretch(border.GetComponent<RectTransform>(), 2, 2);

        MakeUIText(panel.transform, "Title", 13, FontStyle.Bold,
            Gold, new Vector2(0.02f, 0.83f), new Vector2(0.60f, 0.97f))
            .GetComponent<Text>().text = "ECONOMY";

        GameObject closeBtn = new GameObject("CloseBtn", typeof(RectTransform));
        closeBtn.transform.SetParent(panel.transform, false);
        Image closeImg = closeBtn.AddComponent<Image>();
        closeImg.color = new Color(0.5f, 0.15f, 0.08f);
        Button closeBtnComp = closeBtn.AddComponent<Button>();
        closeBtnComp.targetGraphic = closeImg;
        Navigation nav = new Navigation();
        nav.mode = Navigation.Mode.None;
        closeBtnComp.navigation = nav;
        Text closeLabel = MakeUIText(closeBtn.transform, "Label", 10, FontStyle.Bold,
            Parchment, Vector2.zero, Vector2.one).GetComponent<Text>();
        closeLabel.fontSize = 10;
        closeLabel.fontStyle = FontStyle.Bold;
        closeLabel.alignment = TextAnchor.MiddleCenter;
        closeLabel.color = Parchment;
        closeLabel.text = "X";
        closeLabel.raycastTarget = false;
        Anchor(closeBtn.GetComponent<RectTransform>(), 0.90f, 0.83f, 0.98f, 0.96f);
        closeBtnComp.onClick.AddListener(() => Hide());

        float headerY = 0.78f;

        MakeUIText(panel.transform, "HdrName", 8, FontStyle.Bold,
            new Color(Gold.r, Gold.g, Gold.b, 0.7f), new Vector2(0.03f, headerY - 0.04f), new Vector2(0.38f, headerY))
            .GetComponent<Text>().text = "CARD";

        MakeUIText(panel.transform, "HdrCost", 8, FontStyle.Bold,
            new Color(Gold.r, Gold.g, Gold.b, 0.7f), new Vector2(0.38f, headerY - 0.04f), new Vector2(0.58f, headerY))
            .GetComponent<Text>().text = "COST";

        MakeUIText(panel.transform, "HdrEffect", 8, FontStyle.Bold,
            new Color(Gold.r, Gold.g, Gold.b, 0.7f), new Vector2(0.58f, headerY - 0.04f), new Vector2(0.78f, headerY))
            .GetComponent<Text>().text = "EFFECT";

        MakeUIText(panel.transform, "HdrAction", 8, FontStyle.Bold,
            new Color(Gold.r, Gold.g, Gold.b, 0.7f), new Vector2(0.78f, headerY - 0.04f), new Vector2(0.97f, headerY))
            .GetComponent<Text>().text = "";

        tableBody = new GameObject("TableBody", typeof(RectTransform));
        tableBody.transform.SetParent(panel.transform, false);
        Anchor(tableBody.GetComponent<RectTransform>(), 0.01f, 0.02f, 0.99f, headerY - 0.05f);

        panel.SetActive(false);
        RebuildRows();
    }

    void Update()
    {
        if (isVisible && (Input.GetKeyDown(KeyCode.Escape) ||
            (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began &&
             !RectTransformUtility.RectangleContainsScreenPoint(
                 panel.GetComponent<RectTransform>(), Input.GetTouch(0).position, null))))
        {
            Hide();
        }
    }

    public void Refresh()
    {
        if (tableBody == null) return;
        RebuildRows();
    }

    public void Show()
    {
        if (panel == null) return;
        RebuildRows();
        if (backdrop != null) backdrop.SetActive(true);
        panel.SetActive(true);
        isVisible = true;
    }

    public void Hide()
    {
        if (panel == null) return;
        panel.SetActive(false);
        if (backdrop != null) backdrop.SetActive(false);
        isVisible = false;
    }

    public void Toggle()
    {
        if (isVisible) Hide();
        else Show();
    }

    void RebuildRows()
    {
        if (tableBody == null) return;

        foreach (GameObject go in rowGOs) Destroy(go);
        rowGOs.Clear();

        FactionData player = GameManager.Instance?.GetPlayerFaction();
        if (player == null) return;

        if (!factionCards.ContainsKey(player.factionId)) return;
        List<EconomyCard> cards = factionCards[player.factionId];

        float rowH = 1f / Mathf.Max(cards.Count, 1);

        for (int i = 0; i < cards.Count; i++)
        {
            EconomyCard ec = cards[i];
            float y0 = 1f - (i + 1) * rowH;
            float y1 = 1f - i * rowH;

            GameObject row = new GameObject("Row_" + i, typeof(RectTransform));
            row.transform.SetParent(tableBody.transform, false);
            Image rowBg = row.AddComponent<Image>();
            rowBg.color = (i % 2 == 0) ? Dark : new Color(0.12f, 0.06f, 0.02f);
            rowBg.raycastTarget = true;
            Anchor(row.GetComponent<RectTransform>(), 0, y0, 1, y1);

            Button rowBtn = row.AddComponent<Button>();
            rowBtn.targetGraphic = rowBg;
            Navigation nav = new Navigation();
            nav.mode = Navigation.Mode.None;
            rowBtn.navigation = nav;

            bool canAfford = (ec.costType == ResourceType.INFLUENCE && player.influence >= ec.costAmount)
                          || (ec.costType == ResourceType.SECRETS && player.secrets >= ec.costAmount);
            Color interactColor = canAfford ? new Color(0.2f, 0.5f, 0.2f) : new Color(0.3f, 0.3f, 0.3f);

            GameObject actionBtn = new GameObject("ActionBtn", typeof(RectTransform));
            actionBtn.transform.SetParent(row.transform, false);
            Image actImg = actionBtn.AddComponent<Image>();
            actImg.color = interactColor;
            Button actBtn = actionBtn.AddComponent<Button>();
            actBtn.targetGraphic = actImg;
            Navigation anav = new Navigation();
            anav.mode = Navigation.Mode.None;
            actBtn.navigation = anav;
            Text actLabel = MakeUIText(actionBtn.transform, "Label", 7, FontStyle.Bold,
                Parchment, Vector2.zero, Vector2.one).GetComponent<Text>();
            actLabel.alignment = TextAnchor.MiddleCenter;
            actLabel.text = canAfford ? "TRADE" : "LOW";
            Anchor(actionBtn.GetComponent<RectTransform>(), 0.78f, 0.12f, 0.97f, 0.88f);

            if (canAfford)
            {
                int capturedFactionId = player.factionId;
                EconomyCard capturedCard = ec;
                actBtn.onClick.AddListener(() => ExecuteTrade(capturedFactionId, capturedCard));
            }

            string costStr = ec.costType == ResourceType.INFLUENCE
                ? $"INF:{ec.costAmount}"
                : $"SEC:{ec.costAmount}";

            MakeUIText(row.transform, "NameText", 8, FontStyle.Bold,
                Parchment, new Vector2(0.03f, 0.05f), new Vector2(0.38f, 0.95f))
                .GetComponent<Text>().text = ec.cardName;

            MakeUIText(row.transform, "CostText", 8, FontStyle.Normal,
                new Color(0.9f, 0.75f, 0.1f), new Vector2(0.38f, 0.05f), new Vector2(0.58f, 0.95f))
                .GetComponent<Text>().text = costStr;

            MakeUIText(row.transform, "EffectText", 8, FontStyle.Normal,
                new Color(0.3f, 0.7f, 0.3f), new Vector2(0.58f, 0.05f), new Vector2(0.78f, 0.95f))
                .GetComponent<Text>().text = $"+{ec.powerGain} PWR";

            rowGOs.Add(row);
        }
    }

    void ExecuteTrade(int factionId, EconomyCard card)
    {
        FactionData faction = GameManager.Instance?.factions.Find(f => f.factionId == factionId);
        if (faction == null) return;

        if (card.costType == ResourceType.INFLUENCE)
        {
            if (faction.influence < card.costAmount) return;
            faction.influence -= card.costAmount;
        }
        else
        {
            if (faction.secrets < card.costAmount) return;
            faction.secrets -= card.costAmount;
        }
        faction.power = Mathf.Min(100, faction.power + card.powerGain);

        string costStr = card.costType == ResourceType.INFLUENCE
            ? $"INF:{card.costAmount}"
            : $"SEC:{card.costAmount}";
        GameManager.Instance?.LogMessage($"{faction.factionName} executes {card.cardName}: +{card.powerGain} PWR (cost: {costStr})");

        FactionPanelUI[] allPanels = FindObjectsOfType<FactionPanelUI>();
        foreach (FactionPanelUI fp in allPanels)
            fp.UpdateStats();

        RebuildRows();
    }

    void Anchor(RectTransform r, float x0, float y0, float x1, float y1)
    {
        r.anchorMin = new Vector2(x0, y0);
        r.anchorMax = new Vector2(x1, y1);
        r.sizeDelta = Vector2.zero;
        r.anchoredPosition = Vector2.zero;
    }

    void FullStretch(RectTransform r, float w = 0, float h = 0)
    {
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.sizeDelta = new Vector2(w, h);
        r.anchoredPosition = Vector2.zero;
    }

    GameObject MakeUIText(Transform parent, string name, int size, FontStyle style,
        Color color, Vector2 aMin, Vector2 aMax, TextAnchor align = TextAnchor.MiddleCenter)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Text t = go.AddComponent<Text>();
        Font font = GetFont();
        if (font != null) t.font = font;
        t.fontSize = size;
        t.fontStyle = style;
        t.alignment = align;
        t.color = color;
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = aMin;
        r.anchorMax = aMax;
        r.sizeDelta = Vector2.zero;
        r.anchoredPosition = Vector2.zero;
        return go;
    }

    static Font GetFont() => UIFont.Get();
}
