using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 2스테이지 보스: 망각.NULL
/// 플레이어의 카드 사용을 제한하는 기믹 특화 보스.
/// 1페이즈 → 2페이즈(70% 이하) → 3페이즈(40% 이하) → 발악(HP 30 이하)
/// </summary>
public class BossNull : Virus
{
    [Header("페이즈별 외형 이미지")]
    public Sprite phase1Sprite; // 블랙홀 같은 모습
    public Sprite phase2Sprite; // 가시가 돋힌 알과 같은 모습
    public Sprite phase3Sprite; // 낫 형태의 팔을 가진 괴물
    public Sprite desperateSprite; // 한쪽 팔이 흩어진 발악 형태

    [Header("발악 페이즈 카드")]
    [SerializeField] private CardObject _flashCardData; // 데이터 전송 - 플래시 (0코스트, 방어도+5)

    [Header("Status Effect Icons")]
    [SerializeField] private Sprite _iconPhase1Damage;    // 1페이즈: 매 턴 고정 피해
    [SerializeField] private Sprite _iconCostReduction;   // 1페이즈: 데이터 억제 (코스트 -1)
    [SerializeField] private Sprite _iconReducedDraw;     // 2페이즈: 드로우 감소
    [SerializeField] private Sprite _iconDefenseHalf;     // 3페이즈: 방어도 획득 50%
    [SerializeField] private Sprite _iconSelfProtect;     // 3페이즈: 자아 보호 (공격 차단)
    [SerializeField] private Sprite _iconDataAcquisition; // 3페이즈: 데이터 획득 (피해 시 공격력+1)
    [SerializeField] private Sprite _iconDefenseRetain;   // 발악: 방어도 유지
    [SerializeField] private Sprite _iconDefenseBoost;    // 발악: 방어도 획득 500%

    private List<ActiveEffect> _bossEffects;      // 플레이어 StatusUI에 표시되는 효과
    private List<ActiveEffect> _bossOwnEffects;   // 보스 자신의 EnemyStatusUI에 표시되는 효과

    // ─── BossNull 전용 행동 열거형 ────────────────────────────
    private enum NullAction
    {
        Overwrite,             // 1페이즈: 덧씌우기
        DirectAtk,             // 2·3페이즈: 직접 공격
        WeakAtk,               // 2페이즈: 약공격
        SelfStabilize,         // 2페이즈: 자아안정
        Stabilize,             // 3페이즈: 안정화
        EnhancedSelfStabilize, // 3페이즈: 강화된 자아안정
        DesperateWarning,      // 발악: 경고 (첫 발악 진입 시 1턴 대기 + 플래시 카드 지급)
        DesperateAtk,          // 발악: 자아 폭주 공격
        SelfDestruct           // 발악: 자멸
    }

    private NullAction _nullAction;

    private void SetAction(NullAction action)
    {
        _nullAction = action;
    }

    public override void RollNextActionAndUpdateIcon()
    {
        RollNextAction();

        if (enemyUIController != null)
            enemyUIController.state.UpdateStateImage(_nullAction.ToString());
    }

    // ─── 페이즈 추적 ───────────────────────────────────────────
    private int _currentPhase = 1;
    private int _turnInPhase = 0;

    // ─── 1페이즈 상태 ──────────────────────────────────────────
    private bool _costDebuffApplied = false; // 데이터 억제(-1 코스트) 적용 여부

    // ─── 2페이즈 상태 ──────────────────────────────────────────
    private bool _selfDotActive = false;     // 자아붕괴/자아안정 패시브 활성 여부
    private int _selfStabilizeCooldown = 0;  // 자아안정 쿨다운 (사용 후 3턴)

    // ─── 3페이즈 상태 ──────────────────────────────────────────
    private int _selfProtectCount = 0;           // 자아 보호 남은 횟수 (3회 공격 차단)
    private bool _dataAcquisitionActive = false; // 데이터 획득(피해 시 공격력+1) 활성
    private int _dataAcquisitionBonus = 0;       // 데이터 획득 누적량 (안정화 시 초기화 추적용)
    private bool _defHalfDebuffApplied = false;  // 패킷손실(방어도 50%) 적용 여부
    private bool _nextActionIsEnhancedStabilize = false; // 안정화 사용 후 다음 턴 강화된 자아안정 확정 플래그

