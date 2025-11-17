using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StateImageChangeElite : MonoBehaviour
{
    public List<Sprite> stateImages;

    public void UpdateStateImage(Troy.State state)
    {
        //(Troy.State)gameObject.transform.parent.GetComponent<Troy>().RandState
        switch (state)
        {
            case Troy.State.Atk:
                GetComponent<Image>().sprite = stateImages[0];
                break;
            case Troy.State.Sup:
                GetComponent<Image>().sprite = stateImages[1];
                break;
            case Troy.State.Def:
                GetComponent<Image>().sprite = stateImages[2];
                break;
        }

    }
}