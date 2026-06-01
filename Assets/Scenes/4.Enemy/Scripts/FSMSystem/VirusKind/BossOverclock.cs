using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 3스테이지 보스: 오버클럭.BIOS
/// 1페이즈(100%~60%) → 2페이즈(60%~30%) → 3페이즈(30%~0%)
/// 승리 조건: 소화-권한 10스택 달성
/// 패배 조건: 3페이즈 오버클럭-오버클럭 자해로 HP 0
/// </summary>
public class BossOverclock : Virus
{
    [Header("페이즈별 스프라이트")]
    public Sprite phase1Sprite;
    public Sprite phase2Sprite;
    public Sprite phase3Sprite;

    [Header("분신 프리팹")]
    [SerializeField] private GameObject _decoyPrefab;

    [Header("Status Effect Icons")]
    [SerializeField] private Sprite _iconOverheat;
    [SerializeField] private Sprite _iconHeat;
    [SerializeField] private Sprite _iconCoolTempo;
    [SerializeField] private Sprite _iconFire;
    [SerializeField] private Sprite _iconOverclock;
    [SerializeField] private Sprite _iconDigestTempo;
    [SerializeField] private Sprite _iconDigestAuth;
    [SerializeField] private Sprite _iconEnhance;

    // ─── 행동 열거형 ───────────────────────────────────────────────
    private enum OverclockAction
    {
        SummonDecoy,
        DirectAtk,
        Debuf,
        Enhance,
        Overheat,
        Stunned,
        Explode,
        Rampage,
        Reignite
    }

    private OverclockAction _action;

    // ─── 페이즈 ────────────────────────────────────────────────────
    private int _currentPhase = 1;

    // ─── 1페이즈 ───────────────────────────────────────────────────
    private bool _overheatPassiveApplied = false;
    private OverclockDecoy _decoyRef = null;
    private bool _decoySealed = false;
    private bool _nextMustSummonDecoy = false;
    private bool _firstAction = true;
    private int _stunTurns = 0;
    private int _atkHalveTurns = 0;
    private int _atkBeforeHalve = 0;
    private int _enhanceStack = 0;

    // ─── 2페이즈 ───────────────────────────────────────────────────
    private bool _coolTempoApplied = false;
    private int _phase2BaseAtk = 5;
    private int _fireAtkBonus = 0;
    private int _maxFireBonus = 20;
    private int _phase2TurnCounter = 0;
    private int _overheatStunTurns = 0;
    private int _overheatDamageReceived = 0;
    private bool _nextMustDirectAtk = false;
    private int _phase2EnhanceBuff = 0;

    // ─── 3페이즈 ───────────────────────────────────────────────────
    private int _digestAuthStacks = 0;
    private int _reigniteStacks = 0;

    // ─── 효과 레지스트리 ───────────────────────────────────────────
    private List<ActiveEffect> _bossEffects;
    private ActiveEffect _selfDamageEffect;
    private ActiveEffect _enhanceBuff;

    // ══════════════════════════════════════════════════════════════
    // 초기화
    // ══════════════════════════════════════════════════════════════

    private Dictionary<string, string> Phase1Descriptions() => new Dictionary<string, string>
    {
        { "SummonDecoy", "분신을 소환합니다." },
        { "DirectAtk",   "강화 스택만큼 ATK·방어도를 증가시키고 공격합니다." },
        { "Debuf",       "오버클럭-열기를 1 부여합니다.\n열기 스택만큼 카드 부정수치가 증가합니다." },
        { "Stunned",     "행동불능 상태입니다." },
    };

    private Dictionary<string, string> Phase2Descriptions() => new Dictionary<string, string>
    {
        { "DirectAtk",  "공격합니다. 막힌 방어도만큼 자해합니다.\n향상 버프가 있으면 ATK+20 적용 후 소멸합니다." },
        { "Enhance",    "ATK+20을 예약합니다.\n다음 행동이 직접공격으로 고정됩니다." },
        { "Overheat",   "현재 발화 수치만큼 자해하고 2턴 행동불능 상태가 됩니다.\n스턴 중 받은 피해 50 초과 시 발화 최대치 증가." },
        { "Stunned",    "행동불능 상태입니다." },
    };

