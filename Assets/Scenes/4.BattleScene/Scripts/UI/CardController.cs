using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public float floatOffset = 100f;
    public float floatSpeed = 10f;
    private float scaleUpFactor = 1.5f;
    private float scaleSpeed = 10f;

    private Vector3 defaultPosition;
    private Vector3 targetPosition;
    private Vector3 defaultScale;
    private Vector3 targetScale;
    private Vector2 dragOffset;
    private float cardRotation;
    
    private RectTransform rect;

    private bool isFloating = false;
    public bool isDragging = false;

    GameObject cardDeck;
    private CardDeckController  cardDeckController;

    public static GameObject card;
    public Canvas canvas;

    [HideInInspector] public Transform startParent;
    
    // 추가: 원래 렌더 순서(형제 인덱스) 저장
    private int originalSiblingIndex;
    
    void Start()
    {
        cardDeck = GameObject.Find("CardPanel");
        cardDeckController = cardDeck.GetComponent<CardDeckController>();
        rect = GetComponent<RectTransform>();
        
        defaultPosition = transform.position;
        // defaultPosition = rect.anchoredPosition;
        // Debug.Log(transform.name + " 시작 위치: " + defaultPosition);
        targetPosition = defaultPosition;
        
        // 스케일 초기값 저장
        defaultScale = transform.localScale;
        targetScale = defaultScale;
        
        // 추가: 시작 시 원래 sibling index 저장
        originalSiblingIndex = transform.GetSiblingIndex();

        cardRotation = transform.localEulerAngles.z;
    }

    private void Update()
    {
        if (!isDragging)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * floatSpeed);
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (cardDeckController == null) return;
        if (!cardDeckController.isSpread) return;
        if (eventData.pointerEnter != null && 
            eventData.pointerEnter.CompareTag("Card"))
        {
            if (!isFloating)
            {
                // 추가: 원래 순서 저장 후 맨 위로 올리기
                originalSiblingIndex = transform.GetSiblingIndex();
                transform.SetAsLastSibling();
                
                targetPosition = defaultPosition + new Vector3(0f, floatOffset, 0f);
                targetScale = defaultScale * scaleUpFactor;
                
                var e = transform.localEulerAngles;
                e.z = 0f;
                transform.localEulerAngles = e;
                
                isFloating = true;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isDragging) return;
        
        if (isFloating)
        {
            targetPosition = defaultPosition;
            targetScale = defaultScale;
            var e = transform.localEulerAngles;
            e.z = cardRotation;
            transform.localEulerAngles = e;
            
            isFloating = false;
            
            // 추가: 원래 순서로 복구
            transform.SetSiblingIndex(originalSiblingIndex);
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isFloating) return;

        isDragging = true;
        card = gameObject;
        startParent = transform.parent;
        
        // 드래그 시 카드 회전을 0으로 고정
        var e = transform.localEulerAngles;
        e.z = 0f;
        transform.localEulerAngles = e;
        
        // 드래그 시작 시 스케일 복원
        targetScale = defaultScale;
        transform.localScale = defaultScale;
        
        // 드래그 중에도 항상 맨 위 유지
        transform.SetAsLastSibling();
        
        // 드래그 오프셋 계산: "카드 위치 - 포인터 위치"
        RectTransform parentRect = rect.parent as RectTransform;
        Camera cam = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect, eventData.position, cam, out Vector2 pointerLocal))
        {
            // dragOffset = rect.anchoredPosition - pointerLocal;
            dragOffset = Vector2.zero;
            rect.anchoredPosition = pointerLocal;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransform parentRect = rect.parent as RectTransform;
        Camera cam = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect, eventData.position, cam, out Vector2 pointerLocal))
        {
            rect.anchoredPosition = pointerLocal + dragOffset;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(startParent); // Parent 재설정
        
        // Raycast
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(eventData.position);
        Vector2 rayOrigin = new Vector2(worldPoint.x, worldPoint.y);

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.zero);
        
        // 캐릭터(player/enemy) 판별 로직
        if (hit.collider != null)
        {
            GameObject hitObj = hit.collider.gameObject;
            
            if (hitObj.CompareTag("Player"))
            {
                var playerManager = hitObj.GetComponent<PlayerManager>();

                if (playerManager != null && playerManager.currentCost > 0)
                {
                    Debug.Log($"{hitObj.name}의 현재 체력: {playerManager.currentHP}");
                    int AP = transform.GetComponent<PlayerCard>().ap;
                    int DP = transform.GetComponent<PlayerCard>().dp;
                    int cost = transform.GetComponent<PlayerCard>().cost;
                    // int cardAP = int.Parse(card.transform.Find("AttackPower/Text").GetComponent<TextMeshProUGUI>().text);
                    // int cardDP = int.Parse(card.transform.Find("Defense/Text").GetComponent<TextMeshProUGUI>().text);
                    // playerManager.attackPower += cardAP;
                    // playerManager.defensePower += cardDP;

                    if (playerManager.currentCost < cost)
                    {
                        // [TODO] current cost에 따라 사용 가능한 카드만 하이라이트 효과 적용
                        Debug.Log("현재 코스트보다 큰 카드는 사용 불가");
                    }
                    else
                    {
                        // [TODO] 함수화 & 카드 속성에 따라 다르게 적용
                        playerManager.attackPower += AP;
                        playerManager.defensePower += DP;
                        playerManager.currentCost = Mathf.Max(0, playerManager.currentCost - cost);
                        playerManager.UpdateUI();
                    }
                }
                else
                {
                    Debug.Log("코스트=0으로 사용 불가");
                }
            }
        }
        else
        {
            Debug.Log("아무 캐릭터 위에 놓이지 않았습니다.");
        }
        
        // 해당 캐릭터의 manager 호출
        isDragging = false;
        rect.anchoredPosition = defaultPosition;
        
        var e = transform.localEulerAngles;
        e.z = cardRotation;
        transform.localEulerAngles = e;
        
        // 추가: 드래그 종료 후에도 (아직 floating이면) 위에, floating이 해제된 상태면 원래 순서로 복귀
        if (!isFloating)
        {
            transform.SetSiblingIndex(originalSiblingIndex);
        }
        else
        {
            transform.SetAsLastSibling();
        }
    }
}
