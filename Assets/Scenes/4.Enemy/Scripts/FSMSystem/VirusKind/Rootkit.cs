using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rootkit : Virus
{
    private const int SelfDefGain = 8;

    protected override Dictionary<string, string> GetActionDescriptions() =>
        new Dictionary<string, string>
        {
            { "Atk", "플레이어의 방어도를 무시하는 관통 공격을 가합니다." },
            { "Def", $"방어도를 {SelfDefGain} 획득합니다." },
        };

    // 50% 관통 공격, 50% 방어도 대량 획득
    protected override void RollNextAction()
    {
        NextAction = Random.Range(0, 2) == 0 ? State.Atk : State.Def;
    }

    protected override IEnumerator CoRunStateAction(State s)
    {
        switch (s)
        {
            case State.Atk:
                yield return CoPenetrateAttack();
                break;
            case State.Def:
                yield return CoRootHide();
                break;
            default:
                yield break;
        }
    }

    // 방어도 무시 관통 공격 — TakeDamage() 우회, currentHP에 직접 피해
    private IEnumerator CoPenetrateAttack()
    {
        Transform playerTr = PlayerManager.instance.transform;
        if (playerTr == null) yield break;

        Vector3 start = _originPos;
        Vector3 target = playerTr.position;
        target.y = start.y;

        yield return LerpPos(start, target, 0.3f);

        PlayerManager.instance.currentHP = Mathf.Max(0, PlayerManager.instance.currentHP - virusData.AtkDmg);
        PlayerManager.instance.UpdateUI();

        if (PlayerManager.instance.currentHP <= 0)
            GameManager.Instance.GameOver();

        yield return new WaitForSeconds(0.1f);
        yield return LerpPos(target, start, 0.3f);
        transform.position = start;
    }

    // 자신 방어도 대량 획득
    private IEnumerator CoRootHide()
    {
        virusData.DefCnt += SelfDefGain;
        UpdateData();

        Vector3 start = _originPos;
        Vector3 up = start + new Vector3(0f, 0.20f, 0f);

        yield return LerpPos(start, up, 0.3f);
        yield return LerpPos(up, start, 0.3f);
        transform.position = start;
    }
}
