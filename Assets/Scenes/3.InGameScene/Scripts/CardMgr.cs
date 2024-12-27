using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardMgr : MonoBehaviour
{
    public PlayerCardObject[] CardObject;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // 마우스 클릭시 
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);

            if (hit.collider != null)
            {
                GameObject click_obj = hit.transform.gameObject;
                Transform nameTMPTransform = click_obj.transform.Find("NameTMP");
                TextMeshProUGUI tmpText = nameTMPTransform.GetComponentInChildren<TextMeshProUGUI>();

                Debug.Log(hit.collider.gameObject+" \n"+tmpText);
            }
        }
    }
}
