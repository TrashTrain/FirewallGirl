using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;

public class StageMgr : MonoBehaviour
{
    public Button BackBtn;
    public Button StageChangeBtn;

    public GameObject[] StageList;
    private int StageCount = 0;
    private int OutLineCount = 0;

    GameObject player;
    
    // Start is called before the first frame update
    void Start()
    {
        //프레임 안정화
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;

        player = GameObject.Find("Player"); //플레이어 찾기 

        if (BackBtn != null)
        {
            BackBtn.onClick.AddListener(BackBtnClick);
        }

        if(StageChangeBtn != null)
        {
            StageChangeBtn.onClick.AddListener(StageChangeBtnClick);
        }

        

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void StageChangeBtnClick()
    {
        StageCount++;
        player.transform.position = StageList[StageCount].transform.position;

        OutLineCount++;
    }

    private void BackBtnClick()
    {
        SceneManager.LoadScene("MainScene");
    }
}
