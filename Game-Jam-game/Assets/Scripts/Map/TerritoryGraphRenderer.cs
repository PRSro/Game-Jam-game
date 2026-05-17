using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Draws attack-route arrows on the map canvas using pooled Image GameObjects.
/// Attach to any persistent GameObject in the game scene; the arrow container
/// is created automatically under MapLayerController's NodeLayer.
/// </summary>
public class TerritoryGraphRenderer : MonoBehaviour
{
    public static TerritoryGraphRenderer Instance { get; private set; }

    // ── Inspector / public ───────────────────────────────────────────────────

    [Header("Arrow Settings")]
    [Tooltip("Width (height of the rotated rect) of a normal arrow in canvas units.")]
    public float arrowWidth = 7f;

    [Tooltip("Tint for arrows showing an adjacency the player could attack (dim).")]
    public Color attackColor  = new Color(1f, 0.25f, 0.10f, 0.45f);

    [Tooltip("Tint for the bright highlight arrows when a source territory is selected.")]
    public Color highlightColor = new Color(1f, 0.25f, 0.10f, 0.90f);

    [Tooltip("Tint for the confirmed pending-attack arrow.")]
    public Color pendingColor = new Color(1f, 0.85f, 0f, 1f);

    // ── State ────────────────────────────────────────────────────────────────

    public string highlightedSource { get; private set; }

    string pendingAttackSrcName;
    string pendingAttackTgtName;

    RectTransform arrowContainer;
    readonly List<GameObject> pooledArrows = new List<GameObject>();

