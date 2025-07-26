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


    // �ʵ忡 �ִ� ���̷��� Ŭ����
    public void CleanVirus()
    {

    }

    // �ʵ忡 ���� ���� ����
    public void OnButtonSpawnVirus()
    {

    }

    // ���� ���� ����
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
