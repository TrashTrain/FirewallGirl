using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ransomware : Virus
{
    private const int RequiredCost = 3;
    private const int PenaltyDamage = 10;

    [SerializeField] private Sprite _debuffIcon;

    protected override Dictionary<string, string> GetActionDescriptions() =>
        new Dictionary<string, string>
        {
            { "Atk", "직접 공격을 가합니다." },
            { "Debuf", $"다음 플레이어 턴에 코스트를 {RequiredCost} 이상 사용하지 않으면 체력 {PenaltyDamage}을 잃습니다." },
        };

    // 30% 직접 공격, 70% 몸값 요구
    protected override void RollNextAction()
    {
        NextAction = Random.Range(0, 100) < 30 ? State.Atk : State.Debuf;
    }

    protected override IEnumerator CoRunStateAction(State s)
    {
        switch (s)
        {
            case State.Atk:
                yield return CoAttack();
                break;
            case State.Debuf:
                yield return CoApplyRansomware();
                break;
            default:
                yield break;
        }
    }

    private IEnumerator CoApplyRansomware()
    {
        yield return new WaitForSeconds(0.5f);

        bool isDebuffActive = true;

        ActiveEffect effect = new ActiveEffect(
            _debuffIcon,
            false,
            () => isDebuffActive,
            () => $"요구: 코스트 {RequiredCost} 이상 사용\n미충족 시 체력 {PenaltyDamage} 피해"
        );
        PlayerManager.instance.RegisterEffect(effect);
        PlayerManager.instance.UpdateUI();

        System.Action handler = null;
        handler = () =>
        {
            int costSpent = PlayerManager.instance.TotalCost - PlayerManager.instance.currentCost;

            if (costSpent < RequiredCost)
            {
                PlayerManager.instance.currentHP = Mathf.Max(0, PlayerManager.instance.currentHP - PenaltyDamage);
                Debug.Log($"[Ransomware] 요구 미충족 (사용 코스트: {costSpent}) → 체력 {PenaltyDamage} 피해");

                if (PlayerManager.instance.currentHP <= 0)
                    GameManager.Instance.GameOver();
            }
            else
            {
                Debug.Log($"[Ransomware] 요구 충족 (사용 코스트: {costSpent}) → 피해 없음");
            }

            isDebuffActive = false;
            PlayerManager.instance.UnregisterEffect(effect);
            PlayerManager.instance.UpdateUI();
            EnemyTurnManager.OnPlayerTurnEnded -= handler;
        };

        EnemyTurnManager.OnPlayerTurnEnded += handler;

        yield return new WaitForSeconds(0.5f);
    }
}