    // ─── 발악 페이즈 상태 ─────────────────────────────────────
    private bool _desperateActive = false;
    private bool _desperateTriggered = false;
    private bool _desperateWarningDone = false; // 경고 대기 턴 완료 여부
    private int _desperateTurn = 0; // 발악 실제 공격 턴 수 (1=ATK20, 2=ATK40, 3=ATK60)

    // ══════════════════════════════════════════════════════════
    // 초기화
    // ══════════════════════════════════════════════════════════

    protected override void Start()
    {
        if (VirusMgr.instance == null) return;
        InitData();
        ChangeSprite(phase1Sprite);

        if (spawnNum != 3)
            Debug.LogError("[BossNull] 보스는 반드시 Spawn3 위치에 스폰되어야 합니다!");

        RegisterStatusEffects();
        RollNextActionAndUpdateIcon();
    }

    // ══════════════════════════════════════════════════════════
    // 효과 레지스트리
    // ══════════════════════════════════════════════════════════

    private void RegisterStatusEffects()
    {
        if (PlayerManager.instance == null) return;

        _bossEffects = new List<ActiveEffect>
        {
            new ActiveEffect(
                _iconPhase1Damage, false,
                () => _currentPhase == 1,
                () => "매 턴 5 고정 피해"
            ),
            new ActiveEffect(
                _iconCostReduction, false,
                () => _costDebuffApplied,
                () => "데이터 억제 (코스트 -1)"
            ),
            new ActiveEffect(
                _iconReducedDraw, false,
                () => PlayerManager.instance != null &&
                      (PlayerManager.instance.reducedDrawCount > 0 || PlayerManager.instance.appliedDrawReduction > 0),
                () => {
                    int pending = PlayerManager.instance?.reducedDrawCount ?? 0;
                    int applied = PlayerManager.instance?.appliedDrawReduction ?? 0;
                    if (pending > 0)
                        return $"패킷손실\n다음 턴 드로우 -{pending}";
                    return $"패킷손실\n드로우 -{applied} 적용됨";
                }
            ),
            new ActiveEffect(
                _iconDefenseHalf, false,
                () => _defHalfDebuffApplied && !_desperateActive,
                () => "방어도 획득량 50%"
            ),
            new ActiveEffect(
                _iconDefenseRetain, true,
                () => PlayerManager.instance != null && PlayerManager.instance.isDefenseRetained,
                () => "방어도 유지 중"
            ),
            new ActiveEffect(
                _iconDefenseBoost, true,
                () => _desperateActive,
                () => "방어도 획득량 500%"
            ),
        };

        foreach (var effect in _bossEffects)
            PlayerManager.instance.RegisterEffect(effect);

        // ── 보스 자신의 UI에 표시되는 효과 ──────────────────
        _bossOwnEffects = new List<ActiveEffect>
        {
            new ActiveEffect(
                _iconSelfProtect, true,
                () => _selfProtectCount > 0,
                () => $"자아 보호 ({_selfProtectCount})\n공격 {_selfProtectCount}회 차단"
            ),
            new ActiveEffect(
                _iconDataAcquisition, true,
                () => _dataAcquisitionActive,
                () => $"데이터 획득 (+{_dataAcquisitionBonus})\n피해 시 공격력 +1"
            ),
        };

        if (enemyUIController?.enemyStatusUI != null)
            foreach (var effect in _bossOwnEffects)
                enemyUIController.enemyStatusUI.RegisterEffect(effect);
    }

    // ══════════════════════════════════════════════════════════
    // 행동 결정 (RollNextAction)
    // ══════════════════════════════════════════════════════════

    protected override void RollNextAction()
    {
        if (!_desperateActive)
            CheckPhaseTransition();

        if (_desperateActive)
        {
            if (!_desperateWarningDone)
            {
                // 발악 진입 첫 턴: 경고 대기 상태 표시, 다음 공격 피해(20) 미리 보여줌
                SetAction(NullAction.DesperateWarning);
                virusData.AtkDmg = 20;
                UpdateData();
                return;
            }

            // 경고 완료 후: 다음 공격 턴 미리보기
            int nextTurn = _desperateTurn + 1;
            SetAction(nextTurn > 3 ? NullAction.SelfDestruct : NullAction.DesperateAtk);

            if (nextTurn <= 3)
            {
                virusData.AtkDmg = nextTurn switch { 1 => 20, 2 => 40, _ => 60 };
                UpdateData();
            }
            return;
        }

        _turnInPhase++;

        switch (_currentPhase)
        {
            case 1: RollPhase1Action(); break;
            case 2: RollPhase2Action(); break;
            case 3: RollPhase3Action(); break;
        }
    }

