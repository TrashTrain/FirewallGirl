using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 4스테이지 보스: 언렌더드.RAW
/// 1페이즈(100%~80%) → 2페이즈(80%~50%) → 3페이즈(50%~HP≤100) → 4페이즈(HP≤100)
/// 승리 조건: 보스 HP = 0
/// </summary>
public class BossUnrendered : Virus
{
    // ─── 스프라이트 ──────────────────────────────────────────────
    [Header("페이즈별 스프라이트")]
    public Sprite phase1Sprite;
    public Sprite phase2Sprite;
    public Sprite phase3Sprite;
    public Sprite phase4Sprite;

    // ─── 디코이 ──────────────────────────────────────────────────
    [Header("디코이 설정")]
    [SerializeField] private GameObject _decoyPrefab;
    [SerializeField] private Transform[] _decoySpawnPoints; // Inspector에서 5개 위치 연결

    // ─── Status Effect Icons ─────────────────────────────────────
    [Header("Status Effect Icons")]
    [SerializeField] private Sprite _iconGraphicChange;
    [SerializeField] private Sprite _iconCopy;
    [SerializeField] private Sprite _iconCooling;
    [SerializeField] private Sprite _iconPermissionRecovery;
    [SerializeField] private Sprite _iconCollapse;

    // ─── 공통 기믹 static 플래그 ─────────────────────────────────
    /// <summary>2페이즈~: 카드 정보 가리기 활성 (UI TODO)</summary>
    public static bool CardInfoHidingActive = false;
    /// <summary>3페이즈~: 카드 긍/부 반전 활성 (UI TODO)</summary>
    public static bool CardInfoDeceptionActive = false;
    /// <summary>그래픽 변화 레벨: 1=초급/2=중급/3=상급 (UI TODO)</summary>
    public static int GraphicChangeLevel = 0;
    /// <summary>2페이즈~: 붕괴 디버프 활성 (카드 효과 절반, UI TODO)</summary>
    public static bool CollapseDebuffActive = false;

    // ─── 행동 열거형 ─────────────────────────────────────────────
    private enum UnrenderedAction
    {
        // 1페이즈
        GraphicChaos,       // 그래픽-혼란: 디코이 5기 생성
        PixelCollapse,      // 픽셀-붕괴: 플레이어 카드 1장 픽셀화 (TODO stub)
        GraphicCopy,        // 그래픽-복사: 플레이어 공/방 긍정 변화 2턴 복사
        RenderPunch,        // 렌더 펀치!: ATK × 1 (1페이즈), × 1.5 (2페이즈), × 2 (3페이즈)
        // 2페이즈 추가
        GraphicOverheat,    // 그래픽-과열: 직전 턴 총 사용 카드 수 × 5 피해
        GraphicCooling,     // 그래픽-냉각: 2턴간 받는 피해 50% 감소
        GraphicLayerSep,    // 그래픽-레이어 분리: 방어도 +20
        // 3페이즈 추가
        GraphicBlueScreen,  // 그래픽-블루스크린: 다음 플레이어 턴에 블루스크린 활성 (TODO stub)
        GraphicRestore,     // 그래픽-복원: 잃은 HP의 5% 회복 (3턴 쿨타임)
        GraphicSystemFake,  // 그래픽-시스템 페이크: 플레이어 손패에 페이크 카드 추가 (TODO stub)
        GraphicStrike,      // 그래픽-일격: ATK만큼 공격, 실제 HP 피해의 절반 자해
        // 4페이즈
        GraphicResourceRecovery, // 그래픽-리소스 회수: ATK만큼 공격 후 실제 피해만큼 자체 회복
        GraphicSmash,            // 그래픽-박살: ATK × 2회 공격, 다음 턴 행동불능
        // 공통
        Stunned,
    }

    private UnrenderedAction _action;

    // ─── 페이즈 ──────────────────────────────────────────────────
    private int _currentPhase = 1;

    // ─── 디코이 상태 ─────────────────────────────────────────────
    private List<UnrenderedDecoy> _activeDecoys = new List<UnrenderedDecoy>();
    private int _hitboxDecoyIndex = 0;          // 현재 히트박스 디코이 인덱스
    private int _chaosCooldownTurns = 0;        // 그래픽-혼란 쿨다운 남은 턴 (성공:2, 실패:0)
    private int _wrongDecoyClickCount = 0;      // 잘못된 디코이 클릭 횟수 (2회 = 자폭)
    private bool _firstChaosFired = false;       // 최초 혼란 발동 여부 (힌트 팝업용)

    // ─── 그래픽-복사 ─────────────────────────────────────────────
    private int _snapshotAtk = 0;
    private int _snapshotDef = 0;
    private int _copyAppliedAtk = 0;
    private int _copyAppliedDef = 0;
    private int _copyTurnsLeft = 0;

    // ─── 그래픽-냉각 ─────────────────────────────────────────────
    private bool _coolingActive = false;
    private int _coolingTurnsLeft = 0;

    // ─── 그래픽-복원 쿨타임 ──────────────────────────────────────
    private int _restoreCooldown = 0;           // 0이면 사용 가능

    // ─── 4페이즈 행동불능 ─────────────────────────────────────────
    private int _stunTurns = 0;

    // ─── 2페이즈 패시브: 그래픽-붕괴 카운터 ─────────────────────
    private int _collapseTurnCounter = 0;       // 3마다 붕괴 활성

    // ─── 3페이즈 패시브: 그래픽-분리 카운터 ─────────────────────
    private int _phase3SepTurnCounter = 0;
    private bool _sepFlipState = false;         // true=뒤집기, false=원복 교대