    // ── Unity lifecycle ──────────────────────────────────────────────────────

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
        // Build the arrow container once MapLayerController is ready.
        // We do this via a coroutine so we don't race Start() order.
        StartCoroutine(BuildContainerWhenReady());
    }

    System.Collections.IEnumerator BuildContainerWhenReady()
    {
        // Wait until the NodeLayer exists.
        while (MapLayerController.Instance == null || MapLayerController.Instance.NodeLayer == null)
            yield return null;

        // Create a sibling RectTransform above the NodeLayer so arrows appear behind nodes.
        GameObject containerGO = new GameObject("ArrowContainer", typeof(RectTransform));
        containerGO.transform.SetParent(MapLayerController.Instance.NodeLayer.parent, false);
        containerGO.transform.SetAsFirstSibling(); // render behind nodes

        arrowContainer = containerGO.GetComponent<RectTransform>();
        // Full-stretch to cover the same area as the NodeLayer.
        arrowContainer.anchorMin = Vector2.zero;
        arrowContainer.anchorMax = Vector2.one;
        arrowContainer.sizeDelta = Vector2.zero;
        arrowContainer.anchoredPosition = Vector2.zero;

        // Now we can service any pending RedrawArrows call.
        RedrawArrows();
    }

    // ── Public API ───────────────────────────────────────────────────────────

    /// <summary>Redraws all contextual arrows for the current game state.</summary>
    public void RedrawArrows()
    {
        ClearArrows();
        if (arrowContainer == null || GameManager.Instance == null) return;

        GameState state = GameManager.Instance.currentState;

        if (state == GameState.ATTACK_PHASE)
        {
            // Do not draw all possible arrows because any territory can attack any other non-owned territory, 
            // which would result in a massive spiderweb of arrows.
        }

        // Override/add the pending attack arrow on top.
        if (!string.IsNullOrEmpty(pendingAttackSrcName) && !string.IsNullOrEmpty(pendingAttackTgtName))
            DrawArrow(pendingAttackSrcName, pendingAttackTgtName, pendingColor, arrowWidth * 1.5f);
    }

    /// <summary>
    /// Highlights all valid attack targets from <paramref name="source"/> using
    /// bright arrows. Pass null to clear the highlight.
    /// </summary>
    public void HighlightAttackOptions(TerritoryData source)
    {
        highlightedSource = source != null ? source.name : null;
        ClearArrows();
        // Do not highlight all non-owned territories to avoid clutter.
    }

    /// <summary>Draws (or redraws) the confirmed pending-attack arrow.</summary>
    public void ShowPendingAttack(string srcName, string tgtName)
    {
        pendingAttackSrcName = srcName;
        pendingAttackTgtName = tgtName;
        RedrawArrows();
    }

    /// <summary>Removes the pending-attack arrow and redraws the base layer.</summary>
    public void ClearPendingAttack()
    {
        pendingAttackSrcName = null;
        pendingAttackTgtName = null;
        ClearArrows();
    }

    /// <summary>Hides all arrows without changing stored state.</summary>
    public void ClearArrows()
    {
        foreach (GameObject go in pooledArrows)
            if (go != null) go.SetActive(false);
    }

    // ── Legacy stubs kept for binary compatibility ───────────────────────────

    public void CacheBaseGrid(Texture2D grid) { }

    public void ClearHighlight()
    {
        highlightedSource = null;
        ClearArrows();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    void DrawArrow(string fromName, string toName, Color color, float width)
    {
        if (arrowContainer == null) return;

        Vector2 from = GetCanvasAnchor(fromName);
        Vector2 to   = GetCanvasAnchor(toName);
        if (from == Vector2.zero && to == Vector2.zero) return;

        Vector2 dir   = to - from;
        float   len   = dir.magnitude;
        if (len < 1f) return; // degenerate

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // Shorten slightly so the arrow doesn't cover the node circles.
        float shorten = 28f; // canvas pixels
        if (len <= shorten * 2f) shorten = len * 0.25f;
        Vector2 unitDir = dir.normalized;
        Vector2 start   = from + unitDir * shorten;
        Vector2 end     = to   - unitDir * shorten;
        Vector2 mid     = (start + end) * 0.5f;
        float   bodyLen = (end - start).magnitude;

        // ── Body ──────────────────────────────────────────────────────────────
        GameObject bodyGO = GetPooledArrow();
        bodyGO.SetActive(true);
        ApplyArrowRect(bodyGO, mid, new Vector2(bodyLen, width), angle, color, false);

        // ── Arrowhead (a narrower triangle-ish rect at the tip) ───────────────
        float headLen = Mathf.Min(20f, bodyLen * 0.30f);
        Vector2 headPos = end - unitDir * headLen * 0.5f;

        GameObject headGO = GetPooledArrow();
        headGO.SetActive(true);
        // Taper the head by using a triangle if we had a sprite; since we're
        // using solid-color Images, just use a slightly wider & shorter rect.
        Color headColor = color;
        headColor.a = Mathf.Min(1f, color.a * 1.3f);
        ApplyArrowRect(headGO, headPos, new Vector2(headLen, width * 1.8f), angle, headColor, true);
    }

    void ApplyArrowRect(GameObject go, Vector2 anchoredPos, Vector2 sizeDelta, float angle, Color color, bool isHead)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        Image img        = go.GetComponent<Image>();

        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta        = sizeDelta;
        rt.localEulerAngles = new Vector3(0f, 0f, angle);

        img.color        = color;
        img.raycastTarget = false;

        // Give the head a slight alpha punch for visibility.
        if (isHead)
        {
            Color c = img.color;
            c.a = Mathf.Min(1f, c.a * 1.25f);
            img.color = c;
        }
    }

    GameObject GetPooledArrow()
    {
        foreach (GameObject go in pooledArrows)
            if (go != null && !go.activeSelf) return go;

        GameObject newArrow = new GameObject("Arrow", typeof(RectTransform), typeof(Image));
        newArrow.transform.SetParent(arrowContainer, false);
        pooledArrows.Add(newArrow);
        return newArrow;
    }

    // ── Static coordinate helpers ────────────────────────────────────────────

    public static Vector2 GetTerritoryPosition(string name)
    {
        TerritoryData territory = TerritoryDatabase.GetAllTerritories().Find(t => t.name == name);
        return territory != null ? territory.mapPosition : Vector2.zero;
    }

    /// <summary>
    /// Converts a territory's normalised map position into a canvas-space
    /// anchoredPosition relative to the centre of the arrow container
    /// (which is full-stretch, so we use the NodeLayer's world rect).
    /// </summary>
    public static Vector2 GetCanvasAnchor(string name)
    {
        TerritoryData territory = TerritoryDatabase.GetAllTerritories().Find(t => t.name == name);
        if (territory == null) return Vector2.zero;

        // Replicate the same mapping used by MapLayerController.BuildNodeRoots():
        //   canvasX = Lerp(0, 0.74,  mapPos.x)
        //   canvasY = Lerp(0.04, 0.97, 1 - mapPos.y)
        // We need the *pixel* offset from the container's centre pivot.
        // The container is full-stretch (anchorMin=0,0  anchorMax=1,1) so we
        // express the position as a fraction of the parent's rect, then ask
        // Unity to resolve it. But since we can't know the canvas size at
        // edit time, we store as a fraction and convert in DrawArrow using
        // the container's rect — here we just return the *anchor fraction*
        // as a placeholder that DrawArrow rescales.
        //
        // Simpler: we use the same lerp as BuildNodeRoots and express the
        // result as an anchoredPosition on a full-stretch rect, which means
        // anchoredPosition is relative to the container's lower-left corner
        // at runtime. Unity will resolve this correctly when sizeDelta == 0.

        // Return as normalised [0,1] x [0,1]; DrawArrow converts to pixels
        // by reading the container rect.
        float nx = Mathf.Lerp(MapLayerController.MapAnchorMin.x, MapLayerController.MapAnchorMax.x, territory.mapPosition.x);
        float ny = Mathf.Lerp(MapLayerController.MapAnchorMin.y, MapLayerController.MapAnchorMax.y, 1f - territory.mapPosition.y);

        // The container is full-stretch over the canvas root. We need the
        // arrow's anchoredPosition relative to the container's pivot (centre).
        // At runtime the canvas resolves rect sizes; we cache the container
        // rect once it's available.
        if (Instance != null && Instance.arrowContainer != null)
        {
            Rect r = Instance.arrowContainer.rect;
            // nx, ny are fractions of the canvas; the container maps [0,1] to
            // the full canvas width/height. So pixel offset from lower-left:
            float px = nx * r.width;
            float py = ny * r.height;
            // anchoredPosition is from the pivot (centre for pivot 0.5,0.5),
            // but our container pivot is default (0.5,0.5) so subtract half.
            return new Vector2(px - r.width * 0.5f, py - r.height * 0.5f);
        }

        // Fallback: raw normalised values (will be wrong scale but better than zero).
        return new Vector2(nx, ny);
    }
}
