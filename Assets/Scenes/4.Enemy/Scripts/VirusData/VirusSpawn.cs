using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirusSpawn : MonoBehaviour
{
    public List<Transform> spawns = new();
    public List<EnemyUIController> enemyUIController = new();

    public GameObject[] prefabVirus;

    // [추가] 보스 스테이지 관련 변수
    [Header("Boss Stage Settings")]
    public bool isBossStage = false; // 현재 씬/스테이지가 보스전인지 체크
    public GameObject bossPrefab;    // 생성할 보스 몬스터 프리팹

    public static VirusSpawn instance;

    public int virusCnt = 0;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        OnButtonSpawnVirus();
        GetVirusCount();
    }

    // [수정] 특정 프리팹을 지정해서 소환할 수 있도록 매개변수(specificPrefab) 추가
    public void SpawnVirus(int virusIdx, GameObject specificPrefab = null)
    {
        GameObject targetPrefab = specificPrefab;

        // 특정 프리팹이 지정되지 않았다면 기존처럼 랜덤 일반 몬스터 선택
        if (targetPrefab == null)
        {
            int rand = Random.Range(0, prefabVirus.Length);
            targetPrefab = prefabVirus[rand];
        }

        GameObject virusInstance = Instantiate(targetPrefab, spawns[virusIdx]);

        if (virusInstance.TryGetComponent(out Virus virus))
        {
            enemyUIController[virusIdx].panel.SetActive(true);
            virus.enemyUIController = enemyUIController[virusIdx];
            virus.InitData();
            Debug.LogError("virusHP : " + virus.virusData.HpCnt);
            virus.enemyUIController.healthBar.UpdateHPBar(virus.virusData.HpCnt, virus.virusData.HpCnt);
            virus.enemyUIController.atk.text = virus.virusData.AtkDmg.ToString();
            virus.enemyUIController.def.text = virus.virusData.DefCnt.ToString();
            //virus.enemyUIController.hp.text = virus.virusData.ToString();

            virus.enemyUIController.state.UpdateStateImage(virus.NextAction);
            Debug.Log("생성호출: " + targetPrefab.name);
        }
        else
        {
            enemyUIController[virusIdx].panel.SetActive(false);
        }
    }

    // 필드에 있는 바이러스 클리어
    public void CleanVirus()
    {
        for (int i = 0; i < spawns.Count; i++)
        {
            if (spawns[i].childCount > 0)
            {
                enemyUIController[i].panel.SetActive(false);
                Destroy(spawns[i].GetChild(0).gameObject);
            }
        }
    }

    // 필드에 랜덤 몬스터 스폰
    public void OnButtonSpawnVirus()
    {
        int spawnIdx = spawns.Count;

        // =========================================================
        // 💡 [수정] 보스 스테이지일 경우와 일반 스테이지일 경우 분리
        // =========================================================
        if (isBossStage)
        {
            // 보스 스테이지: 3번째 스폰 위치(인덱스 2)에 보스 1마리만 소환
            if (spawns.Count >= 3 && spawns[2].childCount == 0)
            {
                SpawnVirus(2, bossPrefab); // 2번 인덱스(3번째 칸)에 보스 프리팹 소환
            }
            GameManager.Instance.enemyCount = 1; // 적 숫자는 보스 1마리
        }
        else
        {
            // 일반 스테이지: 기존 로직 그대로 모든 빈 칸에 일반 몬스터 랜덤 소환
            for (int i = 0; i < spawnIdx; i++)
            {
                if (spawns[i].childCount == 0)
                {
                    SpawnVirus(i); // 특정 프리팹을 넘기지 않으면 랜덤 소환됨
                }
            }
            GameManager.Instance.enemyCount = spawnIdx;
        }
    }

    // 몬스터 갯수 세기
    public int GetVirusCount()
    {
        virusCnt += spawns.Count;

        Debug.Log("count : " + virusCnt);
        return virusCnt;
    }

    public int SetDiscountVirusCount()
    {
        return --virusCnt;
    }

    public IEnumerator GetReward()
    {
        yield return new WaitForSeconds(1f);

        UIManager.Ins.choicePanel.ShowPanel();
    }
}