    private Dictionary<string, string> Phase3Descriptions() => new Dictionary<string, string>
    {
        { "Explode",    "보스와 플레이어 각 20 피해를 주고받습니다." },
        { "Rampage",    "루트 카드를 사용하지 않으면 소화-권한을 1 감소시킵니다." },
        { "Reignite",   "패치 카드를 사용하지 않으면 재발화 스택을 1 증가시킵니다.\n(매 보스 행동 시 스택×10 자해)" },
        { "Stunned",    "행동불능 상태입니다." },
    };

    protected override void Start()
    {
        if (VirusMgr.instance == null) return;
        InitData();
        ChangeSprite(phase1Sprite);

        if (spawnNum != 3)
            Debug.LogError("[BossOverclock] 보스는 Spawn3 위치에 스폰되어야 합니다!");

        if (enemyUIController != null)
            enemyUIController.state.OverrideDescriptions(Phase1Descriptions());

        RegisterStatusEffects();
        EnemyTurnManager.OnPlayerTurnEnded += HandlePlayerTurnEnded;
        RollNextActionAndUpdateIcon();
    }

    private void OnDestroy()
    {
        EnemyTurnManager.OnPlayerTurnEnded -= HandlePlayerTurnEnded;
    }

    // ══════════════════════════════════════════════════════════════
    // 효과 레지스트리
    // ══════════════════════════════════════════════════════════════

    private void RegisterStatusEffects()
    {
        if (PlayerManager.instance == null) return;

        _selfDamageEffect = new ActiveEffect(
            _iconOverclock, false,
            () => _currentPhase == 3,
            () => "오버클럭-오버클럭\n매턴 50 자해. HP 0 → 플레이어 패배"
        );
        _enhanceBuff = new ActiveEffect(
            _iconEnhance, true,
            () => _currentPhase == 1 && _enhanceStack > 0,
            () => $"오버클럭-강화 (스택 {_enhanceStack})\n다음 직접공격 시 ATK·방어도 +{_enhanceStack + 1}"
        );
        enemyUIController?.enemyStatusUI?.RegisterEffect(_selfDamageEffect);
        enemyUIController?.enemyStatusUI?.RegisterEffect(_enhanceBuff);
        enemyUIController?.enemyStatusUI?.RefreshStatusUI();

        _bossEffects = new List<ActiveEffect>
        {
            new ActiveEffect(
                _iconOverheat, false,
                () => _currentPhase == 1,
                () => "오버클럭-과열\n드로우+4, 코스트+3, 사용카드×3 피해"
            ),
            new ActiveEffect(
                _iconHeat, false,
                () => PlayerManager.instance != null && PlayerManager.instance.heatStacks > 0,
                () => $"오버클럭-열기 ({PlayerManager.instance.heatStacks})\n카드 부정수치 +{PlayerManager.instance.heatStacks}"
            ),
            new ActiveEffect(
                _iconCoolTempo, true,
                () => _coolTempoApplied,
                () => "냉각-템포\n코스트+3, 사용카드×3 방어도"
            ),
            new ActiveEffect(
                _iconFire, false,
                () => _currentPhase == 2,
                () => $"오버클럭-발화\nATK 변화: {_fireAtkBonus:+0;-0;0}"
            ),
            new ActiveEffect(
                _iconDigestTempo, true,
                () => _currentPhase == 3,
                () => "소화-템포\n백신×40 HP회복, 패치/루트 시 소화-권한+1"
            ),
            new ActiveEffect(
                _iconDigestAuth, true,
                () => _currentPhase == 3,
                () => $"소화-권한 ({_digestAuthStacks}/10)\n10스택 달성 시 승리"
            ),
        };

        foreach (var e in _bossEffects)
            PlayerManager.instance.RegisterEffect(e);
    }

    // ══════════════════════════════════════════════════════════════
    // 플레이어 턴 종료 이벤트
    // ══════════════════════════════════════════════════════════════

