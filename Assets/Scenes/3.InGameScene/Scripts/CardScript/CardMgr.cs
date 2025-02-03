using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class CardMgr : MonoBehaviour
{
    public PlayerCardObject[] CardObject;

    public static CardMgr instance;

    public CharacterData charData;

    public GameObject[] CardInfoPanelmage;

    public TextMeshProUGUI[] cardNameInfo;
    public TextMeshProUGUI[] cardPositiveInfo;
    public TextMeshProUGUI[] cardNegativeInfo;
    public TextMeshProUGUI[] cardCostInfo; 

    public List<CardData> cardDatas = new List<CardData>();
    //private int totalCost = 10;

    public CardData CardCreate(PlayerCardObject cardObject)
    {
        CardData cardData = new CardData(cardObject.cardIndex, cardObject.cardImage, cardObject.cardName, cardObject.positiveNum, cardObject.negativeNum, cardObject.costNum, cardObject.description);
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
            Destroy(gameObject);
        }

        foreach (PlayerCardObject card in CardObject)
        {
            cardDatas.Add(CardCreate(card));
        }
    }


    // 코스트 감소 함수
    public void ReduceCostOnClick(Card hitCard)
    {
        //내 코스트 
        int totalCost = int.Parse(charData.stats.Cost.text);
        // 코스트 감소
        if (hitCard.costNum != null)
        {
            // 현재 카드의 코스트 값을 가져와 정수로 변환
            int currentCost = int.Parse(hitCard.costNum.text);
            if (totalCost >= currentCost)
            {
                // 코스트 감소
                totalCost = Mathf.Max(0, totalCost - currentCost);

                // UI 업데이트
                charData.stats.Cost.text = totalCost.ToString();

                Debug.Log($"나의 코스트가 {currentCost}으로 감소했습니다.");

            }
            else
            {
                Debug.Log("코스트가 부족합니다.");
            }

        }
    }

    // 좌클릭시 카드 인포메이션
    public void CardinfoOnClick(Card hitcard)
    {
        int i = hitcard.cardNum;
        bool isActive = CardInfoPanelmage[i-1].gameObject.activeSelf; // 현재 활성 상태 확인
        CardInfoPanelmage[i-1].gameObject.SetActive(!isActive);

    }

    // 좌클릭시 팝업 띄움
    private void UpdateCardInfoPanel(Card hitcard)
    {
        int i = hitcard.cardNum;
        bool isActive = CardInfoPanelmage[i - 1].gameObject.activeSelf; // 현재 활성 상태 확인
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
        // 마우스 좌클릭시 
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);

            // 충돌시
            if (hit.collider != null)
            {
                // 충돌한 객체의 Card 컴포넌트 가져오기
                Card clickCard = hit.collider.GetComponent<Card>();

                if (clickCard != null)
                {
                    ReduceCostOnClick(clickCard);

                }

                else
                {
                    Debug.Log("해당 객체에 연결된 카드 데이터를 찾을 수 없습니다.");
                }


            }
        }

        //마우스 우클릭시
        if (Input.GetMouseButtonDown(1))
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);

            // 충돌시
            if (hit.collider != null)
            {
                // 충돌한 객체의 Card 컴포넌트 가져오기
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

