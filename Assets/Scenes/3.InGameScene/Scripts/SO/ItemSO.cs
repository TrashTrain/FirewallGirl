using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class Item
{
    public string name; //이름 
    public int attack;  //공격력
    public int health;  //체력
    public Sprite sprite;//스프라이트
    public float percent;//카드의 확률
}

[CreateAssetMenu(fileName ="ItemSO", menuName= "Scriptable Object/ItemSO")]  
public class ItemSO : ScriptableObject
{
    public Item[] items;
}
