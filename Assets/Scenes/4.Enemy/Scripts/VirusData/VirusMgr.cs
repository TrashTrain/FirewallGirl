using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirusMgr : MonoBehaviour
{
 
    public List<VirusObjectSO> viruses;

    public static VirusMgr instance;

    void Start()
    {
        //var rand = Random.Range(1, 3);
        //Debug.Log(rand);
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

}
