using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Virus : MonoBehaviour
{
    public Image virusImage;
    public int indexNum;
    public SpriteRenderer imageRender;
    public TextMeshPro virusName;

    public TextMeshPro atkDmg;
    public TextMeshPro hpCnt;


    public void Start()
    {
        if (VirusMgr.instance == null)
        {
            Debug.LogError("CardMgr.instance가 초기화되지 않았습니다.");
            return;
        }

        //dataset.data.FindIndex(card => card.CardNum.Equals(cardNum));

        // 바이러스 SO만든 뒤에 필드에 나오는 로직 만들고 데이터 삽입 기능 추가할 부분.
        //VirusData cardData = CardMgr.instance.cardDatas.Find(card => card.CardNum == cardNum);


        //cardImage.sprite = cardData.CardImage;
        //cardName.text = cardData.CardName;
        //positiveNum.text = cardData.PositiveNum.ToString();
        //negativeNum.text = cardData.NegativeNum.ToString();
        //costNum.text = cardData.CostNum.ToString();

        // 지금은 설명칸이 비어있어서 빼놓음.
        //description.text = cardData.Description;
    }
}