    private void HandlePlayerTurnEnded()
    {
        if (PlayerManager.instance == null) return;

        int vaccines = PlayerManager.instance.turnVaccineCount;
        int patches  = PlayerManager.instance.turnPatchCount;
        int roots    = PlayerManager.instance.turnRootCount;
        int total    = PlayerManager.instance.turnTotalCardCount;

        // 1페이즈: 오버클럭-과열 — 사용 카드 수×3 피해
        if (_currentPhase == 1 && total > 0)
        {
            PlayerManager.instance.TakeDamage(total * 3);
            Debug.Log($"[BossOverclock] 오버클럭-과열: {total * 3} 피해");
        }

        // 2페이즈: 냉각-템포 — 사용 카드 수×3 방어도
        if (_currentPhase == 2)
        {
            if (total > 0)
            {
                virusData.DefCnt += total * 3;
                UpdateData();
                Debug.Log($"[BossOverclock] 냉각-템포: 방어도 +{total * 3}");
            }
            ApplyFirePassive(patches, roots, vaccines);
        }

        // 3페이즈: 소화-템포
        if (_currentPhase == 3)
        {
            if (vaccines > 0)
            {
                virusData.CurHpCnt = Mathf.Min(virusData.HpCnt, virusData.CurHpCnt + vaccines * 40);
                UpdateData();
                Debug.Log($"[BossOverclock] 소화-템포: 체력 +{vaccines * 40}");
            }

            int authGain = patches + roots;
            if (authGain > 0)
            {
                _digestAuthStacks += authGain;
                Debug.Log($"[BossOverclock] 소화-권한: +{authGain} → 총 {_digestAuthStacks}");
            }

            if (_digestAuthStacks >= 10)
            {
                Debug.Log("[BossOverclock] 소화-권한 10스택 → 플레이어 승리!");
                CleanupEffects();
                GameManager.Instance.GameClear();
                return;
            }
        }

        PlayerManager.instance.UpdateUI();
    }

    private void ApplyFirePassive(int patches, int roots, int vaccines)
    {
        int delta = (patches + roots) * 2 - vaccines * 3;
        _fireAtkBonus = Mathf.Clamp(_fireAtkBonus + delta, -9999, _maxFireBonus);
        virusData.AtkDmg = Mathf.Max(5, _phase2BaseAtk + _fireAtkBonus);
        UpdateData();
        Debug.Log($"[BossOverclock] 오버클럭-발화: ATK={virusData.AtkDmg}");
    }

    // ══════════════════════════════════════════════════════════════
    // 행동 결정
    // ══════════════════════════════════════════════════════════════

    public override void RollNextActionAndUpdateIcon()
    {
        RollNextAction();
        if (enemyUIController != null)
            enemyUIController.state.UpdateStateImage(_action.ToString());
    }

    protected override void RollNextAction()
    {
        CheckPhaseTransition();

        switch (_currentPhase)
        {
            case 1: RollPhase1Action(); break;
            case 2: RollPhase2Action(); break;
            case 3: RollPhase3Action(); break;
        }
    }

    private void CheckPhaseTransition()
    {
        if (virusData == null) return;
        float ratio = (float)virusData.CurHpCnt / virusData.HpCnt;

        if (_currentPhase == 1 && ratio <= 0.60f)
            EnterPhase2();
        else if (_currentPhase == 2 && ratio <= 0.30f)
            EnterPhase3();
    }

    // ─── 1페이즈 행동 결정 ─────────────────────────────────────────