    // ─── 페이즈 전환 체크 ──────────────────────────────────────

    private void CheckPhaseTransition()
    {
        float hpRatio = (float)virusData.CurHpCnt / virusData.HpCnt;

        if (_currentPhase == 1 && hpRatio <= 0.70f)
            EnterPhase2();
        else if (_currentPhase == 2 && hpRatio <= 0.40f)
            EnterPhase3();
    }

    // 발악 체크: ApplyDamage 이후 즉시 호출
    private void CheckDesperatePhase()
    {
        if (!_desperateTriggered && virusData.CurHpCnt > 0 && virusData.CurHpCnt <= 30)
        {
            _desperateTriggered = true;
            _desperateActive = true;
            _desperateTurn = 0;
            EnterDesperatePhase();
        }
    }

    // ─── 페이즈 진입 처리 ──────────────────────────────────────

    private void EnterPhase2()
    {
        _currentPhase = 2;
        _turnInPhase = 0;
        ChangeSprite(phase2Sprite);

        // 패시브: 플레이어 5 고정피해 제거 (1페이즈 패시브는 phase==1 체크로 자동 중단)
        // 패시브: 자아붕괴 활성 (매 턴 자신에게 5 피해, 방어도 있으면 자아안정으로 전환)
        _selfDotActive = true;

        Debug.Log("[BossNull] 2페이즈 진입 — 자아붕괴 발동");
    }

    private void EnterPhase3()
    {
        _currentPhase = 3;
        _turnInPhase = 0;
        ChangeSprite(phase3Sprite);

        // 자아붕괴 해제
        _selfDotActive = false;

        // 자아 보호: 3회 공격 차단
        _selfProtectCount = 3;

        // 데이터 획득 활성
        _dataAcquisitionActive = true;

        // 패킷손실: 플레이어 방어도 획득량 50%
        if (!_defHalfDebuffApplied)
        {
            _defHalfDebuffApplied = true;
            PlayerManager.instance.defenseMultiplier = 0.5f;
        }

        // 억제 불가: 1페이즈 코스트 -1 되돌리고 +2 증가
        if (_costDebuffApplied)
        {
            _costDebuffApplied = false;
            PlayerManager.instance.AddPermanentStat(StatType.Cost, 1); // -1 복구
        }
        PlayerManager.instance.AddPermanentStat(StatType.Cost, 2); // 억제 불가 +2

        Debug.Log("[BossNull] 3페이즈 진입 — 자아 보호 3회, 데이터 획득, 패킷손실, 억제 불가");
    }

    private void EnterDesperatePhase()
    {
        ChangeSprite(desperateSprite);

        // 발악 중 방어도 유지 (턴 종료 후 사라지지 않음)
        PlayerManager.instance.isDefenseRetained = true;

        // 패킷 전송 - 플래시: 방어도 획득 5배 적용
        PlayerManager.instance.defenseMultiplier = 5.0f;

        Debug.Log("[BossNull] 발악 페이즈 진입 — 자아 폭주 (3턴 무적), 방어도 5배, 방어도 유지");

        // 보스 의도 아이콘을 즉시 업데이트하여 첫 발악 공격력(20) 표시
        RollNextActionAndUpdateIcon();
    }

    // ─── 페이즈별 행동 결정 로직 ──────────────────────────────

    private void RollPhase1Action()
    {
        // 유일한 행동: 덧씌우기 (매 턴 방어도 +5)
        SetAction(NullAction.Overwrite);
    }

    private void RollPhase2Action()
    {
        // 쿨다운 감소 후 0이면 자아안정 우선 사용
        if (_selfStabilizeCooldown > 0)
            _selfStabilizeCooldown--;

        if (_selfStabilizeCooldown == 0)
        {
            SetAction(NullAction.SelfStabilize);
        }
        else
        {
            SetAction(Random.Range(0, 2) == 0 ? NullAction.DirectAtk : NullAction.WeakAtk);
        }
    }

    private void RollPhase3Action()
    {
        // 안정화 사용 후 다음 턴은 강화된 자아안정 확정
        if (_nextActionIsEnhancedStabilize)
        {
            _nextActionIsEnhancedStabilize = false;
            SetAction(NullAction.EnhancedSelfStabilize);
            return;
        }

        // 3턴 이전: 직접공격만 가능
        if (_turnInPhase < 3)
        {
            SetAction(NullAction.DirectAtk);
            return;
        }

        // 3턴 이후: 직접공격 / 안정화 균등 확률
        if (Random.Range(0, 2) == 0)
        {
            SetAction(NullAction.Stabilize);
            _nextActionIsEnhancedStabilize = true;
        }
        else
        {
            SetAction(NullAction.DirectAtk);
        }
    }