    // ─── 3페이즈: 그래픽-스킵 (플레이어 턴 5초 타이머) ──────────
    private Coroutine _skipCoroutine = null;

    // ─── 효과 레지스트리 ─────────────────────────────────────────
    private List<ActiveEffect> _bossEffects;
    private ActiveEffect _effectGraphicChange;
    private ActiveEffect _effectCopy;
    private ActiveEffect _effectCooling;
    private ActiveEffect _effectPermissionRecovery;
    private ActiveEffect _effectCollapse;

    // ══════════════════════════════════════════════════════════════
    // 초기화
    // ══════════════════════════════════════════════════════════════

    private Dictionary<string, string> Phase1Descriptions() => new Dictionary<string, string>
    {
        { "GraphicChaos",   "그래픽-혼란\n디코이 5기를 생성합니다. (히트박스는 1기)" },
        { "PixelCollapse",  "픽셀-붕괴\n플레이어 카드 1장을 픽셀화합니다." },
        { "GraphicCopy",    "그래픽-복사\n플레이어 공/방 긍정 변화를 2턴간 복사합니다." },
        { "RenderPunch",    "렌더 펀치!\nATK만큼 공격합니다." },
        { "Stunned",        "행동불능 상태입니다." },
    };

    private Dictionary<string, string> Phase2Descriptions() => new Dictionary<string, string>
    {
        { "GraphicChaos",   "그래픽-혼란\n디코이 5기를 생성합니다. (히트박스는 1기)" },
        { "RenderPunch",    "렌더 펀치!\nATK × 1.5만큼 공격합니다." },
        { "GraphicOverheat","그래픽-과열\n직전 턴 총 사용 카드 수 × 5 피해를 줍니다." },
        { "GraphicCooling", "그래픽-냉각\n2턴간 받는 피해를 50% 감소합니다." },
        { "GraphicLayerSep","그래픽-레이어 분리\n방어도 +20을 획득합니다." },
        { "Stunned",        "행동불능 상태입니다." },
    };

    private Dictionary<string, string> Phase3Descriptions() => new Dictionary<string, string>
    {
        { "GraphicBlueScreen","그래픽-블루스크린\n다음 플레이어 턴에 블루스크린을 활성화합니다. (5초 후 자동 턴 종료)" },
        { "GraphicRestore",   "그래픽-복원\n잃은 HP의 5%를 회복합니다. (3턴 쿨타임)" },
        { "GraphicSystemFake","그래픽-시스템 페이크\n플레이어 손패에 페이크 카드를 추가합니다." },
        { "GraphicStrike",    "그래픽-일격\nATK만큼 공격합니다. 실제 HP 피해의 절반만큼 자해합니다." },
        { "RenderPunch",      "렌더 펀치!\nATK × 2만큼 공격합니다." },
        { "Stunned",          "행동불능 상태입니다." },
    };

    private Dictionary<string, string> Phase4Descriptions() => new Dictionary<string, string>
    {
        { "GraphicResourceRecovery","그래픽-리소스 회수\nATK만큼 공격하고, 실제 HP 피해만큼 자신이 회복합니다." },
        { "GraphicSmash",           "그래픽-박살\nATK × 2회 공격합니다. 다음 턴 행동불능 상태가 됩니다." },
        { "Stunned",                "행동불능 상태입니다." },
    };

    protected override void Start()
    {
        if (VirusMgr.instance == null) return;
        InitData();
        ChangeSprite(phase1Sprite);

        if (spawnNum != 3)
            Debug.LogError("[BossUnrendered] 보스는 Spawn3 위치에 스폰되어야 합니다!");

        if (enemyUIController != null)
            enemyUIController.state.OverrideDescriptions(Phase1Descriptions());

        // 1페이즈 패시브: GraphicChangeLevel = 1
        GraphicChangeLevel = 1;
        // TODO: [UI TODO] GraphicChangeLevel=1 적용: 카드 정보 긍/부 수치 가림, 플레이어 공/방 수치 가림

        RegisterStatusEffects();

        EnemyTurnManager.OnPlayerTurnEnded   += HandlePlayerTurnEnded;
        EnemyTurnManager.OnPlayerTurnStarted += HandlePlayerTurnStarted;

        RollNextActionAndUpdateIcon();
    }

    private void OnDestroy()
    {
        EnemyTurnManager.OnPlayerTurnEnded   -= HandlePlayerTurnEnded;
        EnemyTurnManager.OnPlayerTurnStarted -= HandlePlayerTurnStarted;
        StopSkipTimer();
        CleanupAllDecoys();

        // static 플래그 초기화
        CardInfoHidingActive    = false;
        CardInfoDeceptionActive = false;
        GraphicChangeLevel      = 0;
        CollapseDebuffActive    = false;
    }

    // ══════════════════════════════════════════════════════════════
    // 효과 레지스트리
    // ══════════════════════════════════════════════════════════════

