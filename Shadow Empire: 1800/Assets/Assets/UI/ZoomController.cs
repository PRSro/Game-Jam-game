using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ZoomController : MonoBehaviour, IScrollHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    const float MIN_DRAG_PIXELS = 12f;

    Canvas canvas;
    RectTransform targetRect;
    float minZoom = 0.5f;
    float maxZoom = 3f;
    float currentZoom = 1f;
    Vector2 dragStart;
    Vector2 panStart;
    bool isDragging = false;
    bool dragConfirmed = false;

    public void Setup(Canvas c, RectTransform target)
    {
        canvas = c;
        targetRect = target;
    }

    public void SetTarget(RectTransform t)
    {
        targetRect = t;
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (targetRect == null) return;
        float scrollDelta = eventData.scrollDelta.y * 0.1f;
        currentZoom = Mathf.Clamp(currentZoom + scrollDelta, minZoom, maxZoom);
        targetRect.localScale = Vector3.one * currentZoom;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (targetRect == null) return;
        isDragging = true;
        dragConfirmed = false;
        dragStart = eventData.position;
        panStart = targetRect.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || targetRect == null || canvas == null) return;
        if (!dragConfirmed)
        {
            if (Vector2.Distance(eventData.position, dragStart) < MIN_DRAG_PIXELS) return;
            dragConfirmed = true;
        }
        Vector2 delta = eventData.position - dragStart;
        float scaleFactor = canvas.scaleFactor;
        targetRect.anchoredPosition = panStart + delta / scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        dragConfirmed = false;
    }

    void Update()
    {
        if (Input.touchCount == 2 && targetRect != null)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);
            Vector2 prevT0 = t0.position - t0.deltaPosition;
            Vector2 prevT1 = t1.position - t1.deltaPosition;
            float prevDist = Vector2.Distance(prevT0, prevT1);
            float currDist = Vector2.Distance(t0.position, t1.position);
            float delta = (currDist - prevDist) * 0.005f;
            currentZoom = Mathf.Clamp(currentZoom + delta, minZoom, maxZoom);
            targetRect.localScale = Vector3.one * currentZoom;
        }
    }
}
