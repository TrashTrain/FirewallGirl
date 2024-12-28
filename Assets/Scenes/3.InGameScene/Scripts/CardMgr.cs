using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public enum CardExType { TestCardData1, TestCardData2, TestCardData3, TestCardData4 }
public class CardMgr : MonoBehaviour
{
    List<Card> CardInventory = new List<Card>();
    public PlayerCardObject[] CardObject;
    // 현재 출력할 CardObject의 인덱스
    private int currentIndex = 0;

    public void CardCreate(CardExType inputType)
    {
        int cardIndex = (int)inputType;

        GameObject cardGo = new GameObject(inputType.ToString());
        cardGo.transform.parent = this.gameObject.transform;


        Card card = cardGo.AddComponent<Card>();
        BoxCollider2D collider = cardGo.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        card.cardName = CardObject[cardIndex].cardName;
        card.positiveNum = CardObject[cardIndex].positiveNum;
        card.negativeNum = CardObject[cardIndex].negativeNum;
        card.type = CardObject[cardIndex].type;


        CardInventory.Add(card);
    }

    // Start is called before the first frame update
    void Start()
    {
        
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
                CardMgr hitCard = hit.collider.GetComponent<CardMgr>();

                for(int i = 0; i <= currentIndex; i++)
                {
                    if (hitCard != null)
                    {
                        Debug.Log($"{hitCard.CardObject[i].cardName}");
                        Debug.Log($"{hitCard.CardObject[i].positiveNum}");
                        Debug.Log($"{hitCard.CardObject[i].negativeNum}");
                        Debug.Log($"{hitCard.CardObject[i].type}");
                        currentIndex = (currentIndex + 1) % hitCard.CardObject.Length;
                    }
                    else
                    {
                        Debug.Log("해당 객체에 연결된 카드 데이터를 찾을 수 없습니다.");
                    }
                    
                }
               
            }
        }

    }

}
