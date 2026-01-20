using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;

public class StageMgr : MonoBehaviour
{
    public Button BackBtn;
    public Button StageChangeBtn;

    public GameObject[] StageList;
    public Image[] StageImg;
    private Outline StageOutline;

    //LJI
    public int stageCnt = 0;
    public int clearStageCnt = 0;
    public List<Button> StageButtons;
    public TextMeshProUGUI stageCntTxt;
    public static StageMgr Instance;

    private int StageCount = 0;
    ///private int OutLineCount = 0;

    GameObject player;
    
    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null)
            Instance = this;   

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

        OnViewStageCnt();
        SetStageScene();

        //StageOutline = StageImg[StageImg.Length].GetComponent<Outline>();
        //StageOutline.enabled = false;

    }
    private void OnViewStageCnt()
    {
        stageCnt = StageButtons.Count - clearStageCnt;
        stageCntTxt.text = "Stage Cnt : " + stageCnt;

    }
    
    // 새로 시작할때 스테이지씬
    private void SetStageScene()
    {
        foreach(var stage in StageButtons)
        {
            stage.image.color = Color.red;
            stage.onClick.AddListener(() =>
            {
                OnInStageButton();
                stage.image.color = GameColors.FromHex("0080EA");
            } );
        }
        
    }

    private void StageChangeBtnClick()
    {
        StageCount++;
        player.transform.position = StageList[StageCount].transform.position;

        StageOutline.enabled = !StageOutline.enabled;
    }

    private void BackBtnClick()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void OnInStageButton()
    {
        SceneManager.LoadScene("IntegratedScene");
    }
}
