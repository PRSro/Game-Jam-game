using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FactionPanelUI : MonoBehaviour
{
    public Image panelBackground;
    public Image borderBg;
    public Image accentStripe;
    public Text factionNameText;
    public Text statsPwrText;
    public Text statsInfText;
    public Text statsSecText;
    public Text statsShieldText;
    public Text statsGoldText;
    public Text goldText;
    public Text statusText;
    public Image statusBadge;

    public Image powerBar;
    public Image influenceBar;
    public Image secretsBar;

    [HideInInspector] public FactionData faction;

    bool isHighlighted = false;
    Coroutine pulseCoroutine;
    Coroutine flashCoroutine;
    Coroutine dominantPulseCoroutine;
    CanvasGroup cg;
    Image damageOverlay;
    GameObject eliminatedOverlay;
    GameObject dominantBadge;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 1f;
        }

        damageOverlay = CreateDamageOverlay();
        eliminatedOverlay = CreateEliminatedOverlay();
        dominantBadge = CreateDominantBadge();
    }

    Image CreateDamageOverlay()
    {
        GameObject go = new GameObject("DamageOverlay", typeof(RectTransform));
        go.transform.SetParent(transform, false);
        Image img = go.AddComponent<Image>();
        img.color = new Color(0.8f, 0f, 0f, 0f);
        img.raycastTarget = false;
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.sizeDelta = Vector2.zero;
        r.anchoredPosition = Vector2.zero;
        return img;
    }

    GameObject CreateEliminatedOverlay()
    {
        GameObject root = new GameObject("EliminatedOverlay", typeof(RectTransform));
        root.transform.SetParent(transform, false);
        RectTransform rr = root.GetComponent<RectTransform>();
        rr.anchorMin = Vector2.zero; rr.anchorMax = Vector2.one;
        rr.sizeDelta = Vector2.zero;
        rr.anchoredPosition = Vector2.zero;
        root.SetActive(false);

        GameObject bg = new GameObject("Bg", typeof(RectTransform));
        bg.transform.SetParent(root.transform, false);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0f, 0f, 0f, 0.53f);
        bgImg.raycastTarget = false;
        RectTransform bgr = bg.GetComponent<RectTransform>();
        bgr.anchorMin = new Vector2(0.1f, 0.25f);
        bgr.anchorMax = new Vector2(0.9f, 0.55f);
        bgr.sizeDelta = Vector2.zero;

        GameObject text = new GameObject("Text", typeof(RectTransform));
        text.transform.SetParent(root.transform, false);
        Text t = text.AddComponent<Text>();
        t.font = GetFont();
        t.fontSize = 20;
        t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = new Color(0.8f, 0f, 0f);
        t.text = "ELIMINATED";
        RectTransform tr = text.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
        tr.sizeDelta = Vector2.zero;

        // Place overlay on top by setting as last sibling
        root.transform.SetAsLastSibling();
        return root;
    }

    GameObject CreateDominantBadge()
    {
        GameObject root = new GameObject("DominantBadge", typeof(RectTransform));
        root.transform.SetParent(transform, false);
        RectTransform rr = root.GetComponent<RectTransform>();
        rr.anchorMin = new Vector2(0.7f, 0.75f);
        rr.anchorMax = new Vector2(0.97f, 0.93f);
        rr.sizeDelta = Vector2.zero;
        root.SetActive(false);

        GameObject bg = new GameObject("Bg", typeof(RectTransform));
        bg.transform.SetParent(root.transform, false);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(1f, 0.84f, 0f, 0.85f);
        bgImg.raycastTarget = false;
        RectTransform bgr = bg.GetComponent<RectTransform>();
        bgr.anchorMin = Vector2.zero; bgr.anchorMax = Vector2.one;
        bgr.sizeDelta = Vector2.zero;

        GameObject text = new GameObject("Text", typeof(RectTransform));
        text.transform.SetParent(root.transform, false);
        Text t = text.AddComponent<Text>();
        t.font = GetFont();
        t.fontSize = 11;
        t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = new Color(0.04f, 0.04f, 0.1f);
        t.text = "DOMINANT";
        RectTransform tr = text.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
        tr.sizeDelta = Vector2.zero;

        root.transform.SetAsLastSibling();
        return root;
    }

    public void SetFaction(FactionData data)
    {
        faction = data;
        UpdateStats();

        if (factionNameText != null)
        {
            factionNameText.text = faction.factionName;
            factionNameText.color = Color.white;
        }

        if (borderBg != null)
        {
            borderBg.color = faction.factionColor;
        }

        if (accentStripe != null)
        {
            accentStripe.color = faction.factionColor;
        }
    }

    public void UpdateStats()
    {
        if (faction != null)
        {
            if (statsPwrText != null)
            {
                statsPwrText.text = $"PWR:{faction.power}/100";
                statsPwrText.color = new Color(0.75f, 0.35f, 0.25f);
            }
            if (statsInfText != null)
            {
                statsInfText.text = $"INF:{faction.influence}/100";
                statsInfText.color = new Color(1f, 0.84f, 0f);
            }
            if (statsSecText != null)
            {
                statsSecText.text = $"SEC:{faction.secrets}/50";
                statsSecText.color = new Color(0.60f, 0.35f, 0.40f);
            }
            if (statsShieldText != null)
            {
                if (faction.shieldPoints > 0)
                {
                    statsShieldText.text = $"SH:{faction.shieldPoints}";
                    statsShieldText.color = new Color(0.35f, 0.45f, 0.60f);
                    statsShieldText.gameObject.SetActive(true);
                }
                else
                {
                    statsShieldText.gameObject.SetActive(false);
                }
            }
            Text goldDisplay = goldText != null ? goldText : statsGoldText;
            if (goldDisplay != null)
            {
                goldDisplay.text = $"\u269C {faction.gold}g (+{faction.goldIncome})";
                goldDisplay.color = new Color(0.9f, 0.75f, 0.1f);
            }

            if (powerBar != null)
            {
                RectTransform r = powerBar.GetComponent<RectTransform>();
                r.anchorMax = new Vector2(faction.power / 100f, 1f);
            }
            if (influenceBar != null)
            {
                RectTransform r = influenceBar.GetComponent<RectTransform>();
                r.anchorMax = new Vector2(faction.influence / 100f, 1f);
            }
            if (secretsBar != null)
            {
                RectTransform r = secretsBar.GetComponent<RectTransform>();
                r.anchorMax = new Vector2(faction.secrets / 50f, 1f);
            }
        }

        if (statusText != null && faction != null)
        {
            if (faction.isEliminated)
            {
                statusText.text = "ELIMINATED";
                statusText.color = Color.red;
                statusText.gameObject.SetActive(true);
                if (statusBadge != null) statusBadge.color = new Color(0.5f, 0f, 0f);
            }
            else if (faction.isPlayerControlled)
            {
                statusText.text = "YOU";
                statusText.color = new Color(0.45f, 0.70f, 0.35f);
                statusText.gameObject.SetActive(true);
                if (statusBadge != null) statusBadge.color = new Color(0.08f, 0.18f, 0.04f);
            }
            else
            {
                statusText.text = AIController.GetPersonalityLabel(faction.factionId);
                statusText.color = new Color(0.85f, 0.80f, 0.70f);
                statusText.gameObject.SetActive(true);
                if (statusBadge != null) statusBadge.color = new Color(0.10f, 0.06f, 0.03f);
            }
        }

        if (dominantBadge != null)
        {
            bool isDominant = GameManager.Instance != null && GameManager.Instance.IsFactionDominant(faction);
            dominantBadge.SetActive(isDominant);
            if (isDominant && dominantPulseCoroutine == null)
                dominantPulseCoroutine = StartCoroutine(PulseDominant());
            else if (!isDominant && dominantPulseCoroutine != null)
            {
                StopCoroutine(dominantPulseCoroutine);
                dominantPulseCoroutine = null;
            }
        }
    }

    public void SetHighlight(bool highlight)
    {
        isHighlighted = highlight;

        if (highlight)
        {
            if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
            pulseCoroutine = StartCoroutine(PulseHighlight());
        }
        else
        {
            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
                pulseCoroutine = null;
            }
            if (borderBg != null)
                borderBg.color = faction.factionColor;
        }
    }

    IEnumerator PulseHighlight()
    {
        while (isHighlighted)
        {
            float t = Mathf.PingPong(Time.unscaledTime * 2f, 1f);
            Color c = Color.Lerp(Color.white, faction.factionColor, t);
            if (borderBg != null)
                borderBg.color = c;
            yield return null;
        }
    }

    IEnumerator PulseDominant()
    {
        Image badgeBg = dominantBadge?.GetComponentInChildren<Image>();
        while (dominantBadge != null && dominantBadge.activeSelf)
        {
            float t = Mathf.PingPong(Time.unscaledTime * 1.5f, 1f);
            if (badgeBg != null)
            {
                Color c = new Color(1f, 0.84f, 0f, Mathf.Lerp(0.6f, 1f, t));
                badgeBg.color = c;
            }
            yield return null;
        }
        dominantPulseCoroutine = null;
    }

    public void FlashDamage()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        if (damageOverlay == null)
            yield break;

        float elapsed = 0f;
        float fadeIn = 0.1f;
        float fadeOut = 0.2f;

        while (elapsed < fadeIn)
        {
            elapsed += Time.deltaTime;
            damageOverlay.color = new Color(0.8f, 0f, 0f, Mathf.Lerp(0f, 0.4f, elapsed / fadeIn));
            yield return null;
        }
        damageOverlay.color = new Color(0.8f, 0f, 0f, 0.4f);

        elapsed = 0f;
        while (elapsed < fadeOut)
        {
            elapsed += Time.deltaTime;
            damageOverlay.color = new Color(0.8f, 0f, 0f, Mathf.Lerp(0.4f, 0f, elapsed / fadeOut));
            yield return null;
        }
        damageOverlay.color = new Color(0.8f, 0f, 0f, 0f);
    }

    public void SetEliminatedVisuals()
    {
        if (faction != null && !faction.isPlayerControlled)
        {
            if (cg != null)
                cg.alpha = 0.35f;
            if (eliminatedOverlay != null)
                eliminatedOverlay.SetActive(true);
            if (statsPwrText != null)
            {
                statsPwrText.color = new Color(0.5f, 0.5f, 0.5f);
                statsInfText.color = new Color(0.5f, 0.5f, 0.5f);
                statsSecText.color = new Color(0.5f, 0.5f, 0.5f);
                if (statsShieldText != null) statsShieldText.color = new Color(0.5f, 0.5f, 0.5f);
            }
        }
    }

    public void ResetVisualState()
    {
        if (cg != null)
            cg.alpha = 1f;
        if (eliminatedOverlay != null)
            eliminatedOverlay.SetActive(false);
        if (dominantBadge != null)
            dominantBadge.SetActive(false);
        if (dominantPulseCoroutine != null)
        {
            StopCoroutine(dominantPulseCoroutine);
            dominantPulseCoroutine = null;
        }
        if (damageOverlay != null)
            damageOverlay.color = new Color(0.8f, 0f, 0f, 0f);
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }
        if (borderBg != null && faction != null)
            borderBg.color = faction.factionColor;
    }

    static Font GetFont() => UIFont.Get();
}
