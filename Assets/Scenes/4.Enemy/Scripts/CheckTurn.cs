using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckTurn : MonoBehaviour
{
    private void Start()
    {
        gameObject.GetComponent<SpriteRenderer>().color = Color.green;
    }
    void Update()
    {
        if(GameManager.PlayerTurn)
            gameObject.GetComponent<SpriteRenderer>().color = Color.green;
        else
            gameObject.GetComponent<SpriteRenderer>().color = Color.red;
    }
}
