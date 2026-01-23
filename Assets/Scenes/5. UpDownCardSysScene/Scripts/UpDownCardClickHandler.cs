using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UpDownCardClickHandler : MonoBehaviour, IPointerClickHandler
{
    private int index;
    private System.Action<int> onClick;

    public void Init(int index, System.Action<int> onClick)
    {
        this.index = index;
        this.onClick = onClick;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke(index);
    }
}
