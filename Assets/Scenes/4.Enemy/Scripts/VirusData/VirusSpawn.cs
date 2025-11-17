using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirusSpawn : MonoBehaviour
{

    public List<Transform> spawns = new();
    public List<EnemyUIController> enemyUIController = new();

    public GameObject[] prefabVirus;

    public static VirusSpawn instance;


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
        GetVirusCount();
        OnButtonSpawnVirus();
    }
    
    public void SpawnVirus(int virusIdx)
    {
        //prefabVirus.GetComponent<Troy>().virusObjectSO = VirusMgr.instance
        int rand = Random.Range(0, prefabVirus.Length);

        GameObject virusInstance = Instantiate(prefabVirus[rand], spawns[virusIdx]);
        if(virusInstance.TryGetComponent(out Virus virus))
        {
            enemyUIController[virusIdx].panel.SetActive(true);
            virus.enemyUIController = enemyUIController[virusIdx];
            virus.InitData();
            Debug.LogError("virusHP : " + virus.virusData.HpCnt);
            virus.enemyUIController.healthBar.UpdateHPBar(virus.virusData.HpCnt, virus.virusData.HpCnt);
            virus.enemyUIController.atk.text = virus.virusData.AtkDmg.ToString();
            //virus.enemyUIController.hp.text = virus.virusData.ToString();

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
            if(spawns[i].childCount > 0)
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

        for (int i = 0; i < spawnIdx; i++)
        {
            if (spawns[i].childCount == 0)
            {
                SpawnVirus(i);
            }
                
        }
        GameManager.Instance.enemyCount = spawnIdx;
    }

    // 몬스터 갯수 세기
    public int GetVirusCount()
    {
        int count = 0;
        
        count += spawns.Count;

        Debug.Log("count : " + count);
        return count;
    }
}
