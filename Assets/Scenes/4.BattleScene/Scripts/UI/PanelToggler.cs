using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelToggler : MonoBehaviour
{
    public GameObject targetPanel;
    
    private void Update()
    {
        // ESC 키가 눌렸을 때 (한 번 누를 때마다 1회 실행)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePanel();
        }
    }

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
