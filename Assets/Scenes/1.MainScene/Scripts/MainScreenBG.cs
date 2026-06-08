using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainScreenBG : MonoBehaviour
{
    public GameObject SettingPanel;
    private bool isSettingPanel = false;

    [SerializeField] private Button resumeBtn;

    private static readonly Color ResumeBtnActive   = new Color(255f/255f, 255f/255f, 255f/255f, 200f/255f);
    private static readonly Color ResumeBtnInactive = new Color( 75f/255f,  75f/255f,  75f/255f, 203f/255f);

    private void Start()
    {
        if (resumeBtn == null) return;

        bool hasSave = PlayerStateSaveManager.instance.Exists();
        resumeBtn.interactable = hasSave;
        resumeBtn.image.color  = hasSave ? ResumeBtnActive : ResumeBtnInactive;
    }

    public void LoadStageScene()
    {
        SceneLoader.LoadStageScene();
    }

    public void LoadDeckBuildingScene()
    {
        StageSaveManager.ResetStage();
        SceneLoader.LoadDeckBuildingScene();
    }

    public void ResumeButton()
    {
        PlayerStateSaveManager.instance.IsLoadingFromSave = true;
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
        Application.Quit();
#endif
    }
}