    private void RegisterStatusEffects()
    {
        if (enemyUIController == null) return;

        _effectGraphicChange = new ActiveEffect(
            _iconGraphicChange, false,
            () => GraphicChangeLevel > 0,
            () => $"그래픽 변화 (레벨 {GraphicChangeLevel})\n카드/스탯 정보 왜곡 효과 적용 중 (UI TODO)"
        );
        _effectCopy = new ActiveEffect(
            _iconCopy, true,
            () => _copyTurnsLeft > 0,
            () => $"그래픽-복사 (남은 {_copyTurnsLeft}턴)\n플레이어 공+{_copyAppliedAtk} / 방+{_copyAppliedDef} 복사 중"
        );
        _effectCooling = new ActiveEffect(
            _iconCooling, true,
            () => _coolingActive,
            () => $"그래픽-냉각 (남은 {_coolingTurnsLeft}턴)\n받는 피해 50% 감소"
        );
        _effectPermissionRecovery = new ActiveEffect(
            _iconPermissionRecovery, true,
            () => _currentPhase == 4,
            () => "권한 복구-렌더\n플레이어 긍정효과 +150%, 부정수치 0, 매 턴 방어도+10 (TODO)"
        );
        _effectCollapse = new ActiveEffect(
            _iconCollapse, false,
            () => CollapseDebuffActive,
            () => "붕괴 디버프\n카드 효과 절반 (UI TODO)"
        );

        enemyUIController.enemyStatusUI?.RegisterEffect(_effectGraphicChange);
        enemyUIController.enemyStatusUI?.RegisterEffect(_effectCopy);
        enemyUIController.enemyStatusUI?.RegisterEffect(_effectCooling);
        enemyUIController.enemyStatusUI?.RegisterEffect(_effectPermissionRecovery);
        enemyUIController.enemyStatusUI?.RegisterEffect(_effectCollapse);
        enemyUIController.enemyStatusUI?.RefreshStatusUI();
    }

    // ══════════════════════════════════════════════════════════════
    // 플레이어 턴 시작 이벤트
    // ══════════════════════════════════════════════════════════════

    private void HandlePlayerTurnStarted()
    {
        if (PlayerManager.instance == null) return;

        // 그래픽-복사: 플레이어 ATK/DEF 스냅샷 저장
        _snapshotAtk = PlayerManager.instance.AttackPower;
        _snapshotDef = PlayerManager.instance.DefensePower;

        // 2페이즈 패시브: 그래픽-페이크 어택 (30% 확률, TODO: 피격 모션)
        if (_currentPhase >= 2)
        {
            if (Random.value < 0.30f)
            {
                // TODO: [UI TODO] 그래픽-페이크 어택 피격 모션 재생
                Debug.Log("[BossUnrendered] 그래픽-페이크 어택 발동 (피격 모션 TODO)");
            }
        }

        // 4페이즈 패시브: 권한 복구-렌더 — 매 턴 방어도 +10 (→ 소멸형)
        if (_currentPhase >= 4)
        {
            PlayerManager.instance.AddTurnStatDelta(StatType.Defense, 10);
            Debug.Log("[BossUnrendered] 권한 복구-렌더: 플레이어 방어도 +10 (이번 턴 소멸)");
        }

        // 3페이즈~: 그래픽-스킵 타이머 시작 (블루스크린 중 비활성화는 TODO stub)
        if (_currentPhase >= 3)
            StartSkipTimer();
    }

    // ══════════════════════════════════════════════════════════════
    // 플레이어 턴 종료 이벤트
    // ══════════════════════════════════════════════════════════════

