using System.Collections;
using UnityEngine;

/// <summary>
/// 오버클럭.BIOS 1페이즈 분신 — '오버클럭 - 디코이'
/// 5턴 후 자폭, 처치 당하면 본체에 패널티 적용.
/// </summary>
public class OverclockDecoy : Virus
{
    private BossOverclock _boss;
    private int _enhanceStack = 0;
    private int _actedTurns = 0;

    protected override void Start()
    {
        if (VirusMgr.instance == null) return;
        InitData();

        _boss = FindObjectOfType<BossOverclock>();

        if (enemyUIController != null)
        {
            enemyUIController.panel.SetActive(true);
            UpdateData();
        }

        RollNextActionAndUpdateIcon();
    }

    // ─── 행동 결정 ────────────────────────────────────────────────

    protected override void RollNextAction()
    {
        // 4턴 행동 완료 → 다음 턴이 5번째(자폭)
        if (_actedTurns >= 4)
        {
            NextAction = State.Bomb;
            return;
        }
        NextAction = Random.Range(0, 2) == 0 ? State.Atk : State.Debuf;
    }

    // ─── 행동 실행 ────────────────────────────────────────────────

    protected override IEnumerator CoRunStateAction(State s)
    {
        _actedTurns++;

        // 5번째 행동: 자폭
        if (_actedTurns >= 5)
        {
            yield return CoSelfDestruct();
            yield break;
        }

        switch (s)
        {
            case State.Atk:
                yield return CoDecoyAttack();
                break;
            case State.Debuf:
                yield return CoApplyHeat();
                break;
        }
    }

    // 향상 스택 적용 후 직접공격
    private IEnumerator CoDecoyAttack()
    {
        _enhanceStack++;
        ChangeAtkValue(_enhanceStack);
        virusData.DefCnt += _enhanceStack;
        UpdateData();

        yield return CoAttack();
    }

    // 오버클럭-열기 +1 부여
    private IEnumerator CoApplyHeat()
    {
        if (PlayerManager.instance != null)
            PlayerManager.instance.heatStacks++;

        yield return new WaitForSeconds(0.5f);
    }

    // 5턴 자폭: 플레이어에게 10 피해, 본체에 자폭 알림
    private IEnumerator CoSelfDestruct()
    {
        yield return new WaitForSeconds(0.3f);

        if (PlayerManager.instance != null)
            PlayerManager.instance.TakeDamage(10);

        _boss?.OnDecoyExpired(selfDestructed: true);

        if (enemyUIController != null) enemyUIController.panel.SetActive(false);
        VirusSpawn.instance.SetDiscountVirusCount();
        Destroy(gameObject);
    }

    // ─── 피해/사망 처리 ──────────────────────────────────────────

    protected override void OnDeath()
    {
        // 플레이어가 처치: 본체에 패널티 알림 (virusCnt는 여기서 줄이지 않음)
        _boss?.OnDecoyExpired(selfDestructed: false);

        if (enemyUIController != null) enemyUIController.panel.SetActive(false);
        VirusSpawn.instance.SetDiscountVirusCount();
        Destroy(gameObject);
    }
}
