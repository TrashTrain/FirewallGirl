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
    
    private RectTransform rect;

    private bool isFloating = false;
    public bool isDragging = false;

    GameObject cardDeck;

    public static GameObject card;
    public Canvas canvas;

    [HideInInspector] public Transform startParent;
    
    void Start()
    {
        cardDeck = GameObject.Find("CardPanel");
        rect = GetComponent<RectTransform>();
        
        defaultPosition = transform.position;
        targetPosition = defaultPosition;
        
        // 스케일 초기값 저장
        defaultScale = transform.localScale;
        targetScale = defaultScale;
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
        // print(eventData.pointerEnter.name);
        if (cardDeck.GetComponent<CardDeckController>().isSpread && 
            eventData.pointerEnter != null && 
            eventData.pointerEnter.CompareTag("Card"))
        {
            if (!isFloating)
            {
                targetPosition = defaultPosition + new Vector3(0f, floatOffset, 0f);
                targetScale = defaultScale * scaleUpFactor;
                isFloating = true;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isFloating)
        {
            targetPosition = defaultPosition;
            targetScale = defaultScale;
            isFloating = false;
        }
    }
    
    // TODO: 드래그 이벤트 구현
    // 드래그 & 드롭 시 해당 캐릭터에 스탯 적용 함수 호출 (StatManager)
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isFloating)
        {
            isDragging = true;
            card = gameObject;
            startParent = transform.parent;
            
            // 드래그 시작 시 스케일 복원
            targetScale = defaultScale;
            transform.localScale = defaultScale;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            Input.mousePosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out Vector2 localPoint);
        // Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.position.z);
        // transform.position = eventData.position;
        // print("mouse: " + new Vector2(Input.mousePosition.x, Input.mousePosition.y));
        // print("pos: " + pos);
        rect.anchoredPosition = Input.mousePosition;
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

                if (playerManager != null)
                {
                    Debug.Log($"{hitObj.name}의 현재 체력: {playerManager.currentHP}");
                    int AP = transform.GetComponent<PlayerCard>().ap;
                    int DP = transform.GetComponent<PlayerCard>().dp;
                    int cost = transform.GetComponent<PlayerCard>().cost;
                    // int cardAP = int.Parse(card.transform.Find("AttackPower/Text").GetComponent<TextMeshProUGUI>().text);
                    // int cardDP = int.Parse(card.transform.Find("Defense/Text").GetComponent<TextMeshProUGUI>().text);
                    // playerManager.attackPower += cardAP;
                    // playerManager.defensePower += cardDP;
                    playerManager.attackPower += AP;
                    playerManager.defensePower += DP;
                    playerManager.currentCost = Mathf.Max(0, playerManager.currentCost - cost);
                    playerManager.UpdateUI();
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
        // print("end drag");
    }
}
