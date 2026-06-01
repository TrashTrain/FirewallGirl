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

    // ī�� ���� �߰�.
    private string description;
    
    public GameObject hoverView; // HoverView ������Ʈ ����
    public GameObject detailView; // Card (�� ��) ������Ʈ ����
    public Button hoverUseBtn;   // ȣ�� ���� ��� ��ư
    public Button detailUseBtn;  // ������ ���� ��� ��ư
    
    public bool IsInPreview { get; private set; } 
    public TextMeshProUGUI hoverUseBtnText;  // HoverView�� UseBtn �Ʒ��� �ִ� �ؽ�Ʈ
    public TextMeshProUGUI detailUseBtnText; // DetailView�� UseBtn �Ʒ��� �ִ� �ؽ�Ʈ
    
    private Transform originalDetailParent;
    private PlayerManager playerManager; // ī�� ����� ���� �ʿ�
    private Coroutine hideHoverCoroutine;

    [HideInInspector] public int currentCoolTime = 0;
    
    public static event Action OnUseClicked;
    public bool isTemporary = false;

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
            Debug.LogError("StatIconManager�� ���� ����");
            return null;
        }
        return StatIconManager.Instance.GetIcon(stat);
    }

    private void Start()
    {
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

        // ? �� ��ư ��� Ŭ�� �� ������ '���' ������ �����ϵ��� ����
        if (hoverUseBtn != null)
        {
            hoverUseBtn.onClick.RemoveAllListeners();
            hoverUseBtn.onClick.AddListener(TogglePreviewState);
        }

        if (detailUseBtn != null)
        {
            detailUseBtn.onClick.RemoveAllListeners();
            detailUseBtn.onClick.AddListener(TogglePreviewState);
        }
        
        SetPreviewState(false);
    }
    
    // PlayerManager���� ī�带 �ְ� �� �� ���¸� ����ȭ���ִ� �Լ�
    public void SetPreviewState(bool state)
    {
        IsInPreview = state;
        string hoverBtnStr = IsInPreview ? "취소" : "적용";
        string detailBtnStr = IsInPreview ? "취소" : $"적용 ({cost})";

        if (hoverUseBtnText != null) hoverUseBtnText.text = hoverBtnStr;
        if (detailUseBtnText != null) detailUseBtnText.text = detailBtnStr;
    }
    
    /// <summary>hoverView의 사용 버튼 클릭 가능 여부를 설정합니다.</summary>
    public void SetHoverUseBtnInteractable(bool interactable)
    {
        if (hoverUseBtn != null) hoverUseBtn.interactable = interactable;
    }

    // '����/���' ��ư�� ������ �� ����Ǵ� �Լ�
    public void TogglePreviewState()
    {
        if (PlayerManager.instance == null) return;
        
        if (!IsInPreview) OnUseClicked?.Invoke();
        
        // �Ŵ����� ��ٱ��Ͽ� �ְų� ��
        PlayerManager.instance.ToggleCardInPreview(this);

        // ���� ��� '����'�� ������ ��ٱ��Ͽ� �� ����(IsInPreview == true)��� ��â �ݱ�
        if (IsInPreview)
        {
            HideDetailView();
        }
    }
    
    // �����͸� �ܺο��� ���Թ޴� �Լ� �߰�
    public void SetCardData(CardObject data)
    {
        this.cardData = data;
        // if (cardData == null) return;
        
        // ���⼭ UI �ؽ�Ʈ(�̸�, �ڽ�Ʈ ��)�� �����ϴ� ������ ȣ���ϼ���.
        // playerManager.UpdateUI();
        CardDeckController.instance.UpdateCardVisuals(gameObject, data);
    }
    
    // ȣ�� �ִϸ��̼� ���
    public void ToggleHover(bool show)
    {
        if (hoverView == null) return;
        
        // ������ �䰡 ���� ���� ���� ȣ�� �� �۵� �� ��
        if (show && detailView != null && detailView.activeSelf) return;

        if (show)
        {
            // ? ���콺�� 0.15�� �ȿ� HoverView�� ������ ���� ������ ����ϰ� ������
            if (hideHoverCoroutine != null)
            {
                StopCoroutine(hideHoverCoroutine);
                hideHoverCoroutine = null;
            }
            hoverView.SetActive(true);
        }
        else
        {
            // ? ���콺�� ������ �� ��� ���� �ʰ� 0.15�� ����ϴ� �ڷ�ƾ ����
            if (gameObject.activeInHierarchy)
            {
                hideHoverCoroutine = StartCoroutine(HideHoverDelayed());
            }
        }
    }
    
    private IEnumerator HideHoverDelayed()
    {
        yield return new WaitForSeconds(0.15f); // 0.15�� ���� �ð� (�ʿ信 ���� ����)
        hoverView.SetActive(false);
        hideHoverCoroutine = null;
    }

    public void ShowDetailView()
    {
        if (detailView == null || detailView.activeSelf) return;

        ToggleHover(false); // �� â�� ������ ȣ���� ��
        detailView.SetActive(true);
        
        // �ϴ� �г�(���̾ƿ� �׷�)�� ������ ���� �ʵ��� ĵ���� �ֻ������ �θ� �̵�
        Canvas rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
        detailView.transform.SetParent(rootCanvas.transform, true);

        RectTransform rt = detailView.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero; // ȭ�� �߾�
        rt.SetAsLastSibling();
        
        Transform useBtnTransform = detailView.transform.Find("UseBtn"); 
        
        // if (useBtnTransform != null)
        // {
        //     Button detailUseBtn = useBtnTransform.GetComponent<Button>();
        //     
        //     // 1. ������ �ô� ī���� UseCard�� ������� �ʵ��� ���� ������ ��� �����ݴϴ�. (�ſ� �߿�?)
        //     detailUseBtn.onClick.RemoveAllListeners();
        //     
        //     // 2. '���� Ŭ���� �� ī��'�� UseCard() �Լ��� ��ư�� �����մϴ�.
        //     detailUseBtn.onClick.AddListener(() => 
        //     {
        //         UseCard(); 
        //         
        //         // �ʿ��ϴٸ� ī�带 ����� �� ������ �並 �ݴ� �ڵ嵵 ���⿡ �� �� �߰��� �� �ֽ��ϴ�.
        //         detailView.SetActive(false);
        //     });
        // }
    }

    public void HideDetailView()
    {
        if (detailView != null)
        {
            detailView.transform.SetParent(originalDetailParent, true);
            detailView.SetActive(false);
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
