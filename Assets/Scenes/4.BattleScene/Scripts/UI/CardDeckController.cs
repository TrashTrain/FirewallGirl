using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardDeckController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float spreadOffset = 5f;
    public float spreadSpeed = 10f;
    
    private List<RectTransform> cardsRt = new List<RectTransform>();
    private List<CardController> cards = new List<CardController>();
    private Vector3[] defaultPositions;
    private Vector3[] targetPositions;

    public bool isSpread = false;
    
    float snapThreshold = 0.01f;
    
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

        // 기본 위치 저장
        defaultPositions = new Vector3[cards.Count];
        targetPositions = new Vector3[cards.Count];
        
        for (int i = 0; i < cards.Count; i++)
        {
            defaultPositions[i] = cardsRt[i].position;
            targetPositions[i] = defaultPositions[i];
            // print(defaultPositions[i]);
        }
    }

    private void Update()
    {
        for (int i = 0; i < cardsRt.Count; i++)
        {
            if (!cards[i].isDragging)
            {
                cardsRt[i].position = Vector3.Lerp(cardsRt[i].position, targetPositions[i], Time.deltaTime * spreadSpeed);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerEnter != null && eventData.pointerEnter.CompareTag("CardDeck"))
        {
            if (!isSpread)
            {
                for (int i = 0; i < cards.Count; i++)
                {
                    targetPositions[i] = defaultPositions[i] + new Vector3(i * spreadOffset, 0f, 0f);
                }
                isSpread = true;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // print("exit");
        if (isSpread)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                targetPositions[i] = defaultPositions[i];
            }
            isSpread = false;
        }
    }
}
