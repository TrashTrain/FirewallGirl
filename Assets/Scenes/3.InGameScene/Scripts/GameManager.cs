using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    static public bool PlayerTurn = false;

    private bool checkTurn;
    // Start is called before the first frame update
    void Start()
    {
        PlayerTurn = true;
        checkTurn = PlayerTurn;
    }

    public void OnTrunButtonClick()
    {
        // true -> PlayerTurn으로 바꾸기
        if (true)
        {
            Debug.Log("턴 넘기기 성공");
            PlayerTurn = !PlayerTurn;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(checkTurn != PlayerTurn)
        {
            Debug.Log("턴 종료");
            checkTurn = PlayerTurn;
        }   
    }
}
