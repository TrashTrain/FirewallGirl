using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public enum CardMode { Battle, DeckBuilding }

    [Header("Card Mode")]
    public CardMode currentMode = CardMode.Battle;

    [Header("Deck Building Settings")]
    public bool isSelected = false;
    public bool isClone = false;
    public CardController originalCard;
    private DeckManager deckManager;
    public float hoverScaleFactor = 1.2f; // 마우스 올렸을 때 확대 배율 (기존 크기 대비)
    private Vector3 originScale;
    private bool isHoveringDeckBuilder = false;
    private bool isInteractable = true;
    
    [Header("Battle Settings")]
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
    private CardDeckController cardDeckController;
    private PlayerCard playerCard;

    public static GameObject card;
    [SerializeField]
    private Canvas canvas;
    private Image background;
    private Color bgOriginColor;

    [HideInInspector] public Transform startParent;
    
    // 추가: 원래 렌더 순서(형제 인덱스) 저장
    private int originalSiblingIndex;
    
    public void SetScale(float scaleValue)
    {
        // 1. 본래 크기 갱신
        originScale = Vector3.one * scaleValue;
        
        // 2. 현재 마우스가 올라가 있지 않다면 즉시 적용
        // (만약 마우스가 올라가 있는 상태라면, OnPointerExit 때 이 값으로 돌아감)
        if (!isHoveringDeckBuilder) 
        {
            transform.localScale = originScale;
        }
    }
    
    void Start()
    {
        rect = GetComponent<RectTransform>();
        playerCard = GetComponent<PlayerCard>();

        canvas = GetComponentInParent<Canvas>();
        canvas.overrideSorting = false;

        if (currentMode == CardMode.Battle)
        {
            cardDeck = GameObject.Find("CardPanel");
            if (cardDeck != null)
            {
                cardDeckController = cardDeck.GetComponent<CardDeckController>();
            }
        }
        else if (currentMode == CardMode.DeckBuilding)
        {
            deckManager = FindObjectOfType<DeckManager>();
        }
        
        Transform bgTr = transform.Find("Background");
        if (bgTr != null)
        {
            background = bgTr.GetComponent<Image>();
            if (background != null)
            {
                bgOriginColor = background.color;
            }
        }
        
        defaultPosition = transform.position;
        targetPosition = defaultPosition;
        
        // 스케일 초기값 저장
        defaultScale = transform.localScale;
        targetScale = defaultScale;
        
        // 추가: 시작 시 원래 sibling index 저장
        originalSiblingIndex = transform.GetSiblingIndex();
        cardRotation = transform.localEulerAngles.z;
        
        if (originScale == Vector3.zero)
        {
            originScale = transform.localScale;
        }
    }

    // 덱 빌딩 모드 초기화용 함수 (DeckManager에서 카드 생성 직후 호출)
    public void SetupForDeckBuilding(DeckManager manager)
    {
        currentMode = CardMode.DeckBuilding;
        deckManager = manager;
    }

    private void Update()
    {
        if (currentMode == CardMode.Battle && !isDragging)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * floatSpeed);
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
        }

        if (currentMode == CardMode.Battle && PlayerManager.instance != null && playerCard != null)
        {
            bool isUsable = (PlayerManager.instance.currentCost >= playerCard.cost) &&
                            (playerCard.currentCoolTime == 0);
            SetCardUsableVisual(isUsable);
        }
    }
    
    private void SetCardUsableVisual(bool usable)
    {
        if (background == null) return;

        if (usable)
        {
            background.color = bgOriginColor;
            
        }
        else
        {
            Color color = Color.gray;
            color.a = 0.7f;
            background.color = color;
        }
    }
    
    public void SetCollectionState(bool isActive)
    {
        isInteractable = isActive;
        isSelected = !isActive; // 활성 상태면 선택 안 된 것, 비활성이면 선택된 것

        if (background != null)
        {
            // 비활성(선택됨)이면 회색, 아니면 원래 색
            background.color = isActive ? bgOriginColor : Color.gray;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Battle 모드면 클릭 무시
        if (currentMode == CardMode.Battle) return;

        // DeckBuilding 모드면 매니저에게 알림
        if (currentMode == CardMode.DeckBuilding && deckManager != null)
        {
            if (!isInteractable) return;
            
            deckManager.OnCardClicked(this);
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentMode == CardMode.Battle)
        {
            if (cardDeckController == null) return;
            if (!cardDeckController.isSpread) return;
            
            if (!isFloating)
            {
                // 원래 순서 저장 후 맨 위로 올리기
                originalSiblingIndex = transform.GetSiblingIndex();
                transform.SetAsLastSibling();
                
                targetPosition = defaultPosition + new Vector3(0f, floatOffset, 0f);
                targetScale = defaultScale * scaleUpFactor;
                
                var e = transform.localEulerAngles;
                e.z = 0f;
                transform.localEulerAngles = e;
                
                isFloating = true;
            }

            return;
        }

        if (currentMode == CardMode.DeckBuilding)
        {
            if (!isInteractable) return;
            if (isClone) return;
            
            isHoveringDeckBuilder = true;

            if (originScale == Vector3.zero)
            {
                originScale = transform.localScale;
            }
            
            transform.localScale = originScale * hoverScaleFactor;

            if (canvas != null)
            {
                canvas.overrideSorting = true;
                canvas.sortingOrder = 100;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentMode == CardMode.Battle)
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
            
            return;
        }

        if (currentMode == CardMode.DeckBuilding)
        {
            if (!isHoveringDeckBuilder) return;
            
            isHoveringDeckBuilder = false;
            
            transform.localScale = originScale;
            
            if (canvas != null)
            {
                canvas.overrideSorting = false;
                canvas.sortingOrder = 0;
            }
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentMode == CardMode.DeckBuilding) return;
        if (!isFloating) return;
        if (PlayerManager.instance == null) return;
        if (PlayerManager.instance.currentCost < playerCard.cost || playerCard.currentCoolTime > 0) return;

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
        if (currentMode == CardMode.DeckBuilding) return;
        if (!isDragging) return;
        
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
        if (currentMode == CardMode.DeckBuilding) return;
        if (!isDragging) return;
        
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
                // var playerManager = hitObj.GetComponent<PlayerManager>();
                var playerManager = PlayerManager.instance;

                if (playerManager != null && playerManager.currentCost > 0)
                {
                    Debug.Log($"{hitObj.name}의 현재 체력: {playerManager.currentHP}");
                    int posValue = playerCard.posValue;
                    int negValue = playerCard.negValue;
                    int cost = playerCard.cost;

                    if (playerManager.currentCost < cost)
                    {
                        Debug.Log("현재 코스트보다 큰 카드는 사용 불가");
                    }
                    else
                    {
                        StatType posType = playerCard.cardData.positiveStatType;
                        StatType negType = playerCard.cardData.negativeStatType;
                        
                        playerManager.AddTurnStatDelta(posType, posValue);
                        playerManager.AddTurnStatDelta(negType, -negValue);
                        playerManager.currentCost = Mathf.Max(0, playerManager.currentCost - cost);

                        if (playerCard.cardData.coolTime > 0)
                        {
                            playerCard.currentCoolTime = playerCard.cardData.coolTime + 1;
                        }
                        else
                        {
                            playerCard.currentCoolTime = 0;
                        }
                        
                        
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
