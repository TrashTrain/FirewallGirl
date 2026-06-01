using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 오버클럭.BIOS 1페이즈 분신 — '오버클럭 - 디코이'
/// 5턴 후 자폭, 처치 당하면 본체에 패널티 적용.
/// </summary>
public class OverclockDecoy : Virus
{
    [SerializeField] private Sprite _penaltyIcon;
    [SerializeField] private Sprite _iconEnhance;
    [SerializeField] private Sprite _iconTimer;

    private BossOverclock _boss;
    private int _enhanceStack = 0;
    private int _actedTurns = 0;

    private ActiveEffect _enhanceBuff;
    private ActiveEffect _timerBuff;

    protected override Dictionary<string, string> GetActionDescriptions() =>
        new Dictionary<string, string>
        {
            { "Atk",   "강화 스택만큼 ATK·방어도를 증가시키고 공격합니다. (매 공격마다 스택 누적)" },
            { "Debuf", "오버클럭-열기를 1 부여합니다.\n열기 스택만큼 카드 부정수치가 증가합니다." },
            { "Bomb",  "자폭합니다. 플레이어에게 10 피해를 주고 본체를 강화시킵니다.\n(본체: 체력·방어도+10, ATK+3)" },
        };

    protected override void Start()
    {
        if (VirusMgr.instance == null) return;
        InitData();

        _boss = FindObjectOfType<BossOverclock>();

        if (enemyUIController != null)
        {
            enemyUIController.panel.SetActive(true);
            UpdateData();
            enemyUIController.state.OverrideDescriptions(GetActionDescriptions());

            _enhanceBuff = new ActiveEffect(
                _iconEnhance, true,
                () => _enhanceStack > 0,
                () => $"강화 (스택 {_enhanceStack})\n다음 공격 시 ATK·방어도 +{_enhanceStack + 1}"
            );
            _timerBuff = new ActiveEffect(
                _iconTimer, true,
                () => true,
                () => $"자폭 예고\n남은 행동: {Mathf.Max(0, 4 - _actedTurns)}회"
            );
            enemyUIController.enemyStatusUI?.RegisterEffect(new ActiveEffect(
                _penaltyIcon,
                false,
                () => true,
                () => "오버클럭-디코이\n처치 시: 본체 행동불능 1턴 + 공격력 반감 2턴"
            ));
            enemyUIController.enemyStatusUI?.RegisterEffect(_enhanceBuff);
            enemyUIController.enemyStatusUI?.RegisterEffect(_timerBuff);
            enemyUIController.enemyStatusUI?.RefreshStatusUI();
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
        UpdateData();

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
