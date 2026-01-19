using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyTurnManager : MonoBehaviour
{
    public static EnemyTurnManager Instance;

    private bool _running = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!_running && !GameManager.PlayerTurn)
        {
            StartEnemyTurn();
        }
    }

    public void StartEnemyTurn()
    {
        // 중복 시작 방지
        if (_running) return;

        // 플레이어 턴일 때만 적 턴 시작 (원하면 유지)
        if (GameManager.PlayerTurn) return;

        _running = true;
        StartCoroutine(CoEnemyTurnSequence());
    }

    private IEnumerator CoEnemyTurnSequence()
    {
        Debug.Log("적 턴 시작");
        // _running = true;

        // 플레이어 턴 종료 -> 적 턴 시작
        // GameManager.PlayerTurn = false;

        // 현재 살아있는 Virus들 가져오기
        Virus[] enemies = FindObjectsOfType<Virus>();

        // 왼쪽부터 행동: spawnNum 기준 정렬
        System.Array.Sort(enemies, (a, b) => a.spawnNum.CompareTo(b.spawnNum));

        // 1) 이번 턴 행동 수행 (아이콘은 이 동안 바뀌지 않음)
        for (int i = 0; i < enemies.Length; i++)
        {
            Virus e = enemies[i];
            if (e == null) continue;
            if (!e.isActiveAndEnabled) continue;
            if (e.virusData != null && e.virusData.HpCnt <= 0) continue;

            yield return e.CoDoOneAction();
        }

        // 2) 모든 몬스터 행동 종료 후, 다음 행동 아이콘을 "한 번에" 갱신
        for (int i = 0; i < enemies.Length; i++)
        {
            Virus e = enemies[i];
            if (e == null) continue;
            if (!e.isActiveAndEnabled) continue;
            if (e.virusData != null && e.virusData.HpCnt <= 0) continue;

            // Virus에 public으로 만들어둔 함수
            e.RollNextActionAndUpdateIcon();
        }

        // 3) 플레이어 턴 복귀
        GameManager.PlayerTurn = true;

        _running = false;
        
        Debug.Log("적 턴 종료");
    }
    public void InitEnemyIntents()
    {
        StartCoroutine(CoInitEnemyIntents());
    }

    private IEnumerator CoInitEnemyIntents()
    {
        yield return null; // 1프레임 대기 (UI/스폰 안정화)

        Virus[] enemies = FindObjectsOfType<Virus>();
        System.Array.Sort(enemies, (a, b) => a.spawnNum.CompareTo(b.spawnNum));

        foreach (var e in enemies)
        {
            if (e == null) continue;
            if (!e.isActiveAndEnabled) continue;
            if (e.virusData != null && e.virusData.HpCnt <= 0) continue;

            e.RollNextActionAndUpdateIcon();
        }
    }

}
