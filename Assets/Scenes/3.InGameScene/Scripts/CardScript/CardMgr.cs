using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class CardMgr : MonoBehaviour
{
    public CardObject[] CardObject;

    public static CardMgr instance;

    public CharacterData charData;

    public GameObject[] CardInfoPanelmage;

    public TextMeshProUGUI[] cardNameInfo;
    public TextMeshProUGUI[] cardPositiveInfo;
    public TextMeshProUGUI[] cardNegativeInfo;
    public TextMeshProUGUI[] cardCostInfo; 

    public List<CardData> cardDatas = new List<CardData>();
    //private int totalCost = 10;

    public CardData CardCreate(CardObject cardObject)
    {
        CardData cardData = new CardData(cardObject.cardIndex,
                                        cardObject.cardName,
                                        cardObject.cardImage,
                                        cardObject.positiveStatType,
                                        cardObject.negativeStatType,
                                        cardObject.positiveStatValue,
                                        cardObject.negativeStatValue,
                                        cardObject.cost,
                                        cardObject.summaryDescription,
                                        cardObject.description);
        return cardData;
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        foreach (CardObject card in CardObject)
        {
            if (card == null) continue;
            cardDatas.Add(CardCreate(card));
        }
    }


    // �ڽ�Ʈ ���� �Լ�
    public void ReduceCostOnClick(Card hitCard)
    {
        //�� �ڽ�Ʈ 
        int totalCost = int.Parse(charData.stats.Cost.text);
        // �ڽ�Ʈ ����
        if (hitCard.costNum != null)
        {
            // ���� ī���� �ڽ�Ʈ ���� ������ ������ ��ȯ
            int currentCost = int.Parse(hitCard.costNum.text);
            if (totalCost >= currentCost)
            {
                // �ڽ�Ʈ ����
                totalCost = Mathf.Max(0, totalCost - currentCost);

                // UI ������Ʈ
                charData.stats.Cost.text = totalCost.ToString();

                Debug.Log($"���� �ڽ�Ʈ�� {currentCost}���� �����߽��ϴ�.");

            }
            else
            {
                Debug.Log("�ڽ�Ʈ�� �����մϴ�.");
            }

        }
    }

    // ��Ŭ���� ī�� �������̼�
    public void CardinfoOnClick(Card hitcard)
    {
        int i = hitcard.cardNum;
        bool isActive = CardInfoPanelmage[i-1].gameObject.activeSelf; // ���� Ȱ�� ���� Ȯ��
        CardInfoPanelmage[i-1].gameObject.SetActive(!isActive);

    }

    // ��Ŭ���� �˾� ���
    private void UpdateCardInfoPanel(Card hitcard)
    {
        int i = hitcard.cardNum;
        bool isActive = CardInfoPanelmage[i - 1].gameObject.activeSelf; // ���� Ȱ�� ���� Ȯ��
        if (isActive == true)
        {
            cardNameInfo[i - 1].text = hitcard.cardName.text;
            cardPositiveInfo[i - 1].text = "Positive: " + hitcard.positiveNum.text;
            cardNegativeInfo[i - 1].text = "Negative: "+ hitcard.negativeNum.text;
            cardCostInfo[i - 1].text = "Cost: "+hitcard.cardNum.ToString();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // ���콺 ��Ŭ���� 
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);

            // �浹��
            if (hit.collider != null)
            {
                // �浹�� ��ü�� Card ������Ʈ ��������
                Card clickCard = hit.collider.GetComponent<Card>();

                if (clickCard != null)
                {
                    ReduceCostOnClick(clickCard);

                }

                else
                {
                    Debug.Log("�ش� ��ü�� ����� ī�� �����͸� ã�� �� �����ϴ�.");
                }


            }
        }

        //���콺 ��Ŭ����
        if (Input.GetMouseButtonDown(1))
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);

            // �浹��
            if (hit.collider != null)
            {
                // �浹�� ��ü�� Card ������Ʈ ��������
                Card clickCard = hit.collider.GetComponent<Card>();

                if (clickCard != null)
                {
                    CardinfoOnClick(clickCard);
                    UpdateCardInfoPanel(clickCard);
                }

            }
        }
    }
}

