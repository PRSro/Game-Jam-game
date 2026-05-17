using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttackPhaseUI : MonoBehaviour
{
    public static AttackPhaseUI Instance { get; private set; }

    readonly Dictionary<string, Button> buttons = new Dictionary<string, Button>();
    TerritoryData selectedSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += OnStateChanged;
            GameManager.Instance.OnCombatResolved += OnCombatResolved;
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= OnStateChanged;
            GameManager.Instance.OnCombatResolved -= OnCombatResolved;
        }
    }

    void OnStateChanged(GameState state)
    {
        if (state == GameState.ATTACK_PHASE)
            Show();
        else
            Hide();
    }

    void OnCombatResolved(CombatResult result, TerritoryData source, TerritoryData target)
    {
        Refresh();
    }

    public void Show()
    {
        EnsureNodes();
        MapLayerController.Instance?.PlaceForAttackPhase();
        selectedSource = null;
        GameManager gm = GameManager.Instance;
        if (gm != null)
        {
            gm.IsWaitingForAttackSelection = true;
            gm.pendingAttackSource = null;
            gm.pendingAttackTarget = null;
        }
        Refresh();
        TerritoryGraphRenderer.Instance?.RedrawArrows();
    }

    public void Hide()
    {
        selectedSource = null;
        MapLayerController.Instance?.PlaceBehindGameplayMap();
        GameManager gm = GameManager.Instance;
        if (gm != null)
        {
            gm.pendingAttackSource = null;
            gm.pendingAttackTarget = null;
            gm.IsWaitingForAttackSelection = false;
        }
        TerritoryGraphRenderer.Instance?.ClearPendingAttack();
        foreach (Button button in buttons.Values)
            if (button != null) button.gameObject.SetActive(false);
    }

    void EnsureNodes()
    {
        if (MapLayerController.Instance == null || MapLayerController.Instance.NodeLayer == null) return;

        foreach (TerritoryData territory in TerritoryDatabase.GetAllTerritories())
        {
            if (buttons.ContainsKey(territory.name)) continue;
            RectTransform nodeRoot = MapLayerController.Instance.GetNodeRoot(territory.name);
            if (nodeRoot == null) continue;

            GameObject buttonObject = new GameObject("AttackNodeUI", typeof(RectTransform));
            buttonObject.transform.SetParent(nodeRoot, false);

            Image hit = buttonObject.AddComponent<Image>();
            hit.color = Color.clear;
            hit.raycastTarget = true;

            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = hit;
            button.navigation = new Navigation { mode = Navigation.Mode.None };

            RectTransform rt = buttonObject.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(76f, 76f);
            rt.anchoredPosition = Vector2.zero;

            Image ring = CreateImage(buttonObject.transform, "Ring", new Vector2(70f, 70f));
            ring.sprite = CreateCircleSprite();
            ring.color = Color.clear;

            Image badge = CreateImage(buttonObject.transform, "Badge", new Vector2(30f, 20f));
            badge.color = new Color(0.18f, 0.18f, 0.18f, 0.86f);

            Text count = CreateText(badge.transform, "Count", 10, FontStyle.Bold, Color.white);
            count.alignment = TextAnchor.MiddleCenter;

            Text label = CreateText(buttonObject.transform, "Label", 8, FontStyle.Bold, new Color(0.93f, 0.86f, 0.70f, 0.9f));
            RectTransform labelRt = label.GetComponent<RectTransform>();
            labelRt.anchorMin = labelRt.anchorMax = new Vector2(0.5f, 1f);
            labelRt.pivot = new Vector2(0.5f, 0f);
            labelRt.sizeDelta = new Vector2(96f, 16f);
            labelRt.anchoredPosition = Vector2.zero;
            label.alignment = TextAnchor.UpperCenter;
            label.text = territory.name;

            string captured = territory.name;
            button.onClick.AddListener(() => OnNodeClicked(captured));
            buttons[territory.name] = button;
        }
    }

    Image CreateImage(Transform parent, string name, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Image image = go.AddComponent<Image>();
        image.raycastTarget = false;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = Vector2.zero;
        return image;
    }

    Text CreateText(Transform parent, string name, int size, FontStyle style, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Text text = go.AddComponent<Text>();
        text.font = UIFont.Get();
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.raycastTarget = false;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        return text;
    }

    void OnNodeClicked(string territoryName)
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || gm.currentState != GameState.ATTACK_PHASE || !gm.IsWaitingForAttackSelection) return;

        TerritoryData territory = gm.territories.Find(t => t.name == territoryName);
        FactionData player = gm.GetPlayerFaction();
        if (territory == null || player == null) return;

        if (selectedSource == null)
        {
            if (territory.controlledBy == player.factionId && territory.troops > 1 && GetValidTargets(territory).Count > 0)
            {
                selectedSource = territory;
                gm.pendingAttackSource = territory;
                gm.LogMessage($"Attack phase: selected {territory.name}. Choose one of its two attack routes.");
                // Highlight valid attack routes with bright arrows.
                TerritoryGraphRenderer.Instance?.HighlightAttackOptions(territory);
            }
            else
            {
                gm.LogMessage("Select one of your territories with 2+ troops and a valid attack route.");
            }
            Refresh();
            return;
        }

        if (territory == selectedSource)
        {
            selectedSource = null;
            gm.pendingAttackSource = null;
            TerritoryGraphRenderer.Instance?.HighlightAttackOptions(null);
            Refresh();
            return;
        }

        if (territory.controlledBy != player.factionId)
        {
            gm.pendingAttackTarget = territory;
            gm.QueueTerritoryAttack(null, player, selectedSource, territory);
            Debug.Log($"[AttackPhaseUI] Attack enqueued: {selectedSource.name} -> {territory.name}");
            // Show the confirmed pending-attack arrow before hiding the UI.
            TerritoryGraphRenderer.Instance?.ShowPendingAttack(selectedSource.name, territory.name);
            gm.IsWaitingForAttackSelection = false;
            gm.LogMessage($"Attack phase: {selectedSource.name} attacks {territory.name}.");
            Hide();
        }
        else
        {
            gm.LogMessage("You cannot attack your own territory.");
        }
    }

    void Refresh()
    {
        EnsureNodes();
        GameManager gm = GameManager.Instance;
        if (gm == null || gm.currentState != GameState.ATTACK_PHASE) return;

        FactionData player = gm.GetPlayerFaction();
        foreach (var kvp in buttons)
        {
            Button button = kvp.Value;
            if (button == null) continue;
            button.gameObject.SetActive(true);

            TerritoryData territory = gm.territories.Find(t => t.name == kvp.Key);
            if (territory == null) continue;

            Transform badge = button.transform.Find("Badge");
            Transform ring = button.transform.Find("Ring");
            Text count = badge != null ? badge.GetComponentInChildren<Text>() : null;
            if (count != null) count.text = territory.troops > 0 ? territory.troops.ToString() : "-";

            Color ownerColor = territory.controlledBy >= 0 && territory.controlledBy < gm.factions.Count
                ? gm.factions[territory.controlledBy].factionColor
                : Color.gray;

            Image badgeImage = badge != null ? badge.GetComponent<Image>() : null;
            if (badgeImage != null)
                badgeImage.color = new Color(ownerColor.r * 0.62f, ownerColor.g * 0.62f, ownerColor.b * 0.62f, 0.92f);

            Image ringImage = ring != null ? ring.GetComponent<Image>() : null;
            if (ringImage == null) continue;

            List<string> attackNeighbours = selectedSource != null
                ? ProvinceGraph.GetAttackNeighbours(selectedSource.name)
                : new List<string>();
            bool validSource = player != null && territory.controlledBy == player.factionId && territory.troops > 1 && GetValidTargets(territory).Count > 0;
            bool validTarget = player != null && selectedSource != null &&
                               territory.controlledBy != player.factionId &&
                               attackNeighbours.Contains(territory.name);

            if (selectedSource == territory)
                ringImage.color = new Color(0.10f, 0.55f, 0.95f, 0.9f);
            else if (validTarget)
                ringImage.color = new Color(1.0f, 0.25f, 0.10f, 0.9f);
            else if (selectedSource == null && validSource)
                ringImage.color = new Color(0.95f, 0.70f, 0.15f, 0.85f);
            else
                ringImage.color = Color.clear;
        }
    }

    List<TerritoryData> GetValidTargets(TerritoryData source)
    {
        List<TerritoryData> targets = new List<TerritoryData>();
        if (source == null || GameManager.Instance == null) return targets;

        List<string> neighbours = ProvinceGraph.GetAttackNeighbours(source.name);
        foreach (TerritoryData target in GameManager.Instance.territories)
        {
            if (target != null &&
                target.controlledBy != source.controlledBy &&
                neighbours.Contains(target.name))
            {
                targets.Add(target);
            }
        }
        return targets;
    }

    Sprite CreateCircleSprite()
    {
        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float outer = size * 0.47f;
        float inner = size * 0.39f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                pixels[y * size + x] = distance <= outer && distance >= inner ? Color.white : Color.clear;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
