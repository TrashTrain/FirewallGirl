using System.Collections;
using UnityEngine;

public class MemoryLeak : Virus
{
    private const int SelfHealAmount = 3;
    private const int SelfDefGain = 2;

    // 40% 공격(코스트 감소), 60% 자가 복구
    protected override void RollNextAction()
    {
        NextAction = Random.Range(0, 100) < 40 ? State.Atk : State.Def;
    }

    protected override IEnumerator CoRunStateAction(State s)
    {
        switch (s)
        {
            case State.Atk:
                yield return CoLeakAttack();
                break;
            case State.Def:
                yield return CoSelfRecover();
                break;
            default:
                yield break;
        }
    }

    // 공격 후 다음 플레이어 턴 코스트 -1
    private IEnumerator CoLeakAttack()
    {
        yield return CoAttack();

        if (PlayerManager.instance != null)
        {
            // duration 2: OnTurnEndProcess에서 1회 차감되므로 실제로 다음 플레이어 턴 1번에만 영향
            PlayerManager.instance.AddMultiTurnStat(StatType.Cost, -1, 2, "코스트 감소");
        }
    }

    // 자신 체력 회복 + 방어도 획득
    private IEnumerator CoSelfRecover()
    {
        virusData.CurHpCnt = Mathf.Min(virusData.HpCnt, virusData.CurHpCnt + SelfHealAmount);
        virusData.DefCnt += SelfDefGain;
        UpdateData();

        Vector3 start = _originPos;
        Vector3 up = start + new Vector3(0f, 0.20f, 0f);

        yield return LerpPos(start, up, 0.3f);
        yield return LerpPos(up, start, 0.3f);

        transform.position = start;
    }
}