    // ══════════════════════════════════════════════════════════
    // 행동 실행 (CoRunStateAction)
    // ══════════════════════════════════════════════════════════

    protected override IEnumerator CoRunStateAction(State s)
    {
        // ─── 발악 페이즈 처리 ────────────────────────────────
        if (_desperateActive)
        {
            // 경고 대기 턴: 공격 없이 플래시 카드만 지급
            if (_nullAction == NullAction.DesperateWarning)
            {
                if (_flashCardData != null)
                    PlayerManager.instance.pendingFlashCards.Add(_flashCardData);
                _desperateWarningDone = true;
                Debug.Log("[BossNull] 발악 경고: 플래시 카드 예약, 다음 턴부터 자아 폭주 개시");
                yield return new WaitForSeconds(0.5f);
                yield break;
            }

            // 실제 발악 공격: 카운터 증가 (RollNextAction은 미리보기만 했음)
            _desperateTurn++;

            if (_desperateTurn > 3 || _nullAction == NullAction.SelfDestruct)
            {
                yield return new WaitForSeconds(0.5f);
                _desperateActive = false;
                OnDeath();
                yield break;
            }

            // 매 발악 공격 턴마다 플래시 카드 지급
            if (_flashCardData != null)
                PlayerManager.instance.pendingFlashCards.Add(_flashCardData);

            // 턴별 공격력 설정 (RollNextAction에서 이미 설정됐지만 일관성 보장)
            virusData.AtkDmg = _desperateTurn switch { 1 => 20, 2 => 40, _ => 60 };
            UpdateData();

            yield return CoAttack();

            // 3턴 후 자멸
            if (_desperateTurn >= 3)
            {
                yield return new WaitForSeconds(0.5f);
                _desperateActive = false;
                OnDeath();
            }
            yield break;
        }

        // ─── 패시브 처리 (행동 전) ───────────────────────────
        yield return CoPassiveEffects();
        if (virusData.CurHpCnt <= 0) yield break;

        // ─── 페이즈별 행동 실행 ──────────────────────────────
        switch (_currentPhase)
        {
            case 1: yield return CoPhase1Action(); break;
            case 2: yield return CoPhase2Action(); break;
            case 3: yield return CoPhase3Action(); break;
        }
    }

    // ─── 패시브 효과 (매 턴 행동 전) ─────────────────────────

    private IEnumerator CoPassiveEffects()
    {
        // 1페이즈: 데이터 억제 (최초 1회) + 매 턴 플레이어에게 5 피해
        if (_currentPhase == 1)
        {
            if (!_costDebuffApplied)
            {
                _costDebuffApplied = true;
                PlayerManager.instance.AddPermanentStat(StatType.Cost, -1);
                Debug.Log("[BossNull] 데이터 억제: 플레이어 최대 코스트 -1");
            }

            PlayerManager.instance.TakeDamage(5);
            Debug.Log("[BossNull] 1페이즈 패시브: 플레이어 5 피해");
        }

        // 2페이즈: 자아붕괴(방어도 없음 → 자신 5 피해) / 자아안정(방어도 있음 → 5 회복)
        if (_currentPhase == 2 && _selfDotActive)
        {
            if (virusData.DefCnt > 0)
            {
                // 자아안정: 방어도가 있는 동안 매 턴 5 회복
                virusData.CurHpCnt = Mathf.Min(virusData.HpCnt, virusData.CurHpCnt + 5);
                UpdateData();
                Debug.Log("[BossNull] 자아안정: 5 회복");
            }
            else
            {
                // 자아붕괴: 방어도가 0이면 자신에게 5 피해
                ApplyDamageToSelf(5);
                Debug.Log("[BossNull] 자아붕괴: 자신에게 5 피해");
                if (virusData.CurHpCnt <= 0) yield break;
            }
        }

        yield return new WaitForSeconds(0.3f);
    }

