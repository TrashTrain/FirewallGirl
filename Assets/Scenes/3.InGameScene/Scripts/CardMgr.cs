using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardMgr : MonoBehaviour
{
    // 싱글톤 
    public static CardMgr Inst { get; private set; }
    void Awake() => Inst = this;

    [SerializeField] ItemSO itemSO;

    //아이템 버퍼 리스트
    List<Item> itemBuffer;

    void SetupItemBuffer()
    {
        for (int i = 0; i < itemSO.items.Length; i++)
        {
            Item item = itemSO.items[i];
            for (int j = 0; j < item.percent; j++)
            {
                itemBuffer.Add(item);
            }
        }

        //나오는 순서를 랜덤하게
        for (int i = 0; i < itemBuffer.Count; i++)
        {
            int rand = Random.Range(i, itemBuffer.Count);
            Item temp = itemBuffer[i];
            itemBuffer[i] = itemBuffer[rand];
            itemBuffer[rand] = temp;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        SetupItemBuffer();
    }

}
