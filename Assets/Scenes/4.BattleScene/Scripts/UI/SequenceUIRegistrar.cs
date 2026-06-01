using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 씬 전환 후 PlayerManager의 시퀀스 UI 참조를 재연결한다.
/// CostUI/HealthBar/PowerUI와 동일한 자기등록 패턴을 사용한다.
/// </summary>
public class SequenceUIRegistrar : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI sequenceSummaryText;
    [SerializeField] private Button executeCombinationBtn;

    private void Start()
    {
        if (PlayerManager.instance == null) return;

        PlayerManager.instance.sequenceContainer     = transform;
        PlayerManager.instance.sequenceSummaryText   = sequenceSummaryText;
        PlayerManager.instance.executeCombinationBtn = executeCombinationBtn;

        // onClick 리스너를 코드로 재등록 (Inspector 할당은 씬 전환 후 끊김)
        executeCombinationBtn.onClick.RemoveAllListeners();
        executeCombinationBtn.onClick.AddListener(PlayerManager.instance.ExecuteCombination);
    }
}
