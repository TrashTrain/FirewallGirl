using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{

    // Card Panel
    public CardDeckController cardDeckController;

    // PlayerStatus
    //public PowerUI powerUI;
    //public CostUI costUI;



    public static UIManager Ins;

    void Awake()
    {
        Ins = this;
        DontDestroyOnLoad(Ins);
    }

}
