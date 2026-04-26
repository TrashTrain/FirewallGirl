using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public enum CardViewState { Mini, Hover, Detailed }

public class PlayerCard : MonoBehaviour
{
    public CardObject cardData;
    
    [SerializeField]
    private int costValue;

    private int positiveStatValue;
    private int negativeStatValue;

    // 카드 설명 추가.
    private string description;
    
    public GameObject hoverView; // HoverView 오브젝트 연결
    public GameObject detailView; // Card (상세 뷰) 오브젝트 연결
    public Button hoverUseBtn;   // 호버 뷰의 사용 버튼
    public Button detailUseBtn;  // 디테일 뷰의 사용 버튼
    
    private Transform originalDetailParent;
    private PlayerManager playerManager; // 카드 사용을 위해 필요
    private Coroutine hideHoverCoroutine;

    [HideInInspector] public int currentCoolTime = 0;

    public int posValue
    {
        get { return positiveStatValue; }
        set { positiveStatValue = value; }
    }
    
    public int negValue
    {
        get { return negativeStatValue; }
        set { negativeStatValue = value; }
    }
    
    public int cost
    {
        get { return costValue; }
        set { costValue = value; }
    }

    public string desc
    {
        get { return description; } 
        set { description = value; }
    }
    
    private Sprite GetStatIcon(StatType stat)
    {
        if (StatIconManager.Instance == null)
        {
            Debug.LogError("StatIconManager가 씬에 없음");
            return null;
        }
        return StatIconManager.Instance.GetIcon(stat);
    }

    private void Start()
    {
        // if (cardData == null) return;
        //
        // costValue = cardData.cost;
        // positiveStatValue = cardData.positiveStatValue;
        // negativeStatValue = cardData.negativeStatValue;
        //
        // Image cardImage = transform.Find("Content").GetComponent<Image>(); // 카드 그림
        // TextMeshProUGUI cardName = transform.Find("CardName/Text").GetComponent<TextMeshProUGUI>(); // 카드 이름
        //
        // Image positiveIcon = transform.Find("PositiveStat").GetComponent<Image>();
        // Image negativeIcon = transform.Find("NegativeStat").GetComponent<Image>();
        //
        // TextMeshProUGUI posStatText = transform.Find("PositiveStat/Text").GetComponent<TextMeshProUGUI>(); // 긍정 수치 텍스트
        // TextMeshProUGUI negStatText = transform.Find("NegativeStat/Text").GetComponent<TextMeshProUGUI>(); // 부정 수치 텍스트
        //
        // TextMeshProUGUI costText = transform.Find("Cost/CostText").GetComponent<TextMeshProUGUI>(); // 코스트 수치 텍스트
        // TextMeshProUGUI descriptionText = transform.Find("Description").GetComponent<TextMeshProUGUI>(); // 카드 설명 텍스트
        //
        // cardImage.sprite = cardData.cardImage;
        // cardName.text = cardData.cardName;
        //
        // positiveIcon.sprite = GetStatIcon(cardData.positiveStatType);
        // negativeIcon.sprite = GetStatIcon(cardData.negativeStatType);
        //
        // posStatText.text = positiveStatValue.ToString();
        // negStatText.text = negativeStatValue.ToString();
        //
        // costText.text = costValue.ToString();
        // if (descriptionText != null && cardData != null)
        // {
        //     string dynamicDesc = cardData.description
        //         .Replace("{0}", cardData.positiveStatValue.ToString("+#;-#;0"))
        //         .Replace("{1}", cardData.negativeStatValue.ToString("+#;-#;0"));
        //         
        //     descriptionText.text = dynamicDesc;
        // }
        
        playerManager = FindObjectOfType<PlayerManager>();

        costValue = cardData.cost;
        positiveStatValue = cardData.positiveStatValue;
        negativeStatValue = cardData.negativeStatValue;

        if (hoverView != null) hoverView.SetActive(false);
        if (detailView != null)
        {
            detailView.SetActive(false);
            originalDetailParent = detailView.transform.parent;
        }

        // ? 두 버튼 모두 클릭 시 동일한 '사용' 로직을 실행하도록 연결
        if (hoverUseBtn != null) hoverUseBtn.onClick.AddListener(UseCard);
        if (detailUseBtn != null) detailUseBtn.onClick.AddListener(UseCard);
    }
    
