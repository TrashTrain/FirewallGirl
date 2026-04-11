using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelToggler : MonoBehaviour
{
    public GameObject targetPanel;

    // 버튼의 OnClick 이벤트에 연결할 함수
    public void TogglePanel()
    {
        if (targetPanel != null)
        {
            // 패널이 켜져있으면 끄고, 꺼져있으면 켭니다.
            targetPanel.SetActive(!targetPanel.activeSelf);
        }
    }
}