    // 자신에게 피해 (방어도 우선 소모)
    private void ApplyDamageToSelf(int damage)
    {
        int remaining = damage;
        if (virusData.DefCnt > 0)
        {
            int defUsed = Mathf.Min(virusData.DefCnt, remaining);
            virusData.DefCnt -= defUsed;
            remaining -= defUsed;
        }
        if (remaining > 0)
            virusData.CurHpCnt = Mathf.Max(0, virusData.CurHpCnt - remaining);

        UpdateData();

        if (virusData.CurHpCnt <= 0) { OnDeath(); return; }
        CheckDesperatePhase();
    }

    // ─── 1페이즈 행동 코루틴 ──────────────────────────────────

    private IEnumerator CoPhase1Action()
    {
        // 덧씌우기: 매 턴 방어도 +5 획득 유지
        virusData.DefCnt += 5;
        UpdateData();
        Debug.Log("[BossNull] 덧씌우기: 방어도 +5");
        yield return new WaitForSeconds(0.5f);
    }

    // ─── 2페이즈 행동 코루틴 ──────────────────────────────────

    private IEnumerator CoPhase2Action()
    {
        switch (_nullAction)
        {
            case NullAction.DirectAtk:
                yield return CoPhase2DirectAttack();
                break;

            case NullAction.SelfStabilize:
                // 자아안정: 방어도 +10, 쿨다운 3턴
                virusData.DefCnt += 10;
                UpdateData();
                _selfStabilizeCooldown = 3;
                Debug.Log("[BossNull] 자아안정: 방어도 +10 (쿨다운 3턴)");
                yield return new WaitForSeconds(0.5f);
                break;

            case NullAction.WeakAtk:
                yield return CoPhase2DebufAttack();
                break;
        }
    }

    // 2페이즈 직접 공격: 공격력만큼 피해, 방어 관통 시 패킷손실(드로우 -1)
    private IEnumerator CoPhase2DirectAttack()
    {
        int prevDef = PlayerManager.instance.DefensePower;
        yield return CoAttack();

        int penetrated = Mathf.Max(0, virusData.AtkDmg - prevDef);
        if (penetrated > 0)
        {
            PlayerManager.instance.reducedDrawCount++;
            PlayerManager.instance.UpdateUI(); // 디버프 아이콘 즉시 갱신
            Debug.Log("[BossNull] 패킷손실: 다음 턴 드로우 -1");
        }
    }

    // 2페이즈 약공격: 1 피해 + 다음 턴 방어도 획득 불가 + 자신 공격력 +2
    private IEnumerator CoPhase2DebufAttack()
    {
        PlayerManager.instance.TakeDamage(1);
        // 다음 적 턴 종료 시 1 감소하므로 2로 설정해야 1 플레이어 턴에 효과 유지
        PlayerManager.instance.cannotGainDefenseTurns = 2;
        ChangeAtkValue(2);
        UpdateData();
        Debug.Log("[BossNull] 약공격: 플레이어 1 피해 + 방어도 획득 불가 + 자신 공격력 +2");
        yield return new WaitForSeconds(0.5f);
    }

    // ─── 3페이즈 행동 코루틴 ──────────────────────────────────

    private IEnumerator CoPhase3Action()
    {
        switch (_nullAction)
        {
            case NullAction.DirectAtk:
                yield return CoPhase3DirectAttack();
                break;

            case NullAction.Stabilize:
                // 안정화: 공격력 2배 방어도 획득 + 공격력 기본값 초기화
                yield return CoStabilize();
                break;

            case NullAction.EnhancedSelfStabilize:
                // 강화된 자아안정: 방어도만큼 회복 + 공격력 +5
                yield return CoEnhancedSelfStabilize();
                break;
        }
    }

    // 3페이즈 직접 공격: 공격력만큼 피해, 관통 시 자신 공격력 증가
    private IEnumerator CoPhase3DirectAttack()
    {
        int prevDef = PlayerManager.instance.DefensePower;
        yield return CoAttack();

        int penetrated = Mathf.Max(0, virusData.AtkDmg - prevDef);
        if (penetrated > 0)
        {
            ChangeAtkValue(penetrated);
            UpdateData();
            Debug.Log($"[BossNull] 관통 피해 {penetrated} → 자신 공격력 +{penetrated}");
        }
    }

    // 안정화: 공격력 2배 방어도 획득, 전체 공격력 기본값으로 초기화
    private IEnumerator CoStabilize()
    {
        int shield = virusData.AtkDmg * 2;
        virusData.DefCnt += shield;

        virusData.AtkDmg = virusObjectSO.virusAtk; // 기본값 3으로 완전 초기화
        _dataAcquisitionBonus = 0;

        UpdateData();
        Debug.Log($"[BossNull] 안정화: 방어도 +{shield}, 공격력 기본값으로 초기화");
        yield return new WaitForSeconds(0.5f);
    }