    // 데이터를 외부에서 주입받는 함수 추가
    public void SetCardData(CardObject data)
    {
        this.cardData = data;
        // if (cardData == null) return;
        
        // 여기서 UI 텍스트(이름, 코스트 등)를 갱신하는 로직을 호출하세요.
        // playerManager.UpdateUI();
        CardDeckController.instance.UpdateCardVisuals(gameObject, data);
    }
    
    // 호버 애니메이션 토글
    public void ToggleHover(bool show)
    {
        if (hoverView == null) return;
        
        // 디테일 뷰가 켜져 있을 때는 호버 뷰 작동 안 함
        if (show && detailView != null && detailView.activeSelf) return;

        if (show)
        {
            // ? 마우스가 0.15초 안에 HoverView로 들어오면 끄기 예약을 취소하고 유지함
            if (hideHoverCoroutine != null)
            {
                StopCoroutine(hideHoverCoroutine);
                hideHoverCoroutine = null;
            }
            hoverView.SetActive(true);
        }
        else
        {
            // ? 마우스가 나갔을 때 즉시 끄지 않고 0.15초 대기하는 코루틴 실행
            if (gameObject.activeInHierarchy)
            {
                hideHoverCoroutine = StartCoroutine(HideHoverDelayed());
            }
        }
    }
    
    private IEnumerator HideHoverDelayed()
    {
        yield return new WaitForSeconds(0.15f); // 0.15초 유예 시간 (필요에 따라 조절)
        hoverView.SetActive(false);
        hideHoverCoroutine = null;
    }

    public void ShowDetailView()
    {
        if (detailView == null || detailView.activeSelf) return;

        ToggleHover(false); // 상세 창이 켜지면 호버는 끔
        detailView.SetActive(true);
        
        // 하단 패널(레이아웃 그룹)의 영향을 받지 않도록 캔버스 최상단으로 부모 이동
        Canvas rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
        detailView.transform.SetParent(rootCanvas.transform, true);

        RectTransform rt = detailView.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero; // 화면 중앙
        rt.SetAsLastSibling();
        
        Transform useBtnTransform = detailView.transform.Find("UseBtn"); 
        
        if (useBtnTransform != null)
        {
            Button detailUseBtn = useBtnTransform.GetComponent<Button>();
            
            // 1. 이전에 봤던 카드의 UseCard가 실행되지 않도록 기존 연결을 모두 끊어줍니다. (매우 중요?)
            detailUseBtn.onClick.RemoveAllListeners();
            
            // 2. '지금 클릭한 이 카드'의 UseCard() 함수를 버튼에 연결합니다.
            detailUseBtn.onClick.AddListener(() => 
            {
                UseCard(); 
                
                // 필요하다면 카드를 사용한 뒤 디테일 뷰를 닫는 코드도 여기에 한 줄 추가할 수 있습니다.
                detailView.SetActive(false);
            });
        }
    }

    public void HideDetailView()
    {
        if (detailView != null)
        {
            detailView.transform.SetParent(originalDetailParent, true);
            detailView.SetActive(false);
        }
    }

    // ? 기존 CardController에 있던 드래그 사용 로직을 버튼 클릭용으로 이사 옴
    public void UseCard()
    {
        if (playerManager == null || cardData == null) return;
        if (PlayerManager.instance.currentCost < cost || currentCoolTime > 0) return;
        
        Debug.Log("카드 사용!!!");

        if (playerManager.currentCost >= cost)
        {
            Debug.Log($"긍정 효과: {cardData.positiveStatType}, 긍정 수치: {posValue}");
            Debug.Log($"부정 효과: {cardData.negativeStatType}, 부정 수치: {negValue}");
            playerManager.AddTurnStatDelta(cardData.positiveStatType, posValue);
            playerManager.AddTurnStatDelta(cardData.negativeStatType, negValue);
            playerManager.currentCost = Mathf.Max(0, playerManager.currentCost - cost);

            currentCoolTime = cardData.coolTime > 0 ? cardData.coolTime : 0;
            
            playerManager.OnCardUsed(this);
            
            playerManager.UpdateUI();
            Debug.Log($"{cardData.cardName} 사용 완료!");

            // 사용 후 처리 (디테일 뷰 닫고, 카드를 파괴하거나 묘지로 보냄)
            HideDetailView();
        }
        else
        {
            Debug.Log("코스트 부족으로 사용 불가");
        }
    }
    
    public void DecreaseCooldown()
    {
        if (currentCoolTime > 0)
        {
            currentCoolTime--;
        }
    }
}
