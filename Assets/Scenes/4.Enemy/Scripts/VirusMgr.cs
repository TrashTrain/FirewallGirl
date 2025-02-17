using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirusMgr : MonoBehaviour
{
 
    public static VirusMgr instance;

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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
