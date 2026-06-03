using UnityEngine;

[CreateAssetMenu(menuName = "Augments/VirusHealCostDown")]
public class VirusHealCostDownAugment : AugmentBase
{
    [Header("설정")]
    public int healAmount = 5;
    public int costReduction = 1;

    // 바이러스 처치 회복을 이미 사용했는지
    private bool hasHealed = false;

    // 획득 즉시 최대 코스트 감소
    public override void OnEquip(BattleContext context)
    {
        if (context.player == null)
            return;

        context.player.AddPermanentStat(
            StatType.Cost,
            -costReduction
        );

        context.player.UpdateUI();

        Debug.Log(
            $"{augmentName} 획득\n" +
            $"최대 코스트 {costReduction} 감소"
        );
    }

    // 바이러스 처치 시
    public override void OnVirusKilled(BattleContext context)
    {
        if (hasHealed)
            return;

        if (context.player == null)
            return;

        hasHealed = true;

        context.player.currentHP = Mathf.Min(
            context.player.MaxHP,
            context.player.currentHP + healAmount
        );

        context.player.UpdateUI();

        Debug.Log(
            $"{augmentName} 발동!\n" +
            $"첫 바이러스 처치 보상으로 체력 +{healAmount}\n" +
            $"현재 체력 : {context.player.currentHP}/{context.player.MaxHP}"
        );
    }
}