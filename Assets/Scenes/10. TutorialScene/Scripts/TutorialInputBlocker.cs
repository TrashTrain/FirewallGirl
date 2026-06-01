using UnityEngine;

/// <summary>
/// 대사 진행 중 게임 UI 전체 입력을 차단하는 투명 오버레이.
/// Canvas 최상단(단, TutorialDialogPanel 아래)에 배치하고
/// Image 컴포넌트의 Raycast Target = true, Color.a = 0 으로 설정할 것.
/// </summary>
public class TutorialInputBlocker : MonoBehaviour
{
    public void Lock() => gameObject.SetActive(true);
    public void Unlock() => gameObject.SetActive(false);
}