    private void HandlePlayerTurnEnded()
    {
        if (PlayerManager.instance == null) return;

        StopSkipTimer();

        int patches  = PlayerManager.instance.turnPatchCount;
        int roots    = PlayerManager.instance.turnRootCount;
        int vaccines = PlayerManager.instance.turnVaccineCount;

        // 그래픽-복사 만료 처리
        if (_copyTurnsLeft > 0)
        {
            _copyTurnsLeft--;
            if (_copyTurnsLeft == 0)
            {
                // 복사 효과 제거
                if (_copyAppliedAtk > 0) virusData.AtkDmg = Mathf.Max(0, virusData.AtkDmg - _copyAppliedAtk);
                if (_copyAppliedDef > 0) virusData.DefCnt = Mathf.Max(0, virusData.DefCnt - _copyAppliedDef);
                _copyAppliedAtk = 0;
                _copyAppliedDef = 0;
                UpdateData();
                Debug.Log("[BossUnrendered] 그래픽-복사 만료 → 복사 수치 제거");
            }
        }

        // 냉각 쿨다운 카운트
        if (_coolingActive)
        {
            _coolingTurnsLeft--;
            if (_coolingTurnsLeft <= 0)
            {
                _coolingActive = false;
                _coolingTurnsLeft = 0;
                Debug.Log("[BossUnrendered] 그래픽-냉각 해제");
            }
        }

        // 디코이 플레이어 턴 종료 시 전체 소멸 (성공/실패 무관)
        if (_activeDecoys.Count > 0)
        {
            CleanupAllDecoys(); // 내부에서 _wrongDecoyClickCount도 초기화
        }

        // 그래픽-혼란 쿨다운 카운트다운
        if (_chaosCooldownTurns > 0)
        {
            _chaosCooldownTurns--;
            Debug.Log($"[BossUnrendered] 그래픽-혼란 쿨다운 남은 턴: {_chaosCooldownTurns}");
        }

        // 그래픽-복원 쿨타임 감소
        if (_restoreCooldown > 0)
        {
            _restoreCooldown--;
            Debug.Log($"[BossUnrendered] 그래픽-복원 쿨타임: {_restoreCooldown}");
        }

        // 2페이즈 패시브: 패치 카드 체력 회복 (turnPatchCount × 10)
        if (_currentPhase >= 2 && patches > 0)
        {
            int heal = patches * 10;
            virusData.CurHpCnt = Mathf.Min(virusData.HpCnt, virusData.CurHpCnt + heal);
            UpdateData();
            Debug.Log($"[BossUnrendered] 패치 카드 체력 회복: +{heal}");
        }

        // 2페이즈 패시브: 그래픽-붕괴 (3턴마다 CollapseDebuffActive = true, 1턴 유지)
        if (_currentPhase >= 2)
        {
            // 이전 턴에 붕괴가 활성화되어 있었으면 이번 턴 종료 시 해제
            if (CollapseDebuffActive)
            {
                CollapseDebuffActive = false;
                Debug.Log("[BossUnrendered] 그래픽-붕괴 디버프 해제");
            }

            _collapseTurnCounter++;
            if (_collapseTurnCounter >= 3)
            {
                _collapseTurnCounter = 0;
                CollapseDebuffActive = true;
                Debug.Log("[BossUnrendered] 그래픽-붕괴 디버프 활성 (1턴)");
            }
        }

        // 3페이즈 패시브: 소심한 복수-렌더 (보스 자해 10)
        if (_currentPhase >= 3)
        {
            ApplyDamageToSelf(10);
            Debug.Log("[BossUnrendered] 소심한 복수-렌더: 보스 자해 10");
            if (virusData.CurHpCnt <= 0) return;
        }

        // 3페이즈 패시브: 루트 카드 반사 (turnRootCount × 10 자해)
        if (_currentPhase >= 3 && roots > 0)
        {
            int selfDmg = roots * 10;
            ApplyDamageToSelf(selfDmg);
            Debug.Log($"[BossUnrendered] 루트 카드 반사: 보스 자해 {selfDmg}");
            if (virusData.CurHpCnt <= 0) return;
        }

        // 3페이즈 패시브: 백신 카드 반사 (turnVaccineCount × 10 자해)
        if (_currentPhase >= 3 && vaccines > 0)
        {
            int selfDmg = vaccines * 10;
            ApplyDamageToSelf(selfDmg);
            Debug.Log($"[BossUnrendered] 백신 카드 반사: 보스 자해 {selfDmg}");
            if (virusData.CurHpCnt <= 0) return;
        }

        // 3페이즈 패시브: 그래픽-분리 (3턴마다 뒤집기/원복 교대)
        if (_currentPhase >= 3)
        {
            _phase3SepTurnCounter++;
            if (_phase3SepTurnCounter >= 3)
            {
                _phase3SepTurnCounter = 0;
                _sepFlipState = !_sepFlipState;
                // TODO: [UI TODO] 그래픽-분리 효과 (뒤집기/원복)
                Debug.Log($"[BossUnrendered] 그래픽-분리: {(_sepFlipState ? "뒤집기" : "원복")}");
            }
        }

        UpdateData();
        PlayerManager.instance.UpdateUI();
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
            case 4: RollPhase4Action(); break;
        }
    }

    private void CheckPhaseTransition()
    {
        if (virusData == null) return;
        float ratio   = (float)virusData.CurHpCnt / virusData.HpCnt;
        int   curHP   = virusData.CurHpCnt;

        if (_currentPhase == 1 && ratio <= 0.80f)
            EnterPhase2();
        else if (_currentPhase == 2 && ratio <= 0.50f)
            EnterPhase3();
        else if (_currentPhase == 3 && curHP <= 100)
            EnterPhase4();
    }

    // ─── 1페이즈 행동 결정 ────────────────────────────────────────

    private void RollPhase1Action()
    {
        // 그래픽-혼란 쿨다운 중이면 풀에서 제외
        List<UnrenderedAction> pool = new List<UnrenderedAction>
        {
            UnrenderedAction.PixelCollapse,
            UnrenderedAction.GraphicCopy,
            UnrenderedAction.RenderPunch,
        };
        if (_chaosCooldownTurns == 0)
            pool.Add(UnrenderedAction.GraphicChaos);

        _action = pool[Random.Range(0, pool.Count)];
    }

    // ─── 2페이즈 행동 결정 ────────────────────────────────────────

    private void RollPhase2Action()
    {
        List<UnrenderedAction> pool = new List<UnrenderedAction>
        {
            UnrenderedAction.RenderPunch,
            UnrenderedAction.GraphicOverheat,
            UnrenderedAction.GraphicCooling,
            UnrenderedAction.GraphicLayerSep,
        };
        if (_chaosCooldownTurns == 0)
            pool.Add(UnrenderedAction.GraphicChaos);

        _action = pool[Random.Range(0, pool.Count)];
    }

    // ─── 3페이즈 행동 결정 ────────────────────────────────────────

    private void RollPhase3Action()
    {
        List<UnrenderedAction> pool = new List<UnrenderedAction>
        {
            UnrenderedAction.GraphicBlueScreen,
            UnrenderedAction.GraphicSystemFake,
            UnrenderedAction.GraphicStrike,
            UnrenderedAction.RenderPunch,
        };

        // 그래픽-복원: 쿨타임 0이고 HP가 최대가 아닐 때만 풀에 포함
        if (_restoreCooldown == 0 && virusData.CurHpCnt < virusData.HpCnt)
            pool.Add(UnrenderedAction.GraphicRestore);

        _action = pool[Random.Range(0, pool.Count)];
    }

    // ─── 4페이즈 행동 결정 ────────────────────────────────────────

    private void RollPhase4Action()
    {
        if (_stunTurns > 0)
        {
            _stunTurns--;
            _action = UnrenderedAction.Stunned;
            return;
        }

        _action = Random.Range(0, 2) == 0
            ? UnrenderedAction.GraphicResourceRecovery
            : UnrenderedAction.GraphicSmash;
    }

    // ══════════════════════════════════════════════════════════════
    // 페이즈 진입
    // ══════════════════════════════════════════════════════════════

    private void EnterPhase2()
    {
        _currentPhase = 2;
        ChangeSprite(phase2Sprite);

        GraphicChangeLevel      = 2;
        CardInfoHidingActive    = true;
        _collapseTurnCounter    = 0;
        CollapseDebuffActive    = false;

        UpdateData();
        if (PlayerManager.instance != null) PlayerManager.instance.UpdateUI();
        enemyUIController?.state.OverrideDescriptions(Phase2Descriptions());

        // TODO: [UI TODO] GraphicChangeLevel=2, CardInfoHidingActive=true 적용
        // TODO: [UI TODO] 그래픽 화질 저하 실시간 5초 반복
        Debug.Log("[BossUnrendered] 2페이즈 진입");
    }

    private void EnterPhase3()
    {
        _currentPhase = 3;
        ChangeSprite(phase3Sprite);

        GraphicChangeLevel      = 3;
        CardInfoDeceptionActive = true;
        _phase3SepTurnCounter   = 0;
        _restoreCooldown        = 0;

        UpdateData();
        if (PlayerManager.instance != null) PlayerManager.instance.UpdateUI();
        enemyUIController?.state.OverrideDescriptions(Phase3Descriptions());

        // 3페이즈 진입 시 그래픽-분리 첫 발동
        // TODO: [UI TODO] 그래픽-분리 첫 발동
        Debug.Log("[BossUnrendered] 3페이즈 진입 → 그래픽-분리 첫 발동 (UI TODO)");
    }

    private void EnterPhase4()
    {
        _currentPhase = 4;
        ChangeSprite(phase4Sprite);

        // 모든 기존 현상 해제
        CardInfoHidingActive    = false;
        CardInfoDeceptionActive = false;
        CollapseDebuffActive    = false;
        GraphicChangeLevel      = 0;
        _coolingActive          = false;
        _coolingTurnsLeft       = 0;
        _copyTurnsLeft          = 0;
        _copyAppliedAtk         = 0;
        _copyAppliedDef         = 0;
        StopSkipTimer();
        CleanupAllDecoys();

        // 그래픽-몰입: 공격력 +20
        virusData.AtkDmg += 20;

        UpdateData();
        if (PlayerManager.instance != null) PlayerManager.instance.UpdateUI();
        enemyUIController?.state.OverrideDescriptions(Phase4Descriptions());

        // TODO: [UI TODO] 4페이즈 전환 연출 (모든 기존 현상 해제)
        // TODO: [UI TODO] 권한 복구-렌더: 플레이어 긍정효과 +150%
        // TODO: [UI TODO] 권한 복구-렌더: 모든 카드 부정수치 0
        Debug.Log("[BossUnrendered] 4페이즈 진입 → 공격력 +20, 모든 기존 현상 해제");
    }

    // ══════════════════════════════════════════════════════════════
    // 행동 실행
    // ══════════════════════════════════════════════════════════════

    protected override IEnumerator CoRunStateAction(State s)
    {
        // 4페이즈 패시브: 그래픽-몰입 — 매 CoRunStateAction 시작 시 방어도 20 설정
        if (_currentPhase >= 4)
        {
            virusData.DefCnt = 20;
            UpdateData();
        }

        // 디코이가 살아있으면 매 보스 턴마다 히트박스 이동
        MoveDecoyHitbox();

        if (_action == UnrenderedAction.Stunned)
        {
            yield return new WaitForSeconds(0.5f);
            yield break;
        }

        switch (_currentPhase)
        {
            case 1: yield return CoPhase1Action(); break;
            case 2: yield return CoPhase2Action(); break;
            case 3: yield return CoPhase3Action(); break;
            case 4: yield return CoPhase4Action(); break;
        }
    }

    // ─── 1페이즈 행동 코루틴 ──────────────────────────────────────

    private IEnumerator CoPhase1Action()
    {
        switch (_action)
        {
            case UnrenderedAction.GraphicChaos:
                yield return CoGraphicChaos();
                break;

            case UnrenderedAction.PixelCollapse:
                yield return CoPixelCollapse();
                break;

            case UnrenderedAction.GraphicCopy:
                yield return CoGraphicCopy();
                break;

            case UnrenderedAction.RenderPunch:
                yield return CoRenderPunch(1.0f);
                break;
        }
    }

    // ─── 2페이즈 행동 코루틴 ──────────────────────────────────────

    private IEnumerator CoPhase2Action()
    {
        switch (_action)
        {
            case UnrenderedAction.GraphicChaos:
                yield return CoGraphicChaos();
                break;

            case UnrenderedAction.RenderPunch:
                yield return CoRenderPunch(1.5f);
                break;

            case UnrenderedAction.GraphicOverheat:
                yield return CoGraphicOverheat();
                break;

            case UnrenderedAction.GraphicCooling:
                yield return CoGraphicCooling();
                break;

            case UnrenderedAction.GraphicLayerSep:
                yield return CoGraphicLayerSep();
                break;
        }
    }

    // ─── 3페이즈 행동 코루틴 ──────────────────────────────────────

    private IEnumerator CoPhase3Action()
    {
        switch (_action)
        {
            case UnrenderedAction.GraphicBlueScreen:
                yield return CoGraphicBlueScreen();
                break;

            case UnrenderedAction.GraphicRestore:
                yield return CoGraphicRestore();
                break;

            case UnrenderedAction.GraphicSystemFake:
                yield return CoGraphicSystemFake();
                break;

            case UnrenderedAction.GraphicStrike:
                yield return CoGraphicStrike();
                break;

            case UnrenderedAction.RenderPunch:
                yield return CoRenderPunch(2.0f);
                break;
        }
    }

    // ─── 4페이즈 행동 코루틴 ──────────────────────────────────────

    private IEnumerator CoPhase4Action()
    {
        switch (_action)
        {
            case UnrenderedAction.GraphicResourceRecovery:
                yield return CoGraphicResourceRecovery();
                break;

            case UnrenderedAction.GraphicSmash:
                yield return CoGraphicSmash();
                break;
        }
    }

    // ══════════════════════════════════════════════════════════════
    // 개별 행동 코루틴
    // ══════════════════════════════════════════════════════════════

    /// <summary>그래픽-혼란: 디코이 5기 생성 (히트박스 1기 랜덤 배치)</summary>
    private IEnumerator CoGraphicChaos()
    {
        if (!_firstChaosFired)
        {
            _firstChaosFired = true;
            // TODO: [UI 출력 로직] 렌더 힌트 팝업 알림창 출력 (10초 지속)
            // 출력 조건: 최초 그래픽-혼란 발동 시 1회
            // 메시지1: "한 명만 가면을 안쓰고 있는 것 같아요!"
            // 메시지2: "히트박스도 바뀐 것 같아요..."
            // 구현 방법 미정 (UI 설계 후 작성 예정)
        }

        CleanupAllDecoys();

        if (_decoyPrefab == null || _decoySpawnPoints == null || _decoySpawnPoints.Length < 5)
        {
            Debug.LogWarning("[BossUnrendered] 디코이 프리팹 또는 스폰 포인트(5개) 미설정");
            yield return new WaitForSeconds(0.5f);
            yield break;
        }

        // 히트박스 인덱스 랜덤 선정
        _hitboxDecoyIndex = Random.Range(0, 5);

        for (int i = 0; i < 5; i++)
        {
            GameObject obj = Instantiate(_decoyPrefab, _decoySpawnPoints[i].position, Quaternion.identity);
            UnrenderedDecoy decoy = obj.GetComponent<UnrenderedDecoy>();
            if (decoy == null) decoy = obj.AddComponent<UnrenderedDecoy>();

            bool isHitbox = (i == _hitboxDecoyIndex);
            decoy.Setup(this, isHitbox);
            _activeDecoys.Add(decoy);
        }

        Debug.Log($"[BossUnrendered] 그래픽-혼란: 디코이 5기 생성 (히트박스 인덱스 {_hitboxDecoyIndex})");
        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>픽셀-붕괴: 플레이어 카드 1장 픽셀화 (TODO stub)</summary>
    private IEnumerator CoPixelCollapse()
    {
        // TODO: [UI/카드 시스템] 플레이어 손패 카드 1장 랜덤 선택 → 픽셀화 효과 적용
        Debug.Log("[BossUnrendered] 픽셀-붕괴 발동 (카드 픽셀화 TODO)");
        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>그래픽-복사: 플레이어 ATK/DEF 긍정 변화(스냅샷 대비 증가분)를 2턴간 복사</summary>
    private IEnumerator CoGraphicCopy()
    {
        if (PlayerManager.instance == null)
        {
            yield return new WaitForSeconds(0.5f);
            yield break;
        }

        int currentAtk = PlayerManager.instance.AttackPower;
        int currentDef = PlayerManager.instance.DefensePower;

        // 스냅샷 대비 증가분만 복사 (긍정 변화만)
        int gainedAtk = Mathf.Max(0, currentAtk - _snapshotAtk);
        int gainedDef = Mathf.Max(0, currentDef - _snapshotDef);

        // 기존 복사 효과 제거 후 새로 적용
        if (_copyAppliedAtk > 0) virusData.AtkDmg = Mathf.Max(0, virusData.AtkDmg - _copyAppliedAtk);
        if (_copyAppliedDef > 0) virusData.DefCnt = Mathf.Max(0, virusData.DefCnt - _copyAppliedDef);

        _copyAppliedAtk = gainedAtk;
        _copyAppliedDef = gainedDef;
        _copyTurnsLeft  = 2;

        virusData.AtkDmg += _copyAppliedAtk;
        virusData.DefCnt += _copyAppliedDef;
        UpdateData();

        Debug.Log($"[BossUnrendered] 그래픽-복사: ATK+{gainedAtk}, DEF+{gainedDef} (2턴)");
        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>렌더 펀치!: ATK × multiplier (내림)</summary>
    private IEnumerator CoRenderPunch(float multiplier)
    {
        int baseDmg = virusData.AtkDmg;
        int dmg = Mathf.FloorToInt(baseDmg * multiplier);

        // 임시로 ATK 변경 후 공격, 이후 복원
        int originalAtk = virusData.AtkDmg;
        virusData.AtkDmg = dmg;
        UpdateData();

        yield return CoAttack();

        virusData.AtkDmg = originalAtk;
        UpdateData();
        Debug.Log($"[BossUnrendered] 렌더 펀치! {dmg} 피해 (×{multiplier})");
    }

    /// <summary>그래픽-과열: 직전 턴 총 사용 카드 수 × 5 피해</summary>
    private IEnumerator CoGraphicOverheat()
    {
        if (PlayerManager.instance == null)
        {
            yield return new WaitForSeconds(0.5f);
            yield break;
        }

        int cardCount = PlayerManager.instance.turnTotalCardCount;
        int dmg       = cardCount * 5;

        if (dmg > 0)
        {
            PlayerManager.instance.TakeDamage(dmg);
            Debug.Log($"[BossUnrendered] 그래픽-과열: 카드 {cardCount}장 × 5 = {dmg} 피해");
        }
        else
        {
            Debug.Log("[BossUnrendered] 그래픽-과열: 사용 카드 없음, 피해 없음");
        }

        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>그래픽-냉각: 2턴간 받는 피해 50% 감소</summary>
    private IEnumerator CoGraphicCooling()
    {
        _coolingActive    = true;
        _coolingTurnsLeft = 2;
        Debug.Log("[BossUnrendered] 그래픽-냉각 발동: 2턴간 피해 50% 감소");
        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>그래픽-레이어 분리: 방어도 +20</summary>
    private IEnumerator CoGraphicLayerSep()
    {
        virusData.DefCnt += 20;
        UpdateData();
        Debug.Log("[BossUnrendered] 그래픽-레이어 분리: 방어도 +20");
        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>그래픽-블루스크린: 다음 플레이어 턴에 블루스크린 활성, 5초 후 자동 턴 종료 (TODO stub)</summary>
    private IEnumerator CoGraphicBlueScreen()
    {
        // TODO: [UI/카드 시스템] 다음 플레이어 턴에 블루스크린 활성
        // 블루스크린 발동 중 그래픽-스킵 비활성화
        Debug.Log("[BossUnrendered] 그래픽-블루스크린 예약 (다음 플레이어 턴, 5초 자동 종료 TODO)");
        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>그래픽-복원: 잃은 HP의 5% 회복, 3턴 쿨타임</summary>
    private IEnumerator CoGraphicRestore()
    {
        int lost = virusData.HpCnt - virusData.CurHpCnt;
        int heal = Mathf.FloorToInt(lost * 0.05f);

        if (heal > 0)
        {
            virusData.CurHpCnt = Mathf.Min(virusData.HpCnt, virusData.CurHpCnt + heal);
            UpdateData();
        }

        _restoreCooldown = 3;
        Debug.Log($"[BossUnrendered] 그래픽-복원: +{heal} 회복, 쿨타임 3턴");
        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>그래픽-시스템 페이크: 플레이어 손패에 페이크 카드 추가 (TODO stub)</summary>
    private IEnumerator CoGraphicSystemFake()
    {
        // TODO: [UI/카드 시스템] 플레이어 손패에 페이크 카드 추가
        Debug.Log("[BossUnrendered] 그래픽-시스템 페이크 발동 (손패 페이크 카드 추가 TODO)");
        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>그래픽-일격: ATK만큼 공격, 방어도 차감 후 실제 HP 피해의 절반 자해</summary>
    private IEnumerator CoGraphicStrike()
    {
        if (PlayerManager.instance == null)
        {
            yield return new WaitForSeconds(0.5f);
            yield break;
        }

        int prevDef = PlayerManager.instance.DefensePower;
        int prevHP  = PlayerManager.instance.currentHP;

        yield return CoAttack();

        // 실제 HP 피해 계산
        int afterHP    = PlayerManager.instance.currentHP;
        int hpDamage   = Mathf.Max(0, prevHP - afterHP);
        int selfDmg    = Mathf.FloorToInt(hpDamage * 0.5f);

        if (selfDmg > 0)
        {
            ApplyDamageToSelf(selfDmg);
            Debug.Log($"[BossUnrendered] 그래픽-일격 자해: {selfDmg}");
        }
    }

    /// <summary>그래픽-리소스 회수: ATK만큼 공격, 방어도 제외 실제 HP 피해만큼 자신 체력 회복</summary>
    private IEnumerator CoGraphicResourceRecovery()
    {
        if (PlayerManager.instance == null)
        {
            yield return new WaitForSeconds(0.5f);
            yield break;
        }

        int prevHP = PlayerManager.instance.currentHP;

        yield return CoAttack();

        int afterHP  = PlayerManager.instance.currentHP;
        int hpDamage = Mathf.Max(0, prevHP - afterHP);

        if (hpDamage > 0)
        {
            virusData.CurHpCnt = Mathf.Min(virusData.HpCnt, virusData.CurHpCnt + hpDamage);
            UpdateData();
            Debug.Log($"[BossUnrendered] 그래픽-리소스 회수: {hpDamage} 회복");
        }
    }

    /// <summary>그래픽-박살: ATK만큼 2회 공격, 다음 턴 행동불능</summary>
    private IEnumerator CoGraphicSmash()
    {
        yield return CoAttack();
        yield return new WaitForSeconds(0.2f);
        yield return CoAttack();

        _stunTurns = 1;
        Debug.Log("[BossUnrendered] 그래픽-박살: 2회 공격 후 다음 턴 행동불능");
    }

    // ══════════════════════════════════════════════════════════════
    // 디코이 관련
    // ══════════════════════════════════════════════════════════════

    /// <summary>
    /// 플레이어가 디코이 클릭 시 호출
    /// - 히트박스(빛나는 것) 클릭: 보스에게 피해, 전체 디코이 소멸, 2턴 쿨다운
    /// - 잘못된 디코이 클릭: 클릭 수 누적. 2회째: 전체 자폭 30 피해, 패턴 재사용(쿨다운 없음)
    /// </summary>
    public void OnDecoyClicked(UnrenderedDecoy decoy, bool isHitbox)
    {
        if (isHitbox)
        {
            Debug.Log("[BossUnrendered] 히트박스 디코이 클릭 → 본체 공격 성공!");
            // 플레이어 공격력으로 보스에게 피해
            if (PlayerManager.instance != null)
                ApplyDamage(PlayerManager.instance.AttackPower);

            // 전체 디코이 소멸 + 2턴 쿨다운
            _wrongDecoyClickCount = 0;
            _chaosCooldownTurns   = 2;
            CleanupAllDecoys();
        }
        else
        {
            _wrongDecoyClickCount++;
            Debug.Log($"[BossUnrendered] 잘못된 디코이 클릭 ({_wrongDecoyClickCount}/2)");

            if (_wrongDecoyClickCount >= 2)
            {
                // 2회 실패: 전체 자폭 → 30 피해, 쿨다운 없이 패턴 재사용 가능
                Debug.Log("[BossUnrendered] 디코이 전체 자폭 → 플레이어 30 피해");
                if (PlayerManager.instance != null)
                    PlayerManager.instance.TakeDamage(30);

                _wrongDecoyClickCount = 0;
                _chaosCooldownTurns   = 0;   // 쿨다운 없음: 패턴 재사용 가능
                CleanupAllDecoys();
            }
            else
            {
                // 1회 실패 시: 클릭된 디코이만 제거 (자폭 없음)
                if (decoy != null)
                {
                    _activeDecoys.Remove(decoy);
                    Destroy(decoy.gameObject);
                }
            }
        }
    }

    /// <summary>매 보스 턴 히트박스 이동</summary>
    private void MoveDecoyHitbox()
    {
        if (_activeDecoys.Count == 0) return;

        // 기존 히트박스를 비히트박스로
        foreach (var d in _activeDecoys)
            if (d != null && d.IsHitbox) d.SetAsNonHitbox();

        // 새 히트박스 랜덤 선택 (살아있는 디코이 중)
        List<UnrenderedDecoy> alive = new List<UnrenderedDecoy>();
        foreach (var d in _activeDecoys)
            if (d != null) alive.Add(d);

        if (alive.Count > 0)
        {
            int newIdx = Random.Range(0, alive.Count);
            alive[newIdx].SetAsHitbox();
            Debug.Log("[BossUnrendered] 히트박스 이동 완료");
        }
    }

    /// <summary>모든 디코이 파괴 및 잘못된 클릭 수 초기화</summary>
    private void CleanupAllDecoys()
    {
        foreach (var decoy in _activeDecoys)
        {
            if (decoy != null)
                Destroy(decoy.gameObject);
        }
        _activeDecoys.Clear();
        _wrongDecoyClickCount = 0;
        Debug.Log("[BossUnrendered] 모든 디코이 제거");
    }

    // ══════════════════════════════════════════════════════════════
    // 그래픽-스킵 (플레이어 턴 5초 타이머, 3페이즈~)
    // ══════════════════════════════════════════════════════════════

    private void StartSkipTimer()
    {
        StopSkipTimer();
        _skipCoroutine = StartCoroutine(CoSkipTimer());
    }

    private void StopSkipTimer()
    {
        if (_skipCoroutine != null)
        {
            StopCoroutine(_skipCoroutine);
            _skipCoroutine = null;
        }
    }

    /// <summary>
    /// 그래픽-스킵: 플레이어 턴 5초 이상 지속 시 고정 5 피해 (방어도 무시, currentHP 직접 감소)
    /// 5초마다 반복. 행동으로 취급 안 됨. 블루스크린 발동 중 비활성화 (TODO).
    /// </summary>
    private IEnumerator CoSkipTimer()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);

            // TODO: 블루스크린 발동 중이면 비활성화
            if (PlayerManager.instance == null) yield break;
            if (!GameManager.PlayerTurn) yield break;

            PlayerManager.instance.currentHP = Mathf.Max(0, PlayerManager.instance.currentHP - 5);
            PlayerManager.instance.UpdateUI();
            Debug.Log("[BossUnrendered] 그래픽-스킵: 방어도 무시 5 피해");

            if (PlayerManager.instance.currentHP <= 0)
            {
                GameManager.Instance.GameOver();
                yield break;
            }
        }
    }

    // ══════════════════════════════════════════════════════════════
    // 피해 처리
    // ══════════════════════════════════════════════════════════════

    public override int ApplyDamage(int damage)
    {
        int remaining = damage;

        // 냉각 상태: 피해 50% 감소 (내림)
        if (_coolingActive)
        {
            remaining = Mathf.FloorToInt(remaining * 0.5f);
            Debug.Log($"[BossUnrendered] 냉각 상태: 피해 {damage} → {remaining}");
        }

        // 방어도 우선 차감
        if (virusData.DefCnt > 0)
        {
            int defUsed = Mathf.Min(virusData.DefCnt, remaining);
            virusData.DefCnt -= defUsed;
            remaining -= defUsed;
        }

        // HP 감소
        if (remaining > 0)
            virusData.CurHpCnt = Mathf.Max(0, virusData.CurHpCnt - remaining);

        UpdateData();
        CheckPhaseTransition();

        // 3페이즈~: 30% 확률로 사망 페이크 (TODO: 쓰러지는 애니메이션)
        if (_currentPhase >= 3 && Random.value < 0.30f)
        {
            // TODO: [UI TODO] 그래픽-사망 페이크: 쓰러지는 애니메이션 재생
            Debug.Log("[BossUnrendered] 그래픽-사망 페이크 발동 (애니메이션 TODO)");
        }

        Debug.Log($"[BossUnrendered] 피해 {damage} → 체력: {virusData.CurHpCnt}");
        return remaining;
    }

    /// <summary>내부 자해용 피해 — HP 0 시 GameClear</summary>
    private void ApplyDamageToSelf(int damage)
    {
        int remaining = damage;

        // 방어도 우선 차감
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
            Debug.Log("[BossUnrendered] 체력 0 → 플레이어 승리!");
            CleanupEffects();
            GameManager.Instance.GameClear();
        }
    }

    // ══════════════════════════════════════════════════════════════
    // 클린업 / 사망
    // ══════════════════════════════════════════════════════════════

    private void CleanupEffects()
    {
        CardInfoHidingActive    = false;
        CardInfoDeceptionActive = false;
        CollapseDebuffActive    = false;
        GraphicChangeLevel      = 0;

        StopSkipTimer();
        CleanupAllDecoys();

        if (PlayerManager.instance != null)
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
