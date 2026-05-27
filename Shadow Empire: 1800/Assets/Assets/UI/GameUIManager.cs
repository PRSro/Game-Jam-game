using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    static readonly Color Ink        = new Color(0.04f, 0.02f, 0.01f);
    static readonly Color Darkest    = new Color(0.09f, 0.04f, 0.015f);
    static readonly Color Dark       = new Color(0.14f, 0.07f, 0.02f);
    static readonly Color Med        = new Color(0.22f, 0.10f, 0.03f);
    static readonly Color Gold       = new Color(0.88f, 0.68f, 0.18f);
    static readonly Color Parchment  = new Color(0.94f, 0.89f, 0.80f);
    static readonly Color Rust       = new Color(0.50f, 0.22f, 0.08f);
    static readonly Color DimGold    = new Color(0.58f, 0.42f, 0.10f);
    static readonly Color Crimson    = new Color(0.55f, 0.07f, 0.05f);
    static readonly Color Verdigris  = new Color(0.20f, 0.38f, 0.28f);
    static readonly Vector2 MapAnchorMin = new Vector2(0f, 0.04f);
    static readonly Vector2 MapAnchorMax = new Vector2(0.733f, 0.98f);
    static readonly Vector2 TerritoryUvMin = new Vector2(0.02f, 0.04f);
    static readonly Vector2 TerritoryUvMax = new Vector2(0.67f, 0.58f);
    static string _lastSidebarRegion;

    // Pre-generated card sprites
    public static Sprite cardBaseSprite;
    public static Sprite cardArtFrameSprite;
    public static Sprite cardRarityGlowSprite;
    public static Sprite cardPowerBadgeSprite;
    static Sprite phaseCircleSprite;

    Canvas canvas;
    GameObject topBar;
    GameObject rightPanelBg;
    GameObject mapAreaBg;
    GameObject mapFrame;
    GameObject mapClickZonesRoot;
    GameObject factionArea;
    GameObject logArea;
    GameObject endTurnRoot;
    GameObject cardsRemainingRoot;
    Button economyToggleButton;
    List<FactionPanelUI> factionPanels = new List<FactionPanelUI>();
    List<CardUIController> handCards = new List<CardUIController>();

    GameObject handArea;
    GameObject playedArea;
    Button endTurnButton;
    Text endTurnLabel;

    CanvasGroup gameOverCanvasGroup;
    GameObject gameOverPanel;
    Text gameOverTitleText;
    Text gameOverSubtitleText;
    Image gameOverDivider;
    Button playAgainButton;
    Button quitButton;

    GameObject cardPrefab;
    GameObject factionPanelPrefab;
    TurnLogUI turnLog;

    bool isSelectingTarget = false;
    bool waitingForTarget = false;
    bool waitingForTerritoryAttackSource = false;
    bool waitingForTerritoryAttackTarget = false;
    TerritoryData pendingAttackSourceTerritory = null;
    bool waitingForFortifySource = false;
    bool waitingForFortifyTarget = false;
    TerritoryData pendingFortifySource = null;
    TerritoryData pendingFortifyDest = null;
    bool waitingForNeutralExpandSource = false;
    bool waitingForNeutralExpandTarget = false;
    TerritoryData pendingExpandSource = null;
    GameObject expandBtnRoot;
    GameObject fortifyPanel;
    Text fortifyTitleText;
    Text fortifyInstructionText;
    Button fortifySkipBtn;
    // ADDED: UPGRADE 3 - fortify troop count slider
    Slider fortifySlider;
    Text fortifyCountText;
    Button fortifyConfirmBtn;
    int selectedFortifyCount = 0;
    CardData pendingCard = null;
    Vector3 pendingCardPosition;
    Image damageFlashOverlay;

    List<int> eliminatedFactions = new List<int>();

    GameObject targetPopup;
    Text targetPopupCardName;
    Button cancelTargetBtn;

    GameObject tooltipPanel;
    Text tooltipNameText;
    Text tooltipTypeText;
    Text tooltipPowerText;
    Text tooltipDescriptionText;

    GameObject territoryTooltip;
    Text territoryTooltipName;
    Text territoryTooltipOwner;
    Text territoryTooltipStats;

    GameObject attackStatusBanner;
    Text attackStatusText;
    Coroutine attackStatusPulseCoroutine;

    Text turnCounterText;
    Text subTurnText;
    Text yearProgressText;
    Text cardsRemainingText;
    Image endTurnButtonImage;
    Image phaseTimerBar;

    GameObject eventPopup;
    GameObject eventOverlay;
    Text eventPopupTitleText;
    Text eventPopupNameText;
    Text eventPopupDescText;
    Button eventDismissBtn;
    Button eventChoiceABtn;
    Button eventChoiceBBtn;

    GameObject territoryMapPanel;
    Dictionary<string, GameObject> territoryMapEntries = new Dictionary<string, GameObject>();
    Dictionary<string, Button> mapClickButtons = new Dictionary<string, Button>();
    Dictionary<string, Button> mapClickZoneButtons = new Dictionary<string, Button>();
    Dictionary<string, Vector2> territoryMapPositions;
    Dictionary<string, int> mapTroopCache = new Dictionary<string, int>();
    Dictionary<string, int> previousTroopCounts = new Dictionary<string, int>();
    Dictionary<int, GameObject> factionPanelRoots = new Dictionary<int, GameObject>();

    GameObject reinforcementPanel;
    Text reinforcementRemainingText;
    Button reinforcementConfirmBtn;
    GameObject attackArrow;
    List<GameObject> phaseArrows = new List<GameObject>();
    // ADDED: UPGRADE 2 - dice choice popup
    GameObject diceChoicePopup;
    Button dice1Btn, dice2Btn, dice3Btn;
    Text diceChoiceTitle;
    Slider troopSlider;
    Text troopSliderLabel;
    // ADDED: UPGRADE 4 - region bonus display panel
    GameObject regionBonusPanel;
    Text regionBonusText;
    GameObject combatPopup;
    GameObject activeCombatBanner;
    Text combatPopupTitle;
    Text combatPopupDetails;
    Button combatPopupDismissBtn;
    Coroutine combatAutoDismiss;
    ScrollRect combatPopupScrollRect;
    GameObject activeMinigamePanel;
    public int lastMinigameBonus = 0;
    FactionData pendingMinigameFactionTarget;

    GameObject phaseBanner;
    Text phaseBannerText;
    Coroutine phaseBannerCoroutine;

    GameObject actionPopup;
    Button actionAttackBtn;
    Button actionReinforceBtn;
    Text actionPopupLabel;
    Coroutine actionAutoHide;
    string actionPopupTerritory;

    bool uiBuilt = false;
    bool eventsSubscribed = false;

    GameObject probabilityPanel;
    Dictionary<CardType, GameObject> probabilityRows = new Dictionary<CardType, GameObject>();
    List<float> probabilityHistory = new List<float>();
    Coroutine probabilityBarAnim;

    GameObject marketPanel;
    BlackMarketSystem.MarketSlot[] marketSlotsUI;
    Text marketSecretsText;
    List<GameObject> marketSlotCards = new List<GameObject>();
    bool marketVisible = false;

    GameObject marketToggleBtn;
    GameObject marketBadge;
    Text marketBadgeText;
    GameObject attackModePopup;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDestroy()
    {
        MapLayerController.OnMapBuilt -= EnforceDrawOrder;
        UnsubscribeFromEvents();
        if (Instance == this) Instance = null;
    }

    void Start()
    {
        // Destroy stale objects from prior sessions, but never destroy the canvas
        // this instance is about to create — MapLayerController depends on "MainCanvas" existing.
        var staleMap = GameObject.Find("MapCanvas");
        if (staleMap != null) Destroy(staleMap);
        var staleTimeline = GameObject.Find("TimelineCanvas");
        if (staleTimeline != null) Destroy(staleTimeline);
        // Do NOT destroy "MainCanvas" here. GameUIManager.StartGame() creates it freshly
        // via CreateCanvas() only when uiBuilt == false.
    }

    public void StartGame(int factionId)
    {
        if (canvas == null) uiBuilt = false;

        if (!uiBuilt)
        {
            UnsubscribeFromEvents();
            CreateCanvas();
            BuildCardPrefab();
            BuildFactionPanelPrefab();
            BuildUI();
            uiBuilt = true;

            // MapLayerController may have been deactivated during menu scene detection.
            // Ensure it builds the map image now that the game canvas exists.
            if (MapLayerController.Instance != null)
                MapLayerController.Instance.BuildMapForGame(canvas.transform);
        }

        SetGameCanvasVisible(false);

        GameManager.Instance.playerFactionId = factionId;
        SubscribeToEvents();
        UpdateRegionBonusDisplay();

        MapTimelineUI timeline = FindObjectOfType<MapTimelineUI>(true);
        if (timeline != null) timeline.gameObject.SetActive(false);

        // FEATURE: play faction cutscene before handing off to BeginGame
        CutsceneManager cm = Object.FindObjectOfType<CutsceneManager>();
        if (cm != null)
        {
            cm.PlayFactionCutscene(factionId, () =>
            {
                SetGameCanvasVisible(true);
                EnforceDrawOrder();
                if (timeline != null) timeline.gameObject.SetActive(true);
                GameManager.Instance.BeginGame();
            });
        }
        else
        {
            SetGameCanvasVisible(true);
            if (timeline != null) timeline.gameObject.SetActive(true);
            GameManager.Instance.BeginGame();
        }
    }

    void SetGameCanvasVisible(bool visible)
    {
        if (canvas == null) return;
        CanvasGroup group = canvas.GetComponent<CanvasGroup>();
        if (group == null) group = canvas.gameObject.AddComponent<CanvasGroup>();
        group.alpha = visible ? 1f : 0f;
        group.interactable = visible;
        group.blocksRaycasts = visible;

        if (visible && MapLayerController.Instance != null)
        {
            RawImage mapImg = MapLayerController.Instance.MapImage;
            if (mapImg != null)
            {
                mapImg.gameObject.SetActive(true);
                CanvasGroup mapGroup = mapImg.GetComponent<CanvasGroup>();
                if (mapGroup != null)
                {
                    mapGroup.alpha = 1f;
                    mapGroup.interactable = true;
                    mapGroup.blocksRaycasts = true;
                }
                mapImg.color = Color.white;
            }
        }
    }

    void CreateCanvas()
    {
        PreGenerateCardSprites();

        GameObject go = new GameObject("MainCanvas", typeof(RectTransform));
        go.transform.SetParent(transform, false);
        canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;
        canvas.sortingOrder = 10;
        CanvasScaler scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.referencePixelsPerUnit = 100f;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        scaler.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();

        if (GetComponent<EconomyUI>() == null)
            gameObject.AddComponent<EconomyUI>();

        if (Camera.main != null)
        {
            Camera.main.backgroundColor = new Color(0.07f, 0.04f, 0.02f);
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
        }

        GameObject flash = new GameObject("DamageFlash", typeof(RectTransform));
        flash.transform.SetParent(go.transform, false);
        damageFlashOverlay = flash.AddComponent<Image>();
        damageFlashOverlay.color = new Color(1, 0, 0, 0);
        damageFlashOverlay.raycastTarget = false;
        FullStretch(flash.GetComponent<RectTransform>());
    }

    void BuildUI()
    {
        Transform ct = canvas.transform;

        // LAYOUT FIX: timeline bar moved to bottom
        topBar = new GameObject("TopBar", typeof(RectTransform));
        topBar.transform.SetParent(ct, false);
        Image topBarImg = topBar.AddComponent<Image>();
        topBarImg.color = new Color(0.06f, 0.03f, 0.01f);
        topBarImg.raycastTarget = false;
        Anchor(topBar.GetComponent<RectTransform>(), 0, 0, 1, 0.05f);

        MakeHRule(topBar.transform, new Vector2(0, 0.92f), new Vector2(1, 1f), Gold.WithA(0.55f));
        MakeHRule(topBar.transform, new Vector2(0, 0.88f), new Vector2(1, 0.91f), Gold.WithA(0.18f));

        turnCounterText = MakeUIText(topBar.transform, "TurnCounter", 16, FontStyle.Bold,
                                     Parchment, new Vector2(0.01f, 0), new Vector2(0.3f, 1)).GetComponent<Text>();
        turnCounterText.text = "Turn 1 | 1790 AD";

        GameObject timerBarGO = new GameObject("PhaseTimerBar", typeof(RectTransform));
        timerBarGO.transform.SetParent(topBar.transform, false);
        phaseTimerBar = timerBarGO.AddComponent<Image>();
        phaseTimerBar.color = new Color(0.75f, 0.45f, 0.08f, 0.75f);
        phaseTimerBar.raycastTarget = false;
        RectTransform tbR = timerBarGO.GetComponent<RectTransform>();
        tbR.anchorMin = new Vector2(0.01f, 0.02f);
        tbR.anchorMax = new Vector2(1f, 0.06f);
        tbR.sizeDelta = Vector2.zero;
        tbR.anchoredPosition = Vector2.zero;
        timerBarGO.SetActive(false);

        yearProgressText = MakeUIText(topBar.transform, "YearProgress", 12, FontStyle.Normal,
                                       Parchment.WithA(0.65f), new Vector2(0.7f, 0), new Vector2(0.99f, 1)).GetComponent<Text>();
        yearProgressText.text = "200 turns to 1990 AD";
        yearProgressText.alignment = TextAnchor.MiddleRight;

        subTurnText = MakeUIText(topBar.transform, "SubTurnText", 10, FontStyle.Normal,
                                 Parchment.WithA(0.45f), new Vector2(0.3f, 0), new Vector2(0.7f, 1)).GetComponent<Text>();
        subTurnText.text = "";
        subTurnText.alignment = TextAnchor.MiddleCenter;

        Button mtBtnLocal = MakeButton(topBar.transform, "MarketToggleBtn",
                          new Vector2(0, 0), new Vector2(120, 28), " 🛒 MARKET",
                          new Color(0.15f, 0.10f, 0.05f, 0.8f),
                          new Color(0.25f, 0.18f, 0.10f, 0.9f));
        RectTransform mtR = mtBtnLocal.GetComponent<RectTransform>();
        mtR.anchorMin = new Vector2(0.60f, 0.05f);
        mtR.anchorMax = new Vector2(0.72f, 0.95f);
        mtR.sizeDelta = Vector2.zero;
        mtR.anchoredPosition = Vector2.zero;
        mtBtnLocal.GetComponentInChildren<Text>().fontSize = 10;
        mtBtnLocal.onClick.AddListener(ToggleMarketPanel);
        marketToggleBtn = mtBtnLocal.gameObject;

        Canvas mtCanvas = marketToggleBtn.AddComponent<Canvas>();
        mtCanvas.overrideSorting = true;
        mtCanvas.sortingOrder = 15;
        marketToggleBtn.AddComponent<GraphicRaycaster>();

        Button econBtn = MakeButton(topBar.transform, "EconomyToggleBtn",
            new Vector2(0, 0), new Vector2(120, 28), " \u2606 ECONOMY",
            new Color(0.15f, 0.10f, 0.05f, 0.8f),
            new Color(0.25f, 0.18f, 0.10f, 0.9f));
        RectTransform ecR = econBtn.GetComponent<RectTransform>();
        ecR.anchorMin = new Vector2(0.47f, 0.05f);
        ecR.anchorMax = new Vector2(0.59f, 0.95f);
        ecR.sizeDelta = Vector2.zero;
        ecR.anchoredPosition = Vector2.zero;
        econBtn.GetComponentInChildren<Text>().fontSize = 10;
        economyToggleButton = econBtn;

        Canvas ecCanvas = econBtn.gameObject.AddComponent<Canvas>();
        ecCanvas.overrideSorting = true;
        ecCanvas.sortingOrder = 15;
        econBtn.gameObject.AddComponent<GraphicRaycaster>();
        econBtn.onClick.AddListener(() =>
        {
            if (!CanUseActionPanels()) return;
            marketVisible = false;
            if (marketPanel != null) marketPanel.SetActive(false);
            if (EconomyUI.Instance != null) EconomyUI.Instance.Toggle();
        });

        marketBadge = new GameObject("MarketBadge", typeof(RectTransform));
        marketBadge.transform.SetParent(marketToggleBtn.transform, false);
        Image mbImg = marketBadge.AddComponent<Image>();
        mbImg.color = new Color(0.8f, 0.2f, 0.05f, 0.9f);
        RectTransform mbR = marketBadge.GetComponent<RectTransform>();
        mbR.anchorMin = new Vector2(0.7f, 0.6f);
        mbR.anchorMax = Vector2.one;
        mbR.sizeDelta = Vector2.zero;
        marketBadgeText = MakeUIText(marketBadge.transform, "BadgeText", 8, FontStyle.Bold,
                                      Color.white, Vector2.zero, Vector2.one).GetComponent<Text>();
        marketBadgeText.alignment = TextAnchor.MiddleCenter;
        marketBadge.SetActive(false);

        rightPanelBg = new GameObject("RightPanelBg", typeof(RectTransform));
        rightPanelBg.transform.SetParent(ct, false);
        Image rBgImg = rightPanelBg.AddComponent<Image>();
        rBgImg.color = new Color(0.08f, 0.04f, 0.02f, 0.92f);
        rBgImg.raycastTarget = false;
        Anchor(rightPanelBg.GetComponent<RectTransform>(), 0.735f, 0f, 1.0f, 1f);

        mapAreaBg = new GameObject("MapAreaBg", typeof(RectTransform));
        mapAreaBg.transform.SetParent(ct, false);
        Image mapBgImg = mapAreaBg.AddComponent<Image>();
        mapBgImg.enabled = false; // no background image - let camera clear color show through
        Anchor(mapAreaBg.GetComponent<RectTransform>(), MapAnchorMin.x, MapAnchorMin.y, MapAnchorMax.x, MapAnchorMax.y);
        mapAreaBg.transform.SetSiblingIndex(0);

        mapFrame = new GameObject("MapFrame", typeof(RectTransform));
        mapFrame.transform.SetParent(ct, false);
        Image frameImg = mapFrame.AddComponent<Image>();
        frameImg.color = Color.clear;
        frameImg.raycastTarget = false;
        Anchor(mapFrame.GetComponent<RectTransform>(), MapAnchorMin.x, MapAnchorMin.y, MapAnchorMax.x, MapAnchorMax.y);

        GameObject mapBorderTop = new GameObject("MapBorderTop", typeof(RectTransform));
        mapBorderTop.transform.SetParent(mapFrame.transform, false);
        RectTransform mbtR = mapBorderTop.GetComponent<RectTransform>();
        mbtR.anchorMin = new Vector2(0, 0.995f); mbtR.anchorMax = Vector2.one; mbtR.sizeDelta = Vector2.zero;
        Image mapBorderTopImg = mapBorderTop.AddComponent<Image>();
        mapBorderTopImg.color = Gold.WithA(0.70f);
        mapBorderTopImg.raycastTarget = false;

        GameObject mapBorderBot = new GameObject("MapBorderBot", typeof(RectTransform));
        mapBorderBot.transform.SetParent(mapFrame.transform, false);
        RectTransform mbbR = mapBorderBot.GetComponent<RectTransform>();
        mbbR.anchorMin = Vector2.zero; mbbR.anchorMax = new Vector2(1f, 0.005f); mbbR.sizeDelta = Vector2.zero;
        Image mapBorderBotImg = mapBorderBot.AddComponent<Image>();
        mapBorderBotImg.color = Gold.WithA(0.70f);
        mapBorderBotImg.raycastTarget = false;

        GameObject mapBorderL = new GameObject("MapBorderLeft", typeof(RectTransform));
        mapBorderL.transform.SetParent(mapFrame.transform, false);
        RectTransform mblR = mapBorderL.GetComponent<RectTransform>();
        mblR.anchorMin = Vector2.zero; mblR.anchorMax = new Vector2(0.004f, 1f); mblR.sizeDelta = Vector2.zero;
        Image mapBorderLImg = mapBorderL.AddComponent<Image>();
        mapBorderLImg.color = Gold.WithA(0.70f);
        mapBorderLImg.raycastTarget = false;

        GameObject mapBorderR = new GameObject("MapBorderRight", typeof(RectTransform));
        mapBorderR.transform.SetParent(mapFrame.transform, false);
        RectTransform mbrR = mapBorderR.GetComponent<RectTransform>();
        mbrR.anchorMin = new Vector2(0.997f, 0); mbrR.anchorMax = Vector2.one; mbrR.sizeDelta = Vector2.zero;
        Image mapBorderRImg = mapBorderR.AddComponent<Image>();
        mapBorderRImg.color = Gold.WithA(0.70f);
        mapBorderRImg.raycastTarget = false;

        factionArea = new GameObject("FactionArea", typeof(RectTransform));
        factionArea.transform.SetParent(ct, false);
        VerticalLayoutGroup vlg = factionArea.AddComponent<VerticalLayoutGroup>();
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = true;
        vlg.spacing = 3;
        vlg.padding = new RectOffset(3, 3, 3, 3);
        Canvas factionCanvas = factionArea.AddComponent<Canvas>();
        factionCanvas.overrideSorting = true;
        factionCanvas.sortingOrder = 12;
        factionArea.AddComponent<GraphicRaycaster>();
        Anchor(factionArea.GetComponent<RectTransform>(), 0.735f, 0.30f, 0.998f, 0.93f);

        logArea = new GameObject("LogArea", typeof(RectTransform));
        logArea.transform.SetParent(ct, false);
        Canvas logCanvas = logArea.AddComponent<Canvas>();
        logCanvas.overrideSorting = true;
        logCanvas.sortingOrder = 12;
        logArea.AddComponent<GraphicRaycaster>();
        Image logBg = logArea.AddComponent<Image>();
        logBg.color = new Color(0.05f, 0.025f, 0.008f, 0.96f);
        logBg.raycastTarget = false;
        Anchor(logArea.GetComponent<RectTransform>(), 0.735f, 0.18f, 0.998f, 0.30f);

        MakeUIText(logArea.transform, "LogTitle", 10, FontStyle.Bold,
                   Gold, new Vector2(0.02f, 0.70f), new Vector2(0.98f, 0.98f)).GetComponent<Text>().text = "TURN LOG";
        MakeHRule(logArea.transform, new Vector2(0.03f, 0.65f), new Vector2(0.97f, 0.67f), Gold.WithA(0.30f));

        GameObject svGO = new GameObject("ScrollView", typeof(RectTransform));
        svGO.transform.SetParent(logArea.transform, false);
        ScrollRect logSr = svGO.AddComponent<ScrollRect>();
        Image logSvBg = svGO.AddComponent<Image>();
        logSvBg.color = new Color(0.04f, 0.02f, 0.01f, 0.85f);
        logSvBg.raycastTarget = false;
        Anchor(svGO.GetComponent<RectTransform>(), 0, 0, 1, 0.63f);

        GameObject vp = new GameObject("Viewport", typeof(RectTransform));
        vp.transform.SetParent(svGO.transform, false);
        vp.AddComponent<Mask>();
        vp.AddComponent<Image>().color = Color.clear;
        FullStretch(vp.GetComponent<RectTransform>());

        GameObject ctGO = new GameObject("Content", typeof(RectTransform));
        ctGO.transform.SetParent(vp.transform, false);
        Text ctText = ctGO.AddComponent<Text>();
        ctText.font = GetFont();
        ctText.fontSize = 10;
        ctText.color = new Color(0.85f, 0.78f, 0.64f);
        ctText.alignment = TextAnchor.UpperLeft;
        ctText.lineSpacing = 1.1f;
        ctText.supportRichText = true;
        RectTransform ctR = ctGO.GetComponent<RectTransform>();
        ctR.anchorMin = new Vector2(0, 1);
        ctR.anchorMax = new Vector2(1, 1);
        ctR.sizeDelta = Vector2.zero;
        ContentSizeFitter csf = ctGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        logSr.viewport = vp.GetComponent<RectTransform>();
        logSr.content = ctR;
        logSr.vertical = true;
        logSr.horizontal = false;
        logSr.movementType = ScrollRect.MovementType.Clamped;

        turnLog = logArea.AddComponent<TurnLogUI>();
        turnLog.logText = ctText;
        turnLog.scrollRect = logSr;

        endTurnRoot = new GameObject("EndTurnBtn", typeof(RectTransform));
        endTurnRoot.transform.SetParent(ct, false);
        endTurnButtonImage = endTurnRoot.AddComponent<Image>();
        endTurnButtonImage.color = new Color(0.45f, 0.06f, 0.03f);
        endTurnButton = endTurnRoot.AddComponent<Button>();
        endTurnButton.targetGraphic = endTurnButtonImage;
        Navigation enav = new Navigation();
        enav.mode = Navigation.Mode.None;
        endTurnButton.navigation = enav;
        Anchor(endTurnRoot.GetComponent<RectTransform>(), 0.735f, 0.09f, 0.998f, 0.18f);
        endTurnLabel = MakeUIText(endTurnRoot.transform, "Label", 13, FontStyle.Bold,
                                  new Color(0.96f, 0.90f, 0.80f), Vector2.zero, Vector2.one).GetComponent<Text>();
        endTurnLabel.text = "END TURN";

        GameObject etBorder = new GameObject("TopBorder", typeof(RectTransform));
        etBorder.transform.SetParent(endTurnRoot.transform, false);
        Image etBorderImg = etBorder.AddComponent<Image>();
        etBorderImg.color = new Color(0.83f, 0.65f, 0.22f, 0.60f);
        etBorderImg.raycastTarget = false;
        RectTransform etbR = etBorder.GetComponent<RectTransform>();
        etbR.anchorMin = new Vector2(0f, 0.92f);
        etbR.anchorMax = Vector2.one;
        etbR.sizeDelta = Vector2.zero;

        expandBtnRoot = new GameObject("ExpandBtn", typeof(RectTransform));
        expandBtnRoot.transform.SetParent(ct, false);
        Image expandImg = expandBtnRoot.AddComponent<Image>();
        expandImg.color = new Color(0.08f, 0.22f, 0.06f);
        Button expandBtn = expandBtnRoot.AddComponent<Button>();
        expandBtn.targetGraphic = expandImg;
        Navigation exnav = new Navigation { mode = Navigation.Mode.None };
        expandBtn.navigation = exnav;
        Anchor(expandBtnRoot.GetComponent<RectTransform>(), 0.735f, 0.00f, 0.998f, 0.09f);
        GameObject expandLabel = new GameObject("Label", typeof(RectTransform));
        expandLabel.transform.SetParent(expandBtnRoot.transform, false);
        Text expandText = expandLabel.AddComponent<Text>();
        expandText.font = GetFont();
        expandText.text = "EXPAND";
        expandText.fontSize = 12;
        expandText.fontStyle = FontStyle.Bold;
        expandText.color = new Color(0.65f, 0.90f, 0.45f);
        expandText.alignment = TextAnchor.MiddleCenter;
        FullStretch(expandLabel.GetComponent<RectTransform>());
        expandBtn.onClick.AddListener(OnExpandButtonClicked);

        GameObject costLabel = new GameObject("CostLabel", typeof(RectTransform));
        costLabel.transform.SetParent(expandBtnRoot.transform, false);
        Text costText = costLabel.AddComponent<Text>();
        costText.font = GetFont();
        costText.text = "10 troops required";
        costText.fontSize = 8;
        costText.color = new Color(0.65f, 0.90f, 0.45f, 0.70f);
        costText.alignment = TextAnchor.MiddleCenter;
        RectTransform clR = costLabel.GetComponent<RectTransform>();
        clR.anchorMin = new Vector2(0f, 0f);
        clR.anchorMax = new Vector2(1f, 0.38f);
        clR.sizeDelta = Vector2.zero;

        cardsRemainingRoot = new GameObject("CardsRemaining", typeof(RectTransform));
        cardsRemainingRoot.transform.SetParent(ct, false);
        Anchor(cardsRemainingRoot.GetComponent<RectTransform>(), 0.735f, 0.293f, 0.998f, 0.305f);
        cardsRemainingText = cardsRemainingRoot.AddComponent<Text>();
        cardsRemainingText.font = GetFont();
        cardsRemainingText.fontSize = 9;
        cardsRemainingText.fontStyle = FontStyle.Bold;
        cardsRemainingText.alignment = TextAnchor.MiddleRight;
        cardsRemainingText.color = Parchment;
        cardsRemainingText.supportRichText = true;
        cardsRemainingText.text = "";

        handArea = new GameObject("HandArea", typeof(RectTransform));
        handArea.transform.SetParent(ct, false);
        HorizontalLayoutGroup hlgHand = handArea.AddComponent<HorizontalLayoutGroup>();
        hlgHand.childControlWidth = true;
        hlgHand.childControlHeight = true;
        hlgHand.childForceExpandWidth = false;
        hlgHand.childForceExpandHeight = false;
        hlgHand.spacing = 3;
        hlgHand.padding = new RectOffset(6, 6, 3, 3);
        hlgHand.childAlignment = TextAnchor.LowerCenter;
        Anchor(handArea.GetComponent<RectTransform>(), 0.01f, 0.00f, 0.73f, 0.23f);

        playedArea = new GameObject("PlayedArea", typeof(RectTransform));
        playedArea.transform.SetParent(ct, false);
        HorizontalLayoutGroup hlgP = playedArea.AddComponent<HorizontalLayoutGroup>();
        hlgP.childControlWidth = false;
        hlgP.childControlHeight = true;
        hlgP.spacing = 3;
        hlgP.padding = new RectOffset(6, 6, 1, 1);
        hlgP.childAlignment = TextAnchor.LowerCenter;
        Anchor(playedArea.GetComponent<RectTransform>(), 0.01f, 0.20f, 0.73f, 0.24f);

        BuildTerritoryMapPanel(ct);
        BuildTerritorySidebar(ct);
        BuildMapClickZones(ct);

        BuildTargetPopup(ct);
        // ADDED: UPGRADE 2
        BuildDiceChoicePopup(ct);
        BuildTooltip(ct);
        BuildTerritoryTooltip(ct);
        BuildAttackStatusBanner(ct);
        BuildGameOverPanel(ct);
        BuildEventPopup(ct);
        BuildReinforcementPanel(ct);
        BuildCombatPopup(ct);
        BuildFortifyPanel(ct);
        BuildPhaseBanner(ct);
        // ADDED: UPGRADE 4
        BuildRegionBonusPanel(ct);
        BuildProbabilityPanel(ct);
        BuildBlackMarketPanel(ct);
        if (EconomyUI.Instance != null) EconomyUI.Instance.Build(ct);

        EnforceDrawOrder();
        // Re-enforce draw order once the map image is built asynchronously.
        MapLayerController.OnMapBuilt -= EnforceDrawOrder;
        MapLayerController.OnMapBuilt += EnforceDrawOrder;

        ApplyPhaseVisibility(GameManager.Instance != null ? GameManager.Instance.currentState : GameState.DRAW_PHASE);
    }

    void EnforceDrawOrder()
    {
        if (canvas == null) return;

        Transform ct = canvas.transform;
        if (rightPanelBg != null) rightPanelBg.transform.SetSiblingIndex(0);
        if (mapAreaBg != null) mapAreaBg.transform.SetSiblingIndex(1);
        if (MapLayerController.Instance?.MapImage != null)
            MapLayerController.Instance.MapImage.transform.SetSiblingIndex(2);
        else
            Debug.Log("[GameUIManager] EnforceDrawOrder: MapImage not yet in canvas — will be placed when MapLayerController builds it.");
        if (mapFrame != null) mapFrame.transform.SetSiblingIndex(3);
        if (territoryMapPanel != null) territoryMapPanel.transform.SetSiblingIndex(4);
        if (topBar != null) topBar.transform.SetSiblingIndex(5);
        if (logArea != null) logArea.transform.SetSiblingIndex(6);
        if (expandBtnRoot != null) expandBtnRoot.transform.SetSiblingIndex(7);
        if (endTurnRoot != null) endTurnRoot.transform.SetSiblingIndex(8);
        if (factionArea != null) factionArea.transform.SetSiblingIndex(9);
        if (mapClickZonesRoot != null) mapClickZonesRoot.transform.SetSiblingIndex(10);
        if (handArea != null) handArea.transform.SetSiblingIndex(11);
        if (reinforcementPanel != null) reinforcementPanel.transform.SetSiblingIndex(12);
        if (territoryTooltip != null) territoryTooltip.transform.SetSiblingIndex(13);

        if (EconomyUI.Instance != null) EconomyUI.Instance.transform.SetAsLastSibling();
        Transform economyBackdrop = ct.Find("EconomyBackdrop");
        if (economyBackdrop != null) economyBackdrop.SetAsLastSibling();
        Transform economyPanel = ct.Find("EconomyPanel");
        if (economyPanel != null) economyPanel.SetAsLastSibling();
        Transform marketPanelT = ct.Find("MarketPanel");
        if (marketPanelT != null) marketPanelT.SetAsLastSibling();
        Transform blackMarketPanel = ct.Find("BlackMarketPanel");
        if (blackMarketPanel != null) blackMarketPanel.SetAsLastSibling();
        Transform handAreaT = ct.Find("HandArea");
        if (handAreaT != null) handAreaT.SetAsLastSibling();
        Transform cardHandPanel = ct.Find("CardHandPanel");
        if (cardHandPanel != null) cardHandPanel.SetAsLastSibling();
        Transform drawOddsPanelT = ct.Find("DrawOddsPanel");
        if (drawOddsPanelT != null) drawOddsPanelT.SetAsLastSibling();
        Transform probabilityPanelTransform = ct.Find("ProbabilityPanel");
        if (probabilityPanelTransform != null) probabilityPanelTransform.SetAsLastSibling();
        if (damageFlashOverlay != null) damageFlashOverlay.transform.SetAsLastSibling();
    }

    void BuildTerritoryMapPanel(Transform ct)
    {
        territoryMapPanel = new GameObject("TerritoryMapPanel", typeof(RectTransform));
        territoryMapPanel.transform.SetParent(ct, false);
        Image tpBg = territoryMapPanel.AddComponent<Image>();
        tpBg.color = Color.clear;
        tpBg.raycastTarget = false;
        Anchor(territoryMapPanel.GetComponent<RectTransform>(), MapAnchorMin.x, MapAnchorMin.y, MapAnchorMax.x, MapAnchorMax.y);

        GameObject zoomOverlay = new GameObject("ZoomOverlay", typeof(RectTransform));
        zoomOverlay.transform.SetParent(ct, false);
        Image zoImg = zoomOverlay.AddComponent<Image>();
        zoImg.color = Color.clear;
        zoImg.raycastTarget = false;
        Anchor(zoomOverlay.GetComponent<RectTransform>(), MapAnchorMin.x, MapAnchorMin.y, MapAnchorMax.x, MapAnchorMax.y);
        ZoomController zoomCtrl = zoomOverlay.AddComponent<ZoomController>();
        zoomCtrl.Setup(canvas, null);
        StartCoroutine(AssignZoomTarget(zoomCtrl));
    }

    IEnumerator AssignZoomTarget(ZoomController zoomCtrl)
    {
        while (MapLayerController.Instance == null || MapLayerController.Instance.MapImage == null)
            yield return null;
        zoomCtrl.SetTarget(MapLayerController.Instance.MapImage.GetComponent<RectTransform>());
    }

    void CreateTerritoryMapEntry(Transform parent, string territoryName, string region)
    {
        if (_lastSidebarRegion != region)
        {
            _lastSidebarRegion = region;
            Text regionLabel = MakeUIText(parent, "Region_" + region, 9, FontStyle.Bold,
                Gold.WithA(0.5f), new Vector2(0.03f, 0f), new Vector2(0.97f, 1f)).GetComponent<Text>();
            regionLabel.text = "— " + region.ToUpper() + " —";
            LayoutElement regionLayout = regionLabel.gameObject.AddComponent<LayoutElement>();
            regionLayout.preferredHeight = 18;
        }
        GameObject entry = new GameObject("Terr_" + territoryName.Replace(" ", ""), typeof(RectTransform));
        entry.transform.SetParent(parent, false);
        entry.AddComponent<Image>().color = Dark;
        entry.GetComponent<Image>().raycastTarget = false;

        RectTransform eR = entry.GetComponent<RectTransform>();
        eR.sizeDelta = new Vector2(0, 22);
        LayoutElement entryLayout = entry.AddComponent<LayoutElement>();
        entryLayout.preferredHeight = 22;

        GameObject colorDot = new GameObject("ColorDot", typeof(RectTransform));
        colorDot.transform.SetParent(entry.transform, false);
        Image dotImg = colorDot.AddComponent<Image>();
        dotImg.color = Color.gray;
        dotImg.raycastTarget = false;
        RectTransform dotR = colorDot.GetComponent<RectTransform>();
        dotR.anchorMin = new Vector2(0.03f, 0.15f);
        dotR.anchorMax = new Vector2(0.03f, 0.85f);
        dotR.sizeDelta = new Vector2(6, 0);
        dotR.anchoredPosition = Vector2.zero;

        Text nameText = MakeUIText(entry.transform, "Name", 11, FontStyle.Bold,
                                   Parchment, new Vector2(0.12f, 0.45f), new Vector2(0.75f, 0.95f)).GetComponent<Text>();
        nameText.text = territoryName;
        nameText.alignment = TextAnchor.LowerLeft;

        Text troopsText = MakeUIText(entry.transform, "Troops", 10, FontStyle.Bold,
                                     Gold, new Vector2(0.73f, 0.05f), new Vector2(0.98f, 0.55f)).GetComponent<Text>();
        troopsText.text = "";
        troopsText.alignment = TextAnchor.MiddleRight;

        if (GameManager.Instance != null && GameManager.Instance.territories != null)
        {
            TerritoryData tData = GameManager.Instance.territories.Find(t => t.name == territoryName);
            if (tData != null)
            {
                UpdateTerritoryMapEntry(entry, tData);
            }
        }

        territoryMapEntries[territoryName] = entry;
    }

    void UpdateTerritoryMapEntry(GameObject entry, TerritoryData tData)
    {
        if (entry == null) return;

        Image dotImg = entry.transform.Find("ColorDot")?.GetComponent<Image>();
        Text troopsText = entry.transform.Find("Troops")?.GetComponent<Text>();

        Color ownerColor = Color.gray;
        if (tData.controlledBy >= 0 && tData.controlledBy < GameManager.Instance.factions.Count)
            ownerColor = GameManager.Instance.factions[tData.controlledBy].factionColor;

        if (dotImg != null)
            dotImg.color = ownerColor;

        if (troopsText != null)
        {
            if (tData.controlledBy >= 0)
            {
                troopsText.text = tData.troops.ToString();
                troopsText.color = tData.troops >= 20 ? new Color(0.9f, 0.2f, 0.2f)
                    : tData.troops >= 10 ? new Color(0.9f, 0.6f, 0.1f)
                    : tData.troops >= 5 ? new Color(0.8f, 0.8f, 0.3f)
                    : new Color(0.7f, 0.7f, 0.5f);
            }
            else
                troopsText.text = "-";
        }

        if (previousTroopCounts.TryGetValue(tData.name, out int prev) && prev != tData.troops)
            StartCoroutine(PulseTerritoryEntry(entry));
        previousTroopCounts[tData.name] = tData.troops;

        Button btn = entry.GetComponent<Button>();
        if (btn == null)
        {
            btn = entry.AddComponent<Button>();
            btn.targetGraphic = entry.GetComponent<Image>();
            Navigation nav = new Navigation();
            nav.mode = Navigation.Mode.None;
            btn.navigation = nav;
        }

        TerritoryData captured = tData;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => OnTerritoryClicked(captured));

        TerritoryClickHandler rch = entry.GetComponent<TerritoryClickHandler>();
        if (rch == null)
        {
            rch = entry.AddComponent<TerritoryClickHandler>();
            rch.onRightClick = OnTerritoryRightClicked;
        }
        rch.territory = captured;
    }

    // ADDED: UPGRADE 2 - show dice choice popup after source selection
    void OnDiceChoiceSelected(int diceCount)
    {
        if (pendingAttackSourceTerritory == null)
        {
            ExitTerritoryAttackMode();
            return;
        }

        int maxDice = Mathf.Clamp(pendingAttackSourceTerritory.troops - 1, 1, 3);
        GameManager.Instance.pendingAttackerDiceCount = Mathf.Clamp(diceCount, 1, maxDice);
        GameManager.Instance.pendingAttackerTroopCount = troopSlider != null ? Mathf.RoundToInt(troopSlider.value) : 1;
        diceChoicePopup.SetActive(false);
        CardData card = pendingCard;
        TerritoryData source = pendingAttackSourceTerritory;
        EnterTerritoryAttackTargetMode(card, source);
    }

    void ConfigureDiceChoiceButtons(TerritoryData source)
    {
        int maxDice = source != null ? Mathf.Clamp(source.troops - 1, 1, 3) : 1;
        if (dice1Btn != null) dice1Btn.interactable = maxDice >= 1;
        if (dice2Btn != null) dice2Btn.interactable = maxDice >= 2;
        if (dice3Btn != null) dice3Btn.interactable = maxDice >= 3;

        if (troopSlider != null)
        {
            int maxTroops = source != null ? Mathf.Clamp(source.troops - 1, 1, 5) : 1;
            troopSlider.maxValue = maxTroops;
            troopSlider.value = Mathf.Min(troopSlider.value, maxTroops);
        }
    }

    void OnTerritoryClicked(TerritoryData territory)
    {
        if (territory == null || GameManager.Instance == null) return;

        if (GameManager.Instance.IsWaitingForReinforcements)
        {
            FactionData player = GameManager.Instance.GetPlayerFaction();
            if (player != null && territory.controlledBy == player.factionId)
            {
                GameManager.Instance.AssignReinforcementToTerritory(territory, 1);
            }
            else
            {
                AddLogMessage("You can only reinforce territories you already control.");
            }
            return;
        }

        if (GameManager.Instance.currentState == GameState.FORTIFY_PHASE)
        {
            FactionData player = GameManager.Instance.GetPlayerFaction();
            if (player == null) return;
            if (waitingForFortifySource)
            {
                if (territory.controlledBy == player.factionId && territory.troops > 1)
                {
                    pendingFortifySource = territory;
                    waitingForFortifySource = false;
                    waitingForFortifyTarget = true;
                    ClearTerritoryHighlights();
                    ShowFortifyPathArrows(territory, player.factionId);
                    if (territoryMapEntries.ContainsKey(territory.name))
                    {
                        Image bg = territoryMapEntries[territory.name].GetComponent<Image>();
                        if (bg != null) bg.color = new Color(0.20f, 0.40f, 0.20f);
                    }
                    AddLogMessage($"Select a friendly territory to move troops to from {territory.name}...");
                    if (fortifyInstructionText != null)
                        fortifyInstructionText.text = $"Select destination from {territory.name}";
                }
                else
                {
                    AddLogMessage("Select a territory you own with at least 2 troops.");
                }
                return;
            }
            // ADDED: UPGRADE 3 - show slider for troop count instead of moving all
            if (waitingForFortifyTarget)
            {
                if (IsValidFortifyDestination(pendingFortifySource, territory, player.factionId))
                {
                    pendingFortifyDest = territory;
                    waitingForFortifyTarget = false;
                    int maxMove = pendingFortifySource.troops - 1;
                    fortifyInstructionText.text = $"Move how many troops from {pendingFortifySource.name} to {territory.name}?";
                    ShowFortifyCountSlider(maxMove);
                }
                else
                {
                    AddLogMessage("Select a different friendly territory.");
                }
                return;
            }
        }

        if (waitingForNeutralExpandSource)
        {
            FactionData player = GameManager.Instance.GetPlayerFaction();
            if (territory.controlledBy == player.factionId && territory.troops >= 10)
            {
                pendingExpandSource = territory;
                waitingForNeutralExpandSource = false;
                waitingForNeutralExpandTarget = true;
                AddLogMessage($"EXPAND: Select an adjacent neutral territory to claim from {territory.name}.");
                UpdateMapClickZones();
            }
            else AddLogMessage("Select a territory you own with at least 10 troops.");
            return;
        }

        if (waitingForNeutralExpandTarget)
        {
            if (territory.controlledBy == -1 &&
                TerritoryGraph.AreAdjacent(pendingExpandSource.name, territory.name))
            {
                bool ok = GameManager.Instance.ExpandToNeutral(pendingExpandSource, territory);
                AddLogMessage(ok ? $"Expanded into {territory.name}!" : "Expansion failed.");
                if (ok) UpdateTerritoryMap();
            }
            else if (territory.controlledBy != -1)
                AddLogMessage("Select a neutral (unowned) territory.");
            else
                AddLogMessage($"{territory.name} is not adjacent to {pendingExpandSource?.name}.");

            waitingForNeutralExpandTarget = false;
            pendingExpandSource = null;
            UpdateMapClickZones();
            return;
        }

        if (waitingForTerritoryAttackSource)
        {
            FactionData player = GameManager.Instance.GetPlayerFaction();
            if (player != null && territory.controlledBy == player.factionId && territory.troops > 1)
            {
                pendingAttackSourceTerritory = territory;
                waitingForTerritoryAttackSource = false;
                diceChoiceTitle.text = $"ATTACK FROM {territory.name}";
                ConfigureDiceChoiceButtons(territory);
                diceChoicePopup.SetActive(true);
                ShowAttackStatus($"Choose attack dice for {territory.name}");
                return;
            }

            AddLogMessage("Select a territory you own with at least 2 troops.");
            return;
        }

        if (waitingForTerritoryAttackTarget)
        {
            FactionData player = GameManager.Instance.GetPlayerFaction();
            if (territory.controlledBy != player.factionId)
            {
                // Adjacency guard — mirrors AttackPhaseUI fix
                List<string> adjCheck = ProvinceGraph.GetAttackNeighbours(pendingAttackSourceTerritory.name);
                if (!adjCheck.Contains(territory.name))
                {
                    AddLogMessage($"{territory.name} is not adjacent to {pendingAttackSourceTerritory.name}.");
                    return;
                }

                CardData card = pendingCard;
                TerritoryData source = pendingAttackSourceTerritory;
                int chosenDice = GameManager.Instance.pendingAttackerDiceCount;
                ExitTerritoryAttackMode();
                GameManager.Instance.pendingAttackerDiceCount = chosenDice;
                if (card != null)
                    StartCoroutine(AnimateCardPlay(card, pendingCardPosition));
                GameManager.Instance.PlayTerritoryAttackCard(card, source, territory);
                ShowAttackArrow(source, territory);
                if (TerritoryGraphRenderer.Instance != null)
                    TerritoryGraphRenderer.Instance.ShowPendingAttack(source.name, territory.name);
            }
            else
            {
                AddLogMessage("Select an enemy or neutral territory to attack.");
            }
            return;
        }
    }

    public void UpdateTerritoryMap()
    {
        foreach (var kvp in territoryMapEntries)
        {
            TerritoryData tData = GameManager.Instance.territories.Find(t => t.name == kvp.Key);
            if (tData != null)
                UpdateTerritoryMapEntry(kvp.Value, tData);
        }
        UpdateMapClickZones();
        if (TerritoryGraphRenderer.Instance != null)
            TerritoryGraphRenderer.Instance.RedrawArrows();
    }

    Dictionary<string, Vector2> BuildTerritoryMapPositions()
    {
        var dict = new Dictionary<string, Vector2>();
        List<string> names = TerritoryGraph.GetAllTerritoryNames();
        if (names == null || names.Count == 0)
        {
            Debug.LogError("[GameUIManager] TerritoryGraph.GetAllTerritoryNames() returned empty — map click zones will not be built.");
            return dict;
        }
        foreach (string name in TerritoryGraph.GetAllTerritoryNames())
        {
            Vector2 rawUV = TerritoryGraphRenderer.GetTerritoryPosition(name);
            if (rawUV == Vector2.zero) continue;
            dict[name] = TerritoryUvToAnchor(rawUV);
        }
        return dict;
    }

    Vector2 TerritoryUvToAnchor(Vector2 rawUV)
    {
        return new Vector2(
            Mathf.Clamp01(rawUV.x),
            1f - Mathf.Clamp01(rawUV.y));
    }

    IEnumerator AssignMapClickZonesParent()
    {
        while (MapLayerController.Instance == null || MapLayerController.Instance.MapImage == null)
            yield return null;
        if (mapClickZonesRoot == null) yield break;

        mapClickZonesRoot.transform.SetParent(MapLayerController.Instance.MapImage.transform, false);
        Anchor(mapClickZonesRoot.GetComponent<RectTransform>(), 0f, 0f, 1f, 1f);
    }

    void BuildTerritorySidebar(Transform ct)
    {
        GameObject tabContainer = new GameObject("TerritoryTabContainer", typeof(RectTransform));
        tabContainer.transform.SetParent(ct, false);

        GameObject sidePanel = new GameObject("TerritorySidebar", typeof(RectTransform));
        sidePanel.transform.SetParent(tabContainer.transform, false);
        Image spBg = sidePanel.AddComponent<Image>();
        spBg.color = new Color(0.06f, 0.03f, 0.015f, 0.92f);
        spBg.raycastTarget = false;
        RectTransform sideRect = sidePanel.GetComponent<RectTransform>();
        Anchor(sideRect, 0.00f, 0.06f, 0.42f, 0.46f);
        sideRect.pivot = new Vector2(0f, 0f);

        Text sideTitle = MakeUIText(sidePanel.transform, "SideTitle", 12, FontStyle.Bold,
            Gold, new Vector2(0.02f, 0.85f), new Vector2(0.98f, 1f)).GetComponent<Text>();
        sideTitle.text = "TERRITORIES";

        GameObject svGO = new GameObject("ScrollView", typeof(RectTransform));
        svGO.transform.SetParent(sidePanel.transform, false);
        ScrollRect sr = svGO.AddComponent<ScrollRect>();
        Image svBg = svGO.AddComponent<Image>();
        svBg.color = new Color(0.04f, 0.02f, 0.01f, 0.85f);
        svBg.raycastTarget = false;
        Anchor(svGO.GetComponent<RectTransform>(), 0, 0, 1, 0.82f);

        GameObject vp = new GameObject("Viewport", typeof(RectTransform));
        vp.transform.SetParent(svGO.transform, false);
        vp.AddComponent<Mask>();
        vp.AddComponent<Image>().color = Color.clear;
        FullStretch(vp.GetComponent<RectTransform>());

        GameObject content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(vp.transform, false);
        RectTransform cR = content.GetComponent<RectTransform>();
        cR.anchorMin = new Vector2(0, 1);
        cR.anchorMax = new Vector2(1, 1);
        cR.pivot = new Vector2(0.5f, 1f);
        cR.sizeDelta = Vector2.zero;
        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        VerticalLayoutGroup vlgContent = content.AddComponent<VerticalLayoutGroup>();
        vlgContent.childForceExpandWidth = true;
        vlgContent.childControlHeight = true;
        vlgContent.spacing = 1;
        vlgContent.padding = new RectOffset(2, 2, 2, 2);

        sr.viewport = vp.GetComponent<RectTransform>();
        sr.content = cR;
        sr.vertical = true;
        sr.horizontal = false;
        sr.movementType = ScrollRect.MovementType.Clamped;

        territoryMapEntries.Clear();
        _lastSidebarRegion = null;
        List<string> allRegions = TerritoryGraph.GetAllRegionNames();
        foreach (string region in allRegions)
        {
            List<string> terrNames = TerritoryGraph.GetRegionTerritories(region);
            foreach (string name in terrNames)
                CreateTerritoryMapEntry(content.transform, name, region);
        }

        sidePanel.SetActive(false);

        Button toggleBtn = MakeButton(tabContainer.transform, "TerritoryToggleBtn",
            Vector2.zero, new Vector2(100, 28), "TERRITORIES ▾",
            new Color(0.15f, 0.10f, 0.05f, 0.8f),
            new Color(0.25f, 0.18f, 0.10f, 0.9f));
        RectTransform tR = toggleBtn.GetComponent<RectTransform>();
        tR.anchorMin = new Vector2(0.00f, 0.46f);
        tR.anchorMax = new Vector2(0.14f, 0.51f);
        tR.sizeDelta = Vector2.zero;
        tR.anchoredPosition = Vector2.zero;
        toggleBtn.GetComponentInChildren<Text>().fontSize = 11;
        toggleBtn.onClick.AddListener(() =>
        {
            sidePanel.SetActive(!sidePanel.activeSelf);
            toggleBtn.GetComponentInChildren<Text>().text = sidePanel.activeSelf ? "TERRITORIES ▴" : "TERRITORIES ▾";
        });
    }

    void BuildMapClickZones(Transform ct)
    {
        mapClickZonesRoot = new GameObject("MapClickZones", typeof(RectTransform));
        mapClickZonesRoot.transform.SetParent(ct, false);
        Anchor(mapClickZonesRoot.GetComponent<RectTransform>(),
            MapAnchorMin.x, MapAnchorMin.y, MapAnchorMax.x, MapAnchorMax.y);
        StartCoroutine(AssignMapClickZonesParent());

        territoryMapPositions = BuildTerritoryMapPositions();
        if (territoryMapPositions.Count == 0) return;

        foreach (var kvp in territoryMapPositions)
        {
            string terrName = kvp.Key;
            Vector2 pos = kvp.Value;

            GameObject zone = new GameObject("MapBtn_" + terrName.Replace(" ", ""), typeof(RectTransform));
            zone.transform.SetParent(mapClickZonesRoot.transform, false);

            Image zoneImg = zone.AddComponent<Image>();
            zoneImg.color = Color.clear;
            zoneImg.raycastTarget = true;

            Button zoneBtn = zone.AddComponent<Button>();
            zoneBtn.targetGraphic = zoneImg;
            Navigation nav = new Navigation { mode = Navigation.Mode.None };
            zoneBtn.navigation = nav;

            RectTransform zoneR = zone.GetComponent<RectTransform>();
            zoneR.anchorMin = zoneR.anchorMax = pos;
            zoneR.pivot = new Vector2(0.5f, 0.5f);
            zoneR.sizeDelta = new Vector2(60, 60);
            zoneR.anchoredPosition = Vector2.zero;

            Text nameLabel = MakeUIText(zone.transform, "Label", 8, FontStyle.Bold,
                                         Parchment.WithA(0.75f), new Vector2(0f, 1f), new Vector2(1f, 1f)).GetComponent<Text>();
            RectTransform nlR = nameLabel.GetComponent<RectTransform>();
            nlR.pivot = new Vector2(0.5f, 0f);
            nlR.sizeDelta = new Vector2(0, 16);
            nlR.anchoredPosition = Vector2.zero;
            nameLabel.text = terrName;
            nameLabel.alignment = TextAnchor.UpperCenter;
            nameLabel.raycastTarget = false;

            GameObject badgeGO = new GameObject("TroopBadge", typeof(RectTransform));
            badgeGO.transform.SetParent(zone.transform, false);
            Image badgeBg = badgeGO.AddComponent<Image>();
            badgeBg.raycastTarget = false;
            RectTransform badgeR = badgeGO.GetComponent<RectTransform>();
            badgeR.anchorMin = badgeR.anchorMax = new Vector2(0.5f, 0.5f);
            badgeR.sizeDelta = new Vector2(28, 18);
            badgeR.anchoredPosition = Vector2.zero;

            Text badgeText = MakeUIText(badgeGO.transform, "Count", 9, FontStyle.Bold,
                                         Parchment, Vector2.zero, Vector2.one).GetComponent<Text>();
            badgeText.raycastTarget = false;

            GameObject ringGO = new GameObject("PhaseRing", typeof(RectTransform));
            ringGO.transform.SetParent(zone.transform, false);
            Image ringImg = ringGO.AddComponent<Image>();
            ringImg.sprite = GetPhaseCircleSprite();
            ringImg.color = Color.clear;
            ringImg.raycastTarget = false;
            RectTransform ringR = ringGO.GetComponent<RectTransform>();
            ringR.anchorMin = ringR.anchorMax = new Vector2(0.5f, 0.5f);
            ringR.sizeDelta = new Vector2(72, 72);
            ringR.anchoredPosition = Vector2.zero;

            string captured = terrName;
            zoneBtn.onClick.AddListener(() => OnMapTerritoryClicked(captured));

            TerritoryClickHandler rch = zone.AddComponent<TerritoryClickHandler>();
            rch.territory = GameManager.Instance?.territories.Find(t => t.name == captured);
            rch.onRightClick = OnTerritoryRightClicked;

            mapClickButtons[terrName] = zoneBtn;
            mapClickZoneButtons[terrName] = zoneBtn;
        }
    }

    void UpdateMapClickZones()
    {
        if (GameManager.Instance?.territories == null) return;
        FactionData player = GameManager.Instance.GetPlayerFaction();

        foreach (var kvp in mapClickButtons)
        {
            Button btn = kvp.Value;
            if (btn == null) continue;

            TerritoryData tData = GameManager.Instance.territories.Find(t => t.name == kvp.Key);
            if (tData == null) continue;

            TerritoryClickHandler clickHandler = btn.GetComponent<TerritoryClickHandler>();
            if (clickHandler != null)
            {
                clickHandler.territory = tData;
                clickHandler.onRightClick = OnTerritoryRightClicked;
            }

            Transform badge = btn.transform.Find("TroopBadge");
            if (badge != null)
            {
                Image badgeBg = badge.GetComponent<Image>();
                Text badgeText = badge.GetComponentInChildren<Text>();

                Color ownerColor = Color.gray;
                if (tData.controlledBy >= 0 && tData.controlledBy < GameManager.Instance.factions.Count)
                    ownerColor = GameManager.Instance.factions[tData.controlledBy].factionColor;

                if (badgeBg != null)
                {
                    if (tData.controlledBy >= 0)
                        badgeBg.color = new Color(ownerColor.r * 0.6f, ownerColor.g * 0.6f, ownerColor.b * 0.6f, 0.9f);
                    else
                        badgeBg.color = new Color(0.2f, 0.2f, 0.2f, 0.7f);
                }

                if (badgeText != null)
                    badgeText.text = tData.troops > 0 ? tData.troops.ToString() : "-";

                if (!mapTroopCache.TryGetValue(kvp.Key, out int prev) || prev != tData.troops)
                {
                    mapTroopCache[kvp.Key] = tData.troops;
                    StartCoroutine(PulseBadge(badge.gameObject));
                }
            }

            Image zoneImg = btn.GetComponent<Image>();
            if (zoneImg == null) continue;
            Image ringImg = btn.transform.Find("PhaseRing")?.GetComponent<Image>();
            if (ringImg != null) ringImg.color = Color.clear;

            if (GameManager.Instance.IsWaitingForReinforcements)
            {
                bool owned = player != null && tData.controlledBy == player.factionId;
                zoneImg.color = owned ? new Color(0.3f, 0.5f, 0.9f, 0.20f) : Color.clear;
                if (ringImg != null && owned)
                    ringImg.color = new Color(0.25f, 0.65f, 1.0f, 0.85f);
            }
            else if (waitingForTerritoryAttackSource)
            {
                bool valid = player != null && tData.controlledBy == player.factionId && tData.troops > 1;
                zoneImg.color = valid ? new Color(0.1f, 0.7f, 0.1f, 0.30f) : Color.clear;
                if (ringImg != null)
                    ringImg.color = valid ? new Color(0.9f, 0.65f, 0.1f, 0.85f) : Color.clear;

                Transform attackBadge = btn.transform.Find("TroopBadge");
                if (attackBadge != null)
                {
                    Image badgeBg = attackBadge.GetComponent<Image>();
                    if (badgeBg != null)
                        badgeBg.color = valid
                            ? new Color(0.15f, 0.70f, 0.15f, 1.0f)
                            : new Color(0.25f, 0.25f, 0.25f, 0.6f);
                }
            }
            else if (waitingForTerritoryAttackTarget && pendingAttackSourceTerritory != null)
            {
                bool isTarget = tData.controlledBy != player.factionId;
                bool isSource = tData.name == pendingAttackSourceTerritory.name;
                zoneImg.color = isTarget ? new Color(0.8f, 0.2f, 0.1f, 0.30f) : Color.clear;
                if (ringImg != null)
                    ringImg.color = isTarget ? new Color(1.0f, 0.25f, 0.1f, 0.90f) : Color.clear;

                Transform targetBadge = btn.transform.Find("TroopBadge");
                if (targetBadge != null)
                {
                    Image badgeBg = targetBadge.GetComponent<Image>();
                    if (badgeBg != null)
                        badgeBg.color = isTarget
                            ? new Color(0.75f, 0.15f, 0.08f, 1.0f)
                            : isSource
                                ? new Color(0.1f, 0.55f, 0.75f, 1.0f)
                                : new Color(0.25f, 0.25f, 0.25f, 0.6f);
                }
            }
            else if (GameManager.Instance.currentState == GameState.FORTIFY_PHASE)
            {
                if (waitingForFortifySource)
                {
                    bool valid = player != null && tData.controlledBy == player.factionId && tData.troops > 1;
                    zoneImg.color = valid ? new Color(0.3f, 0.7f, 0.3f, 0.25f) : Color.clear;
                    if (ringImg != null && valid)
                        ringImg.color = new Color(0.35f, 0.9f, 0.35f, 0.85f);
                }
                else if (waitingForFortifyTarget && pendingFortifySource != null)
                {
                    bool valid = player != null && IsValidFortifyDestination(pendingFortifySource, tData, player.factionId);
                    zoneImg.color = valid ? new Color(0.3f, 0.5f, 0.3f, 0.25f) : Color.clear;
                    if (ringImg != null && valid)
                        ringImg.color = new Color(0.35f, 0.9f, 0.35f, 0.85f);
                }
                else if (waitingForNeutralExpandSource)
                {
                    bool valid = player != null && tData.controlledBy == player.factionId && tData.troops >= 10;
                    zoneImg.color = valid ? new Color(0.15f, 0.75f, 0.15f, 0.35f) : Color.clear;
                }
                else if (waitingForNeutralExpandTarget && pendingExpandSource != null)
                {
                    bool valid = tData.controlledBy == -1 && TerritoryGraph.AreAdjacent(pendingExpandSource.name, tData.name);
                    zoneImg.color = valid ? new Color(0.45f, 0.85f, 0.25f, 0.40f) : Color.clear;
                }
                else
                {
                    zoneImg.color = Color.clear;
                }
            }
            else if (GameManager.Instance.currentState == GameState.ATTACK_PHASE)
            {
                zoneImg.color = new Color(0, 0, 0, 0.08f);
            }
            else
            {
                zoneImg.color = new Color(0, 0, 0, 0.06f);
                if (badge != null)
                {
                    Image badgeBg = badge.GetComponent<Image>();
                    if (badgeBg != null && tData.controlledBy >= 0)
                    {
                        Color ownerColor = tData.controlledBy < GameManager.Instance.factions.Count
                            ? GameManager.Instance.factions[tData.controlledBy].factionColor
                            : Color.gray;
                        badgeBg.color = new Color(ownerColor.r * 0.6f, ownerColor.g * 0.6f,
                                                  ownerColor.b * 0.6f, 0.9f);
                    }
                }
            }
        }
    }

    void OnMapTerritoryClicked(string territoryName)
    {
        TerritoryData territory = GameManager.Instance?.territories.Find(t => t.name == territoryName);
        if (territory != null)
            OnTerritoryClicked(territory);
    }

    IEnumerator PulseTerritoryEntry(GameObject entry)
    {
        if (entry == null) yield break;
        float duration = 0.15f;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float s = 1f + 0.15f * (1f - t / duration);
            entry.transform.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        entry.transform.localScale = Vector3.one;
    }

    void OnTerritoryRightClicked(TerritoryData territory)
    {
        if (territory == null) return;
        if (GameManager.Instance.IsWaitingForReinforcements)
        {
            FactionData player = GameManager.Instance.GetPlayerFaction();
            if (territory.controlledBy == player.factionId)
                GameManager.Instance.AssignReinforcementToTerritory(territory, player.pendingReinforcements);
        }
    }

    /// <summary>Converts a screen-space click position to normalized UV within the map area.</summary>
    public Vector2 ScreenToMapUV(Vector2 screenPos)
    {
        Vector2 viewPos = new Vector2(screenPos.x / Screen.width, screenPos.y / Screen.height);
        float mapLeft = MapAnchorMin.x;
        float mapRight = MapAnchorMax.x;
        float mapBot = MapAnchorMin.y;
        float mapTop = MapAnchorMax.y;
        float uvX = (viewPos.x - mapLeft) / (mapRight - mapLeft);
        float uvY = (viewPos.y - mapBot) / (mapTop - mapBot);
        return new Vector2(uvX, uvY);
    }

    /// <summary>Finds the nearest province to a screen-space click position using ProvinceData.mapPosition.</summary>
    public ProvinceData FindProvinceAtScreenPoint(Vector2 screenPos)
    {
        Vector2 uv = ScreenToMapUV(screenPos);
        if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1) return null;

        float mapProvY = 1f - uv.y;

        ProvinceData closest = null;
        float closestDist = float.MaxValue;
        float threshold = 0.075f;

        foreach (ProvinceData prov in ProvinceDatabase.GetAllProvinces())
        {
            float dx = prov.mapPosition.x - uv.x;
            float dy = prov.mapPosition.y - mapProvY;
            float dist = dx * dx + dy * dy;
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = prov;
            }
        }

        if (closest != null && closestDist < threshold * threshold)
            return closest;
        return null;
    }

    /// <summary>Called when a province is detected under the click point.</summary>
    public void OnProvinceClicked(ProvinceData province)
    {
        if (province == null) return;
        Debug.Log($"[GameUIManager] Province clicked: {province.provinceName}");
    }

    // FIXED: UI FIX 3 - moved from center-screen to right panel area; confirm button always visible
    void BuildReinforcementPanel(Transform ct)
    {
        reinforcementPanel = new GameObject("ReinforcementPanel", typeof(RectTransform));
        reinforcementPanel.transform.SetParent(ct, false);
        Image rpBg = reinforcementPanel.AddComponent<Image>();
        rpBg.color = new Color(0, 0, 0, 0.85f);
        rpBg.raycastTarget = true;
        Anchor(reinforcementPanel.GetComponent<RectTransform>(), 0.01f, 0.72f, 0.32f, 0.97f);
        reinforcementPanel.SetActive(false);

        GameObject border = new GameObject("Border", typeof(RectTransform));
        border.transform.SetParent(reinforcementPanel.transform, false);
        border.AddComponent<Image>().color = Gold.WithA(0.50f);
        RectTransform bR = border.GetComponent<RectTransform>();
        bR.anchorMin = Vector2.zero;
        bR.anchorMax = Vector2.one;
        bR.sizeDelta = Vector2.zero;

        GameObject inner = new GameObject("Inner", typeof(RectTransform));
        inner.transform.SetParent(reinforcementPanel.transform, false);
        inner.AddComponent<Image>().color = Darkest;
        RectTransform iR = inner.GetComponent<RectTransform>();
        iR.anchorMin = Vector2.zero;
        iR.anchorMax = Vector2.one;
        iR.sizeDelta = new Vector2(-4, -4);

        MakeUIText(reinforcementPanel.transform, "Title", 16, FontStyle.Bold,
                   Gold, new Vector2(0.05f, 0.72f), new Vector2(0.95f, 0.92f))
            .GetComponent<Text>().text = "REINFORCEMENT";

        reinforcementRemainingText = MakeUIText(reinforcementPanel.transform, "Remaining", 13, FontStyle.Normal,
                                                 Parchment, new Vector2(0.05f, 0.45f), new Vector2(0.95f, 0.68f)).GetComponent<Text>();

        MakeUIText(reinforcementPanel.transform, "Instruction", 10, FontStyle.Normal,
                   Parchment.WithA(0.65f), new Vector2(0.05f, 0.52f), new Vector2(0.95f, 0.68f))
            .GetComponent<Text>().text = "Click your territories to place troops";

        reinforcementConfirmBtn = MakeButton(reinforcementPanel.transform, "ConfirmBtn",
                                              new Vector2(0, -10), new Vector2(120, 30), "CONFIRM", Med, Gold);
        reinforcementConfirmBtn.onClick.AddListener(OnReinforcementConfirm);
        reinforcementConfirmBtn.gameObject.SetActive(true);
    }

    // FIXED: UI FIX 3 - confirm button always visible with dynamic count text
    public void ShowReinforcementPanel(int remaining)
    {
        if (reinforcementPanel == null) return;
        if (remaining <= 0)
        {
            reinforcementPanel.SetActive(false);
            return;
        }
        reinforcementRemainingText.text = $"Troops to place: {remaining}";
        reinforcementPanel.SetActive(true);
        reinforcementConfirmBtn.GetComponentInChildren<Text>().text =
            remaining > 0 ? $"CONFIRM ({remaining} left)" : "CONFIRM";
        reinforcementConfirmBtn.gameObject.SetActive(true);
        UpdateTerritoryMap();
    }

    public void UpdateReinforcementPanel(int remaining)
    {
        if (reinforcementRemainingText != null)
            reinforcementRemainingText.text = $"Troops to place: {remaining}";
        if (reinforcementConfirmBtn != null)
        {
            reinforcementConfirmBtn.GetComponentInChildren<Text>().text =
                remaining > 0 ? $"CONFIRM ({remaining} left)" : "CONFIRM";
            reinforcementConfirmBtn.gameObject.SetActive(true);
        }
        UpdateTerritoryMap();
    }

    void OnReinforcementConfirm()
    {
        reinforcementPanel.SetActive(false);
        if (GameManager.Instance != null)
            GameManager.Instance.IsWaitingForReinforcements = false;
    }

    void OnExpandButtonClicked()
    {
        if (GameManager.Instance?.currentState != GameState.PLAY_PHASE) return;
        if (!GameManager.Instance.CanPlayMoreCards())
        {
            AddLogMessage("No card slots remaining for expansion this turn.");
            return;
        }
        waitingForNeutralExpandSource = true;
        waitingForNeutralExpandTarget = false;
        pendingExpandSource = null;
        AddLogMessage("EXPAND: Select one of your territories with 10+ troops.");
        UpdateMapClickZones();
    }

    void BuildCombatPopup(Transform ct)
    {
        combatPopup = new GameObject("CombatPopup", typeof(RectTransform));
        combatPopup.transform.SetParent(ct, false);
        Image cpBg = combatPopup.AddComponent<Image>();
        cpBg.color = new Color(0, 0, 0, 0.88f);
        cpBg.raycastTarget = true;
        FullStretch(combatPopup.GetComponent<RectTransform>());
        combatPopup.SetActive(false);

        GameObject cpBorder = new GameObject("Border", typeof(RectTransform));
        cpBorder.transform.SetParent(combatPopup.transform, false);
        cpBorder.AddComponent<Image>().color = Rust;
        RectTransform cbR = cpBorder.GetComponent<RectTransform>();
        cbR.anchorMin = new Vector2(0.28f, 0.22f);
        cbR.anchorMax = new Vector2(0.72f, 0.72f);
        cbR.sizeDelta = Vector2.zero;

        GameObject cpInner = new GameObject("Inner", typeof(RectTransform));
        cpInner.transform.SetParent(combatPopup.transform, false);
        cpInner.AddComponent<Image>().color = Darkest;
        RectTransform ciR = cpInner.GetComponent<RectTransform>();
        ciR.anchorMin = new Vector2(0.29f, 0.23f);
        ciR.anchorMax = new Vector2(0.71f, 0.71f);
        ciR.sizeDelta = Vector2.zero;

        combatPopupTitle = MakeUIText(combatPopup.transform, "Title", 24, FontStyle.Bold,
                                      Rust, new Vector2(0.30f, 0.62f), new Vector2(0.70f, 0.70f)).GetComponent<Text>();
        combatPopupTitle.text = "BATTLE";

        GameObject svGO = new GameObject("ScrollView", typeof(RectTransform));
        svGO.transform.SetParent(combatPopup.transform, false);
        ScrollRect sr = svGO.AddComponent<ScrollRect>();
        svGO.AddComponent<Image>().color = new Color(0.04f, 0.02f, 0.01f, 0.85f);
        RectTransform svR = svGO.GetComponent<RectTransform>();
        svR.anchorMin = new Vector2(0.30f, 0.28f);
        svR.anchorMax = new Vector2(0.70f, 0.58f);
        svR.sizeDelta = Vector2.zero;

        GameObject vp = new GameObject("Viewport", typeof(RectTransform));
        vp.transform.SetParent(svGO.transform, false);
        vp.AddComponent<Mask>();
        vp.AddComponent<Image>().color = Color.clear;
        FullStretch(vp.GetComponent<RectTransform>());

        GameObject ctGO = new GameObject("Content", typeof(RectTransform));
        ctGO.transform.SetParent(vp.transform, false);
        combatPopupDetails = ctGO.AddComponent<Text>();
        combatPopupDetails.font = GetFont();
        combatPopupDetails.fontSize = 13;
        combatPopupDetails.color = Parchment;
        combatPopupDetails.alignment = TextAnchor.UpperCenter;
        combatPopupDetails.horizontalOverflow = HorizontalWrapMode.Wrap;
        combatPopupDetails.lineSpacing = 1.2f;
        combatPopupDetails.supportRichText = true;
        RectTransform ctR = ctGO.GetComponent<RectTransform>();
        ctR.anchorMin = new Vector2(0, 1);
        ctR.anchorMax = new Vector2(1, 1);
        ctR.sizeDelta = Vector2.zero;
        ContentSizeFitter csf = ctGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        combatPopupScrollRect = sr;
        sr.viewport = vp.GetComponent<RectTransform>();
        sr.content = ctR;
        sr.vertical = true;
        sr.horizontal = false;
        sr.movementType = ScrollRect.MovementType.Clamped;
        sr.scrollSensitivity = 20;

        combatPopupDismissBtn = MakeButton(combatPopup.transform, "DismissBtn",
                                            new Vector2(0, -120), new Vector2(120, 40), "OK", Med, Gold);

        Button closeBtn = MakeButton(combatPopup.transform, "CombatClose",
            new Vector2(0, 0), new Vector2(40, 40), "\u2715", Med, Gold);
        RectTransform closeR = closeBtn.GetComponent<RectTransform>();
        closeR.anchorMin = new Vector2(0.88f, 0.88f);
        closeR.anchorMax = new Vector2(1.00f, 1.00f);
        closeR.sizeDelta = Vector2.zero;
        closeR.anchoredPosition = Vector2.zero;
        closeBtn.onClick.AddListener(() => {
            if (combatAutoDismiss != null) StopCoroutine(combatAutoDismiss);
            combatPopup.SetActive(false);
        });
    }

    public void ShowCombatPopup(CombatResult result, TerritoryData source, TerritoryData target)
    {
        if (combatPopup == null) return;

        string title = result.territoryCaptured ? "VICTORY" : "BATTLE REPORT";
        combatPopupTitle.text = title;
        combatPopupTitle.color = result.territoryCaptured ? Gold : Rust;

        string details = $"<b>{source.name}</b> attacks <b>{target.name}</b>\n\n";

        if (!string.IsNullOrEmpty(result.battleLog))
        {
            details += "<size=11>" + result.battleLog.Replace("\n", "\n") + "</size>\n\n";
        }
        else
        {
            details += $"<size=14>Attacker Dice:</size>\n<size=24><color=#C8A040>{DiceFaces(result.attackerDice)}</color></size>\n";
            details += $"<size=14>Defender Dice:</size>\n<size=24><color=#CC5555>{DiceFaces(result.defenderDice)}</color></size>\n\n";
        }

        details += $"Attacker lost: {result.attackerTroopsLost} troops\n";
        details += $"Defender lost: {result.defenderTroopsLost} troops\n\n";

        if (result.territoryCaptured)
            details += $"<color=#C8A040>TERRITORY CAPTURED!</color>";
        else
            details += $"<color=#CC5555>Attack repelled.</color>";

        combatPopupDetails.text = details;
        combatPopup.SetActive(true);
        if (combatPopupScrollRect != null)
            StartCoroutine(ScrollCombatLogToBottom());

        if (combatAutoDismiss != null)
            StopCoroutine(combatAutoDismiss);
        combatAutoDismiss = StartCoroutine(AutoDismissCombatPopup());
    }

    IEnumerator ScrollCombatLogToBottom()
    {
        yield return null;
        if (combatPopupScrollRect != null)
            combatPopupScrollRect.verticalNormalizedPosition = 0f;
    }

    IEnumerator AutoDismissCombatPopup()
    {
        yield return new WaitForSeconds(3.5f);
        combatPopup.SetActive(false);
        combatAutoDismiss = null;
    }

    string DiceFaces(int[] dice)
    {
        if (dice == null || dice.Length == 0) return "";
        string[] faces = { "\u2680", "\u2681", "\u2682", "\u2683", "\u2684", "\u2685" };
        string result = "";
        foreach (int d in dice)
            result += faces[Mathf.Clamp(d - 1, 0, 5)];
        return result;
    }

    void BuildFortifyPanel(Transform ct)
    {
        fortifyPanel = new GameObject("FortifyPanel", typeof(RectTransform));
        fortifyPanel.transform.SetParent(ct, false);
        Image fpBg = fortifyPanel.AddComponent<Image>();
        fpBg.color = new Color(0, 0, 0, 0.85f);
        fpBg.raycastTarget = false;
        Anchor(fortifyPanel.GetComponent<RectTransform>(), 0.20f, 0.60f, 0.75f, 0.92f);
        fortifyPanel.SetActive(false);

        fortifyTitleText = MakeUIText(fortifyPanel.transform, "Title", 18, FontStyle.Bold,
            Gold, new Vector2(0.05f, 0.70f), new Vector2(0.95f, 0.95f)).GetComponent<Text>();
        fortifyTitleText.text = "FORTIFY PHASE";

        fortifyInstructionText = MakeUIText(fortifyPanel.transform, "Instruction", 11, FontStyle.Normal,
            Parchment.WithA(0.65f), new Vector2(0.05f, 0.28f), new Vector2(0.95f, 0.65f)).GetComponent<Text>();
        fortifyInstructionText.text = "Click a territory with excess troops, then a connected friendly territory.";

        // ADDED: UPGRADE 3 - fortify count slider
        GameObject sliderRow = new GameObject("SliderRow", typeof(RectTransform));
        sliderRow.transform.SetParent(fortifyPanel.transform, false);
        RectTransform srR = sliderRow.GetComponent<RectTransform>();
        srR.anchorMin = new Vector2(0.30f, 0.22f);
        srR.anchorMax = new Vector2(0.70f, 0.36f);
        srR.sizeDelta = Vector2.zero;

        fortifyCountText = MakeUIText(sliderRow.transform, "CountLabel", 12, FontStyle.Bold,
            Parchment, new Vector2(0f, 0.6f), new Vector2(1f, 1f)).GetComponent<Text>();
        fortifyCountText.text = "Troops to move: 1";
        fortifyCountText.alignment = TextAnchor.MiddleCenter;

        GameObject sliderGO = new GameObject("Slider", typeof(RectTransform));
        sliderGO.transform.SetParent(sliderRow.transform, false);
        fortifySlider = sliderGO.AddComponent<Slider>();
        RectTransform slR = sliderGO.GetComponent<RectTransform>();
        slR.anchorMin = new Vector2(0f, 0f);
        slR.anchorMax = new Vector2(1f, 0.5f);
        slR.sizeDelta = Vector2.zero;
        Image sliderBg = sliderGO.AddComponent<Image>();
        sliderBg.color = new Color(0.2f, 0.12f, 0.04f);
        GameObject fillArea = new GameObject("FillArea", typeof(RectTransform));
        fillArea.transform.SetParent(sliderGO.transform, false);
        RectTransform faR = fillArea.GetComponent<RectTransform>();
        faR.anchorMin = Vector2.zero; faR.anchorMax = Vector2.one;
        faR.sizeDelta = new Vector2(-10, -4);
        GameObject fillImg = new GameObject("Fill", typeof(RectTransform));
        fillImg.transform.SetParent(fillArea.transform, false);
        Image fillImgC = fillImg.AddComponent<Image>();
        fillImgC.color = Gold;
        RectTransform fiR2 = fillImg.GetComponent<RectTransform>();
        fiR2.anchorMin = Vector2.zero; fiR2.anchorMax = new Vector2(1, 1);
        fiR2.sizeDelta = Vector2.zero;
        GameObject handle = new GameObject("Handle", typeof(RectTransform));
        handle.transform.SetParent(sliderGO.transform, false);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = Parchment;
        RectTransform haR = handle.GetComponent<RectTransform>();
        haR.sizeDelta = new Vector2(16, 16);
        haR.anchorMin = haR.anchorMax = new Vector2(0.5f, 0.5f);
        fortifySlider.fillRect = fiR2;
        fortifySlider.handleRect = haR;
        fortifySlider.minValue = 1;
        fortifySlider.maxValue = 10;
        fortifySlider.wholeNumbers = true;
        fortifySlider.value = 1;
        fortifySlider.onValueChanged.AddListener(val => { fortifyCountText.text = $"Troops to move: {(int)val}"; selectedFortifyCount = (int)val; });
        sliderRow.SetActive(false);

        fortifySkipBtn = MakeButton(fortifyPanel.transform, "SkipBtn",
            Vector2.zero, new Vector2(100, 40), "SKIP", Med, Gold);
        RectTransform skipR = fortifySkipBtn.GetComponent<RectTransform>();
        skipR.anchorMin = new Vector2(0.05f, 0.05f);
        skipR.anchorMax = new Vector2(0.45f, 0.22f);
        skipR.sizeDelta = Vector2.zero;
        skipR.anchoredPosition = Vector2.zero;
        fortifySkipBtn.onClick.AddListener(OnFortifySkip);

        fortifyConfirmBtn = MakeButton(fortifyPanel.transform, "ConfirmBtn",
            Vector2.zero, new Vector2(100, 40), "CONFIRM", Dark, Med);
        RectTransform confirmR = fortifyConfirmBtn.GetComponent<RectTransform>();
        confirmR.anchorMin = new Vector2(0.55f, 0.05f);
        confirmR.anchorMax = new Vector2(0.95f, 0.22f);
        confirmR.sizeDelta = Vector2.zero;
        confirmR.anchoredPosition = Vector2.zero;
        fortifyConfirmBtn.onClick.AddListener(OnFortifyConfirm);
        fortifyConfirmBtn.gameObject.SetActive(false);
    }

    void ShowFortifyPanel()
    {
        if (fortifyPanel == null) return;
        fortifyPanel.SetActive(true);
        waitingForFortifySource = true;
        waitingForFortifyTarget = false;
        pendingFortifySource = null;
        AddLogMessage("FORTIFY: Click one of your territories with excess troops.");
        UpdateMapClickZones();
    }

    void HideFortifyPanel()
    {
        if (fortifyPanel == null) return;
        fortifyPanel.SetActive(false);
        waitingForFortifySource = false;
        waitingForFortifyTarget = false;
        pendingFortifySource = null;
        pendingFortifyDest = null;
        HidePhaseArrows();
        HideFortifyCountSlider();
        ClearTerritoryHighlights();
    }

    void OnFortifySkip()
    {
        HideFortifyPanel();
        if (GameManager.Instance != null)
            GameManager.Instance.IsWaitingForFortify = false;
    }

    // ADDED: UPGRADE 3 - confirm fortify with selected troop count
    void OnFortifyConfirm()
    {
        if (pendingFortifySource == null || pendingFortifyDest == null) return;
        int count = Mathf.Min(selectedFortifyCount, pendingFortifySource.troops - 1);
        if (count <= 0) { AddLogMessage("Not enough troops to move."); return; }
        if (GameManager.Instance.FortifyTroops(pendingFortifySource, pendingFortifyDest, count))
        {
            HideFortifyPanel();
            UpdateTerritoryMap();
        }
        else
        {
            AddLogMessage("Cannot fortify — territories not connected through friendly territory.");
        }
    }

    void ShowFortifyCountSlider(int maxCount)
    {
        if (fortifySlider == null || fortifyCountText == null) return;
        fortifySlider.maxValue = Mathf.Max(1, maxCount);
        fortifySlider.value = 1;
        selectedFortifyCount = 1;
        fortifyCountText.text = $"Troops to move: 1";
        // sliderRow is child of fortifyPanel, found by name
        Transform sliderRow = fortifyPanel.transform.Find("SliderRow");
        if (sliderRow != null) sliderRow.gameObject.SetActive(true);
        if (fortifyConfirmBtn != null) fortifyConfirmBtn.gameObject.SetActive(true);
    }

    void HideFortifyCountSlider()
    {
        Transform sliderRow = fortifyPanel != null ? fortifyPanel.transform.Find("SliderRow") : null;
        if (sliderRow != null) sliderRow.gameObject.SetActive(false);
        if (fortifyConfirmBtn != null) fortifyConfirmBtn.gameObject.SetActive(false);
    }

    // ADDED: UPGRADE 4 - continent/region bonus display panel
    void BuildRegionBonusPanel(Transform ct)
    {
        regionBonusPanel = new GameObject("RegionBonusPanel", typeof(RectTransform));
        regionBonusPanel.transform.SetParent(ct, false);
        Image rpBg = regionBonusPanel.AddComponent<Image>();
        rpBg.color = new Color(0.04f, 0.02f, 0.01f, 0.88f);
        rpBg.raycastTarget = false;
        Anchor(regionBonusPanel.GetComponent<RectTransform>(), 0.01f, 0.06f, 0.30f, 0.12f);

        regionBonusText = MakeUIText(regionBonusPanel.transform, "BonusText", 7, FontStyle.Normal,
            Gold.WithA(0.85f), new Vector2(0.02f, 0f), new Vector2(0.98f, 1f)).GetComponent<Text>();
        regionBonusText.alignment = TextAnchor.MiddleLeft;
        regionBonusText.supportRichText = true;
    }

    void UpdateRegionBonusDisplay()
    {
        if (regionBonusText == null || GameManager.Instance == null) return;
        FactionData player = GameManager.Instance.GetPlayerFaction();
        if (player == null) return;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        List<string> regions = TerritoryGraph.GetAllRegionNames();
        foreach (string region in regions)
        {
            int bonus = TerritoryGraph.GetRegionBonus(region);
            List<string> terrs = TerritoryGraph.GetRegionTerritories(region);
            string terrStr = string.Join(", ", terrs);

            bool owned = TerritoryGraph.OwnsEntireRegion(region, player.territories);
            string colorTag = owned ? "<color=#44CC44>" : "<color=#C8A040>";
            sb.AppendLine($"{colorTag}{region}</color> ({terrStr}) = +{bonus}");
        }
        regionBonusText.text = sb.ToString().TrimEnd();
    }

    void BuildPhaseBanner(Transform ct)
    {
        phaseBanner = new GameObject("PhaseBanner", typeof(RectTransform));
        phaseBanner.transform.SetParent(ct, false);
        Image bannerBg = phaseBanner.AddComponent<Image>();
        bannerBg.color = new Color(0, 0, 0, 0.82f);
        bannerBg.raycastTarget = false;
        phaseBanner.AddComponent<CanvasGroup>();
        Anchor(phaseBanner.GetComponent<RectTransform>(), 0f, 0.44f, 1f, 0.57f);

        GameObject accent = new GameObject("Accent", typeof(RectTransform));
        accent.transform.SetParent(phaseBanner.transform, false);
        Image accentImg = accent.AddComponent<Image>();
        accentImg.color = Gold;
        accentImg.raycastTarget = false;
        Anchor(accent.GetComponent<RectTransform>(), 0f, 0f, 0.003f, 1f);

        phaseBannerText = MakeUIText(phaseBanner.transform, "BannerText", 36, FontStyle.Bold,
                                      Parchment, new Vector2(0.01f, 0f), Vector2.one).GetComponent<Text>();
        phaseBannerText.alignment = TextAnchor.MiddleLeft;
        phaseBanner.SetActive(false);
    }

    IEnumerator ShowPhaseBanner(string label, Color color, float duration = 1.8f)
    {
        phaseBannerText.text = label;
        phaseBannerText.color = color;

        Image bannerBg = phaseBanner.GetComponent<Image>();
        if (bannerBg != null)
        {
            Color phaseColor = new Color(0.1f, 0.1f, 0.1f, 0.92f);
            string upper = label.ToUpper();
            if (upper.Contains("REINFORCEMENT")) phaseColor = new Color(0.2f, 0.5f, 0.2f, 0.92f);
            else if (upper.Contains("DRAW"))      phaseColor = new Color(0.3f, 0.3f, 0.6f, 0.92f);
            else if (upper.Contains("PLAY"))      phaseColor = new Color(0.5f, 0.4f, 0.1f, 0.92f);
            else if (upper.Contains("RESOLUTION")) phaseColor = new Color(0.5f, 0.2f, 0.1f, 0.92f);
            else if (upper.Contains("FORTIFY"))   phaseColor = new Color(0.2f, 0.4f, 0.5f, 0.92f);
            else if (upper.Contains("ATTACK"))    phaseColor = new Color(0.6f, 0.1f, 0.1f, 0.92f);
            bannerBg.color = phaseColor;
        }

        RectTransform rt = phaseBanner.GetComponent<RectTransform>();
        phaseBanner.SetActive(true);

        for (float t = 0; t < 0.18f; t += Time.deltaTime)
        {
            float x = Mathf.Lerp(-Screen.width, 0f, t / 0.18f);
            rt.anchoredPosition = new Vector2(x, 0f);
            yield return null;
        }
        rt.anchoredPosition = Vector2.zero;
        yield return StartCoroutine(AutoHideBanner(duration));
        phaseBannerCoroutine = null;
    }

    IEnumerator AutoHideBanner(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (phaseBanner == null) yield break;
        CanvasGroup cg = phaseBanner.GetComponent<CanvasGroup>();
        if (cg == null) cg = phaseBanner.AddComponent<CanvasGroup>();
        float t = 0f;
        while (t < 0.35f)
        {
            cg.alpha = 1f - (t / 0.35f);
            t += Time.deltaTime;
            yield return null;
        }
        phaseBanner.SetActive(false);
        cg.alpha = 1f;
    }

    void ShowCombatResultBanner(CombatResult result, TerritoryData source, TerritoryData target)
    {
        if (activeCombatBanner != null) Destroy(activeCombatBanner);

        string outcome = result.attackerWon
            ? $"\u2694 VICTORY! {source.name} \u2192 {target.name} captured!"
            : $"\uD83D\uDEE1 REPELLED! Attack from {source.name} failed.";
        Color bannerColor = result.attackerWon
            ? new Color(0.1f, 0.6f, 0.1f, 0.95f)
            : new Color(0.6f, 0.1f, 0.1f, 0.95f);

        GameObject banner = new GameObject("CombatBanner");
        banner.transform.SetParent(canvas.transform, false);
        RectTransform brt = banner.AddComponent<RectTransform>();
        brt.anchorMin = new Vector2(0f, 0.88f);
        brt.anchorMax = new Vector2(0.735f, 0.96f);
        brt.sizeDelta = Vector2.zero;
        brt.anchoredPosition = Vector2.zero;
        Image bImg = banner.AddComponent<Image>();
        bImg.color = bannerColor;
        Text bTxt = MakeUIText(banner.transform, "BannerText", 14,
            FontStyle.Bold, Color.white,
            new Vector2(0.02f, 0f), new Vector2(0.98f, 1f)).GetComponent<Text>();
        bTxt.text = outcome;
        bTxt.alignment = TextAnchor.MiddleCenter;
        activeCombatBanner = banner;
        Destroy(banner, 2.5f);
    }

    // ADDED: UPGRADE 2 - dice choice popup for attack count selection
    void BuildDiceChoicePopup(Transform ct)
    {
        diceChoicePopup = new GameObject("DiceChoicePopup", typeof(RectTransform));
        diceChoicePopup.transform.SetParent(ct, false);
        Image dcpBg = diceChoicePopup.AddComponent<Image>();
        dcpBg.color = new Color(0, 0, 0, 0.85f);
        dcpBg.raycastTarget = true;
        Anchor(diceChoicePopup.GetComponent<RectTransform>(), 0.3f, 0.30f, 0.7f, 0.55f);
        diceChoicePopup.SetActive(false);

        GameObject dcpBorder = new GameObject("Border", typeof(RectTransform));
        dcpBorder.transform.SetParent(diceChoicePopup.transform, false);
        dcpBorder.AddComponent<Image>().color = Gold.WithA(0.50f);
        FullStretch(dcpBorder.GetComponent<RectTransform>(), 2, 2);

        diceChoiceTitle = MakeUIText(diceChoicePopup.transform, "Title", 22, FontStyle.Bold,
            Gold, new Vector2(0, 0.68f), new Vector2(1, 0.90f)).GetComponent<Text>();
        diceChoiceTitle.text = "CHOOSE ATTACK DICE";

        MakeUIText(diceChoicePopup.transform, "Instruction", 12, FontStyle.Normal,
            Parchment, new Vector2(0, 0.50f), new Vector2(1, 0.65f)).GetComponent<Text>().text = "How many dice will you roll?";

        float btnW = 0.18f;
        float startX = 0.5f - btnW * 1.5f;
        dice1Btn = MakeButton(diceChoicePopup.transform, "Dice1Btn",
            new Vector2(-120, -30), new Vector2(70, 50), "1", Dark, Med);
        dice2Btn = MakeButton(diceChoicePopup.transform, "Dice2Btn",
            new Vector2(0, -30), new Vector2(70, 50), "2", Dark, Med);
        dice3Btn = MakeButton(diceChoicePopup.transform, "Dice3Btn",
            new Vector2(120, -30), new Vector2(70, 50), "3", Dark, Med);

        // Wire dice choice listeners
        if (dice1Btn != null) dice1Btn.onClick.AddListener(() => OnDiceChoiceSelected(1));
        if (dice2Btn != null) dice2Btn.onClick.AddListener(() => OnDiceChoiceSelected(2));
        if (dice3Btn != null) dice3Btn.onClick.AddListener(() => OnDiceChoiceSelected(3));

        // Troop slider
        GameObject sliderRow = new GameObject("TroopSliderRow", typeof(RectTransform));
        sliderRow.transform.SetParent(diceChoicePopup.transform, false);
        RectTransform sr = sliderRow.GetComponent<RectTransform>();
        sr.anchorMin = new Vector2(0.05f, 0.02f);
        sr.anchorMax = new Vector2(0.95f, 0.18f);
        sr.sizeDelta = Vector2.zero;
        sr.anchoredPosition = Vector2.zero;

        troopSliderLabel = MakeUIText(sliderRow.transform, "SliderLabel", 11, FontStyle.Normal,
            Parchment, new Vector2(0f, 0.5f), new Vector2(0.45f, 1f)).GetComponent<Text>();
        troopSliderLabel.text = "Troops: 1";

        GameObject sliderGO = new GameObject("Slider", typeof(RectTransform));
        sliderGO.transform.SetParent(sliderRow.transform, false);
        troopSlider = sliderGO.AddComponent<Slider>();
        troopSlider.minValue = 1;
        troopSlider.maxValue = 5;
        troopSlider.value = 1;
        troopSlider.wholeNumbers = true;
        RectTransform srt = sliderGO.GetComponent<RectTransform>();
        srt.anchorMin = new Vector2(0.50f, 0f);
        srt.anchorMax = new Vector2(1f, 1f);
        srt.sizeDelta = Vector2.zero;
        srt.anchoredPosition = Vector2.zero;

        Image sliderBg = sliderGO.AddComponent<Image>();
        sliderBg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        GameObject fillArea = new GameObject("FillArea", typeof(RectTransform));
        fillArea.transform.SetParent(sliderGO.transform, false);
        RectTransform fa = fillArea.GetComponent<RectTransform>();
        fa.anchorMin = Vector2.zero; fa.anchorMax = Vector2.one;
        fa.sizeDelta = new Vector2(-10, -6);
        fa.anchoredPosition = Vector2.zero;
        Image fillImg = fillArea.AddComponent<Image>();
        fillImg.color = new Color(0.6f, 0.2f, 0.1f, 0.9f);
        troopSlider.fillRect = fa;

        troopSlider.onValueChanged.AddListener((float val) => {
            troopSliderLabel.text = "Troops: " + Mathf.RoundToInt(val);
        });
    }

    void BuildTargetPopup(Transform ct)
    {
        targetPopup = new GameObject("TargetPopup", typeof(RectTransform));
        targetPopup.transform.SetParent(ct, false);
        Image tpBg = targetPopup.AddComponent<Image>();
        tpBg.color = new Color(0, 0, 0, 0.85f);
        tpBg.raycastTarget = false;
        Anchor(targetPopup.GetComponent<RectTransform>(), 0.3f, 0.28f, 0.7f, 0.62f);
        targetPopup.SetActive(false);

        GameObject tpBorder = new GameObject("Border", typeof(RectTransform));
        tpBorder.transform.SetParent(targetPopup.transform, false);
        tpBorder.AddComponent<Image>().color = Gold.WithA(0.50f);
        FullStretch(tpBorder.GetComponent<RectTransform>(), 2, 2);

        MakeUIText(targetPopup.transform, "Title", 28, FontStyle.Bold,
                   Gold, new Vector2(0, 0.68f), new Vector2(1, 0.92f))
        .GetComponent<Text>().text = "SELECT TARGET";

        targetPopupCardName = MakeUIText(targetPopup.transform, "CardName", 18, FontStyle.Normal,
                                         Parchment, new Vector2(0, 0.43f), new Vector2(1, 0.63f)).GetComponent<Text>();

        MakeUIText(targetPopup.transform, "Instruction", 13, FontStyle.Italic,
                   Parchment.WithA(0.55f), new Vector2(0, 0.25f), new Vector2(1, 0.38f))
        .GetComponent<Text>().text = "Click an enemy faction to target them";

        GameObject ctGO = new GameObject("CancelBtn", typeof(RectTransform));
        ctGO.transform.SetParent(targetPopup.transform, false);
        Image ctImg = ctGO.AddComponent<Image>();
        ctImg.color = new Color(0.30f, 0.12f, 0.06f);
        cancelTargetBtn = ctGO.AddComponent<Button>();
        cancelTargetBtn.targetGraphic = ctImg;
        Navigation cnav = new Navigation();
        cnav.mode = Navigation.Mode.None;
        cancelTargetBtn.navigation = cnav;
        Anchor(ctGO.GetComponent<RectTransform>(), 0.3f, 0.03f, 0.7f, 0.16f);
        MakeUIText(ctGO.transform, "Label", 16, FontStyle.Bold,
                   Parchment, Vector2.zero, Vector2.one).GetComponent<Text>().text = "CANCEL";
    }

    void BuildTooltip(Transform ct)
    {
        tooltipPanel = new GameObject("CardTooltip", typeof(RectTransform));
        tooltipPanel.transform.SetParent(ct, false);
        RectTransform ttR = tooltipPanel.GetComponent<RectTransform>();
        ttR.pivot = new Vector2(0.5f, 0f);
        ttR.anchorMin = ttR.anchorMax = Vector2.zero;
        ttR.sizeDelta = new Vector2(200, 150);
        Image ttBg = tooltipPanel.AddComponent<Image>();
        ttBg.color = Dark;
        ttBg.raycastTarget = false;
        tooltipPanel.SetActive(false);

        MakeHRule(tooltipPanel.transform, new Vector2(0.05f, 0.72f), new Vector2(0.95f, 0.74f), Gold.WithA(0.50f))
        .GetComponent<Image>().raycastTarget = false;

        tooltipNameText = MakeUIText(tooltipPanel.transform, "Name", 16, FontStyle.Bold,
                                     Parchment, new Vector2(0.05f, 0.78f), new Vector2(0.95f, 0.98f)).GetComponent<Text>();
        tooltipTypeText = MakeUIText(tooltipPanel.transform, "Type", 11, FontStyle.Bold,
                                     Parchment, new Vector2(0.05f, 0.64f), new Vector2(0.95f, 0.72f)).GetComponent<Text>();
        tooltipPowerText = MakeUIText(tooltipPanel.transform, "Power", 14, FontStyle.Bold,
                                      Gold, new Vector2(0.05f, 0.53f), new Vector2(0.95f, 0.62f)).GetComponent<Text>();
        tooltipDescriptionText = MakeUIText(tooltipPanel.transform, "Description", 12, FontStyle.Normal,
                                            Parchment.WithA(0.65f), new Vector2(0.05f, 0.02f), new Vector2(0.95f, 0.50f)).GetComponent<Text>();
        tooltipDescriptionText.horizontalOverflow = HorizontalWrapMode.Wrap;
    }

    void BuildGameOverPanel(Transform ct)
    {
        gameOverPanel = new GameObject("GameOverPanel", typeof(RectTransform));
        gameOverPanel.transform.SetParent(ct, false);
        gameOverCanvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
        gameOverCanvasGroup.alpha = 0f;
        gameOverCanvasGroup.blocksRaycasts = false;
        gameOverPanel.AddComponent<Image>();
        FullStretch(gameOverPanel.GetComponent<RectTransform>());
        gameOverPanel.SetActive(false);

        gameOverTitleText = MakeUIText(gameOverPanel.transform, "Title", 64, FontStyle.Bold,
                                       Parchment, new Vector2(0, 0.55f), new Vector2(1, 0.78f)).GetComponent<Text>();

        GameObject goDiv = new GameObject("Divider", typeof(RectTransform));
        goDiv.transform.SetParent(gameOverPanel.transform, false);
        gameOverDivider = goDiv.AddComponent<Image>();
        gameOverDivider.color = Gold;
        RectTransform gdR = goDiv.GetComponent<RectTransform>();
        gdR.anchorMin = gdR.anchorMax = new Vector2(0.5f, 0.5f);
        gdR.sizeDelta = new Vector2(300, 2);
        gdR.anchoredPosition = new Vector2(0, 40);

        gameOverSubtitleText = MakeUIText(gameOverPanel.transform, "Subtitle", 28, FontStyle.Normal,
                                          Parchment.WithA(0.65f), new Vector2(0, 0.38f), new Vector2(1, 0.50f)).GetComponent<Text>();

        playAgainButton = MakeButton(gameOverPanel.transform, "PlayAgainBtn",
                                     new Vector2(0, -60), new Vector2(200, 60), "PLAY AGAIN", Dark, Med);
        quitButton = MakeButton(gameOverPanel.transform, "QuitBtn",
                                new Vector2(0, -140), new Vector2(200, 60), "QUIT", Dark, Med);
    }

    void BuildEventPopup(Transform ct)
    {
        eventOverlay = new GameObject("EventOverlay");
        eventOverlay.transform.SetParent(ct, false);
        RectTransform ovR = eventOverlay.AddComponent<RectTransform>();
        ovR.anchorMin = Vector2.zero; ovR.anchorMax = Vector2.one;
        ovR.sizeDelta = Vector2.zero;
        Image ovImg = eventOverlay.AddComponent<Image>();
        ovImg.color = new Color(0f, 0f, 0f, 0.65f);
        ovImg.raycastTarget = true;
        eventOverlay.SetActive(false);

        eventPopup = new GameObject("EventPopup", typeof(RectTransform));
        eventPopup.transform.SetParent(ct, false);
        Image epBg = eventPopup.AddComponent<Image>();
        epBg.color = new Color(0.04f, 0.02f, 0.01f, 0.94f);
        epBg.raycastTarget = true;
        FullStretch(eventPopup.GetComponent<RectTransform>());
        eventPopup.SetActive(false);

        GameObject epBorder = new GameObject("Border", typeof(RectTransform));
        epBorder.transform.SetParent(eventPopup.transform, false);
        epBorder.AddComponent<Image>().color = Gold.WithA(0.40f);
        RectTransform ebR = epBorder.GetComponent<RectTransform>();
        ebR.anchorMin = new Vector2(0.2f, 0.25f);
        ebR.anchorMax = new Vector2(0.8f, 0.80f);
        ebR.sizeDelta = Vector2.zero;

        GameObject epInner = new GameObject("Inner", typeof(RectTransform));
        epInner.transform.SetParent(eventPopup.transform, false);
        epInner.AddComponent<Image>().color = Darkest;
        RectTransform eiR = epInner.GetComponent<RectTransform>();
        eiR.anchorMin = new Vector2(0.21f, 0.26f);
        eiR.anchorMax = new Vector2(0.79f, 0.79f);
        eiR.sizeDelta = Vector2.zero;

        eventPopupTitleText = MakeUIText(eventPopup.transform, "PopupTitle", 13, FontStyle.Bold,
                                         Gold, new Vector2(0.22f, 0.72f), new Vector2(0.78f, 0.78f)).GetComponent<Text>();
        eventPopupTitleText.text = "HISTORICAL EVENT";

        eventPopupNameText = MakeUIText(eventPopup.transform, "PopupName", 32, FontStyle.Bold,
                                        Parchment, new Vector2(0.22f, 0.58f), new Vector2(0.78f, 0.70f)).GetComponent<Text>();

        eventPopupDescText = MakeUIText(eventPopup.transform, "PopupDesc", 24, FontStyle.Normal,
                                        Parchment.WithA(0.70f), new Vector2(0.25f, 0.40f), new Vector2(0.75f, 0.55f)).GetComponent<Text>();
        eventPopupDescText.horizontalOverflow = HorizontalWrapMode.Wrap;

        GameObject dimGO = new GameObject("DismissBtn", typeof(RectTransform));
        dimGO.transform.SetParent(eventPopup.transform, false);
        Image dimImg = dimGO.AddComponent<Image>();
        dimImg.color = Med;
        eventDismissBtn = dimGO.AddComponent<Button>();
        eventDismissBtn.targetGraphic = dimImg;
        Navigation dNav = new Navigation();
        dNav.mode = Navigation.Mode.None;
        eventDismissBtn.navigation = dNav;
        RectTransform dimR = dimGO.GetComponent<RectTransform>();
        dimR.anchorMin = new Vector2(0.35f, 0.28f);
        dimR.anchorMax = new Vector2(0.65f, 0.35f);
        dimR.sizeDelta = Vector2.zero;
        MakeUIText(dimGO.transform, "Label", 18, FontStyle.Bold,
                   Parchment, Vector2.zero, Vector2.one).GetComponent<Text>().text = "DISMISS";

        GameObject choiceAGO = new GameObject("ChoiceABtn", typeof(RectTransform));
        choiceAGO.transform.SetParent(eventPopup.transform, false);
        Image choiceAImg = choiceAGO.AddComponent<Image>();
        choiceAImg.color = Med;
        eventChoiceABtn = choiceAGO.AddComponent<Button>();
        eventChoiceABtn.targetGraphic = choiceAImg;
        RectTransform choiceAR = choiceAGO.GetComponent<RectTransform>();
        choiceAR.anchorMin = new Vector2(0.22f, 0.28f);
        choiceAR.anchorMax = new Vector2(0.48f, 0.35f);
        choiceAR.sizeDelta = Vector2.zero;
        MakeUIText(choiceAGO.transform, "Label", 15, FontStyle.Bold,
                   Parchment, Vector2.zero, Vector2.one);
        choiceAGO.SetActive(false);

        GameObject choiceBGO = new GameObject("ChoiceBBtn", typeof(RectTransform));
        choiceBGO.transform.SetParent(eventPopup.transform, false);
        Image choiceBImg = choiceBGO.AddComponent<Image>();
        choiceBImg.color = Med;
        eventChoiceBBtn = choiceBGO.AddComponent<Button>();
        eventChoiceBBtn.targetGraphic = choiceBImg;
        RectTransform choiceBR = choiceBGO.GetComponent<RectTransform>();
        choiceBR.anchorMin = new Vector2(0.52f, 0.28f);
        choiceBR.anchorMax = new Vector2(0.78f, 0.35f);
        choiceBR.sizeDelta = Vector2.zero;
        MakeUIText(choiceBGO.transform, "Label", 15, FontStyle.Bold,
                   Parchment, Vector2.zero, Vector2.one);
        choiceBGO.SetActive(false);
    }

    void SubscribeToEvents()
    {
        if (eventsSubscribed) return;

        endTurnButton.onClick.AddListener(() => GameManager.Instance?.EndPlayerTurn());
        playAgainButton.onClick.AddListener(() => StartCoroutine(FadeOutAndRestart()));
        quitButton.onClick.AddListener(() => Application.Quit());
        cancelTargetBtn.onClick.AddListener(ExitTargetMode);
        combatPopupDismissBtn.onClick.AddListener(() => combatPopup.SetActive(false));
        if (eventChoiceABtn != null) eventChoiceABtn.onClick.AddListener(() => ResolveEventChoice(true));
        if (eventChoiceBBtn != null) eventChoiceBBtn.onClick.AddListener(() => ResolveEventChoice(false));
        // ADDED: UPGRADE 2 - dice choice buttons
        dice1Btn.onClick.AddListener(() => OnDiceChoiceSelected(1));
        dice2Btn.onClick.AddListener(() => OnDiceChoiceSelected(2));
        dice3Btn.onClick.AddListener(() => OnDiceChoiceSelected(3));

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLogMessage += AddLogMessage;
            GameManager.Instance.OnGameOver += ShowGameOver;
            GameManager.Instance.OnFactionEliminated += OnFactionEliminated;
            GameManager.Instance.OnTurnChanged += UpdateTurnCounter;
            GameManager.Instance.OnStateChanged += OnGameStateChanged;
            GameManager.Instance.OnYearChanged += UpdateYearDisplay;
            GameManager.Instance.OnSubTurnChanged += UpdateSubTurnDisplay;
            GameManager.Instance.OnHistoricalEventTriggered += ShowEventPopup;
            GameManager.Instance.OnReinforcementPhase += OnReinforcementPhaseChanged;
            GameManager.Instance.OnCombatResolved += OnCombatResolved;
            GameManager.Instance.OnPhaseTimerUpdate += OnPhaseTimerUpdate;
            GameManager.Instance.OnCardPlayed += OnAnyCardPlayed;
            GameManager.Instance.OnTurnChanged += OnTurnChangedUpdateRegionBonus;
            GameManager.Instance.OnMarketRefresh += OnMarketRefreshed;
            GameManager.Instance.OnGoldChanged += OnGoldChanged;
        }
        MapLayerController.OnMapBuilt += OnMapLayerBuilt;

        eventDismissBtn.onClick.AddListener(DismissEventPopup);
        eventsSubscribed = true;
    }

    void UnsubscribeFromEvents()
    {
        if (!eventsSubscribed) return;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLogMessage -= AddLogMessage;
            GameManager.Instance.OnGameOver -= ShowGameOver;
            GameManager.Instance.OnFactionEliminated -= OnFactionEliminated;
            GameManager.Instance.OnTurnChanged -= UpdateTurnCounter;
            GameManager.Instance.OnStateChanged -= OnGameStateChanged;
            GameManager.Instance.OnYearChanged -= UpdateYearDisplay;
            GameManager.Instance.OnSubTurnChanged -= UpdateSubTurnDisplay;
            GameManager.Instance.OnHistoricalEventTriggered -= ShowEventPopup;
            GameManager.Instance.OnReinforcementPhase -= OnReinforcementPhaseChanged;
            GameManager.Instance.OnCombatResolved -= OnCombatResolved;
            GameManager.Instance.OnPhaseTimerUpdate -= OnPhaseTimerUpdate;
            GameManager.Instance.OnCardPlayed -= OnAnyCardPlayed;
            GameManager.Instance.OnTurnChanged -= OnTurnChangedUpdateRegionBonus;
            GameManager.Instance.OnMarketRefresh -= OnMarketRefreshed;
            GameManager.Instance.OnGoldChanged -= OnGoldChanged;
        }
        MapLayerController.OnMapBuilt -= OnMapLayerBuilt;

        eventsSubscribed = false;
    }

    void OnMapLayerBuilt()
    {
        if (canvas == null) return;

        if (mapAreaBg != null) mapAreaBg.transform.SetSiblingIndex(0);
        MapLayerController.Instance?.PlaceBehindGameplayMap();

        Transform ct = canvas.transform;
        ZoomController zoomCtrl = canvas.GetComponentInChildren<ZoomController>();
        if (zoomCtrl != null && MapLayerController.Instance?.MapImage != null)
            zoomCtrl.SetTarget(MapLayerController.Instance.MapImage.rectTransform);

        Transform clickZones = mapClickZonesRoot != null ? mapClickZonesRoot.transform : ct.Find("MapClickZones");
        if (clickZones != null && MapLayerController.Instance?.MapImage != null)
        {
            clickZones.SetParent(MapLayerController.Instance.MapImage.transform, false);
            RectTransform czR = clickZones.GetComponent<RectTransform>();
            czR.anchorMin = Vector2.zero;
            czR.anchorMax = Vector2.one;
            czR.sizeDelta = Vector2.zero;
            czR.anchoredPosition = Vector2.zero;
        }

        Transform handAreaT = ct.Find("HandArea");
        if (handAreaT != null) handAreaT.SetAsLastSibling();
        if (probabilityPanel != null) probabilityPanel.transform.SetAsLastSibling();
        if (damageFlashOverlay != null) damageFlashOverlay.transform.SetAsLastSibling();
        EnforceDrawOrder();
    }

    void OnTurnChangedUpdateRegionBonus(int turn)
    {
        UpdateRegionBonusDisplay();
    }

    void OnReinforcementPhaseChanged(int remaining)
    {
        if (remaining > 0)
            ShowReinforcementPanel(remaining);
        else
            UpdateReinforcementPanel(0);
        UpdateTerritoryMap();
        UpdateMapClickZones();
    }

    void OnCombatResolved(CombatResult result, TerritoryData source, TerritoryData target)
    {
        ShowCombatPopup(result, source, target);
        ShowCombatResultBanner(result, source, target);
        if (result.territoryCaptured) StartCoroutine(ShakeCamera());
        UpdateTerritoryMap();
    }

    public void InitializeUI(List<FactionData> factions)
    {
        ExitTargetMode();
        if (eventPopup != null) eventPopup.SetActive(false);
        if (combatPopup != null) combatPopup.SetActive(false);
        if (reinforcementPanel != null) reinforcementPanel.SetActive(false);
        if (fortifyPanel != null) fortifyPanel.SetActive(false);
        if (GameManager.Instance != null) GameManager.Instance.IsWaitingForEventDismiss = false;
        if (gameOverCanvasGroup != null)
        {
            gameOverCanvasGroup.alpha = 0f;
            gameOverCanvasGroup.blocksRaycasts = false;
            gameOverPanel.SetActive(false);
        }
        eliminatedFactions.Clear();
        HideTooltip();
        UpdateCardsRemaining();
        if (turnCounterText != null) turnCounterText.text = "Turn 1 | 1790 AD";
        if (subTurnText != null) subTurnText.text = "";

        Transform factionArea = canvas.transform.Find("FactionArea");
        if (factionArea == null) return;
        foreach (Transform child in factionArea) Destroy(child.gameObject);
        factionPanels.Clear();
        foreach (FactionData faction in factions) CreateFactionPanel(faction, factionArea);

        UpdateTerritoryMap();
        if (EconomyUI.Instance != null) EconomyUI.Instance.Refresh();
    }

    void UpdateSubTurnDisplay(int subTurn)
    {
        if (subTurnText == null) return;
        int maxSt = GameManager.MAX_SUB_TURNS;
        int pct = Mathf.RoundToInt((float)subTurn / maxSt * 100f);
        subTurnText.text = $"{subTurn}/{maxSt} days ({pct}%)";
        subTurnText.color = pct < 50
        ? Parchment.WithA(0.45f)
        : pct < 85
        ? Gold.WithA(0.65f)
        : new Color(0.80f, 0.30f, 0.10f);
    }

    void ShowEventPopup(string eventName, string description)
    {
        if (eventPopup == null) return;
        eventPopupNameText.text = eventName;
        eventPopupDescText.text = description;
        HistoricalEvent activeEvent = HistoricalEventManager.ActiveEvent;
        bool choiceEvent = activeEvent != null && activeEvent.requiresPlayerChoice;
        if (eventDismissBtn != null) eventDismissBtn.gameObject.SetActive(!choiceEvent);
        if (eventChoiceABtn != null)
        {
            eventChoiceABtn.gameObject.SetActive(choiceEvent);
            Text label = eventChoiceABtn.transform.Find("Label")?.GetComponent<Text>();
            if (label != null) label.text = activeEvent != null ? activeEvent.choiceALabel : "CHOICE A";
        }
        if (eventChoiceBBtn != null)
        {
            eventChoiceBBtn.gameObject.SetActive(choiceEvent);
            Text label = eventChoiceBBtn.transform.Find("Label")?.GetComponent<Text>();
            if (label != null) label.text = activeEvent != null ? activeEvent.choiceBLabel : "CHOICE B";
        }
        if (eventOverlay != null) eventOverlay.SetActive(true);
        eventPopup.SetActive(true);
        EnableCardInteractions(false);
    }

    void ResolveEventChoice(bool isChoiceA)
    {
        if (GameManager.Instance != null)
            HistoricalEventManager.ResolveChoice(GameManager.Instance, isChoiceA);
        DismissEventPopup();
    }

    void DismissEventPopup()
    {
        if (eventPopup == null) return;
        if (eventOverlay != null) eventOverlay.SetActive(false);
        eventPopup.SetActive(false);
        if (GameManager.Instance != null)
            GameManager.Instance.IsWaitingForEventDismiss = false;
    }

    void CreateFactionPanel(FactionData faction, Transform parent)
    {
        GameObject go = Instantiate(factionPanelPrefab, parent);
        go.SetActive(true);
        go.name = faction.factionName + "Panel";
        FactionPanelUI pui = go.GetComponent<FactionPanelUI>();
        pui.SetFaction(faction);
        Color factionStatColor = SaturatedFactionColor(faction.factionColor);
        if (pui.accentStripe != null) pui.accentStripe.color = faction.factionColor.WithA(0.9f);
        if (pui.powerBar != null) pui.powerBar.color = factionStatColor;
        if (pui.influenceBar != null) pui.influenceBar.color = factionStatColor;
        if (pui.secretsBar != null) pui.secretsBar.color = factionStatColor;
        factionPanels.Add(pui);
        factionPanelRoots[faction.factionId] = go;
        FactionData cap = faction;
        go.GetComponent<Button>().onClick.AddListener(() => OnFactionPanelClicked(cap));
    }

    Color SaturatedFactionColor(Color source)
    {
        Color.RGBToHSV(source, out float h, out float s, out float v);
        Color c = Color.HSVToRGB(h, 0.85f, Mathf.Max(v, 0.45f));
        c.a = source.a;
        return c;
    }

    public void UpdatePlayerHand(List<CardData> hand)
    {
        HideTooltip();
        foreach (Transform t in handArea.transform) Destroy(t.gameObject);
        foreach (Transform t in playedArea.transform) Destroy(t.gameObject);
        handCards.Clear();
        UpdateCardsRemaining();

        List<CardData> sorted = new List<CardData>(hand);
        sorted.Sort((a, b) => SortOrder(a.cardType).CompareTo(SortOrder(b.cardType)));
        foreach (CardData card in sorted) CreateHandCard(card);
        ApplyHandFanLayout();
    }

    void RefreshHandUI()
    {
        FactionData debugPlayer = GameManager.Instance?.GetPlayerFaction();
        Debug.Log($"[GameUIManager] RefreshHandUI called. State={GameManager.Instance?.currentState}, HandCount={debugPlayer?.hand?.Count ?? -1}, HandArea active={handArea?.activeInHierarchy}");
        if (debugPlayer != null)
            UpdatePlayerHand(debugPlayer.hand);
    }

    void ApplyHandFanLayout()
    {
        int count = handCards.Count;
        float fanAngle = Mathf.Min(4f, 20f / Mathf.Max(count, 1));
        float startAngle = -(count - 1) * fanAngle / 2f;

        for (int i = 0; i < handCards.Count; i++)
        {
            float angle = startAngle + i * fanAngle;
            float verticalOffset = -Mathf.Abs(angle) * 0.8f;

            RectTransform rt = handCards[i].GetComponent<RectTransform>();
            rt.localRotation = Quaternion.Euler(0, 0, angle);
            Vector2 basePos = handCards[i].originalPosition != Vector2.zero
                ? handCards[i].originalPosition
                : rt.anchoredPosition;
            rt.anchoredPosition = new Vector2(basePos.x, basePos.y + verticalOffset);

            handCards[i].originalAngle = angle;
            if (handCards[i].originalPosition == Vector2.zero)
                handCards[i].originalPosition = basePos;
        }
    }

    int SortOrder(CardType t) => t switch
    {
        CardType.ATTACK => 0,
        CardType.SABOTAGE => 1,
        CardType.DEFENSE => 2,
        CardType.INFLUENCE => 3,
        CardType.FARMING => 4,
        CardType.BLACK_MARKET => 5,
        _ => 6
    };

    void CreateHandCard(CardData card)
    {
        GameObject go = Instantiate(cardPrefab, handArea.transform);
        go.SetActive(true);
        go.name = card.cardName;
        CardUIController cc = go.GetComponent<CardUIController>();
        cc.BuildCard();
        Color fc = Color.gray;
        if (GameManager.Instance != null && card.factionId >= 0 && card.factionId < GameManager.Instance.factions.Count)
            fc = GameManager.Instance.factions[card.factionId].factionColor;
        cc.SetCard(card, fc);
        cc.SetInteractable(GameManager.Instance.currentState == GameState.PLAY_PHASE);
        CardData cap = card;
        cc.onCardClicked += () => OnHandCardClicked(cap);
        handCards.Add(cc);
    }

    void OnHandCardClicked(CardData card)
    {
        if (GameManager.Instance.currentState != GameState.PLAY_PHASE) return;
        if (isSelectingTarget) return;
        if (!GameManager.Instance.CanPlayMoreCards()) { AddLogMessage($"Maximum {GameManager.Instance.MaxCardsPerTurn} cards per turn."); StartCoroutine(ShakeCard(FindCardGO(card))); return; }

        switch (card.cardType)
        {
            case CardType.ATTACK:
                ShowAttackModeChoice(card);
                break;
            case CardType.SABOTAGE:
            case CardType.BLACK_MARKET:
                EnterTargetMode(card);
                break;
            default:
                StartCoroutine(AnimateCardPlay(card, GetCardWorldPosition(card)));
                GameManager.Instance.PlayCard(card, GameManager.Instance.GetPlayerFaction());
                break;
        }
    }

    GameObject FindCardGO(CardData card)
    {
        foreach (CardUIController cc in handCards)
            if (cc.CurrentCard == card) return cc.gameObject;
        return null;
    }

    IEnumerator ShakeCamera(float duration = 0.25f, float magnitude = 0.008f)
    {
        Camera cam = Camera.main;
        if (cam == null) yield break;
        Vector3 origin = cam.transform.localPosition;
        float t = 0f;
        while (t < duration)
        {
            float decay = 1f - (t / duration);
            cam.transform.localPosition = origin + new Vector3(
                UnityEngine.Random.Range(-1f, 1f),
                UnityEngine.Random.Range(-1f, 1f),
                0f) * magnitude * decay;
            t += Time.deltaTime;
            yield return null;
        }
        cam.transform.localPosition = origin;
    }

    IEnumerator PulseBadge(GameObject badge)
    {
        float t = 0f;
        while (t < 0.25f)
        {
            float s = 1f + 0.4f * Mathf.Sin(t / 0.25f * Mathf.PI);
            badge.transform.localScale = Vector3.one * s;
            t += Time.deltaTime;
            yield return null;
        }
        badge.transform.localScale = Vector3.one;
    }

    IEnumerator ShakeCard(GameObject cardGO)
    {
        if (cardGO == null) yield break;
        RectTransform rt = cardGO.GetComponent<RectTransform>();
        Vector2 origin = rt.anchoredPosition;
        float[] offsets = { -8f, 8f, -6f, 6f, -4f, 4f, 0f };
        foreach (float off in offsets)
        {
            rt.anchoredPosition = new Vector2(origin.x + off, origin.y);
            yield return new WaitForSeconds(0.04f);
        }
        rt.anchoredPosition = origin;
    }

    void EnterTerritoryAttackSourceMode(CardData card)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsWaitingForAttackSelection)
        {
            AddLogMessage("Use the attack phase map instead of a card during ATTACK PHASE.");
            return;
        }
        pendingCard = card;
        pendingCardPosition = GetCardWorldPosition(card);
        isSelectingTarget = true;
        waitingForTerritoryAttackSource = true;
        waitingForTerritoryAttackTarget = false;
        pendingAttackSourceTerritory = null;

        if (TerritoryGraphRenderer.Instance != null)
            TerritoryGraphRenderer.Instance.ClearHighlight();

        FactionData player = GameManager.Instance.GetPlayerFaction();
        ShowAttackStatus("⚔ TAP a GREEN territory to attack from (needs 2+ troops)");
        AddLogMessage("⚔ ATTACK: Click one of your GREEN territories (need 2+ troops) to begin.");
        if (probabilityPanel != null) probabilityPanel.SetActive(false);
        UpdateMapClickZones();
        HighlightValidAttackSources(player);
        ShowPhase("\u2694  ATTACK PHASE", new Color(0.8f, 0.2f, 0.1f));
    }

    void HighlightValidAttackSources(FactionData player)
    {
        if (player == null || GameManager.Instance == null) return;
        foreach (var kvp in territoryMapEntries)
        {
            TerritoryData tData = GameManager.Instance.territories.Find(t => t.name == kvp.Key);
            if (tData == null || tData.controlledBy != player.factionId) continue;
            Image bg = kvp.Value.GetComponent<Image>();
            if (bg == null) continue;
            if (tData.troops > 1)
                bg.color = new Color(0.1f, 0.7f, 0.1f, 0.9f);
            else
                bg.color = new Color(0.4f, 0.4f, 0.1f, 0.6f);
        }

        foreach (var kvp in mapClickButtons)
        {
            TerritoryData tData = GameManager.Instance.territories.Find(t => t.name == kvp.Key);
            if (tData == null) continue;
            Transform badge = kvp.Value.transform.Find("TroopBadge");
            if (badge == null) continue;
            Image badgeBg = badge.GetComponent<Image>();
            if (badgeBg == null) continue;
            bool valid = player != null && tData.controlledBy == player.factionId && tData.troops > 1;
            badgeBg.color = valid
                ? new Color(0.15f, 0.72f, 0.15f, 1.0f)
                : new Color(0.22f, 0.22f, 0.22f, 0.55f);
        }
    }

    void ClearTerritoryHighlights()
    {
        foreach (var kvp in territoryMapEntries)
        {
            Image bg = kvp.Value.GetComponent<Image>();
            if (bg != null) bg.color = Dark;
        }
        UpdateMapClickZones();
        foreach (var kvp in mapClickZoneButtons)
        {
            Image img = kvp.Value.GetComponent<Image>();
            if (img != null) img.color = Color.clear;
        }

        foreach (var kvp in mapClickButtons)
        {
            TerritoryData tData = GameManager.Instance?.territories?.Find(t => t.name == kvp.Key);
            if (tData == null) continue;
            Transform badge = kvp.Value.transform.Find("TroopBadge");
            if (badge == null) continue;
            Image badgeBg = badge.GetComponent<Image>();
            if (badgeBg == null) continue;
            Color ownerColor = tData.controlledBy >= 0 && tData.controlledBy < GameManager.Instance.factions.Count
                ? GameManager.Instance.factions[tData.controlledBy].factionColor : Color.gray;
            badgeBg.color = tData.controlledBy >= 0
                ? new Color(ownerColor.r * 0.6f, ownerColor.g * 0.6f, ownerColor.b * 0.6f, 0.9f)
                : new Color(0.2f, 0.2f, 0.2f, 0.7f);
        }
    }

    void ShowAttackArrow(TerritoryData source, TerritoryData target)
    {
        HideAttackArrow();
        if (territoryMapPositions == null || !territoryMapPositions.ContainsKey(source.name) || !territoryMapPositions.ContainsKey(target.name))
            return;

        attackArrow = DrawMapArrow("AttackArrow", source.name, target.name, Gold, 6f);
    }

    void HideAttackArrow()
    {
        if (attackArrow != null)
        {
            Destroy(attackArrow);
            attackArrow = null;
        }
    }

    void ShowAttackOptionArrows(TerritoryData source, List<TerritoryData> targets)
    {
        HidePhaseArrows();
        if (source == null || targets == null) return;

        foreach (TerritoryData target in targets)
        {
            if (target == null) continue;
            GameObject arrow = DrawMapArrow("AttackOptionArrow", source.name, target.name,
                new Color(1.0f, 0.25f, 0.1f, 0.85f), 4f);
            if (arrow != null) phaseArrows.Add(arrow);
        }
    }

    void ShowFortifyPathArrows(TerritoryData source, int factionId)
    {
        HidePhaseArrows();
        if (source == null || GameManager.Instance == null) return;

        foreach (TerritoryData target in GameManager.Instance.territories)
        {
            if (target == null || !IsValidFortifyDestination(source, target, factionId)) continue;
            GameObject arrow = DrawMapArrow("FortifyPathArrow", source.name, target.name,
                new Color(0.25f, 0.9f, 0.35f, 0.75f), 3f);
            if (arrow != null) phaseArrows.Add(arrow);
        }
    }

    GameObject DrawMapArrow(string name, string sourceName, string targetName, Color color, float thickness)
    {
        if (territoryMapPositions == null ||
            !territoryMapPositions.ContainsKey(sourceName) ||
            !territoryMapPositions.ContainsKey(targetName))
            return null;

        Vector2 srcPos = territoryMapPositions[sourceName];
        Vector2 tgtPos = territoryMapPositions[targetName];
        RectTransform arrowParent = mapClickZonesRoot != null
            ? mapClickZonesRoot.GetComponent<RectTransform>()
            : canvas.GetComponent<RectTransform>();
        Vector2 parentSize = arrowParent != null ? arrowParent.rect.size : new Vector2(1920f, 1080f);
        if (parentSize.x <= 1f || parentSize.y <= 1f)
            parentSize = new Vector2(1920f, 1080f);
        Vector2 pixelDir = new Vector2((tgtPos.x - srcPos.x) * parentSize.x, (tgtPos.y - srcPos.y) * parentSize.y);
        float dist = pixelDir.magnitude;
        if (dist <= 1f) return null;

        GameObject arrow = new GameObject(name, typeof(RectTransform));
        arrow.transform.SetParent(mapClickZonesRoot != null ? mapClickZonesRoot.transform : canvas.transform, false);
        Image arrowImg = arrow.AddComponent<Image>();
        arrowImg.color = color;
        arrowImg.raycastTarget = false;

        RectTransform ar = arrow.GetComponent<RectTransform>();
        ar.anchorMin = ar.anchorMax = srcPos;
        ar.pivot = new Vector2(0f, 0.5f);
        ar.sizeDelta = new Vector2(dist, thickness);
        ar.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(pixelDir.y, pixelDir.x) * Mathf.Rad2Deg);
        ar.anchoredPosition = Vector2.zero;

        AddArrowWing(arrow.transform, "HeadA", color, thickness, 32f, 150f);
        AddArrowWing(arrow.transform, "HeadB", color, thickness, 32f, -150f);
        return arrow;
    }

    void AddArrowWing(Transform parent, string name, Color color, float thickness, float length, float angle)
    {
        GameObject wing = new GameObject(name, typeof(RectTransform));
        wing.transform.SetParent(parent, false);
        Image img = wing.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        RectTransform wr = wing.GetComponent<RectTransform>();
        wr.anchorMin = wr.anchorMax = new Vector2(1f, 0.5f);
        wr.pivot = new Vector2(1f, 0.5f);
        wr.sizeDelta = new Vector2(length, Mathf.Max(2f, thickness));
        wr.anchoredPosition = Vector2.zero;
        wr.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    void HidePhaseArrows()
    {
        foreach (GameObject arrow in phaseArrows)
            if (arrow != null) Destroy(arrow);
        phaseArrows.Clear();
    }

    bool IsValidFortifyDestination(TerritoryData source, TerritoryData destination, int factionId)
    {
        if (source == null || destination == null || source == destination) return false;
        if (source.controlledBy != factionId || destination.controlledBy != factionId) return false;
        if (GameManager.Instance == null || GameManager.Instance.territories == null) return false;

        Queue<TerritoryData> open = new Queue<TerritoryData>();
        HashSet<TerritoryData> visited = new HashSet<TerritoryData>();
        open.Enqueue(source);
        visited.Add(source);

        while (open.Count > 0)
        {
            TerritoryData current = open.Dequeue();
            foreach (string adjName in ProvinceGraph.GetAttackNeighbours(current.name))
            {
                TerritoryData next = GameManager.Instance.territories.Find(t => t.name == adjName);
                if (next == null || next.controlledBy != factionId || visited.Contains(next)) continue;
                if (next == destination) return true;
                visited.Add(next);
                open.Enqueue(next);
            }
        }

        return false;
    }

    void EnterTerritoryAttackTargetMode(CardData card, TerritoryData source)
    {
        pendingAttackSourceTerritory = source;
        waitingForTerritoryAttackSource = false;
        waitingForTerritoryAttackTarget = true;

        ClearTerritoryHighlights();

        if (TerritoryGraphRenderer.Instance != null)
            TerritoryGraphRenderer.Instance.HighlightAttackOptions(source);

        List<TerritoryData> targets = GameManager.Instance.GetAdjacentEnemyTerritories(source);
        ShowAttackOptionArrows(source, targets);
        foreach (var kvp in territoryMapEntries)
        {
            TerritoryData tData = GameManager.Instance.territories.Find(t => t.name == kvp.Key);
            if (targets.Contains(tData))
            {
                Image bg = kvp.Value.GetComponent<Image>();
                if (bg != null) bg.color = new Color(0.50f, 0.15f, 0.10f);
            }
        }

        UpdateMapClickZones();
        foreach (var kvp in mapClickButtons)
        {
            TerritoryData tData = GameManager.Instance.territories.Find(t => t.name == kvp.Key);
            if (tData == null) continue;
            Transform badge = kvp.Value.transform.Find("TroopBadge");
            if (badge == null) continue;
            Image badgeBg = badge.GetComponent<Image>();
            if (badgeBg == null) continue;
            bool isTarget = targets.Contains(tData);
            bool isSource = tData.name == source.name;
            badgeBg.color = isTarget ? new Color(0.80f, 0.14f, 0.08f, 1.0f)
                : isSource ? new Color(0.10f, 0.50f, 0.80f, 1.0f)
                : new Color(0.22f, 0.22f, 0.22f, 0.55f);
        }

        ShowAttackStatus($"Select ATTACK TARGET territory from {source.name}");
        AddLogMessage($"\u2694 ATTACK: Click a RED enemy territory adjacent to {source.name} to attack.");
        if (probabilityPanel != null) probabilityPanel.SetActive(false);
        ShowPhase("\u2694  SELECT TARGET", new Color(0.8f, 0.2f, 0.1f));
    }

    void ExitTerritoryAttackMode()
    {
        if (pendingCard != null)
        {
            FactionData player = GameManager.Instance?.GetPlayerFaction();
            if (player != null && !player.hand.Contains(pendingCard))
            {
                player.hand.Add(pendingCard);
                UpdatePlayerHand(player.hand);
                AddLogMessage($"{pendingCard.cardName} returned to hand (attack cancelled).");
            }
            pendingCard = null;
        }
        isSelectingTarget = false;
        waitingForTerritoryAttackSource = false;
        waitingForTerritoryAttackTarget = false;
        waitingForNeutralExpandSource = false;
        waitingForNeutralExpandTarget = false;
        pendingExpandSource = null;
        pendingCard = null;
        pendingAttackSourceTerritory = null;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.pendingAttackerDiceCount = 0;
            GameManager.Instance.pendingAttackerTroopCount = 0;
        }
        if (diceChoicePopup != null) diceChoicePopup.SetActive(false);
        if (attackModePopup != null) attackModePopup.SetActive(false);
        HidePhaseArrows();
        ClearTerritoryHighlights();
        HideAttackArrow();
        HideAttackStatus();
        if (TerritoryGraphRenderer.Instance != null)
        {
            TerritoryGraphRenderer.Instance.ClearHighlight();
            TerritoryGraphRenderer.Instance.ClearPendingAttack();
        }
    }

    void ShowAttackModeChoice(CardData card)
    {
        if (attackModePopup == null) BuildAttackModePopup();
        pendingCard = card;
        pendingCardPosition = GetCardWorldPosition(card);
        attackModePopup.SetActive(true);
    }

    void BuildAttackModePopup()
    {
        Transform ct = canvas.transform;
        attackModePopup = new GameObject("AttackModePopup", typeof(RectTransform));
        attackModePopup.transform.SetParent(ct, false);
        Image bg = attackModePopup.AddComponent<Image>();
        bg.color = new Color(0.10f, 0.05f, 0.01f, 0.95f);
        Anchor(attackModePopup.GetComponent<RectTransform>(), 0.30f, 0.38f, 0.70f, 0.58f);

        MakeUIText(attackModePopup.transform, "Title", 14, FontStyle.Bold,
            new Color(0.83f, 0.65f, 0.22f), new Vector2(0.05f, 0.72f), new Vector2(0.95f, 0.98f))
            .GetComponent<Text>().text = "CHOOSE ATTACK TYPE";

        Button terrBtn = MakeButton(attackModePopup.transform, "TerritoryBtn",
            new Vector2(0.05f, 0.40f), new Vector2(0.95f, 0.68f),
            "\u2694 TERRITORY ATTACK\n(select on map)",
            new Color(0.3f, 0.1f, 0.05f), new Color(0.9f, 0.4f, 0.1f));
        terrBtn.onClick.AddListener(() =>
        {
            attackModePopup.SetActive(false);
            EnterTerritoryAttackSourceMode(pendingCard);
        });

        Button directBtn = MakeButton(attackModePopup.transform, "DirectBtn",
            new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.38f),
            "\u2694 DIRECT ASSAULT\n(damage enemy faction)",
            new Color(0.1f, 0.05f, 0.2f), new Color(0.6f, 0.2f, 0.9f));
        directBtn.onClick.AddListener(() =>
        {
            attackModePopup.SetActive(false);
            EnterTargetMode(pendingCard);
        });

        attackModePopup.SetActive(false);
    }

    void EnterTargetMode(CardData card)
    {
        isSelectingTarget = true;
        waitingForTarget = true;
        pendingCard = card;
        pendingCardPosition = GetCardWorldPosition(card);
        targetPopupCardName.text = card.cardName;
        targetPopup.SetActive(true);
        FactionData player = GameManager.Instance.GetPlayerFaction();
        foreach (FactionPanelUI p in factionPanels)
            if (p.faction != player && !p.faction.isEliminated) p.SetHighlight(true);
        AddLogMessage($"Select a target for {card.cardName}...");
    }

    void ExitTargetMode()
    {
        isSelectingTarget = false;
        waitingForTarget = false;
        waitingForTerritoryAttackSource = false;
        waitingForTerritoryAttackTarget = false;
        pendingCard = null;
        pendingAttackSourceTerritory = null;
        if (targetPopup != null) targetPopup.SetActive(false);
        foreach (FactionPanelUI p in factionPanels) p.SetHighlight(false);
        ClearTerritoryHighlights();
    }

    void OnFactionPanelClicked(FactionData faction)
    {
        if (!waitingForTarget || pendingCard == null) return;
        FactionData player = GameManager.Instance.GetPlayerFaction();
        if (faction == player || faction.isEliminated) return;
        CardData card = pendingCard;
        pendingCard = null;
        if (card.cardType == CardType.SABOTAGE)
        {
            pendingMinigameFactionTarget = faction;
            BuildCipherMinigame(card, faction.territories.FirstOrDefault());
            ExitTargetMode();
            return;
        }
        if (card.cardType == CardType.INFLUENCE)
        {
            pendingMinigameFactionTarget = faction;
            BuildNegotiationMinigame(card, faction);
            ExitTargetMode();
            return;
        }
        StartCoroutine(AnimateCardPlay(card, pendingCardPosition));
        GameManager.Instance.PlayCard(card, faction);
        ExitTargetMode();
    }

    public GameObject BuildCipherMinigame(CardData card, TerritoryData target)
    {
        GameObject overlay = CreateMinigameOverlay("CipherMinigame", "THE CIPHER");
        Transform panel = overlay.transform.Find("Panel");
        Text resultText = MakeUIText(panel, "Result", 15, FontStyle.Bold, Parchment,
            new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.18f)).GetComponent<Text>();
        resultText.text = "Find the marked cells.";

        HashSet<int> marked = new HashSet<int>();
        while (marked.Count < 6) marked.Add(Random.Range(0, 16));
        int clicks = 0;
        int bonus = 0;

        for (int i = 0; i < 16; i++)
        {
            int idx = i;
            Button btn = CreateMinigameButton(panel, "C" + i, "", 0.12f + (i % 4) * 0.19f, 0.22f + (3 - i / 4) * 0.13f, 0.16f, 0.10f);
            btn.onClick.AddListener(() =>
            {
                if (!btn.interactable || clicks >= 4) return;
                clicks++;
                bool hit = marked.Contains(idx);
                bonus += hit ? 3 : -1;
                btn.GetComponent<Image>().color = hit ? new Color(0.25f, 0.55f, 0.20f) : new Color(0.55f, 0.15f, 0.10f);
                btn.interactable = false;
                resultText.text = $"Clicks {clicks}/4 | Bonus {bonus:+#;-#;0}";
                if (clicks >= 4)
                {
                    lastMinigameBonus = bonus;
                    GameManager.Instance.pendingCardPowerBonus = bonus;
                    CompleteMinigameCard(card, pendingMinigameFactionTarget, overlay);
                }
            });
        }
        return overlay;
    }

    public GameObject BuildNegotiationMinigame(CardData card, FactionData target)
    {
        GameObject overlay = CreateMinigameOverlay("NegotiationMinigame", "THE NEGOTIATION");
        Transform panel = overlay.transform.Find("Panel");
        int hiddenTarget = Random.Range(30, 71);
        Slider slider = new GameObject("OfferSlider", typeof(RectTransform)).AddComponent<Slider>();
        slider.transform.SetParent(panel, false);
        Anchor(slider.GetComponent<RectTransform>(), 0.12f, 0.42f, 0.88f, 0.52f);
        slider.minValue = 0f;
        slider.maxValue = 100f;
        slider.value = 50f;
        GameObject sliderBg = new GameObject("Background", typeof(RectTransform));
        sliderBg.transform.SetParent(slider.transform, false);
        Image sliderBgImg = sliderBg.AddComponent<Image>();
        sliderBgImg.color = Med;
        FullStretch(sliderBg.GetComponent<RectTransform>());
        GameObject sliderFill = new GameObject("Fill", typeof(RectTransform));
        sliderFill.transform.SetParent(slider.transform, false);
        Image sliderFillImg = sliderFill.AddComponent<Image>();
        sliderFillImg.color = Gold.WithA(0.75f);
        FullStretch(sliderFill.GetComponent<RectTransform>());
        GameObject sliderHandle = new GameObject("Handle", typeof(RectTransform));
        sliderHandle.transform.SetParent(slider.transform, false);
        Image sliderHandleImg = sliderHandle.AddComponent<Image>();
        sliderHandleImg.color = Parchment;
        RectTransform handleRect = sliderHandle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(18, 18);
        slider.targetGraphic = sliderHandleImg;
        slider.fillRect = sliderFill.GetComponent<RectTransform>();
        slider.handleRect = handleRect;
        Text resultText = MakeUIText(panel, "Result", 15, FontStyle.Bold, Parchment,
            new Vector2(0.12f, 0.25f), new Vector2(0.88f, 0.35f)).GetComponent<Text>();
        resultText.text = "Offer";

        Button submit = CreateMinigameButton(panel, "Submit", "SUBMIT", 0.35f, 0.12f, 0.30f, 0.10f);
        submit.onClick.AddListener(() =>
        {
            submit.interactable = false;
            int diff = Mathf.Abs(Mathf.RoundToInt(slider.value) - hiddenTarget);
            int bonus = diff <= 10 ? 5 : diff <= 20 ? 0 : -Mathf.Max(1, card.power / 2);
            lastMinigameBonus = bonus;
            GameManager.Instance.pendingCardPowerBonus = bonus;
            resultText.text = diff <= 10 ? "Perfect agreement." : diff <= 20 ? "Accepted." : "Rejected. Half effect.";
            CompleteMinigameCard(card, target, overlay);
        });
        return overlay;
    }

    public GameObject BuildAmbushMinigame(TerritoryData source, TerritoryData target, System.Action<int> onResult)
    {
        GameObject overlay = CreateMinigameOverlay("AmbushMinigame", "THE AMBUSH");
        Transform panel = overlay.transform.Find("Panel");
        string[] routes = { "N", "S", "E", "W", "HOLD" };
        string escape = routes[Random.Range(0, routes.Length)];
        bool resolved = false;
        Text resultText = MakeUIText(panel, "Result", 15, FontStyle.Bold, Parchment,
            new Vector2(0.10f, 0.22f), new Vector2(0.90f, 0.32f)).GetComponent<Text>();
        resultText.text = "Choose the escape route.";
        for (int i = 0; i < routes.Length; i++)
        {
            string route = routes[i];
            Button btn = CreateMinigameButton(panel, "Route" + route, route, 0.08f + i * 0.17f, 0.45f, 0.14f, 0.12f);
            btn.onClick.AddListener(() =>
            {
                if (resolved) return;
                resolved = true;
                int bonus = route == escape ? 2 : 0;
                lastMinigameBonus = bonus;
                resultText.text = bonus > 0 ? "Ambush successful." : "The target slips away.";
                StartCoroutine(CloseMinigameAfterDelay(overlay, () => onResult?.Invoke(bonus)));
            });
        }
        return overlay;
    }

    GameObject CreateMinigameOverlay(string name, string title)
    {
        if (activeMinigamePanel != null) Destroy(activeMinigamePanel);
        GameObject overlay = new GameObject(name, typeof(RectTransform));
        overlay.transform.SetParent(canvas.transform, false);
        FullStretch(overlay.GetComponent<RectTransform>());
        Image bg = overlay.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.75f);
        bg.raycastTarget = true;

        GameObject panel = new GameObject("Panel", typeof(RectTransform));
        panel.transform.SetParent(overlay.transform, false);
        Image panelBg = panel.AddComponent<Image>();
        panelBg.color = Darkest;
        Anchor(panel.GetComponent<RectTransform>(), 0.30f, 0.30f, 0.70f, 0.70f);
        MakeUIText(panel.transform, "Title", 22, FontStyle.Bold, Gold,
            new Vector2(0.08f, 0.80f), new Vector2(0.92f, 0.94f)).GetComponent<Text>().text = title;
        activeMinigamePanel = overlay;
        return overlay;
    }

    Button CreateMinigameButton(Transform parent, string name, string label, float x, float y, float w, float h)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = Med;
        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        Anchor(go.GetComponent<RectTransform>(), x, y, x + w, y + h);
        MakeUIText(go.transform, "Label", 16, FontStyle.Bold, Parchment, Vector2.zero, Vector2.one).GetComponent<Text>().text = label;
        return btn;
    }

    void CompleteMinigameCard(CardData card, FactionData target, GameObject overlay)
    {
        StartCoroutine(CloseMinigameAfterDelay(overlay, () =>
        {
            StartCoroutine(AnimateCardPlay(card, pendingCardPosition));
            GameManager.Instance.PlayCard(card, target);
            pendingMinigameFactionTarget = null;
        }));
    }

    IEnumerator CloseMinigameAfterDelay(GameObject overlay, System.Action afterClose)
    {
        yield return new WaitForSeconds(1.5f);
        if (overlay != null) Destroy(overlay);
        if (activeMinigamePanel == overlay) activeMinigamePanel = null;
        afterClose?.Invoke();
    }

    void OnFactionEliminated(FactionData faction)
    {
        if (faction == null) return;
        if (!eliminatedFactions.Contains(faction.factionId)) eliminatedFactions.Add(faction.factionId);
        foreach (FactionPanelUI p in factionPanels)
            if (p.faction == faction) { p.SetEliminatedVisuals(); break; }
        if (factionPanelRoots.ContainsKey(faction.factionId))
        {
            GameObject panelRoot = factionPanelRoots[faction.factionId];
            Image bg = panelRoot.GetComponent<Image>();
            if (bg != null) bg.color = new Color(0.12f, 0.08f, 0.06f, 0.5f);
            GameObject stamp = new GameObject("EliminatedStamp");
            stamp.transform.SetParent(panelRoot.transform, false);
            RectTransform sr = stamp.AddComponent<RectTransform>();
            sr.anchorMin = Vector2.zero; sr.anchorMax = Vector2.one;
            sr.sizeDelta = Vector2.zero;
            Text st = stamp.AddComponent<Text>();
            st.text = "ELIMINATED";
            st.font = GetFont();
            st.fontSize = 14; st.fontStyle = FontStyle.Bold;
            st.color = new Color(0.8f, 0.1f, 0.1f, 0.7f);
            st.alignment = TextAnchor.MiddleCenter;
            st.raycastTarget = false;
            stamp.transform.localRotation = Quaternion.Euler(0, 0, -25f);
        }
        UpdateTerritoryMap();
    }

    Vector3 GetCardWorldPosition(CardData card)
    {
        foreach (CardUIController cc in handCards)
            if (cc.CurrentCard == card) return cc.transform.position;
        return Vector3.zero;
    }

    IEnumerator AnimateCardPlay(CardData card, Vector3 startPos)
    {
        GameObject ghost = Instantiate(cardPrefab, canvas.transform);
        ghost.SetActive(true);
        ghost.transform.position = startPos;
        CardUIController gc = ghost.GetComponent<CardUIController>();
        if (gc != null)
        {
            gc.BuildCard();
            Color ghostColor = Color.gray;
            if (GameManager.Instance != null && card.factionId >= 0 && card.factionId < GameManager.Instance.factions.Count)
                ghostColor = GameManager.Instance.factions[card.factionId].factionColor;
            gc.SetCard(card, ghostColor);
            gc.SetInteractable(false);
        }
        CanvasGroup gcg = ghost.GetComponent<CanvasGroup>();
        if (gcg != null) gcg.alpha = 1f;
        Vector3 center = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        for (float e = 0; e < 0.25f; e += Time.deltaTime)
        {
            ghost.transform.position = Vector3.Lerp(startPos, center, e / 0.25f);
            yield return null;
        }
        ghost.transform.position = center;
        yield return new WaitForSeconds(0.3f);
        if (gcg != null)
        {
            for (float e = 0; e < 0.2f; e += Time.deltaTime)
            {
                gcg.alpha = Mathf.Lerp(1f, 0f, e / 0.2f);
                yield return null;
            }
        }
        Destroy(ghost);
    }

    IEnumerator PlayCardAnimation(GameObject cardGO)
    {
        if (cardGO == null) yield break;
        RectTransform rt = cardGO.GetComponent<RectTransform>();
        if (rt == null) yield break;
        Vector2 startPos = rt.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0f, 200f);
        CanvasGroup cg = cardGO.GetComponent<CanvasGroup>();
        float t = 0f;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            float frac = Mathf.Clamp01(t / 0.3f);
            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, frac);
            float scale = Mathf.Lerp(1f, 1.3f, frac);
            rt.localScale = Vector3.one * scale;
            if (cg != null && frac > 0.5f)
                cg.alpha = Mathf.Lerp(1f, 0f, (frac - 0.5f) * 2f);
            yield return null;
        }
        Destroy(cardGO);
    }

    public void FlashFactionPanel(int factionId)
    {
        foreach (FactionPanelUI p in factionPanels)
            if (p.faction != null && p.faction.factionId == factionId) { p.FlashDamage(); break; }
        if (factionPanelRoots.TryGetValue(factionId, out GameObject panel))
            StartCoroutine(FlashPanelBorder(panel, factionId));
    }

    IEnumerator FlashPanelBorder(GameObject panel, int factionId)
    {
        Image bg = panel.GetComponentInChildren<Image>();
        if (bg == null)
        {
            Debug.LogWarning("[GameUIManager] FlashPanelBorder: No Image component found on panel or children");
            yield break;
        }

        Color orig = bg.color;
        Color hitColor = new Color(0.8f, 0.1f, 0.05f, 0.85f);
        float t = 0f;
        while (t < 0.4f)
        {
            bg.color = Color.Lerp(hitColor, orig, t / 0.4f);
            t += Time.deltaTime;
            yield return null;
        }
        bg.color = orig;
    }

    public void UpdateAllFactions()
    {
        foreach (FactionPanelUI p in factionPanels) p.UpdateStats();
        // ADDED: UPGRADE 4 - refresh region bonus display every turn
        UpdateRegionBonusDisplay();
    }

    public void EnableCardInteractions(bool enabled)
    {
        if (!enabled) HideTooltip();
        foreach (CardUIController c in handCards) c.SetInteractable(enabled);
    }

    public void AddLogMessage(string msg) => turnLog?.AddEntry(msg);

    void UpdateTurnCounter(int turn)
    {
        if (turnCounterText != null)
            turnCounterText.text = $"Turn {turn} | {GameManager.Instance.CurrentYear} AD";
        StartCoroutine(PulseTurnCounter());
    }

    IEnumerator PulseTurnCounter()
    {
        if (turnCounterText == null) yield break;
        RectTransform rt = turnCounterText.GetComponent<RectTransform>();
        float t = 0f;
        while (t < 0.3f) {
            t += Time.deltaTime;
            float s = 1f + Mathf.Sin(t / 0.3f * Mathf.PI) * 0.15f;
            rt.localScale = Vector3.one * s;
            yield return null;
        }
        rt.localScale = Vector3.one;
    }

    void UpdateYearDisplay(int year)
    {
        if (yearProgressText == null || GameManager.Instance == null) return;
        int turnsLeft = GameManager.MAX_TURNS - GameManager.Instance.currentTurn;
        int yearsLeft = GameManager.END_YEAR - year;
        yearProgressText.text = $"{turnsLeft} turns | {yearsLeft} years to {GameManager.END_YEAR} AD";
        yearProgressText.color = turnsLeft < 50 ? new Color(0.85f, 0.40f, 0.10f) : Parchment.WithA(0.65f);
    }

    void OnGameStateChanged(GameState state)
    {
        ApplyPhaseVisibility(state);

        switch (state)
        {
            case GameState.DRAW_PHASE:
                AddLogMessage("\u2500\u2500 DRAW PHASE: Cards are being dealt. Wait for PLAY PHASE to act.");
                break;
            case GameState.PLAY_PHASE:
                AddLogMessage("\u2500\u2500 PLAY PHASE: Click a card to play it, or click END TURN.");
                break;
            case GameState.REINFORCEMENT_PHASE:
                AddLogMessage("\u2500\u2500 REINFORCEMENT PHASE: Click your territories on the map to place troops, then CONFIRM.");
                break;
            case GameState.ATTACK_PHASE:
                AddLogMessage("\u2500\u2500 ATTACK PHASE: Click one of your territories (with 2+ troops) to attack from.");
                break;
            case GameState.FORTIFY_PHASE:
                AddLogMessage("\u2500\u2500 FORTIFY PHASE: Move troops between adjacent territories, or END TURN.");
                break;
            case GameState.RESOLUTION_PHASE:
                AddLogMessage("\u2500\u2500 RESOLUTION: Cards are being resolved...");
                break;
        }

        // When entering DRAW_PHASE, refresh the hand and enforce sibling order
        // so HandArea renders above market/probability panels.
        if (state == GameState.DRAW_PHASE || state == GameState.PLAY_PHASE)
        {
            RefreshHandUI();
            EnforceDrawOrder();
        }
        UpdateEndTurnButtonState(state);
        UpdateCardsRemaining();

        HideAttackArrow();

        switch (state)
        {
            case GameState.REINFORCEMENT_PHASE:
                EnterReinforcementPhaseUI();
                break;
            case GameState.DRAW_PHASE:
                EnterDrawPhaseUI();
                break;
            case GameState.PLAY_PHASE:
                EnterPlayPhaseUI();
                break;
            case GameState.RESOLUTION_PHASE:
                EnterResolutionPhaseUI();
                break;
            case GameState.ATTACK_PHASE:
                EnterAttackPhaseUI();
                break;
            case GameState.FORTIFY_PHASE:
                EnterFortifyPhaseUI();
                break;
            case GameState.GAME_OVER:
                EnterGameOverPhaseUI();
                break;
        }
    }

    void EnterReinforcementPhaseUI()
    {
        ShowPhase("\u2694  REINFORCEMENT PHASE \u2014 Click map to place troops", new Color(0.55f, 0.35f, 0.05f), 6f);
        UpdateTerritoryMap();
    }

    void EnterDrawPhaseUI()
    {
        ShowPhase("\u2726  DRAW PHASE", Gold);
        RefreshProbabilityForCurrentPlayer();
    }

    void EnterPlayPhaseUI()
    {
        ShowPhase("\u25B6  PLAY PHASE", new Color(0.3f, 0.7f, 0.3f));
        RefreshProbabilityForCurrentPlayer();
        UpdateTerritoryMap();
    }

    void EnterResolutionPhaseUI()
    {
        ShowPhase("\u25CF  RESOLUTION", new Color(0.8f, 0.2f, 0.1f));
        if (probabilityPanel != null) probabilityPanel.SetActive(false);
    }

    void EnterAttackPhaseUI()
    {
        ShowPhase("\u2694  ATTACK PHASE", new Color(0.8f, 0.2f, 0.1f));
        if (probabilityPanel != null) probabilityPanel.SetActive(false);
    }

    void EnterFortifyPhaseUI()
    {
        ShowPhase("\u27F3  FORTIFY PHASE", new Color(0.6f, 0.4f, 0.1f));
        if (probabilityPanel != null) probabilityPanel.SetActive(false);
        ShowFortifyPanel();
    }

    void EnterGameOverPhaseUI()
    {
        ShowPhase("\u2605  GAME OVER", new Color(0.5f, 0.0f, 0.5f));
    }

    void ShowPhase(string label, Color color, float duration = 1.8f)
    {
        if (phaseBannerCoroutine != null)
        {
            StopCoroutine(phaseBannerCoroutine);
            phaseBannerCoroutine = null;
        }
        if (phaseBanner != null) phaseBanner.SetActive(false);
        phaseBannerCoroutine = StartCoroutine(ShowPhaseBanner(label, color, duration));
    }

    void RefreshProbabilityForCurrentPlayer()
    {
        if (GameManager.Instance == null) return;
        FactionData player = GameManager.Instance.GetPlayerFaction();
        if (player == null) return;

        var weights = CardProbabilitySystem.GetAdjustedWeights(
            player,
            GameManager.Instance.factions,
            GameManager.Instance.territories,
            GameManager.Instance.currentTurn);
        UpdateProbabilityDisplay(weights);
    }

    void ApplyPhaseVisibility(GameState state)
    {
        bool gameOver = state == GameState.GAME_OVER;
        bool shellVisible = !gameOver;
        bool drawPhase = state == GameState.DRAW_PHASE;
        bool playPhase = state == GameState.PLAY_PHASE;
        bool reinforcePhase = state == GameState.REINFORCEMENT_PHASE;
        bool resolutionPhase = state == GameState.RESOLUTION_PHASE;
        bool fortifyPhase = state == GameState.FORTIFY_PHASE;
        bool mapVisible = shellVisible;
        bool attackPhase = state == GameState.ATTACK_PHASE;
        bool tacticalMapPhase = reinforcePhase || playPhase || resolutionPhase || fortifyPhase;

        SetActiveIfExists(topBar, shellVisible);
        SetActiveIfExists(rightPanelBg, shellVisible);
        SetActiveIfExists(mapAreaBg, mapVisible);
        SetActiveIfExists(mapFrame, mapVisible);
        SetActiveIfExists(territoryMapPanel, mapVisible);
        SetActiveIfExists(mapClickZonesRoot, shellVisible);
        SetActiveIfExists(factionArea, shellVisible);
        SetActiveIfExists(logArea, shellVisible);
        SetActiveIfExists(handArea, reinforcePhase || drawPhase || playPhase);
        SetActiveIfExists(playedArea, playPhase || resolutionPhase);
        SetActiveIfExists(endTurnRoot, playPhase || fortifyPhase);
        SetActiveIfExists(cardsRemainingRoot, drawPhase || playPhase);
        SetActiveIfExists(regionBonusPanel, reinforcePhase || playPhase || fortifyPhase);
        bool attackSelection = waitingForTerritoryAttackSource || waitingForTerritoryAttackTarget;
        if (probabilityPanel != null)
            probabilityPanel.SetActive((drawPhase || playPhase) && !attackSelection);

        if (marketToggleBtn != null) marketToggleBtn.SetActive(playPhase);
        if (economyToggleButton != null) economyToggleButton.gameObject.SetActive(playPhase);

        if (!playPhase)
        {
            marketVisible = false;
            if (marketPanel != null) marketPanel.SetActive(false);
            if (EconomyUI.Instance != null) EconomyUI.Instance.Hide();
            HideTooltip();
            ExitTargetMode();
            ExitTerritoryAttackMode();
        }

        if (reinforcePhase && factionArea != null)
            factionArea.transform.SetAsLastSibling();
        if (reinforcePhase && reinforcementPanel != null)
            reinforcementPanel.transform.SetAsLastSibling();
        if (reinforcePhase && mapClickZonesRoot != null)
            mapClickZonesRoot.transform.SetAsLastSibling();

        if (!reinforcePhase && reinforcementPanel != null)
            reinforcementPanel.SetActive(false);

        if (!fortifyPhase)
            HideFortifyPanel();

        if (!resolutionPhase && combatPopup != null)
        {
            if (combatAutoDismiss != null)
            {
                StopCoroutine(combatAutoDismiss);
                combatAutoDismiss = null;
            }
            combatPopup.SetActive(false);
        }

        SetActiveIfExists(expandBtnRoot, playPhase);
        if (playPhase && endTurnRoot != null && !endTurnRoot.activeSelf)
            endTurnRoot.SetActive(true);

        if (gameOver)
        {
            HideAttackArrow();
            HideTerritoryTooltip();
            HideAttackStatus();
        }

        UpdateMapClickZones();
    }

    bool CanUseActionPanels()
    {
        return GameManager.Instance != null && GameManager.Instance.currentState == GameState.PLAY_PHASE;
    }

    void SetActiveIfExists(GameObject go, bool active)
    {
        if (go != null && go.activeSelf != active)
            go.SetActive(active);
    }

    void UpdateEndTurnButtonState(GameState state)
    {
        bool interactable = state == GameState.PLAY_PHASE || state == GameState.FORTIFY_PHASE;
        endTurnButton.interactable = interactable;
        if (endTurnButtonImage == null) return;

        if (state == GameState.PLAY_PHASE)
        {
            bool maxed = GameManager.Instance != null &&
            GameManager.Instance.CardsPlayedThisTurn >= GameManager.Instance.MaxCardsPerTurn;
            endTurnButtonImage.color = maxed
            ? new Color(0.38f, 0.05f, 0.03f)
            : new Color(0.48f, 0.08f, 0.04f);
            if (endTurnLabel != null) endTurnLabel.text = maxed ? "END TURN" : "END TURN";
        }
        else if (state == GameState.FORTIFY_PHASE)
        {
            endTurnButtonImage.color = new Color(0.35f, 0.20f, 0.05f);
            if (endTurnLabel != null) endTurnLabel.text = "SKIP FORTIFY";
        }
        else
        {
            endTurnButtonImage.color = new Color(0.15f, 0.06f, 0.03f);
            if (endTurnLabel != null)
                endTurnLabel.text = state == GameState.RESOLUTION_PHASE ? "RESOLVING..."
                : state == GameState.DRAW_PHASE ? "DRAWING..."
                : state == GameState.REINFORCEMENT_PHASE ? "REINFORCING..."
                : "END TURN";
        }
    }

    public void OnPhaseTimerUpdate(float timeLeft)
    {
        if (phaseTimerBar == null) return;
        if (timeLeft <= 0f)
        {
            phaseTimerBar.gameObject.SetActive(false);
            return;
        }
        phaseTimerBar.gameObject.SetActive(true);
        float pct = timeLeft / GameManager.PHASE_TIME_LIMIT;
        phaseTimerBar.rectTransform.anchorMax = new Vector2(pct * 0.99f + 0.01f, 0.06f);
        float urgency = Mathf.Clamp01(1f - (timeLeft / 10f));
        phaseTimerBar.color = Color.Lerp(
            new Color(0.75f, 0.45f, 0.08f, 0.75f),
            new Color(0.55f, 0.07f, 0.05f, 0.9f),
            urgency);
    }

    void OnAnyCardPlayed(CardData card, FactionData source, FactionData target)
    {
        if (GameManager.Instance == null) return;
        int remaining = GameManager.Instance.MaxCardsPerTurn - GameManager.Instance.CardsPlayedThisTurn;
        string label = $"\u25B6  {remaining} card{(remaining != 1 ? "s" : "")} remaining";
        ShowPhase(label, new Color(0.3f, 0.7f, 0.3f));
    }

    void UpdateCardsRemaining()
    {
        if (cardsRemainingText == null || GameManager.Instance == null) return;
        int played = GameManager.Instance.CardsPlayedThisTurn;
        int max = GameManager.Instance.MaxCardsPerTurn;
        FactionData player = GameManager.Instance.GetPlayerFaction();
        int handCount = player != null ? player.hand.Count : 0;
        string handStr = handCount >= 7
        ? $"<color=#C8A040>HAND FULL</color>"
        : $"Hand:{handCount}/7";
        cardsRemainingText.text = $"Cards:{played}/{max} {handStr}";
        cardsRemainingText.color = played >= max
        ? new Color(0.80f, 0.25f, 0.10f)
        : Parchment;
        if (GameManager.Instance.CardsPlayedThisTurn >= GameManager.Instance.MaxCardsPerTurn)
            StartCoroutine(PulseEndTurnButton());
    }

    IEnumerator PulseEndTurnButton()
    {
        if (endTurnButton == null) yield break;
        Image btnImg = endTurnButton.GetComponent<Image>();
        if (btnImg == null) yield break;
        Color original = btnImg.color;
        Color pulse = new Color(0.85f, 0.65f, 0.10f);
        for (int i = 0; i < 3; i++)
        {
            float t = 0f;
            while (t < 0.3f) {
                t += Time.deltaTime;
                btnImg.color = Color.Lerp(original, pulse, Mathf.Sin(t/0.3f * Mathf.PI));
                yield return null;
            }
            btnImg.color = original;
            yield return new WaitForSeconds(0.2f);
        }
    }

    // FIXED: UI FIX 5 - game over panel tinted with winner's faction color + territory count
    void ShowGameOver(bool playerWon)
    {
        HideTooltip();
        FactionData winner = GameManager.Instance.factions.Find(f => !f.isEliminated);
        FactionData playerFaction = GameManager.Instance.GetPlayerFaction();
        string factionName = playerFaction != null ? playerFaction.factionName : "";
        Image panelBg = gameOverPanel.GetComponent<Image>();
        if (playerWon)
        {
            panelBg.color = winner != null
                ? new Color(winner.factionColor.r * 0.3f, winner.factionColor.g * 0.3f,
                            winner.factionColor.b * 0.3f, 0.95f)
                : Darkest;
            gameOverTitleText.text = "VICTORY";
            gameOverTitleText.color = Gold;
            int owned = winner != null
                ? GameManager.Instance.territories.Count(t => t.controlledBy == winner.factionId)
                : 0;
            gameOverSubtitleText.text = $"{factionName} rules the shadows of Europe — {owned} territories";
            gameOverSubtitleText.color = Parchment.WithA(0.65f);
        }
        else
        {
            panelBg.color = winner != null
                ? new Color(winner.factionColor.r * 0.3f, winner.factionColor.g * 0.3f,
                            winner.factionColor.b * 0.3f, 0.95f)
                : new Color(0.08f, 0.01f, 0.01f);
            gameOverTitleText.text = "ELIMINATED";
            gameOverTitleText.color = new Color(0.80f, 0.10f, 0.05f);
            int owned = winner != null
                ? GameManager.Instance.territories.Count(t => t.controlledBy == winner.factionId)
                : 0;
            gameOverSubtitleText.text = winner != null
                ? $"{winner.factionName} wins with {owned} territories"
                : "The shadows swallow you whole";
            gameOverSubtitleText.color = Parchment.WithA(0.50f);
        }
        gameOverDivider.gameObject.SetActive(true);
        gameOverCanvasGroup.alpha = 0f;
        gameOverCanvasGroup.blocksRaycasts = true;
        gameOverPanel.SetActive(true);
        StartCoroutine(FadeInGameOver());
    }

    IEnumerator FadeInGameOver()
    {
        for (float e = 0; e < 0.5f; e += Time.deltaTime)
        {
            gameOverCanvasGroup.alpha = Mathf.Lerp(0f, 1f, e / 0.5f);
            yield return null;
        }
        gameOverCanvasGroup.alpha = 1f;
    }

    IEnumerator FadeOutAndRestart()
    {
        for (float e = 0; e < 0.3f; e += Time.deltaTime)
        {
            gameOverCanvasGroup.alpha = Mathf.Lerp(1f, 0f, e / 0.3f);
            yield return null;
        }
        gameOverCanvasGroup.alpha = 0f;
        gameOverCanvasGroup.blocksRaycasts = false;
        gameOverPanel.SetActive(false);
        GameManager.Instance.RestartGame();
    }

    public void ShowTooltip(CardData card, Vector3 worldPosition)
    {
        if (GameManager.Instance.currentState != GameState.PLAY_PHASE) return;
        tooltipNameText.text = card.cardName;
        (tooltipTypeText.text, tooltipTypeText.color) = card.cardType switch
        {
            CardType.ATTACK => ("ATTACK", new Color(0.75f, 0.30f, 0.25f)),
            CardType.DEFENSE => ("DEFENSE", new Color(0.30f, 0.45f, 0.70f)),
            CardType.INFLUENCE => ("INFLUENCE", new Color(0.30f, 0.65f, 0.30f)),
            CardType.SABOTAGE => ("SABOTAGE", new Color(0.60f, 0.30f, 0.60f)),
            CardType.FARMING => ("FARMING", new Color(0.50f, 0.38f, 0.20f)),
            CardType.BLACK_MARKET => ("BLACK MARKET", new Color(0.65f, 0.45f, 0.12f)),
            _ => ("UNKNOWN", Color.white)
        };
        tooltipPowerText.text = $"Power: {card.power}";
        tooltipDescriptionText.text = card.description;

        RectTransform canvasRT = canvas.GetComponent<RectTransform>();
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT,
                                                                new Vector2(worldPosition.x, worldPosition.y), null, out localPoint);
        Vector2 ap = localPoint + Vector2.Scale(canvasRT.rect.size, canvasRT.pivot);
        ap.y += ap.y + 330f > canvasRT.rect.height ? -170f : 170f;
        tooltipPanel.GetComponent<RectTransform>().anchoredPosition = ap;
        tooltipPanel.SetActive(true);
    }

    public void HideTooltip() { if (tooltipPanel != null) tooltipPanel.SetActive(false); }

    void BuildTerritoryTooltip(Transform ct)
    {
        territoryTooltip = new GameObject("TerritoryTooltip", typeof(RectTransform));
        territoryTooltip.transform.SetParent(ct, false);
        RectTransform tr = territoryTooltip.GetComponent<RectTransform>();
        tr.pivot = new Vector2(0f, 0f);
        tr.anchorMin = tr.anchorMax = Vector2.zero;
        tr.sizeDelta = new Vector2(220, 110);
        Image tb = territoryTooltip.AddComponent<Image>();
        tb.color = Dark;
        tb.raycastTarget = false;
        Canvas ttCanvas = territoryTooltip.AddComponent<Canvas>();
        ttCanvas.overrideSorting = true;
        ttCanvas.sortingOrder = 50;
        territoryTooltip.AddComponent<GraphicRaycaster>();
        territoryTooltip.SetActive(false);

        MakeHRule(territoryTooltip.transform, new Vector2(0.05f, 0.68f), new Vector2(0.95f, 0.70f), Gold.WithA(0.50f))
            .GetComponent<Image>().raycastTarget = false;

        territoryTooltipName = MakeUIText(territoryTooltip.transform, "Name", 14, FontStyle.Bold,
            Parchment, new Vector2(0.05f, 0.74f), new Vector2(0.95f, 0.96f)).GetComponent<Text>();

        territoryTooltipOwner = MakeUIText(territoryTooltip.transform, "Owner", 11, FontStyle.Normal,
            Parchment.WithA(0.75f), new Vector2(0.05f, 0.56f), new Vector2(0.95f, 0.68f)).GetComponent<Text>();

        territoryTooltipStats = MakeUIText(territoryTooltip.transform, "Stats", 10, FontStyle.Normal,
            Parchment.WithA(0.65f), new Vector2(0.05f, 0.02f), new Vector2(0.95f, 0.54f)).GetComponent<Text>();
        territoryTooltipStats.horizontalOverflow = HorizontalWrapMode.Wrap;
    }

    public void ShowTerritoryTooltip(TerritoryData t)
    {
        if (territoryTooltip == null || canvas == null) return;

        territoryTooltipName.text = t.name;
        string ownerName = t.controlledBy >= 0 && t.controlledBy < GameManager.Instance.factions.Count
            ? GameManager.Instance.factions[t.controlledBy].factionName : "Neutral";
        territoryTooltipOwner.text = $"Owner: {ownerName}";
        territoryTooltipStats.text = $"Troops: {t.troops}   Farms: {t.farmingValue}   Strategic: {t.strategicValue}";

        float anchorX = Mathf.Lerp(MapAnchorMin.x, MapAnchorMax.x, t.mapPosition.x);
        float anchorY = Mathf.Lerp(MapAnchorMin.y, MapAnchorMax.y, 1f - t.mapPosition.y);

        RectTransform canvasRT = canvas.GetComponent<RectTransform>();
        float cw = canvasRT.rect.width;
        float ch = canvasRT.rect.height;
        float rawX = anchorX * cw;
        float rawY = anchorY * ch;

        RectTransform tr = territoryTooltip.GetComponent<RectTransform>();
        float tw = tr.sizeDelta.x;
        float th = tr.sizeDelta.y;

        float finalX = Mathf.Clamp(rawX + 16f, 4f, cw - tw - 4f);
        float finalY = Mathf.Clamp(rawY + 16f, 4f, ch - th - 4f);

        tr.anchoredPosition = new Vector2(finalX, finalY);
        territoryTooltip.SetActive(true);
    }

    public void HideTerritoryTooltip()
    {
        if (territoryTooltip != null) territoryTooltip.SetActive(false);
    }

    void BuildAttackStatusBanner(Transform ct)
    {
        attackStatusBanner = new GameObject("AttackStatusBanner", typeof(RectTransform));
        attackStatusBanner.transform.SetParent(ct, false);
        Image asbImg = attackStatusBanner.AddComponent<Image>();
        asbImg.color = new Color(0.6f, 0.10f, 0.04f, 0.96f);
        Anchor(attackStatusBanner.GetComponent<RectTransform>(), 0.01f, 0.86f, 0.72f, 0.95f);

        attackStatusText = MakeUIText(attackStatusBanner.transform, "StatusText", 15, FontStyle.Bold,
            new Color(1.0f, 0.88f, 0.55f, 1.0f),
            new Vector2(0.02f, 0f), new Vector2(0.98f, 1f)).GetComponent<Text>();
        attackStatusText.alignment = TextAnchor.MiddleLeft;
        attackStatusBanner.SetActive(false);
    }

    public void ShowAttackStatus(string message)
    {
        if (attackStatusBanner == null || attackStatusText == null) return;
        attackStatusText.text = message;
        attackStatusBanner.SetActive(true);
        if (attackStatusPulseCoroutine != null)
            StopCoroutine(attackStatusPulseCoroutine);
        attackStatusPulseCoroutine = StartCoroutine(PulseAttackStatusBanner());
    }

    public void HideAttackStatus()
    {
        if (attackStatusBanner != null) attackStatusBanner.SetActive(false);
        if (attackStatusPulseCoroutine != null)
        {
            StopCoroutine(attackStatusPulseCoroutine);
            attackStatusPulseCoroutine = null;
        }
    }

    IEnumerator PulseAttackStatusBanner()
    {
        Image bg = attackStatusBanner.GetComponent<Image>();
        while (attackStatusBanner != null && attackStatusBanner.activeSelf)
        {
            float pulse = 0.82f + 0.18f * Mathf.PingPong(Time.unscaledTime * 2f, 1f);
            if (bg != null)
                bg.color = new Color(0.6f, 0.10f, 0.04f, pulse);
            yield return null;
        }
        attackStatusPulseCoroutine = null;
    }

    public void FlashDamageEffect() => StartCoroutine(DamageFlashRoutine());

    IEnumerator DamageFlashRoutine()
    {
        damageFlashOverlay.color = new Color(1, 0, 0, 0.30f);
        for (float e = 0; e < 0.35f; e += Time.deltaTime)
        {
            damageFlashOverlay.color = new Color(1, 0, 0, Mathf.Lerp(0.30f, 0f, e / 0.35f));
            yield return null;
        }
        damageFlashOverlay.color = new Color(1, 0, 0, 0);
    }

    void BuildProbabilityPanel(Transform ct)
    {
        probabilityPanel = new GameObject("ProbabilityPanel", typeof(RectTransform));
        probabilityPanel.transform.SetParent(ct, false);
        Image ppBg = probabilityPanel.AddComponent<Image>();
        ppBg.color = new Color(0.04f, 0.02f, 0.01f, 0.82f);
        ppBg.raycastTarget = false;
        RectTransform probabilityRect = probabilityPanel.GetComponent<RectTransform>();
        Anchor(probabilityRect, 0.00f, 0.68f, 0.18f, 1.00f);
        probabilityRect.pivot = new Vector2(0f, 1f);
        probabilityRect.anchoredPosition = Vector2.zero;

        MakeUIText(probabilityPanel.transform, "Title", 9, FontStyle.Bold,
                   Gold, new Vector2(0.05f, 0.90f), new Vector2(0.95f, 0.99f))
            .GetComponent<Text>().text = "DRAW ODDS";

        MakeHRule(probabilityPanel.transform, new Vector2(0.05f, 0.86f), new Vector2(0.95f, 0.87f),
                  Gold.WithA(0.30f));

        float rowHeight = 0.12f;
        float startY = 0.80f;
        CardType[] allTypes = new CardType[] {
            CardType.ATTACK, CardType.DEFENSE, CardType.INFLUENCE,
            CardType.SABOTAGE, CardType.FARMING, CardType.BLACK_MARKET
        };

        Color[] typeColors = new Color[] {
            new Color(0.8f, 0.2f, 0.2f),
            new Color(0.2f, 0.4f, 0.9f),
            new Color(0.8f, 0.7f, 0.1f),
            new Color(0.5f, 0.1f, 0.7f),
            new Color(0.2f, 0.7f, 0.3f),
            new Color(0.6f, 0.4f, 0.1f)
        };

        for (int i = 0; i < allTypes.Length; i++)
        {
            CardType type = allTypes[i];
            Color color = typeColors[i];
            float yMin = startY - (i + 1) * rowHeight;
            float yMax = startY - i * rowHeight;

            GameObject row = new GameObject("Row_" + type, typeof(RectTransform));
            row.transform.SetParent(probabilityPanel.transform, false);
            Anchor(row.GetComponent<RectTransform>(), 0.02f, yMin, 0.98f, yMax);

            GameObject dot = new GameObject("Dot", typeof(RectTransform));
            dot.transform.SetParent(row.transform, false);
            Image dotImg = dot.AddComponent<Image>();
            dotImg.color = color;
            dotImg.raycastTarget = false;
            RectTransform dotR = dot.GetComponent<RectTransform>();
            dotR.anchorMin = new Vector2(0f, 0.2f);
            dotR.anchorMax = new Vector2(0f, 0.8f);
            dotR.sizeDelta = new Vector2(8, 0);
            dotR.anchoredPosition = new Vector2(4, 0);

            Text labelText = MakeUIText(row.transform, "Label", 8, FontStyle.Bold,
                Parchment, new Vector2(0.12f, 0f), new Vector2(0.48f, 1f)).GetComponent<Text>();
            labelText.text = type.ToString();
            labelText.alignment = TextAnchor.MiddleLeft;

            GameObject barBg = new GameObject("BarBg", typeof(RectTransform));
            barBg.transform.SetParent(row.transform, false);
            Image barBgImg = barBg.AddComponent<Image>();
            barBgImg.color = new Color(0.05f, 0.03f, 0.01f);
            barBgImg.raycastTarget = false;
            RectTransform barBgR = barBg.GetComponent<RectTransform>();
            barBgR.anchorMin = new Vector2(0.50f, 0.15f);
            barBgR.anchorMax = new Vector2(0.92f, 0.85f);
            barBgR.sizeDelta = Vector2.zero;

            GameObject barFill = new GameObject("BarFill", typeof(RectTransform));
            barFill.transform.SetParent(barBg.transform, false);
            Image barFillImg = barFill.AddComponent<Image>();
            barFillImg.color = color;
            barFillImg.raycastTarget = false;
            RectTransform barFillR = barFill.GetComponent<RectTransform>();
            barFillR.anchorMin = Vector2.zero;
            barFillR.anchorMax = new Vector2(0f, 1f);
            barFillR.sizeDelta = Vector2.zero;

            row.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            probabilityRows[type] = row;
        }

        probabilityPanel.SetActive(false);
    }

    public void UpdateProbabilityDisplay(Dictionary<CardType, float> weights)
    {
        if (probabilityPanel == null) return;

        bool isDrawOrPlay = GameManager.Instance != null &&
            (GameManager.Instance.currentState == GameState.DRAW_PHASE ||
             GameManager.Instance.currentState == GameState.PLAY_PHASE) &&
            !waitingForTerritoryAttackSource &&
            !waitingForTerritoryAttackTarget;
        probabilityPanel.SetActive(isDrawOrPlay);

        if (!isDrawOrPlay || weights == null) return;

        foreach (var kvp in probabilityRows)
        {
            CardType type = kvp.Key;
            GameObject row = kvp.Value;
            if (row == null) continue;

            float pct = weights.ContainsKey(type) ? weights[type] : 0f;

            Text pctText = row.transform.Find("Label")?.GetComponent<Text>();
            if (pctText != null)
                pctText.text = $"{type.ToString()} {Mathf.RoundToInt(pct)}%";

            Transform barBg = row.transform.Find("BarBg");
            if (barBg == null) continue;
            Transform barFill = barBg.Find("BarFill");
            if (barFill == null) continue;

            RectTransform barFillR = barFill.GetComponent<RectTransform>();
            float targetWidth = pct / 100f;
            if (probabilityBarAnim != null) StopCoroutine(probabilityBarAnim);
            probabilityBarAnim = StartCoroutine(AnimateBarWidth(barFillR, targetWidth));
        }

        CardType dominant = weights.OrderByDescending(kv => kv.Value).First().Key;
        float dominantWeight = weights[dominant];
        probabilityHistory.Add(dominantWeight);
        if (probabilityHistory.Count > 10)
            probabilityHistory.RemoveAt(0);
        Color lineColor = GetTypeColor(dominant);
        UpdateSparklineChart(lineColor);
    }

    IEnumerator AnimateBarWidth(RectTransform bar, float target)
    {
        float start = bar.anchorMax.x;
        float elapsed = 0f;
        float duration = 0.3f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            bar.anchorMax = new Vector2(Mathf.Lerp(start, target, t), 1f);
            yield return null;
        }
        bar.anchorMax = new Vector2(target, 1f);
    }

    void BuildBlackMarketPanel(Transform ct)
    {
        marketPanel = new GameObject("BlackMarketPanel", typeof(RectTransform));
        marketPanel.transform.SetParent(ct, false);
        Image mpBg = marketPanel.AddComponent<Image>();
        mpBg.color = new Color(0.06f, 0.03f, 0.01f, 0.97f);
        mpBg.raycastTarget = true;
        Anchor(marketPanel.GetComponent<RectTransform>(), 0.00f, 0.06f, 0.73f, 0.44f);
        marketPanel.SetActive(false);

        GameObject border = new GameObject("Border", typeof(RectTransform));
        border.transform.SetParent(marketPanel.transform, false);
        Image borderImg = border.AddComponent<Image>();
        borderImg.color = Gold;
        borderImg.raycastTarget = false;
        FullStretch(border.GetComponent<RectTransform>(), 2, 2);

        MakeUIText(marketPanel.transform, "Title", 14, FontStyle.Bold,
                   Gold, new Vector2(0.02f, 0.85f), new Vector2(0.98f, 0.98f))
            .GetComponent<Text>().text = "BLACK MARKET";

        marketSecretsText = MakeUIText(marketPanel.transform, "Secrets", 10, FontStyle.Normal,
            Parchment, new Vector2(0.02f, 0.76f), new Vector2(0.98f, 0.84f)).GetComponent<Text>();
        MakeUIText(marketPanel.transform, "CurrencyNote", 9, FontStyle.Italic,
            new Color(0.7f, 0.9f, 0.5f), new Vector2(0.02f, 0.86f), new Vector2(0.98f, 0.91f))
            .GetComponent<Text>().text = "Costs SECRETS \u2014 use black market intel & sabotage";

        int cols = 3;
        int rows = 2;
        float slotW = 1f / cols;
        float slotH = 0.28f;
        float startX = 0.02f;
        float startY = 0.72f;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int idx = r * cols + c;
                if (idx >= 6) break;

                GameObject slotGO = new GameObject("MarketSlot_" + idx, typeof(RectTransform));
                slotGO.transform.SetParent(marketPanel.transform, false);
                Image slotBg = slotGO.AddComponent<Image>();
                Outline slotOutline = slotGO.AddComponent<Outline>();
                slotOutline.effectColor = new Color(0.55f, 0.40f, 0.08f, 0.7f);
                slotOutline.effectDistance = new Vector2(1, -1);
                float shade = (idx % 2 == 0) ? 0.07f : 0.10f;
                slotBg.color = new Color(shade, shade * 0.5f, shade * 0.2f);
                RectTransform sR = slotGO.GetComponent<RectTransform>();
                sR.anchorMin = new Vector2(startX + c * slotW + 0.005f, startY - (r + 1) * slotH - 0.01f);
                sR.anchorMax = new Vector2(startX + (c + 1) * slotW - 0.005f, startY - r * slotH + 0.01f);
                sR.sizeDelta = Vector2.zero;

                GameObject iconArea = new GameObject("IconArea", typeof(RectTransform));
                iconArea.transform.SetParent(slotGO.transform, false);
                Anchor(iconArea.GetComponent<RectTransform>(), 0.02f, 0.40f, 0.98f, 0.95f);

                Text iconText = MakeUIText(iconArea.transform, "IconText", 9, FontStyle.Bold,
                    Parchment, Vector2.zero, Vector2.one).GetComponent<Text>();
                iconText.alignment = TextAnchor.MiddleCenter;

                GameObject costArea = new GameObject("CostArea", typeof(RectTransform));
                costArea.transform.SetParent(slotGO.transform, false);
                Anchor(costArea.GetComponent<RectTransform>(), 0.02f, 0.01f, 0.98f, 0.38f);

                Text costText = MakeUIText(costArea.transform, "CostText", 8, FontStyle.Normal,
                    Parchment.WithA(0.85f), new Vector2(0f, 0.55f), new Vector2(1f, 1f)).GetComponent<Text>();
                costText.alignment = TextAnchor.LowerCenter;

                GameObject buyBtnGO = new GameObject("BuyBtn", typeof(RectTransform));
                buyBtnGO.transform.SetParent(costArea.transform, false);
                Image buyImg = buyBtnGO.AddComponent<Image>();
                buyImg.color = new Color(0.2f, 0.5f, 0.2f);
                Button buyBtn = buyBtnGO.AddComponent<Button>();
                buyBtn.targetGraphic = buyImg;
                Navigation nav = new Navigation();
                nav.mode = Navigation.Mode.None;
                buyBtn.navigation = nav;
                RectTransform buyR = buyBtnGO.GetComponent<RectTransform>();
                buyR.anchorMin = new Vector2(0.1f, 0f);
                buyR.anchorMax = new Vector2(0.9f, 0.45f);
                buyR.sizeDelta = Vector2.zero;

                Text buyLabel = MakeUIText(buyBtnGO.transform, "Label", 7, FontStyle.Bold,
                    Parchment, Vector2.zero, Vector2.one).GetComponent<Text>();
                buyLabel.text = "BUY";
                buyLabel.alignment = TextAnchor.MiddleCenter;

                GameObject soldStamp = new GameObject("SoldStamp", typeof(RectTransform));
                soldStamp.transform.SetParent(slotGO.transform, false);
                Text soldText = soldStamp.AddComponent<Text>();
                soldText.font = GetFont();
                soldText.fontSize = 16;
                soldText.fontStyle = FontStyle.Bold;
                soldText.color = new Color(0.8f, 0.1f, 0.1f, 0.7f);
                soldText.text = "SOLD";
                soldText.alignment = TextAnchor.MiddleCenter;
                RectTransform soldR = soldStamp.GetComponent<RectTransform>();
                soldR.anchorMin = Vector2.zero;
                soldR.anchorMax = Vector2.one;
                soldR.sizeDelta = Vector2.zero;
                soldStamp.SetActive(false);

                marketSlotCards.Add(slotGO);
            }
        }

        MakeUIText(marketPanel.transform, "RefreshNote", 9, FontStyle.Italic,
                   Parchment.WithA(0.4f), new Vector2(0.02f, 0f), new Vector2(0.98f, 0.035f))
            .GetComponent<Text>().text = "Market refreshes next turn";
    }

    public void RefreshMarketUI()
    {
        if (marketPanel == null || GameManager.Instance == null) return;

        FactionData player = GameManager.Instance.GetPlayerFaction();
        if (player == null) return;

        bool shouldShow = marketVisible && GameManager.Instance.currentState == GameState.PLAY_PHASE;
        marketPanel.SetActive(shouldShow);

        if (marketSecretsText != null)
            marketSecretsText.text = $"Secrets available: {player.secrets}";

        if (GameManager.Instance.currentMarket == null) return;

        for (int i = 0; i < marketSlotCards.Count; i++)
        {
            GameObject slotGO = marketSlotCards[i];
            if (slotGO == null) continue;

            if (i >= GameManager.Instance.currentMarket.Length)
            {
                slotGO.SetActive(false);
                continue;
            }

            BlackMarketSystem.MarketSlot slot = GameManager.Instance.currentMarket[i];
            if (slot == null) { slotGO.SetActive(false); continue; }

            slotGO.SetActive(true);
            int idx = i;

            Text iconText = slotGO.transform.Find("IconArea")?.Find("IconText")?.GetComponent<Text>();
            Text costText = slotGO.transform.Find("CostArea")?.Find("CostText")?.GetComponent<Text>();
            Transform buyBtnGO = slotGO.transform.Find("CostArea")?.Find("BuyBtn");
            Button buyBtn = buyBtnGO?.GetComponent<Button>();
            Text buyLabel = buyBtnGO?.Find("Label")?.GetComponent<Text>();
            GameObject soldStamp = slotGO.transform.Find("SoldStamp")?.gameObject;

            Image slotBg = slotGO.GetComponent<Image>();

            int effectiveCost = Mathf.RoundToInt(slot.secretsCost * player.marketDiscount);

            if (slot.purchased)
            {
                if (soldStamp != null) soldStamp.SetActive(true);
                if (buyBtnGO != null) buyBtnGO.gameObject.SetActive(false);
                if (costText != null) costText.text = "SOLD";
                if (iconText != null) iconText.text = "";
                continue;
            }

            if (soldStamp != null) soldStamp.SetActive(false);
            if (buyBtnGO != null) buyBtnGO.gameObject.SetActive(true);

            switch (slot.itemType)
            {
                case BlackMarketSystem.MarketItemType.CARD:
                    if (slotBg != null) slotBg.color = new Color(0.15f, 0.1f, 0.05f);
                    if (iconText != null && slot.card != null)
                        iconText.text = $"<color=#{GetTypeHexColor(slot.card.cardType)}>{slot.card.cardName}</color>\n({slot.card.cardType})";
                    break;
                case BlackMarketSystem.MarketItemType.TROOPS:
                    if (slotBg != null) slotBg.color = new Color(0.1f, 0.2f, 0.1f);
                    if (iconText != null) iconText.text = $"\u2694 +{slot.quantity} TROOPS";
                    if (iconText != null) iconText.color = new Color(0.2f, 0.7f, 0.3f);
                    break;
                case BlackMarketSystem.MarketItemType.INTEL:
                    if (slotBg != null) slotBg.color = new Color(0.15f, 0.12f, 0.05f);
                    if (iconText != null) iconText.text = "\uD83D\uDC41 INTEL";
                    if (iconText != null) iconText.color = Gold;
                    break;
                case BlackMarketSystem.MarketItemType.SABOTAGE_KIT:
                    if (slotBg != null) slotBg.color = new Color(0.2f, 0.08f, 0.08f);
                    if (iconText != null) iconText.text = "\uD83D\uDCA3 SABOTAGE";
                    if (iconText != null) iconText.color = new Color(0.8f, 0.2f, 0.1f);
                    break;
            }

            if (costText != null)
                costText.text = $"{effectiveCost} secrets";

            if (buyBtn != null)
            {
                bool affordable = player.secrets >= effectiveCost && !slot.purchased;
                Image buyImg = buyBtn.GetComponent<Image>();
                if (buyImg != null)
                    buyImg.color = affordable ? new Color(0.2f, 0.5f, 0.2f) : new Color(0.3f, 0.3f, 0.3f);
                buyBtn.interactable = affordable && !slot.purchased;

                buyBtn.onClick.RemoveAllListeners();
                buyBtn.onClick.AddListener(() =>
                {
                    if (slot.purchased) return;

                    if (slot.itemType == BlackMarketSystem.MarketItemType.TROOPS)
                    {
                        ShowTroopTargetSelector(idx);
                    }
                    else
                    {
                        if (BlackMarketSystem.PurchaseMarketSlot(player, slot))
                        {
                            StartCoroutine(GoldFlashAnimation(slotGO));
                            RefreshMarketUI();
                            UpdateAllFactions();
                            UpdatePlayerHandWithAnimation(player.hand);
                        }
                        else
                        {
                            StartCoroutine(ShakeAnimation(slotGO));
                        }
                    }
                });
            }

            if (buyLabel != null)
            {
                bool canAfford = player.secrets >= effectiveCost && !slot.purchased;
                if (slot.itemType == BlackMarketSystem.MarketItemType.CARD && player.hand.Count >= 7)
                    buyLabel.text = "FULL";
                else if (!canAfford)
                    buyLabel.text = "BROKE";
                else
                    buyLabel.text = "BUY";
            }
        }
    }

    void ToggleMarketPanel()
    {
        if (!CanUseActionPanels()) return;

        marketVisible = !marketVisible;
        if (marketVisible && EconomyUI.Instance != null)
            EconomyUI.Instance.Hide();
        marketPanel.SetActive(marketVisible);
        if (marketVisible)
        {
            RefreshMarketUI();
            marketBadge.SetActive(false);
        }
    }

    void OnMarketRefreshed()
    {
        RefreshMarketUI();
        CheckMarketBadge();
    }

    void OnGoldChanged(FactionData faction)
    {
        foreach (FactionPanelUI p in factionPanels)
            if (p.faction == faction) { p.UpdateStats(); break; }
        if (EconomyUI.Instance != null) EconomyUI.Instance.Refresh();
    }

    void ShowTroopTargetSelector(int slotIdx)
    {
        if (GameManager.Instance == null) return;
        FactionData player = GameManager.Instance.GetPlayerFaction();
        if (player == null) return;

        GameObject popup = new GameObject("TroopTargetPopup", typeof(RectTransform));
        popup.transform.SetParent(canvas.transform, false);
        Image popBg = popup.AddComponent<Image>();
        popBg.color = new Color(0, 0, 0, 0.9f);
        Anchor(popup.GetComponent<RectTransform>(), 0.25f, 0.25f, 0.75f, 0.75f);

        MakeUIText(popup.transform, "Title", 16, FontStyle.Bold,
                   Gold, new Vector2(0.05f, 0.80f), new Vector2(0.95f, 0.95f))
            .GetComponent<Text>().text = "Select target territory";

        float yPos = 0.70f;
        foreach (TerritoryData t in player.territories)
        {
            if (t == null) continue;
            Button terrBtn = MakeButton(popup.transform, "Terr_" + t.name.Replace(" ", ""),
                new Vector2(0, Screen.height * (yPos - 0.5f)),
                new Vector2(200, 30), $"{t.name} ({t.troops})", Med, Gold);
            TerritoryData captured = t;
            int capturedIdx = slotIdx;
            terrBtn.onClick.AddListener(() =>
            {
                if (GameManager.Instance.currentMarket != null &&
                    capturedIdx < GameManager.Instance.currentMarket.Length)
                {
                    BlackMarketSystem.MarketSlot slot = GameManager.Instance.currentMarket[capturedIdx];
                    if (BlackMarketSystem.PurchaseMarketSlot(player, slot, captured))
                    {
                        RefreshMarketUI();
                        UpdateAllFactions();
                        UpdateTerritoryMap();
                    }
                }
                Destroy(popup);
            });
            yPos -= 0.08f;
        }

        Button cancelBtn = MakeButton(popup.transform, "CancelBtn",
            new Vector2(0, -Screen.height * 0.3f),
            new Vector2(120, 35), "CANCEL", Dark, Med);
        cancelBtn.onClick.AddListener(() => Destroy(popup));
    }

    IEnumerator GoldFlashAnimation(GameObject slotGO)
    {
        if (slotGO == null) yield break;
        Image bg = slotGO.GetComponent<Image>();
        Color original = bg.color;
        bg.color = Gold;
        float elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            bg.color = Color.Lerp(Gold, original, elapsed / 0.3f);
            yield return null;
        }
        bg.color = original;
    }

    IEnumerator ShakeAnimation(GameObject slotGO)
    {
        if (slotGO == null) yield break;
        Vector3 original = slotGO.transform.localPosition;
        float elapsed = 0f;
        while (elapsed < 0.2f)
        {
            elapsed += Time.deltaTime;
            float offset = Mathf.Sin(elapsed * 60f) * 5f;
            slotGO.transform.localPosition = original + new Vector3(offset, 0, 0);
            yield return null;
        }
        slotGO.transform.localPosition = original;
    }

    Color GetTypeColor(CardType type)
    {
        switch (type)
        {
            case CardType.ATTACK: return new Color(0.8f, 0.2f, 0.2f);
            case CardType.DEFENSE: return new Color(0.2f, 0.4f, 0.9f);
            case CardType.INFLUENCE: return new Color(0.8f, 0.7f, 0.1f);
            case CardType.SABOTAGE: return new Color(0.5f, 0.1f, 0.7f);
            case CardType.FARMING: return new Color(0.2f, 0.7f, 0.3f);
            case CardType.BLACK_MARKET: return new Color(0.6f, 0.4f, 0.1f);
            default: return Color.gray;
        }
    }

    string GetTypeHexColor(CardType type)
    {
        Color c = GetTypeColor(type);
        return ColorUtility.ToHtmlStringRGB(c);
    }

    public void FlashLegendaryDraw()
    {
        StartCoroutine(LegendaryDrawRoutine());
    }

    IEnumerator LegendaryDrawRoutine()
    {
        Image flashImg = null;
        GameObject flashGO = new GameObject("LegendaryFlash", typeof(RectTransform));
        flashGO.transform.SetParent(canvas.transform, false);
        flashImg = flashGO.AddComponent<Image>();
        flashImg.color = new Color(1f, 0.84f, 0f, 0.3f);
        flashImg.raycastTarget = false;
        FullStretch(flashGO.GetComponent<RectTransform>());

        float elapsed = 0f;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            flashImg.color = new Color(1f, 0.84f, 0f, Mathf.Lerp(0.3f, 0f, elapsed / 0.5f));
            yield return null;
        }
        Destroy(flashGO);

        if (handCards.Count > 0)
        {
            GameObject legendLabel = new GameObject("LegendaryLabel", typeof(RectTransform));
            legendLabel.transform.SetParent(canvas.transform, false);
            Text lt = legendLabel.AddComponent<Text>();
            lt.font = GetFont();
            lt.fontSize = 24;
            lt.fontStyle = FontStyle.Bold;
            lt.color = new Color(1f, 0.84f, 0f);
            lt.text = "\u2726 LEGENDARY \u2726";
            lt.alignment = TextAnchor.MiddleCenter;
            RectTransform lr = legendLabel.GetComponent<RectTransform>();
            lr.anchorMin = new Vector2(0.3f, 0.1f);
            lr.anchorMax = new Vector2(0.7f, 0.18f);
            lr.sizeDelta = Vector2.zero;

            yield return new WaitForSeconds(1.5f);

            for (float f = 0; f < 0.3f; f += Time.deltaTime)
            {
                lt.color = new Color(1f, 0.84f, 0f, Mathf.Lerp(1f, 0f, f / 0.3f));
                yield return null;
            }
            Destroy(legendLabel);
        }

        yield return new WaitForSeconds(0.3f);

        if (handCards.Count > 0)
        {
            CardUIController lastCard = handCards[handCards.Count - 1];
            if (lastCard != null && lastCard.CurrentCard != null &&
                lastCard.CurrentCard.rarity == CardRarity.LEGENDARY)
            {
                for (int i = 0; i < 8; i++)
                {
                    GameObject particle = new GameObject("GoldParticle", typeof(RectTransform));
                    particle.transform.SetParent(canvas.transform, false);
                    Image pImg = particle.AddComponent<Image>();
                    pImg.color = new Color(1f, 0.84f, 0f);
                    pImg.raycastTarget = false;
                    RectTransform pR = particle.GetComponent<RectTransform>();
                    pR.sizeDelta = new Vector2(4, 4);

                    Vector3 cardPos = lastCard.transform.position;
                    pR.anchorMin = pR.anchorMax = new Vector2(0.5f, 0.5f);
                    pR.anchoredPosition = cardPos;

                    Vector2 dir = Random.insideUnitCircle.normalized * Random.Range(30f, 80f);
                    StartCoroutine(ParticleBurst(particle, dir));
                }
            }
        }
    }

    IEnumerator ParticleBurst(GameObject particle, Vector2 velocity)
    {
        if (particle == null) yield break;
        RectTransform pr = particle.GetComponent<RectTransform>();
        Image pImg = particle.GetComponent<Image>();
        float elapsed = 0f;
        Vector3 startPos = pr.anchoredPosition;
        while (elapsed < 0.5f && particle != null)
        {
            elapsed += Time.deltaTime;
            pr.anchoredPosition = startPos + (Vector3)(velocity * elapsed);
            if (pImg != null)
                pImg.color = new Color(1f, 0.84f, 0f, Mathf.Lerp(1f, 0f, elapsed / 0.5f));
            yield return null;
        }
        if (particle != null) Destroy(particle);
    }

    void UpdateSparklineChart(Color lineColor)
    {
        if (probabilityHistory.Count < 2) return;

        Transform pp = probabilityPanel != null ? probabilityPanel.transform : null;
        if (pp == null) return;
        Transform existing = pp.Find("SparklineChart");
        if (existing != null) Destroy(existing.gameObject);

        GameObject chartGO = new GameObject("SparklineChart", typeof(RectTransform));
        chartGO.transform.SetParent(pp, false);

        int w = 80;
        int h = 40;
        RectTransform chartR = chartGO.GetComponent<RectTransform>();
        chartR.anchorMin = new Vector2(0.02f, 0.01f);
        chartR.anchorMax = new Vector2(0.98f, 0.06f);
        chartR.sizeDelta = Vector2.zero;

        Texture2D tex = new Texture2D(w, h);
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                tex.SetPixel(x, y, new Color(0.04f, 0.02f, 0.01f));

        int count = probabilityHistory.Count;
        for (int i = 0; i < count - 1; i++)
        {
            float x1 = (float)i / (count - 1) * (w - 1);
            float x2 = (float)(i + 1) / (count - 1) * (w - 1);
            float y1 = (1f - probabilityHistory[i] / 100f) * (h - 1);
            float y2 = (1f - probabilityHistory[i + 1] / 100f) * (h - 1);
            DrawLine(tex, Mathf.RoundToInt(x1), Mathf.RoundToInt(y1),
                     Mathf.RoundToInt(x2), Mathf.RoundToInt(y2), lineColor);
        }
        tex.Apply();

        Image chartImg = chartGO.AddComponent<Image>();
        chartImg.sprite = Sprite.Create(tex, new Rect(0, 0, w, h), Vector2.zero);
        chartImg.raycastTarget = false;
    }

    void DrawLine(Texture2D tex, int x0, int y0, int x1, int y1, Color color)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            if (x0 >= 0 && x0 < tex.width && y0 >= 0 && y0 < tex.height)
                tex.SetPixel(x0, y0, color);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }

    public void SetMarketBadge(bool show, string text = "!")
    {
        if (marketBadge != null) marketBadge.SetActive(show);
        if (marketBadgeText != null) marketBadgeText.text = text;
    }

    public void CheckMarketBadge()
    {
        if (GameManager.Instance?.currentMarket == null || GameManager.Instance.GetPlayerFaction() == null)
        {
            SetMarketBadge(false);
            return;
        }

        FactionData player = GameManager.Instance.GetPlayerFaction();
        BlackMarketSystem.MarketSlot[] market = GameManager.Instance.currentMarket;
        bool showBadge = false;

        if (BlackMarketSystem.HasLegendaryAvailable(market))
            showBadge = true;

        if (!showBadge)
        {
            foreach (var slot in market)
            {
                if (slot == null || slot.purchased) continue;
                int effectiveCost = Mathf.RoundToInt(slot.secretsCost * player.marketDiscount);
                if (player.secrets >= effectiveCost)
                {
                    showBadge = true;
                    break;
                }
            }
        }

        SetMarketBadge(showBadge);
    }

    void UpdatePlayerHandWithAnimation(List<CardData> hand)
    {
        HideTooltip();

        int oldCount = handCards.Count;
        foreach (Transform t in handArea.transform) Destroy(t.gameObject);
        foreach (Transform t in playedArea.transform) Destroy(t.gameObject);
        handCards.Clear();
        UpdateCardsRemaining();

        List<CardData> sorted = new List<CardData>(hand);
        sorted.Sort((a, b) => SortOrder(a.cardType).CompareTo(SortOrder(b.cardType)));

        int newCardCount = 0;
        foreach (CardData card in sorted)
        {
            GameObject go = Instantiate(cardPrefab, handArea.transform);
            go.SetActive(true);
            go.name = card.cardName;
            CardUIController cc = go.GetComponent<CardUIController>();
            cc.BuildCard();
            Color fc = Color.gray;
            if (GameManager.Instance != null && card.factionId >= 0 && card.factionId < GameManager.Instance.factions.Count)
                fc = GameManager.Instance.factions[card.factionId].factionColor;
            cc.SetCard(card, fc);
            cc.SetInteractable(GameManager.Instance.currentState == GameState.PLAY_PHASE);
            CardData cap = card;
            cc.onCardClicked += () => OnHandCardClicked(cap);
            handCards.Add(cc);

            if (oldCount < hand.Count && newCardCount >= hand.Count - oldCount)
            {
                StartCoroutine(AnimateNewCard(go));
            }
            newCardCount++;
        }
        ApplyHandFanLayout();
    }

    IEnumerator AnimateNewCard(GameObject cardGO)
    {
        if (cardGO == null) yield break;
        CanvasGroup cg = cardGO.GetComponent<CanvasGroup>();
        if (cg == null) cg = cardGO.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cardGO.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

        float elapsed = 0f;
        float duration = 0.25f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            if (cg != null) cg.alpha = Mathf.Lerp(0f, 1f, t);
            cardGO.transform.localScale = Vector3.Lerp(new Vector3(0.5f, 0.5f, 1f), Vector3.one, t);
            yield return null;
        }
        if (cg != null) cg.alpha = 1f;
        cardGO.transform.localScale = Vector3.one;

        CardUIController cc = cardGO.GetComponent<CardUIController>();
        if (cc != null && cc.CurrentCard != null && cc.CurrentCard.rarity == CardRarity.LEGENDARY)
        {
            FlashLegendaryDraw();
        }
    }

    void PreGenerateCardSprites()
    {
        // Base card body: 200x280px, radius 16, dark fill + dim gold border
        cardBaseSprite = RoundedRectGenerator.Generate(200, 280, 16,
            new Color(0.08f, 0.05f, 0.02f),
            new Color(0.55f, 0.40f, 0.08f), 3);

        // Art frame: 180x120px, radius 10
        cardArtFrameSprite = RoundedRectGenerator.Generate(180, 120, 10,
            new Color(0.12f, 0.08f, 0.04f),
            new Color(0.40f, 0.30f, 0.05f), 2);

        // Rarity glow ring: 210x290px, radius 18, transparent fill + gold border
        cardRarityGlowSprite = RoundedRectGenerator.Generate(210, 290, 18,
            Color.clear,
            new Color(1f, 0.84f, 0f, 0.85f), 4);

        // Power badge circle: 24x24px, radius 12
        cardPowerBadgeSprite = RoundedRectGenerator.Generate(24, 24, 12,
            new Color(0.05f, 0.03f, 0.01f),
            new Color(0.83f, 0.65f, 0.22f), 2);
    }

    static Sprite GetPhaseCircleSprite()
    {
        if (phaseCircleSprite == null)
        {
            phaseCircleSprite = RoundedRectGenerator.Generate(64, 64, 32,
                Color.clear,
                Color.white,
                4);
        }
        return phaseCircleSprite;
    }

    void BuildCardPrefab()
    {
        cardPrefab = new GameObject("CardPrefab", typeof(RectTransform));
        cardPrefab.transform.SetParent(null);
        cardPrefab.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 116);
        LayoutElement le = cardPrefab.AddComponent<LayoutElement>();
        le.preferredWidth = 80f;
        le.preferredHeight = 116f;
        le.minWidth = 60f;
        le.minHeight = 90f;
        le.flexibleWidth = 0f;
        le.flexibleHeight = 0f;

        cardPrefab.AddComponent<CardUIController>();
        if (cardPrefab.GetComponent<CanvasGroup>() == null)
            cardPrefab.AddComponent<CanvasGroup>();
        cardPrefab.SetActive(false);
    }

    void BuildFactionPanelPrefab()
    {
        factionPanelPrefab = new GameObject("FactionPanelPrefab", typeof(RectTransform));
        factionPanelPrefab.transform.SetParent(null);
        factionPanelPrefab.AddComponent<CanvasGroup>();
        factionPanelPrefab.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 80);

        Image borderImg = MakeChild(factionPanelPrefab.transform, "BorderBg", Vector2.zero, Vector2.one, new Vector2(1, 1))
        .AddComponent<Image>();
        borderImg.color = Color.gray;
        Image bgImg = MakeChild(factionPanelPrefab.transform, "Bg", Vector2.zero, Vector2.one, new Vector2(-3, -3))
        .AddComponent<Image>();
        bgImg.color = new Color(0.10f, 0.055f, 0.02f, 0.97f);
        Image stripeImg = MakeChild(factionPanelPrefab.transform, "AccentStripe", new Vector2(0, 0.855f), Vector2.one, Vector2.zero)
        .AddComponent<Image>();
        stripeImg.color = Color.gray.WithA(0.9f);

        MakeHRule(factionPanelPrefab.transform, new Vector2(0.03f, 0.02f), new Vector2(0.97f, 0.045f), Gold.WithA(0.20f));

        Text nameText = MakeUIText(factionPanelPrefab.transform, "FactionName", 11, FontStyle.Bold,
                                   Parchment, new Vector2(0, 0.62f), new Vector2(0.68f, 0.86f)).GetComponent<Text>();
        Text shieldText = MakeUIText(factionPanelPrefab.transform, "StatsShield", 7, FontStyle.Normal,
                                     new Color(0.35f, 0.45f, 0.60f), new Vector2(0.70f, 0.65f), new Vector2(0.96f, 0.80f)).GetComponent<Text>();
        Text goldText = MakeUIText(factionPanelPrefab.transform, "GoldText", 8, FontStyle.Normal,
                                   new Color(0.9f, 0.75f, 0.1f), new Vector2(0.04f, 0.01f), new Vector2(0.62f, 0.14f)).GetComponent<Text>();

        Image powerBar, influenceBar, secretsBar;
        AddStatBar(factionPanelPrefab.transform, "PWR", new Color(0.75f, 0.25f, 0.15f), 0.43f, 0.57f, out powerBar);
        AddStatBar(factionPanelPrefab.transform, "INF", new Color(0.25f, 0.60f, 0.30f), 0.29f, 0.43f, out influenceBar);
        AddStatBar(factionPanelPrefab.transform, "SEC", new Color(0.55f, 0.40f, 0.10f), 0.15f, 0.29f, out secretsBar);

        GameObject statusBadgeGO = MakeChild(factionPanelPrefab.transform, "StatusBadge",
                                             new Vector2(0.65f, 0f), new Vector2(0.96f, 0.16f), Vector2.zero);
        Image sBadgeImg = statusBadgeGO.AddComponent<Image>();
        sBadgeImg.color = new Color(0.10f, 0.06f, 0.03f);
        sBadgeImg.raycastTarget = false;
        Text statusText = MakeUIText(statusBadgeGO.transform, "Status", 7, FontStyle.Bold,
                                     Parchment, Vector2.zero, Vector2.one).GetComponent<Text>();

        Button btn = factionPanelPrefab.AddComponent<Button>();
        btn.targetGraphic = bgImg;
        Navigation nav = new Navigation();
        nav.mode = Navigation.Mode.None;
        btn.navigation = nav;

        FactionPanelUI pui = factionPanelPrefab.AddComponent<FactionPanelUI>();
        pui.panelBackground = bgImg;
        pui.borderBg = borderImg;
        pui.accentStripe = stripeImg;
        pui.factionNameText = nameText;
        pui.statsPwrText = null;
        pui.statsInfText = null;
        pui.statsSecText = null;
        pui.statsShieldText = shieldText;
        pui.statsGoldText = goldText;
        pui.goldText = goldText;
        pui.statusText = statusText;
        pui.statusBadge = sBadgeImg;
        pui.powerBar = powerBar;
        pui.influenceBar = influenceBar;
        pui.secretsBar = secretsBar;
        factionPanelPrefab.SetActive(false);
    }

    void AddStatBar(Transform parent, string label, Color fillColor,
                    float yMin, float yMax, out Image fillBarOut)
    {
        Text lbl = MakeUIText(parent, label + "Lbl", 7, FontStyle.Bold, Parchment,
                              new Vector2(0.03f, yMin), new Vector2(0.18f, yMax)).GetComponent<Text>();
        lbl.text = label;
        lbl.alignment = TextAnchor.MiddleLeft;

        GameObject track = MakeChild(parent, label + "Track",
                                      new Vector2(0.20f, yMin + 0.01f), new Vector2(0.97f, yMax - 0.01f), Vector2.zero);
        track.AddComponent<Image>().color = new Color(0.06f, 0.03f, 0.01f, 1f);

        GameObject fill = new GameObject(label + "Fill", typeof(RectTransform));
        fill.transform.SetParent(track.transform, false);
        fillBarOut = fill.AddComponent<Image>();
        fillBarOut.color = fillColor;
        RectTransform fr = fill.GetComponent<RectTransform>();
        fr.anchorMin = Vector2.zero;
        fr.anchorMax = new Vector2(0.5f, 1f);
        fr.sizeDelta = Vector2.zero;
        fr.anchoredPosition = Vector2.zero;
    }

    GameObject MakeUIText(Transform parent, string name, int size, FontStyle style,
                          Color color, Vector2 aMin, Vector2 aMax, TextAnchor align = TextAnchor.MiddleCenter)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Text t = go.AddComponent<Text>();
        t.font = GetFont();
        t.fontSize = size;
        t.resizeTextForBestFit = false;
        t.fontStyle = style;
        t.alignment = align;
        t.color = color;
        t.raycastTarget = false;
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = aMin;
        r.anchorMax = aMax;
        r.sizeDelta = Vector2.zero;
        r.anchoredPosition = Vector2.zero;
        return go;
    }

    GameObject MakeChild(Transform parent, string name, Vector2 aMin, Vector2 aMax, Vector2 sizeDelta)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = aMin;
        r.anchorMax = aMax;
        r.sizeDelta = sizeDelta;
        r.anchoredPosition = Vector2.zero;
        return go;
    }

    GameObject MakeHRule(Transform parent, Vector2 aMin, Vector2 aMax, Color color)
    {
        GameObject go = MakeChild(parent, "HRule", aMin, aMax, Vector2.zero);
        go.AddComponent<Image>().color = color;
        return go;
    }

    public void FlashTerritory(string territoryName)
    {
        Vector2 uv = TerritoryGraphRenderer.GetTerritoryPosition(territoryName);
        if (uv == Vector2.zero) return;
        Vector2 anchor = new Vector2(
            Mathf.Lerp(MapAnchorMin.x, MapAnchorMax.x, Mathf.Clamp01(uv.x)),
            Mathf.Lerp(MapAnchorMin.y, MapAnchorMax.y, 1f - Mathf.Clamp01(uv.y)));
        StartCoroutine(PulseRingCoroutine(anchor.x, anchor.y));
    }

    IEnumerator PulseRingCoroutine(float ax, float ay)
    {
        GameObject ring = new GameObject("PulseRing", typeof(RectTransform));
        ring.transform.SetParent(canvas.transform, false);
        Image ringImg = ring.AddComponent<Image>();
        ringImg.color = new Color(0.83f, 0.65f, 0.22f, 0.8f);
        ringImg.raycastTarget = false;
        ringImg.type = Image.Type.Sliced;
        RectTransform rr = ring.GetComponent<RectTransform>();
        rr.anchorMin = new Vector2(ax, ay);
        rr.anchorMax = new Vector2(ax, ay);
        rr.sizeDelta = Vector2.zero;

        float duration = 1.2f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float size = Mathf.Lerp(4f, 40f, t);
            rr.sizeDelta = new Vector2(size, size);
            ringImg.color = new Color(0.83f, 0.65f, 0.22f, Mathf.Lerp(0.8f, 0f, t));
            yield return null;
        }
        Destroy(ring);
    }

    void FullStretch(RectTransform r, float w = 0, float h = 0)
    {
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.sizeDelta = new Vector2(w, h);
        r.anchoredPosition = Vector2.zero;
    }

    void Anchor(RectTransform r, float x0, float y0, float x1, float y1)
    {
        r.anchorMin = new Vector2(x0, y0);
        r.anchorMax = new Vector2(x1, y1);
        r.sizeDelta = Vector2.zero;
        r.anchoredPosition = Vector2.zero;
    }

    Button MakeButton(Transform parent, string name, Vector2 anchoredPos, Vector2 size, string label,
                      Color normal, Color highlighted)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = normal;
        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        Navigation nav = new Navigation();
        nav.mode = Navigation.Mode.None;
        btn.navigation = nav;
        ColorBlock cb = btn.colors;
        cb.normalColor = normal;
        cb.highlightedColor = highlighted;
        btn.colors = cb;
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.sizeDelta = size;
        r.anchoredPosition = anchoredPos;
        MakeUIText(go.transform, "Label", 20, FontStyle.Bold,
                   Parchment, Vector2.zero, Vector2.one).GetComponent<Text>().text = label;
        return btn;
    }

    static Font GetFont() => UIFont.Get();
}

public class TerritoryClickHandler : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public TerritoryData territory;
    public System.Action<TerritoryData> onRightClick;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            onRightClick?.Invoke(territory);
            return;
        }

        if (GameUIManager.Instance != null)
        {
            ProvinceData pvd = GameUIManager.Instance.FindProvinceAtScreenPoint(eventData.position);
            if (pvd != null) GameUIManager.Instance.OnProvinceClicked(pvd);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (territory == null || GameUIManager.Instance == null) return;
        GameUIManager.Instance.ShowTerritoryTooltip(territory);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (GameUIManager.Instance != null)
            GameUIManager.Instance.HideTerritoryTooltip();
    }

    static Font GetFont() => UIFont.Get();
}