    private void RollPhase1Action()
    {
        // 행동불능
        if (_stunTurns > 0)
        {
            _stunTurns--;
            _action = OverclockAction.Stunned;
            return;
        }

        // 공격력 반감 종료
        if (_atkHalveTurns > 0)
        {
            _atkHalveTurns--;
            if (_atkHalveTurns == 0 && _atkBeforeHalve > 0)
            {
                virusData.AtkDmg = _atkBeforeHalve;
                _atkBeforeHalve = 0;
                UpdateData();
                Debug.Log("[BossOverclock] 공격력 반감 해제");
            }
        }

        // 보스전 최초 행동 = 분신 소환
        if (_firstAction)
        {
            _firstAction = false;
            _action = OverclockAction.SummonDecoy;
            return;
        }

        // 자폭 후 강제 소환
        if (_nextMustSummonDecoy && !_decoySealed)
        {
            _nextMustSummonDecoy = false;
            _action = OverclockAction.SummonDecoy;
            return;
        }

        // 분신 없고 봉인 안 된 경우 소환
        if (_decoyRef == null && !_decoySealed)
        {
            _action = OverclockAction.SummonDecoy;
            return;
        }

        // 분신 존재 or 봉인 → 50/50
        _action = Random.Range(0, 2) == 0 ? OverclockAction.DirectAtk : OverclockAction.Debuf;
    }

    // ─── 2페이즈 행동 결정 ─────────────────────────────────────────

    private void RollPhase2Action()
    {
        // 초과열 스턴 처리
        if (_overheatStunTurns > 0)
        {
            _overheatStunTurns--;
            if (_overheatStunTurns == 0)
            {
                if (_overheatDamageReceived > 50)
                {
                    _maxFireBonus += 10;
                    Debug.Log($"[BossOverclock] 초과열 조건: 발화 최대 상승치 → {_maxFireBonus}");
                }
                _overheatDamageReceived = 0;
                _phase2TurnCounter = 0;
            }
            _action = OverclockAction.Stunned;
            return;
        }

        // 매 행동 카운터 증가 (스턴 시 증가 안 함)
        _phase2TurnCounter++;

        // 초과열이 향상보다 우선 (A2 답변)
        if (_phase2TurnCounter >= 3)
        {
            _nextMustDirectAtk = false; // 향상 고정 취소
            _action = OverclockAction.Overheat;
            return;
        }

        // 향상 후 직접공격 고정
        if (_nextMustDirectAtk)
        {
            _nextMustDirectAtk = false;
            _action = OverclockAction.DirectAtk;
            return;
        }

        // 50/50 향상 vs 직접공격
        _action = Random.Range(0, 2) == 0 ? OverclockAction.Enhance : OverclockAction.DirectAtk;
    }

    // ─── 3페이즈 행동 결정 ─────────────────────────────────────────

    private void RollPhase3Action()
    {
        int r = Random.Range(0, 3);
        _action = r switch
        {
            0 => OverclockAction.Explode,
            1 => OverclockAction.Rampage,
            _ => OverclockAction.Reignite
        };
    }

    // ══════════════════════════════════════════════════════════════
    // 페이즈 진입
    // ══════════════════════════════════════════════════════════════

    private void EnterPhase2()
    {
        _currentPhase = 2;
        ChangeSprite(phase2Sprite);

        // 오버클럭-과열 해제 (코스트+3, 드로우+4 복원)
        if (_overheatPassiveApplied)
        {
            _overheatPassiveApplied = false;
            PlayerManager.instance.AddPermanentStat(StatType.Cost, -3);
            PlayerManager.instance.bonusDrawCount = 0;
        }

        // 열기 흡수
        int heat = PlayerManager.instance.heatStacks;
        PlayerManager.instance.heatStacks = 0;
        if (heat > 0)
        {
            virusData.CurHpCnt = Mathf.Min(virusData.HpCnt, virusData.CurHpCnt + heat * 2);
            ChangeAtkValue(heat);
            Debug.Log($"[BossOverclock] 열기 흡수: 체력+{heat * 2}, ATK+{heat}");
        }

        // 분신 소멸
        if (_decoyRef != null)
        {
            if (_decoyRef.enemyUIController != null)
                _decoyRef.enemyUIController.panel.SetActive(false);
            VirusSpawn.instance.SetDiscountVirusCount();
            Destroy(_decoyRef.gameObject);
            _decoyRef = null;
        }

        // 냉각-템포: 코스트+3
        _coolTempoApplied = true;
        PlayerManager.instance.AddPermanentStat(StatType.Cost, 3);

        // 발화 기준 ATK
        _phase2BaseAtk = virusData.AtkDmg;
        _phase2TurnCounter = 0;

        UpdateData();
        PlayerManager.instance.UpdateUI();
        enemyUIController?.state.OverrideDescriptions(Phase2Descriptions());
        Debug.Log("[BossOverclock] 2페이즈 진입");
    }

