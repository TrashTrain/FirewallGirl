using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardCard : MonoBehaviour
{
    [Header("�� ī���� UI ������Ʈ��")]
    public Image positiveIcon;
    public Image negativeIcon;
    public TextMeshProUGUI positiveSkillText;
    public TextMeshProUGUI negativeSkillText;
    public TextMeshProUGUI positiveValueText;
    public TextMeshProUGUI negativeValueText;

    // UpDownMgr���� ȣ���Ͽ� UI�� ����
    public void SetupCard(UpDownMgr.GenerateCard pos, UpDownMgr.GenerateCard neg, Sprite posSprite, Sprite negSprite)
    {
        // �ؽ�Ʈ ����
        if (positiveSkillText != null) positiveSkillText.text = pos.stat.ToKorean();
        if (positiveValueText != null) positiveValueText.text = pos.ToString();

        if (negativeSkillText != null) negativeSkillText.text = neg.stat.ToKorean();
        if (negativeValueText != null) negativeValueText.text = neg.ToString();

        // ��������Ʈ ����
        if (positiveIcon != null) positiveIcon.sprite = posSprite;
        if (negativeIcon != null) negativeIcon.sprite = negSprite;

        Debug.Log($"{gameObject.name} UI ���� �Ϸ�");
    }
    
    public void SetupAugmentCard(AugmentBase augment)
    {
        // 1. 긍정(Positive) UI 요소들을 증강체용(이름, 설명, 아이콘)으로 재활용합니다.
        if (positiveSkillText != null) positiveSkillText.text = augment.augmentName;
        if (positiveValueText != null) positiveValueText.text = augment.description; // 스탯 수치 대신 설명을 넣음
        if (positiveIcon != null) positiveIcon.sprite = augment.icon;

        // 2. 사용하지 않는 부정(Negative) UI 요소들은 화면에서 숨깁니다.
        if (negativeSkillText != null) negativeSkillText.gameObject.SetActive(false);
        if (negativeValueText != null) negativeValueText.gameObject.SetActive(false);
        if (negativeIcon != null) negativeIcon.gameObject.SetActive(false);

        Debug.Log($"{gameObject.name} 증강체 UI 갱신 완료: {augment.augmentName}");
    }
}