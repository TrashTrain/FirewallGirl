using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    static public bool PlayerTurn = false;
    static public GameManager Instance = null;

    public int enemyCount = 0;
    
    public GameObject gameOverPanel;

   // private bool checkTurn;
    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
        PlayerTurn = true;
        
        // 게임 시작 시 첫 턴 전처리
        if (PlayerManager.instance != null)
        {
            PlayerManager.instance.OnTurnEndProcess();
        }
        

        //checkTurn = PlayerTurn;
    }

    private void Start()
    {
        if (EnemyTurnManager.Instance != null)
        {
            EnemyTurnManager.Instance.InitEnemyIntents();
        }
    }
    public void OnTrunButtonClick()
    {
        // true -> PlayerTurn으로 바꾸기
        //if (PlayerTurn)
        //{
        //    Debug.Log("턴 넘기기 성공");
        //    PlayerTurn = !PlayerTurn;
        //    SequenceTurn.instance.SetResetSequenceCheck();

        //}
        if (!PlayerTurn) return; // 이미 적턴이면 무시
        
        // 플레이어 턴 구현
        PlayerManager.instance.StartPlayerTurn();

        // EnemyTurnManager.Instance.StartEnemyTurn();
    }

    public void GameOver()
    {
        Debug.Log("게임 오버!");
        // StartCoroutine(GameOverSequence());
        
        // 1. 게임 진행 멈추기 (모든 움직임과 Update 정지)
        Time.timeScale = 0f; 

        // 2. 게임오버 창 띄우기
        if (gameOverPanel != null)
        {
            StartCoroutine(ShowGameOverUIAnim());
        }
    }
    
    private IEnumerator ShowGameOverUIAnim()
    {
        // CanvasGroup이 없으면 코드로 자동 추가 (투명도 조절용)
        CanvasGroup canvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
        }

        // 초기 상태 세팅 (완전 투명, 원래 크기보다 1.3배 크게)
        canvasGroup.alpha = 0f;
        gameOverPanel.transform.localScale = new Vector3(1.3f, 1.3f, 1f);
        gameOverPanel.SetActive(true);

        float duration = 0.3f; // 애니메이션 진행 시간 (0.3초)
        float elapsed = 0f;

        // Time.timeScale이 0이므로, 현실 시간 기준인 unscaledDeltaTime을 사용합니다.
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            
            // Ease-Out 효과 (처음엔 빠르고 끝날 때 부드럽게 감속)
            float easeT = 1f - Mathf.Pow(1f - t, 3f);

            // 투명도와 크기를 서서히 목표값으로 변경
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, easeT);
            gameOverPanel.transform.localScale = Vector3.Lerp(new Vector3(1.3f, 1.3f, 1f), Vector3.one, easeT);

            yield return null; // 다음 프레임까지 대기
        }

        // 루프가 끝난 후 목표 상태로 정확히 고정
        canvasGroup.alpha = 1f;
        gameOverPanel.transform.localScale = Vector3.one;
    }
    
    public void GoToMainMenu()
    {
        // 멈췄던 시간을 다시 정상화
        Time.timeScale = 1f;

        // StageMgr의 스테이지 초기화 함수 호출
        if (StageMgr.Instance != null)
        {
            StageMgr.Instance.OnResetStageInfo();
        }
        else
        {
            // 전투 씬이라 StageMgr가 존재하지 않는다면 데이터베이스(SaveManager)라도 직접 초기화
            StageSaveManager.ResetStage(); 
        }

        // 메인 메뉴 씬으로 전환 (본인 프로젝트의 씬 이름에 맞게 문자열을 수정하세요!)
        SceneManager.LoadScene("MainScene"); 
    }
    
    public void QuitGame()
    {
        Debug.Log("게임 완전 종료");
        
        // 에디터에서는 동작하지 않고, 실제 빌드된 게임에서만 프로그램이 종료됩니다.
        Application.Quit();
    }

    private IEnumerator GameOverSequence()
    {
        yield return new WaitForSeconds(3f);
        SceneLoader.LoadStageScene();
    }
}

// 바이러스 SO 데이터 모음
public class VirusSOData
{
    public List<VirusObjectSO> virusObjectList;

}
