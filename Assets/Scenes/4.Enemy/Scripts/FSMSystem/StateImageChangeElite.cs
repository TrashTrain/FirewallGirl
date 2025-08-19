using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateImageChangeElite : MonoBehaviour
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
        switch ((EliteVirus.State)gameObject.transform.parent.GetComponent<EliteVirus>().RandState)
        {
            case EliteVirus.State.Atk:
                GetComponent<SpriteRenderer>().sprite = stateImages[0];
                break;
            case EliteVirus.State.Sup:
                GetComponent<SpriteRenderer>().sprite = stateImages[1];
                break;
            case EliteVirus.State.Def:
                GetComponent<SpriteRenderer>().sprite = stateImages[2];
                break;
        }

    }
}