    private void EnterPhase3()
    {
        _currentPhase = 3;
        ChangeSprite(phase3Sprite);

        // 냉각-템포 해제
        if (_coolTempoApplied)
        {
            _coolTempoApplied = false;
            PlayerManager.instance.AddPermanentStat(StatType.Cost, -3);
        }

        // 모든 열기/발화 초기화
        PlayerManager.instance.heatStacks = 0;
        _fireAtkBonus = 0;

        UpdateData();
        PlayerManager.instance.UpdateUI();
        enemyUIController?.state.OverrideDescriptions(Phase3Descriptions());
        Debug.Log("[BossOverclock] 3페이즈 진입");
    }

    // ══════════════════════════════════════════════════════════════
    // 행동 실행
    // ══════════════════════════════════════════════════════════════

    protected override IEnumerator CoRunStateAction(State s)
    {
        // 1페이즈 과열 패시브 최초 1회 적용
        if (_currentPhase == 1 && !_overheatPassiveApplied)
        {
            _overheatPassiveApplied = true;
            PlayerManager.instance.AddPermanentStat(StatType.Cost, 3);
            PlayerManager.instance.bonusDrawCount = 1;
            PlayerManager.instance.UpdateUI();
            Debug.Log("[BossOverclock] 오버클럭-과열 적용: 코스트+3, 드로우+1");
        }

        if (_action == OverclockAction.Stunned)
        {
            yield return new WaitForSeconds(0.5f);
            yield break;
        }

        // 3페이즈: 재발화 DoT → 오버클럭-오버클럭 자해 → 행동
        if (_currentPhase == 3)
        {
            if (_reigniteStacks > 0)
            {
                ApplyDamageToSelf(_reigniteStacks * 10);
                Debug.Log($"[BossOverclock] 재발화 DoT: {_reigniteStacks * 10}");
                if (virusData.CurHpCnt <= 0) yield break;
            }

            ApplyDamageToSelf(50);
            Debug.Log("[BossOverclock] 오버클럭-오버클럭: 50 자해");
            if (virusData.CurHpCnt <= 0) yield break;
        }

        switch (_currentPhase)
        {
            case 1: yield return CoPhase1Action(); break;
            case 2: yield return CoPhase2Action(); break;
            case 3: yield return CoPhase3Action(); break;
        }
    }

    // ─── 1페이즈 행동 코루틴 ──────────────────────────────────────

    private IEnumerator CoPhase1Action()
    {
        switch (_action)
        {
            case OverclockAction.SummonDecoy:
                yield return CoSummonDecoy();
                break;

            case OverclockAction.DirectAtk:
                _enhanceStack++;
                ChangeAtkValue(_enhanceStack);
                virusData.DefCnt += _enhanceStack;
                UpdateData();
                yield return CoAttack();
                break;

            case OverclockAction.Debuf:
                PlayerManager.instance.heatStacks++;
                PlayerManager.instance.UpdateUI();
                Debug.Log("[BossOverclock] 오버클럭-열기 +1");
                yield return new WaitForSeconds(0.5f);
                break;
        }
    }

    private IEnumerator CoSummonDecoy()
    {
        if (_decoyPrefab == null || VirusSpawn.instance == null)
        {
            yield return new WaitForSeconds(0.5f);
            yield break;
        }

        VirusSpawn.instance.SpawnVirus(1, _decoyPrefab);
        _decoyRef = VirusSpawn.instance.spawns[1].GetComponentInChildren<OverclockDecoy>();
        VirusSpawn.instance.virusCnt++;

        Debug.Log("[BossOverclock] 오버클럭-디코이 소환");
        yield return new WaitForSeconds(0.5f);
    }

