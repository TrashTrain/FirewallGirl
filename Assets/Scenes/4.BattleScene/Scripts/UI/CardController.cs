using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
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
    
    private RectTransform rect;
    public bool isDragging = false;

    GameObject cardDeck;
    private CardDeckController cardDeckController;
    private PlayerCard playerCard;

    public static GameObject card;
    [SerializeField]
    private Canvas canvas;
    private Image background;
    private Image miniViewImage;
    private Color bgOriginColor = Color.white;
    
    private Transform originalParent;
    [HideInInspector] public Transform startParent;
    private PlayerManager playerManager;
    
    // 추가: 원래 렌더 순서(형제 인덱스) 저장
    private int originalSiblingIndex;
    
    private void Awake()
    {
        playerCard = GetComponent<PlayerCard>();
        
        Transform bgTransform = transform.Find("Background");
        
        if (bgTransform != null)
        {
            background = bgTransform.GetComponent<Image>();
        }

        if (transform != null)
        {
            miniViewImage = transform.GetComponent<Image>();
        }
    }
    
    void Start()
    {
        originScale = transform.localScale;

        if (currentMode == CardMode.DeckBuilding)
        {
            deckManager = FindObjectOfType<DeckManager>();
        }
        else if (currentMode == CardMode.Battle)
        {
            playerManager = FindObjectOfType<PlayerManager>();
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
        if (currentMode == CardMode.DeckBuilding)
        {
            if (isHoveringDeckBuilder && isInteractable)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, originScale * hoverScaleFactor, Time.deltaTime * 10f);
            }
            else
            {
                transform.localScale = Vector3.Lerp(transform.localScale, originScale, Time.deltaTime * 10f);
            }
        }
        else if (currentMode == CardMode.Battle)
        {
            UpdateBattleVisuals();
        }
    }
    
    private void UpdateBattleVisuals()
    {
        if (miniViewImage == null || playerCard == null || playerManager == null) return;

        // 사용 가능 조건: 쿨타임이 0 이하이고, 플레이어 코스트가 카드 코스트보다 많을 때
        bool isUsable = (playerCard.currentCoolTime <= 0) && (playerManager.currentCost >= playerCard.cost);

        // 사용 가능하면 흰색(원래색), 불가능하면 회색
        miniViewImage.color = isUsable ? Color.white : Color.gray;
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
        if (currentMode == CardMode.Battle)
        {
            if (playerCard != null)
            {
                playerCard.ShowDetailView();
            }
        }
        // DeckBuilding 모드면 매니저에게 알림
        else if (currentMode == CardMode.DeckBuilding && deckManager != null)
        {
            // if (!isInteractable) return;
            
            deckManager.OnCardClicked(this);
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentMode == CardMode.Battle && playerCard != null)
        {
            playerCard.ToggleHover(true); // HoverView 애니메이션과 함께 켜기
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
            if (!isDragging && playerCard != null)
            {
                playerCard.ToggleHover(false); // HoverView 부드럽게 끄기
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
    
    // public void OnBeginDrag(PointerEventData eventData)
    // {
    //     if (currentMode == CardMode.DeckBuilding) return;
    //     if (PlayerManager.instance == null) return;
    //     if (PlayerManager.instance.currentCost < playerCard.cost || playerCard.currentCoolTime > 0) return;
    //
    //     isDragging = true;
    //
    //     if (playerCard != null)
    //     {
    //         playerCard.ToggleHover(false); // 드래그 시작 시 방해 안 되게 호버 뷰 숨기기
    //     }
    //
    //     // 💡 [핵심] Horizontal Layout Group의 자동 정렬을 벗어나 자유롭게 이동하려면
    //     // 카드의 부모를 캔버스 최상단으로 임시로 변경해야 합니다.
    //     originalParent = transform.parent;
    //     originalSiblingIndex = transform.GetSiblingIndex();
    //         
    //     Canvas rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
    //     transform.SetParent(rootCanvas.transform, true);
    //     transform.SetAsLastSibling(); // 다른 UI들에 가려지지 않게 맨 앞으로 가져오기
    // }

    // public void OnDrag(PointerEventData eventData)
    // {
    //     if (currentMode == CardMode.DeckBuilding) return;
    //     if (!isDragging) return;
    //     
    //     Canvas rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
    //     rect.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
    // }

    // public void OnEndDrag(PointerEventData eventData)
    // {
    //     if (currentMode == CardMode.DeckBuilding) return;
    //     if (!isDragging) return;
    //     
    //     isDragging = false;
    //         bool cardUsed = false; // 카드 사용 성공 여부
    //
    //         // 기존 카드 사용 로직 유지 (캐릭터 위에 올려놓았는지 판별)
    //         List<RaycastResult> results = new List<RaycastResult>();
    //         EventSystem.current.RaycastAll(eventData, results);
    //
    //         foreach (RaycastResult result in results)
    //         {
    //             if (result.gameObject.CompareTag("Player"))
    //             {
    //                 if (playerManager != null && playerCard != null)
    //                 {
    //                     int cost = playerCard.cost;
    //                     if (playerManager.currentCost >= cost)
    //                     {
    //                         // 코스트 차감 및 스탯 적용
    //                         int posValue = playerCard.posValue;
    //                         int negValue = playerCard.negValue;
    //                         StatType posType = playerCard.cardData.positiveStatType;
    //                         StatType negType = playerCard.cardData.negativeStatType;
    //
    //                         playerManager.AddTurnStatDelta(posType, posValue);
    //                         playerManager.AddTurnStatDelta(negType, negValue);
    //                         playerManager.currentCost = Mathf.Max(0, playerManager.currentCost - cost);
    //
    //                         if (playerCard.cardData.coolTime > 0)
    //                             playerCard.currentCoolTime = playerCard.cardData.coolTime;
    //                         else
    //                             playerCard.currentCoolTime = 0;
    //                         
    //                         playerManager.UpdateUI();
    //                         cardUsed = true;
    //                         break; // 사용 완료
    //                     }
    //                     else
    //                     {
    //                         Debug.Log("코스트 부족으로 사용 불가");
    //                     }
    //                 }
    //             }
    //         }
    //         
    //         // 💡 드래그 취소되거나 코스트 부족으로 사용 실패 시 원래 자리로 되돌리기
    //         if (!cardUsed)
    //         {
    //             // 부모를 원래 작업표시줄 패널로 되돌리고, 순서도 원래대로 복구
    //             transform.SetParent(originalParent, true);
    //             transform.SetSiblingIndex(originalSiblingIndex);
    //         }
    //         else
    //         {
    //             // TODO: 카드를 사용한 이후의 처리 (덱에서 버리기, 파괴하기 등)
    //             // 현재는 테스트용으로 원래 자리로 되돌리거나 삭제하는 코드를 상황에 맞게 넣으세요.
    //             // Destroy(gameObject); // 카드를 무덤으로 보내거나 파괴한다면 주석 해제
    //             
    //             // 임시로 원래 자리로 돌려둠
    //             transform.SetParent(originalParent, true);
    //             transform.SetSiblingIndex(originalSiblingIndex);
    //         }
    // }
}
