using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CardMgr : MonoBehaviour
{
    //List<Card> CardInventory = new List<Card>();
    public PlayerCardObject[] CardObject;
    public static CardMgr instance;

    public List<CardData> cardDatas = new List<CardData>();

    // 현재 출력할 CardObject의 인덱스
    private int currentIndex = 0;

    public CardData CardCreate(PlayerCardObject cardObject)
    {
        CardData cardData = new CardData(cardObject.cardIndex, cardObject.cardImage, cardObject.cardName, cardObject.positiveNum, cardObject.negativeNum , cardObject.description);
        return cardData;
    }

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        foreach(PlayerCardObject card in CardObject)
        {
            cardDatas.Add(CardCreate(card));
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
                Card hitCard = hit.collider.GetComponent<Card>();

                if (hitCard != null)
                {
                    Debug.Log($"{hitCard.cardName.text}");
                    Debug.Log($"{hitCard.positiveNum.text}");
                    Debug.Log($"{hitCard.negativeNum.text}");    
                    if(hitCard.description != null)
                        Debug.Log($"{hitCard.description.text}");



                }
                
                else
                {
                    Debug.Log("해당 객체에 연결된 카드 데이터를 찾을 수 없습니다.");
                }


            }
        }

    }

}