    public void OnDecoyExpired(bool selfDestructed)
    {
        _decoyRef = null;

        if (selfDestructed)
        {
            virusData.CurHpCnt = Mathf.Min(virusData.HpCnt, virusData.CurHpCnt + 10);
            virusData.DefCnt += 10;
            ChangeAtkValue(3);
            UpdateData();
            _nextMustSummonDecoy = true;
            Debug.Log("[BossOverclock] 분신 자폭: 체력+10, 방어도+10, ATK+3");
        }
        else
        {
            // 플레이어 처치: 1턴 행동불능, 2턴 공격력 반감, 영구 봉인
            _stunTurns = 1;
            _atkHalveTurns = 2;
            _atkBeforeHalve = virusData.AtkDmg;
            virusData.AtkDmg = Mathf.Max(1, virusData.AtkDmg / 2);
            UpdateData();
            _decoySealed = true;
            Debug.Log("[BossOverclock] 분신 처치: 행동불능 1턴, 공격력 반감 2턴, 봉인");
        }
    }

    // ─── 2페이즈 행동 코루틴 ──────────────────────────────────────

    private IEnumerator CoPhase2Action()
    {
        switch (_action)
        {
            case OverclockAction.Enhance:
                _phase2EnhanceBuff = 20;
                _nextMustDirectAtk = true;
                Debug.Log("[BossOverclock] 향상: ATK+20 예약, 다음 행동 직접공격 고정");
                yield return new WaitForSeconds(0.5f);
                break;

            case OverclockAction.DirectAtk:
                yield return CoPhase2DirectAtk();
                break;

            case OverclockAction.Overheat:
                yield return CoOverheat();
                break;
        }
    }

    private IEnumerator CoPhase2DirectAtk()
    {
        // 향상 버프 임시 적용
        if (_phase2EnhanceBuff > 0)
        {
            virusData.AtkDmg += _phase2EnhanceBuff;
            _phase2EnhanceBuff = 0;
            UpdateData();
        }

        int currentAtk = virusData.AtkDmg;
        int prevDef = PlayerManager.instance.DefensePower;
        yield return CoAttack();

        // 막힌 방어도만큼 자해
        int blocked = Mathf.Min(currentAtk, prevDef);
        if (blocked > 0)
        {
            ApplyDamageToSelf(blocked);
            Debug.Log($"[BossOverclock] 직접공격 반사: {blocked} 자해");
        }

        // 다음 발화 패시브가 AtkDmg를 재계산하므로 버프는 자동 소멸
    }

    private IEnumerator CoOverheat()
    {
        int selfDmg = Mathf.Max(0, _fireAtkBonus);
        if (selfDmg > 0)
        {
            ApplyDamageToSelf(selfDmg);
            Debug.Log($"[BossOverclock] 초과열 자해: {selfDmg}");
            if (virusData.CurHpCnt <= 0) yield break;
        }

        _overheatStunTurns = 2;
        _overheatDamageReceived = 0;
        Debug.Log("[BossOverclock] 초과열: 2턴 스턴 돌입");
        yield return new WaitForSeconds(0.5f);
    }

    // ─── 3페이즈 행동 코루틴 ──────────────────────────────────────

    private IEnumerator CoPhase3Action()
    {
        int roots   = PlayerManager.instance.turnRootCount;
        int patches = PlayerManager.instance.turnPatchCount;

        switch (_action)
        {
            case OverclockAction.Explode:
                ApplyDamageToSelf(20);
                if (virusData.CurHpCnt <= 0) yield break;
                PlayerManager.instance.TakeDamage(20);
                Debug.Log("[BossOverclock] 폭발: 보스·플레이어 각 20 피해");
                break;

            case OverclockAction.Rampage:
                if (roots > 0)
                    Debug.Log("[BossOverclock] 발악 실패 (루트 사용됨)");
                else
                {
                    _digestAuthStacks = Mathf.Max(0, _digestAuthStacks - 1);
                    Debug.Log($"[BossOverclock] 발악: 소화-권한 → {_digestAuthStacks}");
                }
                break;

            case OverclockAction.Reignite:
                if (patches > 0)
                    Debug.Log("[BossOverclock] 재발화 실패 (패치 사용됨)");
                else
                {
                    _reigniteStacks++;
                    Debug.Log($"[BossOverclock] 재발화: DoT 스택 {_reigniteStacks} (×10={_reigniteStacks * 10}/턴)");
                }
                break;
        }

        yield return new WaitForSeconds(0.5f);
    }

