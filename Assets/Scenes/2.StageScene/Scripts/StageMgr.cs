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

    // [추가] 보스 스테이지 버튼 참조
    [Header("Boss Stage")]
    public Button bossStageBtn;

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

        OnViewStageIndex();
        SetStageScene();
        OnViewStageCnt();
    }

    private void OnViewStageCnt()
    {
        stageCnt = StageButtons.Count - clearStageCnt;
        stageCntTxt.text = "Stage Cnt : " + stageCnt;
    }

    // 초기화 함수(임시)
    public void OnResetStageInfo()
    {
        StageSaveManager.ResetStage();
        clearStageCnt = 0;
        OnViewStageIndex();
        SetStageScene();
        OnViewStageCnt();
    }

    private void OnViewStageIndex()
    {
        for (int i = 0; i < StageButtons.Count; i++)
        {
            StageButtons[i].GetComponent<NomalStage>().stageIdx = i;
        }
    }

    // 새로 시작할때 스테이지씬
    private void SetStageScene()
    {
        // [안전 장치] 함수가 여러 번 호출될 경우 중복 카운팅을 방지하기 위해 0으로 초기화
        clearStageCnt = 0;

        foreach (var stage in StageButtons)
        {
            if (StageSaveManager.IsStageCleared(stage.GetComponent<NomalStage>().stageIdx))
            {
                // [참고] GameColors 클래스가 없어서 임시로 Color 사용 (기존 코드 사용 시 주석 해제)
                // stage.image.color = GameColors.FromHex("#0080EA");
                stage.image.color = Color.blue;

                clearStageCnt++;
            }
            else
            {
                stage.image.color = Color.red;

                // [안전 장치] 리스너 중복 추가 방지
                stage.onClick.RemoveAllListeners();
                stage.onClick.AddListener(() =>
                {
                    StageSaveManager.CurrentStageIdx = stage.GetComponent<NomalStage>().stageIdx;
                    OnInStageButton();
                });
            }
        }

        // ==========================================
        // 💡 [추가] 보스 스테이지 활성화 로직
        // ==========================================
        if (bossStageBtn == null)
            Debug.LogError("보스 버튼이 없어 샤갈");
        if (bossStageBtn != null)
        {
            // 클리어한 스테이지 수가 전체 스테이지 수와 같거나 크다면 (모두 클리어)
            if (clearStageCnt >= StageButtons.Count)
            {
                bossStageBtn.gameObject.SetActive(true);
                bossStageBtn.interactable = true; // 버튼 클릭 활성화
                //bossStageBtn.image.color = Color.red; // 활성화 상태 색상 (원하시는 색으로 변경)

                bossStageBtn.onClick.RemoveAllListeners();
                bossStageBtn.onClick.AddListener(() =>
                {
                    // 보스 스테이지 씬 이동 함수 호출
                    OnInBossStageButton();
                });
            }
            else
            {
                // 아직 모두 클리어하지 못했다면
                bossStageBtn.interactable = false; // 버튼 클릭 비활성화
                bossStageBtn.image.color = Color.gray; // 비활성화 상태 색상 (회색)
            }
        }
    }

    private void StageChangeBtnClick()
    {
        StageCount++;
        player.transform.position = StageList[StageCount].transform.position;

        StageOutline.enabled = !StageOutline.enabled;
    }

    public void BackBtnClick()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void OnInStageButton()
    {
        SceneManager.LoadScene("IntegratedScene");
    }

    // [추가] 보스 스테이지 입장 시 호출할 함수
    public void OnInBossStageButton()
    {
        // 보스 스테이지 인덱스를 지정해주거나, 전용 보스 씬 이름으로 변경하세요.
        // StageSaveManager.CurrentStageIdx = 99; // 예시: 보스 스테이지 인덱스
        SceneManager.LoadScene("Stage1BossScene"); // 보스 씬 이름이 다르다면 변경 필요
    }
}