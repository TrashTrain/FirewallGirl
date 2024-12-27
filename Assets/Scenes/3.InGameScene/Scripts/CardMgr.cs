using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum CardExType { TestCardData1 }
public class CardMgr : MonoBehaviour
{
    List<Card> CardInventory = new List<Card>();
    public PlayerCardObject[] CardObject;

    public void CardCreate(CardExType inputType)
    {
        int cardIndex = (int)inputType;

        GameObject cardGo = new GameObject(inputType.ToString());
        cardGo.transform.parent = this.gameObject.transform;


        Card card = cardGo.AddComponent<Card>();

        card.cardName = CardObject[cardIndex].cardName;

        CardInventory.Add(card);
    }

    // Start is called before the first frame update
    void Start()
    {
        //카드 생성
        CardCreate(CardExType.TestCardData1);
    }

    void ShowInventoryItems()
    {
        //CardInventory에 저장된 카드 이름 출력
        foreach (Card c in CardInventory)
        {
            Debug.Log(c.cardName);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 마우스 클릭시 
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);

            // 충돌시
            if (hit.collider != null)
            {
                ShowInventoryItems();
            }
        }

    }

}
