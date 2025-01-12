using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CardMgr : MonoBehaviour
{
    public PlayerCardObject[] CardObject;
    public TextMeshPro cost;

    public static CardMgr instance;

    public List<CardData> cardDatas = new List<CardData>();
    private int totalCost = 10;

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

    
    public void ReduceCostOnClick(Card hitCard)
    {
        // 코스트 감소
        if (hitCard.costNum != null)
        {
            // 현재 코스트 값을 가져와 정수로 변환
            int currentCost = int.Parse(hitCard.costNum.text);
            if (totalCost > 0)
            {
                // 코스트 감소
                totalCost = totalCost - currentCost;

                // UI 업데이트
                cost.text = totalCost.ToString();

                Debug.Log($"카드 {hitCard.cardName.text}의 코스트가 {currentCost}로 감소했습니다.");


            }
            else
            {
                //Debug.Log($"카드 {hitCard.cardName.text}의 코스트가 이미 0입니다.");
                Debug.Log("코스트가 부족합니다.");
                //Destroy(hitCard.gameObject);
            }

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
                        //Debug.Log($"{clickCard.cardName.text}");
                        //Debug.Log($"{clickCard.positiveNum.text}");
                        //Debug.Log($"{clickCard.negativeNum.text}");
                        //Debug.Log($"{clickCard.costNum.text}");
                        //if (hitCard.description != null)
                        //    Debug.Log($"{clickCard.description.text}");


                        ReduceCostOnClick(clickCard);

                    }

                    else
                    {
                        Debug.Log("해당 객체에 연결된 카드 데이터를 찾을 수 없습니다.");
                    }


                }
            }

        }
    }

