using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardDeckController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float spreadOffset = 165f;
    public float spreadSpeed = 10f;
    
    private List<RectTransform> cardsRt = new List<RectTransform>();
    private List<CardController> cards = new List<CardController>();
    
    // Vector3 -> Vector2 (UI는 anchoredPosition)
    private Vector2[] defaultPositions;
    private Vector2[] targetPositions;

    public bool isSpread = false;
    
    private bool isAnimating = false;
    private float snapEpsilon = 0.5f; // 픽셀 단위 허용 오차
    
    // 추가: 스프레딩(펼치기/접기) 중인지 외부에 공개
    public bool isSpreading = false;

    // 추가: 도달 판정 오차(픽셀)
    private float reachEps = 0.5f;
    
    void Start()
    {
        // 자식 UI 카드 RectTransform 모으기
        foreach (Transform child in transform)
        {
            cards.Add(child.GetComponent<CardController>());
            if (child is RectTransform rt)
            {
                cardsRt.Add(rt);
            }
        }
        
        defaultPositions = new Vector2[cards.Count];
        targetPositions  = new Vector2[cards.Count];

        for (int i = 0; i < cards.Count; i++)
        {
            // position -> anchoredPosition, 그리고 시작부터 픽셀 스냅(정수화)
            Vector2 p = cardsRt[i].anchoredPosition;
            p = new Vector2(Mathf.Round(p.x), Mathf.Round(p.y));
            cardsRt[i].anchoredPosition = p;

            defaultPositions[i] = p;
            targetPositions[i] = p;
        }
    }

    private void Update()
    {
        int step = Mathf.Max(1, Mathf.RoundToInt(spreadSpeed * Time.deltaTime * 100f));

        for (int i = 0; i < cardsRt.Count; i++)
        {
            if (!cards[i].isDragging)
            {
                Vector2 cur = cardsRt[i].anchoredPosition;
                cur = new Vector2(Mathf.Round(cur.x), Mathf.Round(cur.y));

                Vector2 tar = targetPositions[i];
                tar = new Vector2(Mathf.Round(tar.x), Mathf.Round(tar.y));

                // Lerp 제거: MoveTowards로 정수 단위 이동 (소수점 지터 원천 차단)
                float nx = Mathf.MoveTowards(cur.x, tar.x, step);
                float ny = Mathf.MoveTowards(cur.y, tar.y, step);

                // 결과도 정수로 스냅
                cardsRt[i].anchoredPosition = new Vector2(Mathf.Round(nx), Mathf.Round(ny));
            }
        }
        
        // 추가: 다 펼쳐졌는지 체크해서 애니메이션 종료
        if (isSpread && isAnimating)
        {
            bool allReached = true;
            for (int i = 0; i < cardsRt.Count; i++)
            {
                if (cards[i].isDragging) continue;
                Vector2 cur = cardsRt[i].anchoredPosition;
                Vector2 tar = targetPositions[i];
                if ((cur - tar).sqrMagnitude > snapEpsilon * snapEpsilon)
                {
                    allReached = false;
                    break;
                }
            }
            if (allReached) isAnimating = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerEnter != null && eventData.pointerEnter.CompareTag("CardDeck"))
        {
            if (!isSpread)
            {
                float baseX = defaultPositions[0].x;
                for (int i = 0; i < cards.Count; i++)
                {
                    Vector2 t = defaultPositions[i];
                    t.x = baseX + i * spreadOffset;
                    targetPositions[i] = new Vector2(Mathf.Round(t.x), Mathf.Round(t.y));
                }
                isSpread = true;
                isAnimating = true;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isSpread)
        {
            if (isAnimating) return;
            
            for (int i = 0; i < cards.Count; i++)
            {
                targetPositions[i] = defaultPositions[i]; // 이미 정수
            }
            isSpread = false;
        }
    }
}
