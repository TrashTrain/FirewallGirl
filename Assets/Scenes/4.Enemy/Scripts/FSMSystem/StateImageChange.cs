using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// [추가] 인스펙터에서 상태(State)와 이미지(Sprite)를 한 쌍으로 묶어서 보여주기 위한 구조체
[System.Serializable]
public struct StateIconMapping
{
    public Virus.State state; // 공통으로 사용하는 State 열거형 (Troy.State가 아님)
    public Sprite iconSprite;
}

public class StateImageChange : MonoBehaviour
{
    [Header("상태별 아이콘 맵핑")]
    // 기존 List<Sprite> 대신 구조체 리스트를 사용하여 인덱스 순서에 의존하지 않게 만듭니다.
    public List<StateIconMapping> stateIcons;

    private Image _imageComponent;

    private void Awake()
    {
        _imageComponent = GetComponent<Image>();
    }

    /// <summary>
    /// 몬스터의 상태를 전달받아 그에 맞는 아이콘으로 UI를 갱신합니다.
    /// </summary>
    /// <param name="state">변경할 몬스터의 다음 행동 상태</param>
    public void UpdateStateImage(Virus.State state)
    {
        if (_imageComponent == null) return;

        // 리스트를 순회하며 요청받은 상태와 짝지어진 이미지를 찾아 적용합니다.
        foreach (var mapping in stateIcons)
        {
            if (mapping.state == state)
            {
                Debug.Log("mapping state : " + mapping.state);
                _imageComponent.sprite = mapping.iconSprite;
                return; // 찾았으니 함수 종료
            }
        }

        // 만약 인스펙터에 해당 상태의 이미지를 넣지 않았다면 경고를 띄워줍니다.
        Debug.LogWarning($"[StateImageChange] {state} 상태에 해당하는 아이콘이 설정되지 않았습니다! (오브젝트명: {gameObject.name})");
    }
}