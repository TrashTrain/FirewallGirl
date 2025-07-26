using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateImageChange : MonoBehaviour
{
    public List<Sprite> stateImages;

    /* È®ÀÎ¿ë
     *     public enum State
    {
        Idle,
        Atk,
        Def,
        Sup,
        Death
    }
     */
    private void Update()
    {
        switch (gameObject.transform.parent.GetComponent<Troy>().GetState())
        {
            case Troy.State.Atk:
                GetComponent<SpriteRenderer>().sprite = stateImages[0];
                break;
            case Troy.State.Sup:
                GetComponent<SpriteRenderer>().sprite = stateImages[1];
                break;
            case Troy.State.Def:
                GetComponent<SpriteRenderer>().sprite = stateImages[2];
                break;
        }

    }
}
