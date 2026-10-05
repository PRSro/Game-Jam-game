using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class GameUIManager : MonoBehaviour
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

    public static void ResetStaticState()
    {
        cardBaseSprite = null;
        cardArtFrameSprite = null;
        cardRarityGlowSprite = null;
        cardPowerBadgeSprite = null;
        phaseCircleSprite = null;
        _lastSidebarRegion = null;
    }

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





    // ADDED: UPGRADE 2 - show dice choice popup after source selection




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









    /// <summary>Converts a screen-space click position to normalized UV within the map area.</summary>

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

    // FIXED: UI FIX 3 - moved from center-screen to right panel area; confirm button always visible

    // FIXED: UI FIX 3 - confirm button always visible with dynamic count text









    // ADDED: UPGRADE 3 - confirm fortify with selected troop count



    // ADDED: UPGRADE 4 - continent/region bonus display panel






    // ADDED: UPGRADE 2 - dice choice popup for attack count selection












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












































    public void AddLogMessage(string msg) => turnLog?.AddEntry(msg);






















    // FIXED: UI FIX 5 - game over panel tinted with winner's faction color + territory count
















































}

