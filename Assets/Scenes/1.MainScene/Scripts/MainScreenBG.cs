using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainScreenBG : MonoBehaviour
{
    public GameObject SettingPanel;
    private bool isSettingPanel = false;
    public void LoadStageScene()
    {
        SceneLoader.LoadStageScene();
    }

    public void OnSettingPanel()
    {
        if (isSettingPanel)
        {
            isSettingPanel = false;
            SettingPanel.SetActive(isSettingPanel);
        }
        else
        {
            isSettingPanel = true;
            SettingPanel.SetActive(isSettingPanel);
        }
        
    }

    public void ExitButtonClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Apllication.Quit();
#endif
    }
}
