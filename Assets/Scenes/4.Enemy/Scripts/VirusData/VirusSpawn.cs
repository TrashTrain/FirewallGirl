using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirusSpawn : MonoBehaviour
{

    public GameObject spawn1;
    public GameObject spawn2;
    public GameObject spawn3;

    public GameObject prefabVirus;

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

    public void SpawnVirus()
    {
        //prefabVirus.GetComponent<Troy>().virusObjectSO = VirusMgr.instance
    }


    // 필드에 있는 바이러스 클리어
    public void CleanVirus()
    {

    }

    // 필드에 랜덤 몬스터 스폰
    public void OnButtonSpawnVirus()
    {

    }

    // 몬스터 갯수 세기
    public int GetVirusCount()
    {
        int count = 0;
        count += spawn1.GetComponent<Transform>().childCount;
        count += spawn2.GetComponent<Transform>().childCount;
        count += spawn3.GetComponent<Transform>().childCount;
        Debug.Log("count : " + count);
        return count;
    }
}
