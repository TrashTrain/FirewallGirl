using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName="CardSO", menuName="Scriptable Object/Card", order=int.MaxValue)]

public class CardObject : ScriptableObject
{
    public float floatOffset = 5f;
    public float floatSpeed = 10f;
    
    public bool isDragging = false;
    
    GameObject cardDeck;
    
    public static GameObject card;
    public Canvas canvas;

    [HideInInspector] public Transform startParent;
}
