using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnChanger : MonoBehaviour
{
    public Button turnChangeBT;

    public void OnClickButton()
    {
        GameManager.Instance.OnTrunButtonClick();
        //PlayerManager.instance.OnTurnEndProcess(); // 디버프/쿨타임 갱신
    }

}
