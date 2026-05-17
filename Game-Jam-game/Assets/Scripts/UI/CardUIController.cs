using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardUIController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public CardData CurrentCard { get; private set; }
    bool interactable = false;
    bool isSelected = false;
    CanvasGroup canvasGroup;

    public event System.Action onCardClicked;

    // Card layers (built in BuildCard)
    Image glowRing;
    Image cardBody;
    Image factionStripe;
    Image artFrame;
    Image artImage;
    Image typeBadge;
    Text typeBadgeText;
    Image powerBadge;
    Text powerBadgeText;
    Text cardNameText;
    Text descText;
    Text rarityStars;
    Image hoverOverlay;
    Image selectOverlay;

    // Fan layout state
    public float originalAngle;
    public Vector2 originalPosition;

    static readonly Color attackColor = new Color(0.75f, 0.10f, 0.10f);
    static readonly Color defenseColor = new Color(0.10f, 0.25f, 0.75f);
    static readonly Color influenceColor = new Color(0.75f, 0.65f, 0.05f);
    static readonly Color sabotageColor = new Color(0.40f, 0.05f, 0.60f);
    static readonly Color farmingColor = new Color(0.10f, 0.55f, 0.15f);
    static readonly Color blackMarketColor = new Color(0.45f, 0.25f, 0.05f);

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void BuildCard()
    {
        // Create each layer from bottom to top

        // [0] GlowRing
        GameObject glowGO = new GameObject("GlowRing", typeof(RectTransform));
        glowGO.transform.SetParent(transform, false);
        glowRing = glowGO.AddComponent<Image>();
        glowRing.raycastTarget = false;
        RectTransform glowR = glowGO.GetComponent<RectTransform>();
        glowR.anchorMin = new Vector2(-0.05f, -0.02f);
        glowR.anchorMax = new Vector2(1.05f, 1.02f);
        glowR.sizeDelta = Vector2.zero;
        glowR.anchoredPosition = Vector2.zero;

        // [1] CardBody
        GameObject bodyGO = new GameObject("CardBody", typeof(RectTransform));
        bodyGO.transform.SetParent(transform, false);
        cardBody = bodyGO.AddComponent<Image>();
        cardBody.raycastTarget = false;
        RectTransform bodyR = bodyGO.GetComponent<RectTransform>();
        bodyR.anchorMin = Vector2.zero;
        bodyR.anchorMax = Vector2.one;
        bodyR.sizeDelta = Vector2.zero;
        bodyR.anchoredPosition = Vector2.zero;

        // [2] FactionStripe
        GameObject stripeGO = new GameObject("FactionStripe", typeof(RectTransform));
        stripeGO.transform.SetParent(transform, false);
        factionStripe = stripeGO.AddComponent<Image>();
        factionStripe.raycastTarget = false;
        RectTransform stripeR = stripeGO.GetComponent<RectTransform>();
        stripeR.anchorMin = new Vector2(0f, 0.05f);
        stripeR.anchorMax = new Vector2(0.06f, 0.95f);
        stripeR.sizeDelta = Vector2.zero;
        stripeR.anchoredPosition = Vector2.zero;

        // [3] ArtFrame
        GameObject afGO = new GameObject("ArtFrame", typeof(RectTransform));
        afGO.transform.SetParent(transform, false);
        artFrame = afGO.AddComponent<Image>();
        artFrame.raycastTarget = false;
        RectTransform afR = afGO.GetComponent<RectTransform>();
        afR.anchorMin = new Vector2(0.05f, 0.38f);
        afR.anchorMax = new Vector2(0.95f, 0.78f);
        afR.sizeDelta = Vector2.zero;
        afR.anchoredPosition = Vector2.zero;

        // [4] ArtImage
        GameObject artGO = new GameObject("ArtImage", typeof(RectTransform));
        artGO.transform.SetParent(transform, false);
        artImage = artGO.AddComponent<Image>();
        artImage.raycastTarget = false;
        RectTransform artR = artGO.GetComponent<RectTransform>();
        artR.anchorMin = new Vector2(0.08f, 0.40f);
        artR.anchorMax = new Vector2(0.92f, 0.76f);
        artR.sizeDelta = Vector2.zero;
        artR.anchoredPosition = Vector2.zero;

        // [5] TypeBadge
        GameObject tbGO = new GameObject("TypeBadge", typeof(RectTransform));
        tbGO.transform.SetParent(transform, false);
        typeBadge = tbGO.AddComponent<Image>();
        typeBadge.raycastTarget = false;
        RectTransform tbR = tbGO.GetComponent<RectTransform>();
        tbR.anchorMin = new Vector2(0.05f, 0.80f);
        tbR.anchorMax = new Vector2(0.35f, 0.95f);
        tbR.sizeDelta = Vector2.zero;
        tbR.anchoredPosition = Vector2.zero;

        GameObject tbLabel = new GameObject("Label", typeof(RectTransform));
        tbLabel.transform.SetParent(tbGO.transform, false);
        typeBadgeText = tbLabel.AddComponent<Text>();
        typeBadgeText.font = GetFont();
        typeBadgeText.fontSize = 7;
        typeBadgeText.fontStyle = FontStyle.Bold;
        typeBadgeText.color = Color.white;
        typeBadgeText.alignment = TextAnchor.MiddleCenter;
        typeBadgeText.supportRichText = true;
        RectTransform tbLabelR = tbLabel.GetComponent<RectTransform>();
        tbLabelR.anchorMin = Vector2.zero;
        tbLabelR.anchorMax = Vector2.one;
        tbLabelR.sizeDelta = Vector2.zero;

        // [6] PowerBadge
        GameObject pbGO = new GameObject("PowerBadge", typeof(RectTransform));
        pbGO.transform.SetParent(transform, false);
        powerBadge = pbGO.AddComponent<Image>();
        powerBadge.raycastTarget = false;
        RectTransform pbR = pbGO.GetComponent<RectTransform>();
        pbR.anchorMin = new Vector2(0.68f, 0.02f);
        pbR.anchorMax = new Vector2(0.95f, 0.22f);
        pbR.sizeDelta = Vector2.zero;
        pbR.anchoredPosition = Vector2.zero;

        GameObject pbLabel = new GameObject("Label", typeof(RectTransform));
        pbLabel.transform.SetParent(pbGO.transform, false);
        powerBadgeText = pbLabel.AddComponent<Text>();
        powerBadgeText.font = GetFont();
        powerBadgeText.fontSize = 11;
        powerBadgeText.fontStyle = FontStyle.Bold;
        powerBadgeText.color = new Color(0.83f, 0.65f, 0.22f);
        powerBadgeText.alignment = TextAnchor.MiddleCenter;
        powerBadgeText.supportRichText = true;
        RectTransform pbLabelR = pbLabel.GetComponent<RectTransform>();
        pbLabelR.anchorMin = Vector2.zero;
        pbLabelR.anchorMax = Vector2.one;
        pbLabelR.sizeDelta = Vector2.zero;

        // [7] CardNameText
        GameObject nameGO = new GameObject("CardName", typeof(RectTransform));
        nameGO.transform.SetParent(transform, false);
        cardNameText = nameGO.AddComponent<Text>();
        cardNameText.font = GetFont();
        cardNameText.fontSize = 8;
        cardNameText.fontStyle = FontStyle.Bold;
        cardNameText.color = new Color(0.91f, 0.86f, 0.78f);
        cardNameText.alignment = TextAnchor.MiddleCenter;
        cardNameText.horizontalOverflow = HorizontalWrapMode.Wrap;
        cardNameText.verticalOverflow = VerticalWrapMode.Truncate;
        cardNameText.supportRichText = true;
        RectTransform nameR = nameGO.GetComponent<RectTransform>();
        nameR.anchorMin = new Vector2(0.05f, 0.25f);
        nameR.anchorMax = new Vector2(0.95f, 0.38f);
        nameR.sizeDelta = Vector2.zero;
        nameR.anchoredPosition = Vector2.zero;

        // [8] DescText
        GameObject descGO = new GameObject("DescText", typeof(RectTransform));
        descGO.transform.SetParent(transform, false);
        descText = descGO.AddComponent<Text>();
        descText.font = GetFont();
        descText.fontSize = 6;
        descText.fontStyle = FontStyle.Normal;
        descText.color = new Color(0.65f, 0.60f, 0.52f);
        descText.alignment = TextAnchor.UpperCenter;
        descText.horizontalOverflow = HorizontalWrapMode.Wrap;
        descText.verticalOverflow = VerticalWrapMode.Truncate;
        descText.supportRichText = true;
        RectTransform descR = descGO.GetComponent<RectTransform>();
        descR.anchorMin = new Vector2(0.05f, 0.05f);
        descR.anchorMax = new Vector2(0.95f, 0.24f);
        descR.sizeDelta = Vector2.zero;
        descR.anchoredPosition = Vector2.zero;

        // [9] RarityStars
        GameObject rsGO = new GameObject("RarityStars", typeof(RectTransform));
        rsGO.transform.SetParent(transform, false);
        rarityStars = rsGO.AddComponent<Text>();
        rarityStars.font = GetFont();
        rarityStars.fontSize = 8;
        rarityStars.fontStyle = FontStyle.Bold;
        rarityStars.alignment = TextAnchor.MiddleRight;
        rarityStars.supportRichText = true;
        RectTransform rsR = rsGO.GetComponent<RectTransform>();
        rsR.anchorMin = new Vector2(0.62f, 0.82f);
        rsR.anchorMax = new Vector2(0.95f, 0.96f);
        rsR.sizeDelta = Vector2.zero;
        rsR.anchoredPosition = Vector2.zero;

        // [10] HoverOverlay
        GameObject hoGO = new GameObject("HoverOverlay", typeof(RectTransform));
        hoGO.transform.SetParent(transform, false);
        hoverOverlay = hoGO.AddComponent<Image>();
        hoverOverlay.color = new Color(1f, 1f, 1f, 0f);
        hoverOverlay.raycastTarget = false;
        RectTransform hoR = hoGO.GetComponent<RectTransform>();
        hoR.anchorMin = Vector2.zero;
        hoR.anchorMax = Vector2.one;
        hoR.sizeDelta = Vector2.zero;
        hoR.anchoredPosition = Vector2.zero;

        // [11] SelectOverlay
        GameObject soGO = new GameObject("SelectOverlay", typeof(RectTransform));
        soGO.transform.SetParent(transform, false);
        selectOverlay = soGO.AddComponent<Image>();
        selectOverlay.color = new Color(0.83f, 0.65f, 0.22f, 0f);
        selectOverlay.raycastTarget = false;
        RectTransform soR = soGO.GetComponent<RectTransform>();
        soR.anchorMin = Vector2.zero;
        soR.anchorMax = Vector2.one;
        soR.sizeDelta = Vector2.zero;
        soR.anchoredPosition = Vector2.zero;

        GameObject hitArea = new GameObject("HitArea", typeof(RectTransform));
        hitArea.transform.SetParent(transform, false);
        Image hitImg = hitArea.AddComponent<Image>();
        hitImg.color = Color.clear;
        hitImg.raycastTarget = true;
        RectTransform hitR = hitArea.GetComponent<RectTransform>();
        hitR.anchorMin = Vector2.zero;
        hitR.anchorMax = Vector2.one;
        hitR.sizeDelta = Vector2.zero;
        hitR.anchoredPosition = Vector2.zero;
    }

    public void SetCard(CardData card, Color factionColor)
    {
        CurrentCard = card;

        Color typeColor = GetTypeColor(card.cardType);

        // [0] GlowRing rarity glow
        if (glowRing != null)
        {
            if (card.rarity == CardRarity.LEGENDARY || card.rarity == CardRarity.RARE)
            {
                glowRing.gameObject.SetActive(true);
                if (GameUIManager.Instance != null)
                {
                    if (card.rarity == CardRarity.LEGENDARY)
                        glowRing.sprite = GameUIManager.cardRarityGlowSprite;
                    else
                        glowRing.sprite = GameUIManager.cardRarityGlowSprite;
                }
                if (card.rarity == CardRarity.RARE)
                    glowRing.color = new Color(0.2f, 0.4f, 1.0f, 0.5f);
                else
                    glowRing.color = new Color(1.0f, 0.84f, 0f, 0.8f);

                if (card.rarity == CardRarity.LEGENDARY)
                    StartCoroutine(PulseGlow(glowRing));
            }
            else
            {
                glowRing.gameObject.SetActive(false);
            }
        }

        // [1] CardBody
        if (cardBody != null && GameUIManager.Instance != null)
        {
            cardBody.sprite = GameUIManager.cardBaseSprite;
            Color tint;
            switch (card.factionId)
            {
                case 0: tint = new Color(0.10f, 0.09f, 0.03f); break;
                case 1: tint = new Color(0.12f, 0.04f, 0.03f); break;
                case 2: tint = new Color(0.03f, 0.05f, 0.12f); break;
                case 3: tint = new Color(0.05f, 0.08f, 0.03f); break;
                default: tint = new Color(0.08f, 0.05f, 0.02f); break;
            }
            cardBody.color = tint;
        }

        // [2] FactionStripe
        if (factionStripe != null)
        {
            Color stripeColor = factionColor;
            stripeColor.a = 0.85f;
            factionStripe.color = stripeColor;
        }

        // [3] ArtFrame
        if (artFrame != null && GameUIManager.Instance != null)
        {
            artFrame.sprite = GameUIManager.cardArtFrameSprite;
            artFrame.color = new Color(0.15f, 0.10f, 0.05f);
        }

        // [4] ArtImage — load PNG or placeholder
        if (artImage != null)
        {
            Texture2D artTex = Resources.Load<Texture2D>(
                "Cards/Archive/" + card.cardId);
            if (artTex != null)
            {
                Sprite artSprite = Sprite.Create(artTex,
                    new Rect(0, 0, artTex.width, artTex.height),
                    new Vector2(0.5f, 0.5f), 100f);
                artImage.sprite = artSprite;
                artImage.preserveAspect = true;
                artImage.color = Color.white;
            }
            else
            {
                artImage.sprite = GeneratePlaceholderArt(card.cardType);
                artImage.preserveAspect = true;
                artImage.color = Color.white;
            }
        }

        // [5] TypeBadge
        if (typeBadge != null)
        {
            typeBadge.color = typeColor;
            if (typeBadgeText != null)
            {
                switch (card.cardType)
                {
                    case CardType.ATTACK: typeBadgeText.text = "ATK"; break;
                    case CardType.DEFENSE: typeBadgeText.text = "DEF"; break;
                    case CardType.INFLUENCE: typeBadgeText.text = "INF"; break;
                    case CardType.SABOTAGE: typeBadgeText.text = "SAB"; break;
                    case CardType.FARMING: typeBadgeText.text = "FRM"; break;
                    case CardType.BLACK_MARKET: typeBadgeText.text = "MKT"; break;
                }
            }
        }

        // [6] PowerBadge
        if (powerBadge != null && GameUIManager.Instance != null)
        {
            if (card.power == 0)
            {
                powerBadge.gameObject.SetActive(false);
            }
            else
            {
                powerBadge.gameObject.SetActive(true);
                powerBadge.sprite = GameUIManager.cardPowerBadgeSprite;
                powerBadge.color = Color.white;
                if (powerBadgeText != null)
                    powerBadgeText.text = card.power.ToString();
            }
        }

        // [7] CardNameText
        if (cardNameText != null)
            cardNameText.text = card.cardName;

        // [8] DescText
        if (descText != null)
        {
            if (!string.IsNullOrEmpty(card.description))
                descText.text = card.description.Length > 60
                    ? card.description.Substring(0, 60) + "..."
                    : card.description;
            else
            {
                switch (card.cardType)
                {
                    case CardType.ATTACK: descText.text = "Strike at your enemies"; break;
                    case CardType.DEFENSE: descText.text = "Fortify your holdings"; break;
                    case CardType.INFLUENCE: descText.text = "Bend minds, shift loyalty"; break;
                    case CardType.SABOTAGE: descText.text = "Disrupt and discard"; break;
                    case CardType.FARMING: descText.text = "Grow your strength"; break;
                    case CardType.BLACK_MARKET: descText.text = "Secrets for sale"; break;
                }
            }
        }

        // [9] RarityStars
        if (rarityStars != null)
        {
            switch (card.rarity)
            {
                case CardRarity.COMMON:
                    rarityStars.text = "\u00B7";
                    rarityStars.color = new Color(0.5f, 0.5f, 0.5f);
                    break;
                case CardRarity.UNCOMMON:
                    rarityStars.text = "\u2726";
                    rarityStars.color = new Color(0.2f, 0.8f, 0.3f);
                    break;
                case CardRarity.RARE:
                    rarityStars.text = "\u2726\u2726";
                    rarityStars.color = new Color(0.2f, 0.4f, 1.0f);
                    break;
                case CardRarity.LEGENDARY:
                    rarityStars.text = "\u2726\u2726\u2726";
                    rarityStars.color = new Color(1.0f, 0.84f, 0f);
                    break;
            }
        }
    }

    public void SetInteractable(bool value)
    {
        interactable = value;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = value ? 1f : 0.5f;
            canvasGroup.blocksRaycasts = value;
        }
    }

    Sprite GeneratePlaceholderArt(CardType type)
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Point;

        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;

        switch (type)
        {
            case CardType.ATTACK:
                // Red diagonal cross (sword shape)
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        if (Mathf.Abs(x - y) < 3 || Mathf.Abs(x - (size - 1 - y)) < 3)
                            pixels[y * size + x] = new Color(0.75f, 0.10f, 0.10f);
                        if (x > size / 3 && x < 2 * size / 3 && y > size / 4 && y < 3 * size / 4)
                            pixels[y * size + x] = new Color(0.75f, 0.10f, 0.10f);
                    }
                break;
            case CardType.DEFENSE:
                // Blue shield outline
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        int dx = x - size / 2;
                        float shieldTop = y * 1.8f - size * 0.4f;
                        float shieldBot = size - y;
                        if (shieldTop > Mathf.Abs(dx) * 0.8f && shieldBot > Mathf.Abs(dx) * 0.6f && Mathf.Abs(dx) > 3)
                            pixels[y * size + x] = new Color(0.10f, 0.25f, 0.75f);
                    }
                break;
            case CardType.INFLUENCE:
                // Gold eye (3 concentric ovals)
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        float dx = (x - size / 2f) / (size / 4f);
                        float dy = (y - size / 2f) / (size / 3f);
                        float d = dx * dx + dy * dy;
                        if ((d > 0.3f && d < 0.5f) || (d > 0.7f && d < 1.0f))
                            pixels[y * size + x] = new Color(0.75f, 0.65f, 0.05f);
                        if (d < 0.2f)
                            pixels[y * size + x] = new Color(0.75f, 0.65f, 0.05f);
                    }
                break;
            case CardType.SABOTAGE:
                // Purple flame silhouette
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        float dx = (x - size / 2f) / (size / 5f);
                        float dy = (y - size * 0.2f) / (size / 3f);
                        float flame = dy + 0.3f - dx * dx * 0.5f;
                        if (flame > 0.6f && flame < 1.2f && y > size / 4)
                            pixels[y * size + x] = new Color(0.40f, 0.05f, 0.60f);
                    }
                break;
            case CardType.FARMING:
                // Green wheat stalk
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        if (Mathf.Abs(x - size / 2) < 2 && y > size / 4)
                            pixels[y * size + x] = new Color(0.10f, 0.55f, 0.15f);
                        int stalkX = size / 2;
                        if (y > size * 0.6f && Mathf.Abs(x - stalkX) < 8 && (y % 8 < 3))
                            pixels[y * size + x] = new Color(0.10f, 0.55f, 0.15f);
                    }
                break;
            case CardType.BLACK_MARKET:
                // Brown coin stack
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        float dx = (x - size / 2f) / (size / 6f);
                        float dy = (y - size / 2f) / (size / 8f);
                        float d = dx * dx + dy * dy;
                        if (d < 1.0f)
                        {
                            int stack = y / (size / 3);
                            if (stack == 0 || stack == 1 || stack == 2)
                                pixels[y * size + x] = new Color(0.45f, 0.25f, 0.05f);
                        }
                    }
                break;
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!interactable) return;
        StopAllCoroutines();
        StartCoroutine(AnimateHover(true));

        // Snap rotation to 0, translate up, scale up
        RectTransform rt = GetComponent<RectTransform>();
        rt.localRotation = Quaternion.identity;
        rt.anchoredPosition = originalPosition + new Vector2(0, 15f);

        if (GameUIManager.Instance != null && CurrentCard != null)
        {
            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            Vector3 bottomCenter = new Vector3((corners[0].x + corners[3].x) * 0.5f, corners[0].y, 0f);
            GameUIManager.Instance.ShowTooltip(CurrentCard, bottomCenter);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(AnimateHover(false));

        // Restore fan rotation and position
        RectTransform rt = GetComponent<RectTransform>();
        rt.localRotation = Quaternion.Euler(0, 0, originalAngle);
        rt.anchoredPosition = originalPosition;

        isSelected = false;
        if (selectOverlay != null)
            selectOverlay.color = new Color(0.83f, 0.65f, 0.22f, 0f);

        if (GameUIManager.Instance != null)
            GameUIManager.Instance.HideTooltip();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!interactable) return;
        isSelected = !isSelected;
        if (selectOverlay != null)
            selectOverlay.color = isSelected
                ? new Color(0.83f, 0.65f, 0.22f, 0.20f)
                : new Color(0.83f, 0.65f, 0.22f, 0f);

        if (onCardClicked != null)
            onCardClicked.Invoke();
    }

    IEnumerator AnimateHover(bool entering)
    {
        float elapsed = 0f;
        float duration = 0.1f;
        Vector3 targetScale = entering ? Vector3.one * 1.15f : Vector3.one;
        Vector3 startScale = transform.localScale;
        float targetAlpha = entering ? 0.08f : 0f;
        float startAlpha = hoverOverlay != null ? hoverOverlay.color.a : 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            if (hoverOverlay != null)
            {
                Color c = hoverOverlay.color;
                c.a = Mathf.Lerp(startAlpha, targetAlpha, t);
                hoverOverlay.color = c;
            }
            yield return null;
        }
        transform.localScale = targetScale;
        if (hoverOverlay != null)
        {
            Color c = hoverOverlay.color;
            c.a = targetAlpha;
            hoverOverlay.color = c;
        }
    }

    IEnumerator PulseGlow(Image glow)
    {
        while (true)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * 1.5f;
                Color c = glow.color;
                c.a = Mathf.Lerp(0.4f, 0.9f, Mathf.Sin(t * Mathf.PI));
                glow.color = c;
                yield return null;
            }
        }
    }

    Color GetTypeColor(CardType type)
    {
        switch (type)
        {
            case CardType.ATTACK: return attackColor;
            case CardType.DEFENSE: return defenseColor;
            case CardType.INFLUENCE: return influenceColor;
            case CardType.SABOTAGE: return sabotageColor;
            case CardType.FARMING: return farmingColor;
            case CardType.BLACK_MARKET: return blackMarketColor;
            default: return Color.gray;
        }
    }

    static Font GetFont() => UIFont.Get();
}