    // 강화된 자아안정: 보유 방어도만큼 회복 + 공격력 +5, 이후 방어도 소멸
    private IEnumerator CoEnhancedSelfStabilize()
    {
        int currentDef = virusData.DefCnt;
        if (currentDef > 0)
        {
            virusData.CurHpCnt = Mathf.Min(virusData.HpCnt, virusData.CurHpCnt + currentDef);
            ChangeAtkValue(5);
            virusData.DefCnt = 0;
            UpdateData();
            Debug.Log($"[BossNull] 강화된 자아안정: {currentDef} 회복 + 공격력 +5, 방어도 소멸");
        }
        else
        {
            Debug.Log("[BossNull] 강화된 자아안정: 방어도 없음 — 효과 발동 안 됨");
        }
        yield return new WaitForSeconds(0.5f);
    }

    // ══════════════════════════════════════════════════════════
    // 피해 처리 오버라이드
    // ══════════════════════════════════════════════════════════

    public override int ApplyDamage(int damage)
    {
        // 발악 페이즈: 자아 폭주 (3턴간 무적)
        if (_desperateActive)
        {
            Debug.Log("[BossNull] 자아 폭주: 피해 무시");
            return damage;
        }

        // 자아 보호: 3회 공격 차단
        if (_selfProtectCount > 0)
        {
            _selfProtectCount--;
            Debug.Log($"[BossNull] 자아 보호: 공격 차단 (남은 횟수 {_selfProtectCount})");
            UpdateData();
            return 0;
        }

        int remaining = damage;

        if (virusData.DefCnt > 0)
        {
            int defUsed = Mathf.Min(virusData.DefCnt, remaining);
            virusData.DefCnt = Mathf.Max(0, virusData.DefCnt - defUsed);
            remaining -= defUsed;
        }

        if (remaining > 0 && virusData.CurHpCnt > 0)
        {
            int hpUsed = Mathf.Min(virusData.CurHpCnt, remaining);
            virusData.CurHpCnt = Mathf.Max(0, virusData.CurHpCnt - hpUsed);
            remaining -= hpUsed;

            // 데이터 획득: 실제 체력 피해 시 공격력 +1
            if (_dataAcquisitionActive && hpUsed > 0)
            {
                _dataAcquisitionBonus++;
                ChangeAtkValue(1);
                Debug.Log($"[BossNull] 데이터 획득: 공격력 +1 (누적 {_dataAcquisitionBonus})");
            }
        }

        UpdateData();
        Debug.Log($"[BossNull] 받은 피해: {damage}, 현재 체력: {virusData.CurHpCnt}");

        if (virusData.CurHpCnt <= 0)
        {
            OnDeath();
            return remaining;
        }

        CheckDesperatePhase();
        return remaining;
    }

    // ══════════════════════════════════════════════════════════
    // 사망 처리
    // ══════════════════════════════════════════════════════════

    protected override void OnDeath()
    {
        // PlayerManager에 적용했던 디버프/버프 모두 복원
        if (_costDebuffApplied)
            PlayerManager.instance.AddPermanentStat(StatType.Cost, 1);
        if (_defHalfDebuffApplied || _desperateActive)
            PlayerManager.instance.defenseMultiplier = 1.0f;
        PlayerManager.instance.isDefenseRetained = false;

        // 플레이어 UI에 등록했던 효과 해제
        if (_bossEffects != null)
            foreach (var effect in _bossEffects)
                PlayerManager.instance.UnregisterEffect(effect);

        // 보스 자신의 UI에 등록했던 효과 해제
        if (_bossOwnEffects != null && enemyUIController?.enemyStatusUI != null)
            foreach (var effect in _bossOwnEffects)
                enemyUIController.enemyStatusUI.UnregisterEffect(effect);

        VirusSpawn.instance.SetDiscountVirusCount();

        if (VirusSpawn.instance.virusCnt <= 0)
            GameManager.Instance.GameClear();

        if (enemyUIController != null) enemyUIController.panel.SetActive(false);
        Destroy(gameObject);
    }

    // ══════════════════════════════════════════════════════════
    // 유틸
    // ══════════════════════════════════════════════════════════

    private void ChangeSprite(Sprite newSprite)
    {
        if (newSprite == null) return;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.sprite = newSprite;
    }
}
