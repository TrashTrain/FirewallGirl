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
    public GameObject gameClearPanel;

   // private bool checkTurn;
    void Awake()
    {
        Instance = this;
        PlayerTurn = true;
        // UI 의존 초기화는 모든 Awake() 완료 후 Start()에서 처리 (CardDeckController, PlayerStatusUI 등)
    }

    private void Start()
    {
        if (PlayerManager.instance != null)
        {
            PlayerSaveData pending = PlayerStateSaveManager.instance.PendingRestoreData;
            if (pending != null)
            {
                // 이어하기 경로: 스탯·증강체·덱 복원
                PlayerManager.instance.RestoreFromSave(pending);
                PlayerManager.instance.RestoreAugments(pending.augmentIds);

                if (CardDatabaseManager.instance == null)
                {
                    // DeckBuildingScene을 거치지 않으므로 CDM이 없음 — 동적 생성
                    // AddComponent 즉시 Awake()가 실행되어 카드 딕셔너리가 채워짐
                    var go = new GameObject("CardDatabaseManager");
                    go.AddComponent<CardDatabaseManager>();
                }
                CardDatabaseManager.instance.RestoreDeckFromSave(pending.deck);
                PlayerStateSaveManager.instance.ClearPendingRestore();
            }

            // 이어하기/일반 경로 공통: Start()에서 실행하므로 handContainer·PlayerStatusUI 재연결 완료 보장
            PlayerManager.instance.InitializeBattleDeck();
            PlayerManager.instance.OnTurnEndProcess();
        }

        if (EnemyTurnManager.Instance != null)
        {
            EnemyTurnManager.Instance.InitEnemyIntents();
        }

        // 튜토리얼 중에는 TutorialManager가 타이밍을 제어하므로 자동 드로우를 건너뜀
        if (PlayerManager.instance != null && TutorialManager.instance == null)
        {
            PlayerManager.instance.PreparePlayerTurn();
        }
    }
    public void OnTrunButtonClick()
    {
        // true -> PlayerTurn���� �ٲٱ�
        //if (PlayerTurn)
        //{
        //    Debug.Log("�� �ѱ�� ����");
        //    PlayerTurn = !PlayerTurn;
        //    SequenceTurn.instance.SetResetSequenceCheck();

        //}
        if (!PlayerTurn) return; // �̹� �����̸� ����
        
        // �÷��̾� �� ����
        PlayerManager.instance.StartPlayerTurn();

        // EnemyTurnManager.Instance.StartEnemyTurn();
    }

    public void GameOver()
    {
        Debug.Log("���� ����!");
        // StartCoroutine(GameOverSequence());
        
        // 1. ���� ���� ���߱� (��� �����Ӱ� Update ����)
        Time.timeScale = 0f; 

        // 2. ���ӿ��� â ����
        if (gameOverPanel != null)
        {
            StartCoroutine(ShowGameOverUIAnim());
        }
    }
    
    private IEnumerator ShowGameOverUIAnim()
    {
        // CanvasGroup�� ������ �ڵ�� �ڵ� �߰� (������ ������)
        CanvasGroup canvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
        }

        // �ʱ� ���� ���� (���� ����, ���� ũ�⺸�� 1.3�� ũ��)
        canvasGroup.alpha = 0f;
        gameOverPanel.transform.localScale = new Vector3(1.3f, 1.3f, 1f);
        gameOverPanel.SetActive(true);

        float duration = 0.3f; // �ִϸ��̼� ���� �ð� (0.3��)
        float elapsed = 0f;

        // Time.timeScale�� 0�̹Ƿ�, ���� �ð� ������ unscaledDeltaTime�� ����մϴ�.
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            
            // Ease-Out ȿ�� (ó���� ������ ���� �� �ε巴�� ����)
            float easeT = 1f - Mathf.Pow(1f - t, 3f);

            // �������� ũ�⸦ ������ ��ǥ������ ����
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, easeT);
            gameOverPanel.transform.localScale = Vector3.Lerp(new Vector3(1.3f, 1.3f, 1f), Vector3.one, easeT);

            yield return null; // ���� �����ӱ��� ���
        }

        // ������ ���� �� ��ǥ ���·� ��Ȯ�� ����
        canvasGroup.alpha = 1f;
        gameOverPanel.transform.localScale = Vector3.one;
    }
    
    public void GameClear()
    {
        Debug.Log("���� Ŭ����!");
        Time.timeScale = 0f; // ���� ����

        if (gameClearPanel != null)
        {
            StartCoroutine(ShowGameClearUIAnim());
        }
    }
    
    private IEnumerator ShowGameClearUIAnim()
    {
        CanvasGroup canvasGroup = gameClearPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameClearPanel.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        // ���� ũ���� ����(0.5��)���� ����
        gameClearPanel.transform.localScale = new Vector3(0.5f, 0.5f, 1f); 
        gameClearPanel.SetActive(true);

        float duration = 0.5f; // Ŭ����� ���� �� �����Ӱ� (0.5��)
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            // �������� �ε巴�� ������ (�Ϲ� Ease-out)
            float alphaEase = 1f - Mathf.Pow(1f - t, 3f);
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, alphaEase);

            // ũ��� ���� Ƣ�� Ease-Out Back ���� ���� (��ǥġ�� 1�� ��¦ �Ѿ��ٰ� ���ƿ�)
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            float t2 = t - 1f;
            float bounceEase = 1f + c3 * Mathf.Pow(t2, 3f) + c1 * Mathf.Pow(t2, 2f);

            // LerpUnclamped�� ����ؾ� 1�� �ʰ��ϴ� ũ��(�ٿ ȿ��)�� ���������� ����˴ϴ�.
            gameClearPanel.transform.localScale = Vector3.LerpUnclamped(new Vector3(0.5f, 0.5f, 1f), Vector3.one, bounceEase);

            yield return null;
        }

        canvasGroup.alpha = 1f;
        gameClearPanel.transform.localScale = Vector3.one;
    }
    
    public void GoToMainMenu()
    {
        // ����� �ð��� �ٽ� ����ȭ
        Time.timeScale = 1f;

        // StageMgr�� �������� �ʱ�ȭ �Լ� ȣ��
        if (StageMgr.Instance != null)
        {
            StageMgr.Instance.OnResetStageInfo();
        }
        else
        {
            // ���� ���̶� StageMgr�� �������� �ʴ´ٸ� �����ͺ��̽�(SaveManager)�� ���� �ʱ�ȭ
            StageSaveManager.ResetStage(); 
        }

        // ���� �޴� ������ ��ȯ (���� ������Ʈ�� �� �̸��� �°� ���ڿ��� �����ϼ���!)
        SceneManager.LoadScene("MainScene"); 
    }
    
    public void QuitGame()
    {
        // 스테이지 진입 시점 스냅샷으로 저장 (진행 중 변화는 버리고 직전 상태 보존)
        PlayerSaveData snapshot = PlayerStateSaveManager.instance.StageEntrySnapshot;
        if (snapshot != null)
        {
            PlayerSaveData quitData = new PlayerSaveData
            {
                currentHP       = snapshot.currentHP,
                fullHP          = snapshot.fullHP,
                baseStats       = snapshot.baseStats,
                augmentIds      = new List<string>(snapshot.augmentIds),
                deck            = new List<CardSaveData>(snapshot.deck),
                clearedStageIds = new List<int>(snapshot.clearedStageIds),
                resumeStageIndex = StageSaveManager.CurrentStageIdx
            };
            PlayerStateSaveManager.instance.Save(quitData);
        }
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private IEnumerator GameOverSequence()
    {
        yield return new WaitForSeconds(3f);
        SceneLoader.LoadStageScene();
    }
}

// ���̷��� SO ������ ����
public class VirusSOData
{
    public List<VirusObjectSO> virusObjectList;

}
