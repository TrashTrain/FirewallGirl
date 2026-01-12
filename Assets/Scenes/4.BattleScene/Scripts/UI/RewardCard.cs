using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardCard : MonoBehaviour
{
    [Header("이 카드의 UI 컴포넌트들")]
    public Image positiveIcon;
    public Image negativeIcon;
    public TextMeshProUGUI positiveSkillText;
    public TextMeshProUGUI negativeSkillText;
    public TextMeshProUGUI positiveValueText;
    public TextMeshProUGUI negativeValueText;

    // UpDownMgr에서 호출하여 UI를 갱신
    public void SetupCard(UpDownMgr.GenerateCard pos, UpDownMgr.GenerateCard neg, Sprite posSprite, Sprite negSprite)
    {
        // 텍스트 적용
        if (positiveSkillText != null) positiveSkillText.text = pos.stat.ToKorean();
        if (positiveValueText != null) positiveValueText.text = pos.ToString();

        if (negativeSkillText != null) negativeSkillText.text = neg.stat.ToKorean();
        if (negativeValueText != null) negativeValueText.text = neg.ToString();

        // 스프라이트 적용
        if (positiveIcon != null) positiveIcon.sprite = posSprite;
        if (negativeIcon != null) negativeIcon.sprite = negSprite;

        Debug.Log($"{gameObject.name} UI 갱신 완료");
    }
}