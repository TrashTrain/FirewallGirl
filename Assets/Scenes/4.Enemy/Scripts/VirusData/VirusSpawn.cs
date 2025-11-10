using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirusSpawn : MonoBehaviour
{

    public GameObject[] spawns;

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
    }

    public void SpawnVirus(int virusIdx)
    {
        //prefabVirus.GetComponent<Troy>().virusObjectSO = VirusMgr.instance
        int rand = Random.Range(0, prefabVirus.Length);

        GameObject virusInstance = Instantiate(prefabVirus[rand], spawns[virusIdx].transform);
    }



    // 필드에 있는 바이러스 클리어
    public void CleanVirus()
    {
        for (int i = 0; i < spawns.Length; i++)
        {
            Destroy(spawns[i].transform.GetChild(0).gameObject);
        }

    }

    // 필드에 랜덤 몬스터 스폰
    public void OnButtonSpawnVirus()
    {
        int spawnIdx = spawns.Length;
        for (int i = 0; i < spawnIdx; i++)
        {
            if (spawns[i].transform.childCount == 0)
                SpawnVirus(i);
        }
    }

    // 몬스터 갯수 세기
    public int GetVirusCount()
    {
        int count = 0;
        
        count += spawns.Length;

        Debug.Log("count : " + count);
        return count;
    }
}