    // ══════════════════════════════════════════════════════════════
    // 피해 처리
    // ══════════════════════════════════════════════════════════════

    public override int ApplyDamage(int damage)
    {
        // 3페이즈: 플레이어 공격 무효
        if (_currentPhase == 3)
        {
            Debug.Log("[BossOverclock] 소화-템포: 플레이어 공격 차단");
            return damage;
        }

        int remaining = damage;

        if (virusData.DefCnt > 0)
        {
            int defUsed = Mathf.Min(virusData.DefCnt, remaining);
            virusData.DefCnt -= defUsed;
            remaining -= defUsed;
        }

        if (remaining > 0)
        {
            int hpDmg = Mathf.Min(remaining, virusData.CurHpCnt);
            virusData.CurHpCnt = Mathf.Max(0, virusData.CurHpCnt - remaining);
            remaining = 0;

            // 2페이즈 스턴 중 받은 피해 추적
            if (_currentPhase == 2 && _overheatStunTurns > 0)
                _overheatDamageReceived += hpDmg;
        }

        UpdateData();
        CheckPhaseTransition();
        Debug.Log($"[BossOverclock] 피해 {damage} → 체력: {virusData.CurHpCnt}");
        return remaining;
    }

    // 자신에게 피해 — HP 0 시 GameOver
    private void ApplyDamageToSelf(int damage)
    {
        int remaining = damage;

        if (virusData.DefCnt > 0)
        {
            int used = Mathf.Min(virusData.DefCnt, remaining);
            virusData.DefCnt -= used;
            remaining -= used;
        }

        if (remaining > 0)
            virusData.CurHpCnt = Mathf.Max(0, virusData.CurHpCnt - remaining);

        UpdateData();

        if (virusData.CurHpCnt <= 0)
        {
            Debug.Log("[BossOverclock] 체력 0 → 플레이어 패배");
            CleanupEffects();
            GameManager.Instance.GameOver();
        }
    }

    // ══════════════════════════════════════════════════════════════
    // 클린업 / 사망
    // ══════════════════════════════════════════════════════════════

    private void CleanupEffects()
    {
        if (PlayerManager.instance == null) return;

        if (_overheatPassiveApplied)
        {
            PlayerManager.instance.AddPermanentStat(StatType.Cost, -3);
            PlayerManager.instance.bonusDrawCount = 0;
        }
        if (_coolTempoApplied)
            PlayerManager.instance.AddPermanentStat(StatType.Cost, -3);

        PlayerManager.instance.heatStacks = 0;

        if (_bossEffects != null)
            foreach (var e in _bossEffects)
                PlayerManager.instance.UnregisterEffect(e);

        enemyUIController?.enemyStatusUI?.UnregisterEffect(_selfDamageEffect);
        enemyUIController?.enemyStatusUI?.UnregisterEffect(_enhanceBuff);

        PlayerManager.instance.UpdateUI();
    }

    protected override void OnDeath()
    {
        CleanupEffects();
        VirusSpawn.instance.SetDiscountVirusCount();

        if (VirusSpawn.instance.virusCnt <= 0)
            GameManager.Instance.GameClear();

        if (enemyUIController != null) enemyUIController.panel.SetActive(false);
        Destroy(gameObject);
    }

    // ══════════════════════════════════════════════════════════════
    // 유틸
    // ══════════════════════════════════════════════════════════════

    private void ChangeSprite(Sprite sp)
    {
        if (sp == null) return;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.sprite = sp;
    }
}
