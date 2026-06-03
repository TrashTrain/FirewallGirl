using UnityEngine;

[CreateAssetMenu(menuName = "Augments/DefUpHpDownOnCard")]
public class DefUpHpDownOnCardAugment : AugmentBase
{
    [Header("설정")]
    public int defenseBonus = 5;
    public int cardsPerPenalty = 3;
    public int hpLoss = 1;

    private int cardUseCount = 0;

    public override void OnEquip(BattleContext context)
    {
        if (context.player == null)
            return;

        context.player.AddPermanentStat(
            StatType.Defense,
            defenseBonus
        );

        context.player.UpdateUI();

        Debug.Log(
            $"{augmentName} 획득\n" +
            $"방어력 +{defenseBonus}"
        );
    }

    public override void OnCardUsed(
        BattleContext context,
        PlayerCard usedCard)
    {
        if (context.player == null)
            return;

        cardUseCount++;

        if (cardUseCount % cardsPerPenalty == 0)
        {
            context.player.currentHP =
                Mathf.Max(
                    1,
                    context.player.currentHP - hpLoss
                );

            context.player.UpdateUI();

            Debug.Log(
                $"{augmentName} 발동\n" +
                $"누적 카드 사용 {cardUseCount}장\n" +
                $"체력 -{hpLoss}"
            );
        }
    }
}