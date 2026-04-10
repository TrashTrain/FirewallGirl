using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableWindow : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        // 켜질 때마다 최상위 캔버스를 찾음 (부모가 캔버스로 바뀌기 때문)
        canvas = GetComponentInParent<Canvas>().rootCanvas;
    }

    // 창을 클릭했을 때 화면 맨 앞으로 가져오기
    public void OnPointerDown(PointerEventData eventData)
    {
        rectTransform.SetAsLastSibling();
    }

    // 마우스 드래그 시 창 이동
    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }
}
