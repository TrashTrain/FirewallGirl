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
    private Transform[] _decoySpawnPoints;

    // ─── Status Effect Icons ─────────────────────────────────────
    [Header("Status Effect Icons")]
    [SerializeField] private Sprite _iconGraphicChange;
    [SerializeField] private Sprite _iconCopy;
    [SerializeField] private Sprite _iconCooling;
    [SerializeField] private Sprite _iconPermissionRecovery;
    [SerializeField] private Sprite _iconCollapse;
    [SerializeField] private Sprite _iconImmunity;
    [SerializeField] private Sprite _iconBluescreen;
    [SerializeField] private Sprite _iconSkip;

    // ─── 공통 기믹 static 플래그 ─────────────────────────────────
    /// <summary>2페이즈~: 카드 정보 가리기 활성</summary>
    public static bool CardInfoHidingActive = false;
    /// <summary>3페이즈~: 카드 긍/부 수치 반전 활성</summary>
    public static bool CardInfoDeceptionActive = false;
    /// <summary>그래픽 변화 레벨: 1=초급/2=중급/3=상급</summary>
    public static int GraphicChangeLevel = 0;
    /// <summary>2페이즈~: 붕괴 디버프 활성 (카드 효과 절반)</summary>
    public static bool CollapseDebuffActive = false;
    /// <summary>4페이즈~: 권한복구-렌더 활성 (긍정효과 ×1.5, 부정수치 0)</summary>
    public static bool PermissionRecoveryActive = false;
    /// <summary>3페이즈~: 블루스크린 예고/활성 중 — 플레이어 공격 불가</summary>
    public static bool BluescreenAttackBlocked = false;
    /// <summary>2페이즈~: 플레이어 HP 수치를 ???로 가림</summary>
    public static bool PlayerHPHiddenActive = false;

    // ─── 행동 열거형 ─────────────────────────────────────────────
    private enum UnrenderedAction
    {
        // 1페이즈
        GraphicChaos,       // 그래픽-혼란: 디코이 5기 생성
        PixelCollapse,      // 픽셀-붕괴: 플레이어 카드 1장 픽셀화
        GraphicCopy,        // 그래픽-복사: 플레이어 공/방 긍정 변화 2턴 복사
        RenderPunch,        // 렌더 펀치!: ATK × 1 (1페이즈), × 1.5 (2페이즈), × 2 (3페이즈)
        // 2페이즈 추가
        GraphicOverheat,    // 그래픽-과열: 직전 턴 총 사용 카드 수 × 5 피해
        GraphicCooling,     // 그래픽-냉각: 2턴간 받는 피해 50% 감소
        GraphicLayerSep,    // 그래픽-레이어 분리: 방어도 +20
        // 3페이즈 추가
        GraphicBlueScreen,  // 그래픽-블루스크린: 다음 플레이어 턴에 블루스크린 활성 (5초 강제 턴 종료)
        GraphicRestore,     // 그래픽-복원: 잃은 HP의 5% 회복 (3턴 쿨타임)
        GraphicSystemFake,  // 그래픽-시스템 페이크: 플레이어 손패에 페이크 카드 추가
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
    private int _hitboxDecoyIndex = 0;
    private int _chaosCooldownTurns = 0;
    private bool _firstChaosFired = false;

    // ─── 디코이 패턴 진행 상태 ───────────────────────────────────
    private bool _decoyAnnouncedActive = false; // GraphicChaos 예고 턴 — 실행 전 무적
    private bool _decoyPatternActive = false;
    private bool _hasClickedDecoyThisTurn = false;
    private bool _lastClickedDecoyIsHitbox = false;
    private int _decoyRetryCount = 0;
    private bool _skipBossTurnAfterDecoy = false;
    private bool _decoyHitActive = false;


    // ─── TurnChanger 참조 ────────────────────────────────────────
    private TurnChanger _turnChanger;

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
    private int _restoreCooldown = 0;

    // ─── 4페이즈 행동불능 ─────────────────────────────────────────
    private int _stunTurns = 0;

    // ─── 2페이즈 패시브: 그래픽-붕괴 카운터 ─────────────────────
    private int _collapseTurnCounter = 0;

    // ─── 3페이즈 패시브: 그래픽-분리 카운터 ─────────────────────
    private int _phase3SepTurnCounter = 0;
    private bool _sepFlipState = false;

    // ─── 3페이즈: 그래픽-스킵 (플레이어 턴 5초 타이머) ──────────
    private Coroutine _skipCoroutine = null;

    // ─── 알림 UI ─────────────────────────────────────────────────
    private BattleNoticeUI _battleNotice;

    // ─── 붕괴 화면 모자이크 ──────────────────────────────────────
    [Header("붕괴 화면 효과 대상 캔버스")]
    [SerializeField] private Canvas _battleUICanvas;
    private PixelateEffect _collapseScreenEffect;

    // ─── 그래픽-블루스크린 ───────────────────────────────────────
    private BluescreenPanel _bluescreenUI;
    private bool _pendingBluescreen = false;
    private Coroutine _bluescreenCoroutine = null;

    // ─── 픽셀-붕괴 ───────────────────────────────────────────────
    private bool _pendingPixelCollapse = false;
    private List<PixelateEffect> _pixelatedCards = new List<PixelateEffect>();

    // ─── 효과 레지스트리 ─────────────────────────────────────────
    private List<ActiveEffect> _bossEffects;
    private ActiveEffect _effectGraphicChange;
    private ActiveEffect _effectCopy;
    private ActiveEffect _effectCooling;
    private ActiveEffect _effectPermissionRecovery;
    private ActiveEffect _effectCollapse;
    private ActiveEffect _effectImmunity;
    private ActiveEffect _effectBluescreenBlock;
    private ActiveEffect _effectSkip;

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

        if (VirusSpawn.instance != null)
        {
            Transform root = VirusSpawn.instance.transform;
            _decoySpawnPoints = new Transform[5];
            for (int i = 0; i < 5; i++)
                _decoySpawnPoints[i] = root.Find($"Spawn{i + 1}");
        }

        _turnChanger = FindObjectOfType<TurnChanger>();
        if (_turnChanger == null)
            Debug.LogWarning("[BossUnrendered] TurnChanger를 찾을 수 없습니다.");

        // true = 비활성 오브젝트 포함 탐색 (패널은 꺼진 상태로 씬에 배치됨)
        _bluescreenUI = FindObjectOfType<BluescreenPanel>(true);
        if (_bluescreenUI == null)
            Debug.LogWarning("[BossUnrendered] BluescreenPanel을 씬에서 찾을 수 없습니다.");

        _battleNotice = FindObjectOfType<BattleNoticeUI>(true);
        if (_battleNotice == null)
            Debug.LogWarning("[BossUnrendered] BattleNoticeUI를 씬에서 찾을 수 없습니다.");

        // 붕괴 상태 모자이크 효과 대상 캔버스 준비
        if (_battleUICanvas == null)
            _battleUICanvas = FindObjectOfType<Canvas>();
        if (_battleUICanvas != null)
        {
            _collapseScreenEffect = _battleUICanvas.GetComponent<PixelateEffect>();
            if (_collapseScreenEffect == null)
                _collapseScreenEffect = _battleUICanvas.gameObject.AddComponent<PixelateEffect>();
        }
        else
        {
            Debug.LogWarning("[BossUnrendered] _battleUICanvas를 찾을 수 없습니다. 붕괴 모자이크 비활성.");
        }

        if (spawnNum != 3)
            Debug.LogError("[BossUnrendered] 보스는 Spawn3 위치에 스폰되어야 합니다!");

        if (enemyUIController != null)
            enemyUIController.state.OverrideDescriptions(Phase1Descriptions());

        // 1페이즈 패시브: GraphicChangeLevel = 1
        GraphicChangeLevel = 1;
        PlayerManager.instance?.RefreshHandVisuals();
        PlayerManager.instance?.UpdateUI();

        RegisterStatusEffects();

        EnemyTurnManager.OnPlayerTurnEnded   += HandlePlayerTurnEnded;
        EnemyTurnManager.OnPlayerTurnStarted += HandlePlayerTurnStarted;
        PlayerManager.OnHandRefreshed        += HandleHandRefreshed;

        // 첫 번째 보스 턴 전에는 OnPlayerTurnStarted가 발행되지 않으므로 직접 초기화
        if (PlayerManager.instance != null)
        {
            _snapshotAtk = PlayerManager.instance.AttackPower;
            _snapshotDef = PlayerManager.instance.DefensePower;
        }

        RollNextActionAndUpdateIcon();
    }

    private void OnDestroy()
    {
        EnemyTurnManager.OnPlayerTurnEnded   -= HandlePlayerTurnEnded;
        EnemyTurnManager.OnPlayerTurnStarted -= HandlePlayerTurnStarted;
        PlayerManager.OnHandRefreshed        -= HandleHandRefreshed;
        StopSkipTimer();
        StopBluescreen();

        // 플레이어 버프/디버프 슬롯 효과 해제
        if (_effectPermissionRecovery != null) PlayerManager.instance?.UnregisterEffect(_effectPermissionRecovery);
        if (_effectCollapse != null) PlayerManager.instance?.UnregisterEffect(_effectCollapse);
        if (_effectBluescreenBlock != null) PlayerManager.instance?.UnregisterEffect(_effectBluescreenBlock);
        if (_effectSkip != null) PlayerManager.instance?.UnregisterEffect(_effectSkip);
        PlayerManager.instance?.UpdateUI();
        _collapseScreenEffect?.Remove();

        if (_decoyPatternActive || _decoyAnnouncedActive)
        {
            SetEndTurnInteractable(true);
            ShowBossUI();
        }
        CleanupAllDecoys();

        // static 플래그 초기화
        CardInfoHidingActive     = false;
        CardInfoDeceptionActive  = false;
        GraphicChangeLevel       = 0;
        CollapseDebuffActive     = false;
        PermissionRecoveryActive = false;
        BluescreenAttackBlocked  = false;
        PlayerHPHiddenActive     = false;
        ApplySepFlip(false);
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
            () => $"그래픽 변화 (레벨 {GraphicChangeLevel})\n카드/스탯 정보 왜곡 효과 적용 중"
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
            () => "권한 복구-렌더\n긍정효과 ×1.5, 부정수치 0, 매 턴 방어도 +10"
        );
        _effectCollapse = new ActiveEffect(
            _iconCollapse, false,
            () => CollapseDebuffActive,
            () => "붕괴 디버프\n카드 효과 절반"
        );
        _effectImmunity = new ActiveEffect(
            _iconImmunity, true,
            () => _decoyAnnouncedActive || _decoyPatternActive,
            () => "디코이 패턴 진행 중\n직접 공격 무효"
        );
        _effectBluescreenBlock = new ActiveEffect(
            _iconBluescreen, false,
            () => BluescreenAttackBlocked,
            () => "공격 불가\n블루스크린 패턴 진행 중"
        );
        _effectSkip = new ActiveEffect(
            _iconSkip, false,
            () => _skipCoroutine != null,
            () => "시간 압박\n5초마다 방어도 무시 5 피해"
        );

        enemyUIController.enemyStatusUI?.RegisterEffect(_effectGraphicChange);
        enemyUIController.enemyStatusUI?.RegisterEffect(_effectCopy);
        enemyUIController.enemyStatusUI?.RegisterEffect(_effectCooling);
        enemyUIController.enemyStatusUI?.RegisterEffect(_effectImmunity);
        // 플레이어 디버프/버프 슬롯 효과
        PlayerManager.instance?.RegisterEffect(_effectPermissionRecovery);
        PlayerManager.instance?.RegisterEffect(_effectCollapse);
        PlayerManager.instance?.RegisterEffect(_effectBluescreenBlock);
        PlayerManager.instance?.RegisterEffect(_effectSkip);
        enemyUIController.enemyStatusUI?.RefreshStatusUI();
    }

    // ══════════════════════════════════════════════════════════════
    // 플레이어 턴 시작 이벤트
    // ══════════════════════════════════════════════════════════════

    private void HandlePlayerTurnStarted()
    {
        if (PlayerManager.instance == null) return;

        // 디코이 패턴 재시도: 클릭 상태 초기화, 턴 종료 버튼 비활성화
        if (_decoyPatternActive)
        {
            _hasClickedDecoyThisTurn = false;
            _lastClickedDecoyIsHitbox = false;
            SetEndTurnInteractable(false);
            return;
        }

        // 그래픽-복사: 플레이어 ATK/DEF 스냅샷 저장
        _snapshotAtk = PlayerManager.instance.AttackPower;
        _snapshotDef = PlayerManager.instance.DefensePower;

        // 2페이즈 패시브: 그래픽-페이크 어택 (30% 확률)
        if (_currentPhase >= 2 && Random.value < 0.30f)
            StartCoroutine(CoFakeAttack());

        // 4페이즈 패시브: 권한 복구-렌더 — 매 턴 방어도 +10 (소멸형)
        if (_currentPhase >= 4)
        {
            PlayerManager.instance.AddTurnStatDelta(StatType.Defense, 10);
            PlayerManager.instance.UpdateUI();
            Debug.Log("[BossUnrendered] 권한 복구-렌더: 플레이어 방어도 +10");
        }

        // 3페이즈~: 블루스크린 예약이 있으면 블루스크린, 없으면 스킵 타이머
        if (_currentPhase >= 3)
        {
            if (_pendingBluescreen)
            {
                _pendingBluescreen = false;
                StartBluescreen();
            }
            else
            {
                StartSkipTimer();
            }
        }
    }

    // ══════════════════════════════════════════════════════════════
    // 플레이어 턴 종료 이벤트
    // ══════════════════════════════════════════════════════════════

    private void HandlePlayerTurnEnded()
    {
        if (PlayerManager.instance == null) return;

        StopSkipTimer();
        StopBluescreen();


        // ─── 디코이 패턴 처리 ─────────────────────────────────────
        if (_decoyPatternActive)
        {
            if (_hasClickedDecoyThisTurn && _lastClickedDecoyIsHitbox)
            {
                // 성공: 30 고정 피해 (면역 체크 우회)
                Debug.Log("[BossUnrendered] 디코이 패턴 성공 — 보스 피격!");
                int decoyDmg = 30;
                _chaosCooldownTurns = 2;
                FinishDecoyPattern();
                _decoyHitActive = true;
                ApplyDamage(decoyDmg);
                _decoyHitActive = false;
            }
            else
            {
                _decoyRetryCount++;
                Debug.Log($"[BossUnrendered] 디코이 패턴 실패 ({_decoyRetryCount}/2)");

                if (_decoyRetryCount >= 2)
                {
                    // 2회 실패: 플레이어에게 30 피해, 패턴 종료
                    Debug.Log("[BossUnrendered] 디코이 2회 실패 → 플레이어 30 피해");
                    PlayerManager.instance.TakeDamage(30);
                    _chaosCooldownTurns = 0;
                    FinishDecoyPattern();
                }
                // else: 디코이 유지, HandlePlayerTurnStarted에서 버튼 재비활성화
            }
        }

        int patches  = PlayerManager.instance.turnPatchCount;
        int roots    = PlayerManager.instance.turnRootCount;
        int vaccines = PlayerManager.instance.turnVaccineCount;

        // 그래픽-복사 만료 처리
        if (_copyTurnsLeft > 0)
        {
            _copyTurnsLeft--;
            if (_copyTurnsLeft == 0)
            {
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
            if (CollapseDebuffActive)
            {
                _collapseScreenEffect?.Remove(); // 붕괴 모자이크 해제
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
                ApplySepFlip(_sepFlipState);
                Debug.Log($"[BossUnrendered] 그래픽-분리: {(_sepFlipState ? "뒤집기" : "원복")}");
            }
        }

        CleanupPixelatedCards();

        UpdateData();
        PlayerManager.instance.UpdateUI();
    }

    // ══════════════════════════════════════════════════════════════
    // 행동 결정
    // ══════════════════════════════════════════════════════════════

    public override void RollNextActionAndUpdateIcon()
    {
        // 디코이 예고 또는 진행 중엔 행동 갱신 안 함 — _action을 GraphicChaos로 유지
        if (_decoyAnnouncedActive || _decoyPatternActive) return;

        RollNextAction();

        // GraphicChaos가 결정되면 즉시 예고 무적 활성화 (플레이어가 아이콘을 보는 턴부터 무효)
        if (_action == UnrenderedAction.GraphicChaos)
            _decoyAnnouncedActive = true;

        // GraphicBlueScreen이 결정되면 플레이어 공격 예고 차단 (아이콘을 보는 턴부터 공격 불가)
        if (_action == UnrenderedAction.GraphicBlueScreen)
            BluescreenAttackBlocked = true;

        if (enemyUIController != null)
            enemyUIController.state.UpdateStateImage(_action.ToString());
        UpdateData();
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
        float ratio = (float)virusData.CurHpCnt / virusData.HpCnt;
        int   curHP = virusData.CurHpCnt;

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
        List<UnrenderedAction> pool = new List<UnrenderedAction>
        {
            UnrenderedAction.PixelCollapse,
            UnrenderedAction.GraphicCopy,
            UnrenderedAction.RenderPunch,
        };
        if (_chaosCooldownTurns == 0 && !_decoyPatternActive)
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
        if (_chaosCooldownTurns == 0 && !_decoyPatternActive)
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

        GraphicChangeLevel       = 2;
        CardInfoHidingActive     = true;
        PlayerHPHiddenActive     = true;
        _collapseTurnCounter     = 0;
        CollapseDebuffActive     = false;
        PermissionRecoveryActive = false;

        UpdateData();
        if (PlayerManager.instance != null)
        {
            PlayerManager.instance.UpdateUI();
            PlayerManager.instance.RefreshHandVisuals();
        }
        enemyUIController?.state.OverrideDescriptions(Phase2Descriptions());

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
        if (PlayerManager.instance != null)
        {
            PlayerManager.instance.UpdateUI();
            PlayerManager.instance.RefreshHandVisuals(); // CardInfoDeceptionActive 적용
        }
        enemyUIController?.state.OverrideDescriptions(Phase3Descriptions());

        _battleNotice?.Show("그래픽 레이어가 분리되었습니다.\n카드 정보가 왜곡됩니다.", 3f);
        Debug.Log("[BossUnrendered] 3페이즈 진입 → 그래픽-분리 첫 발동");
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
        PlayerHPHiddenActive    = false;
        PermissionRecoveryActive = true; // 4페이즈 진입 시 활성화
        _coolingActive          = false;
        _coolingTurnsLeft       = 0;
        _copyTurnsLeft          = 0;
        _copyAppliedAtk         = 0;
        _copyAppliedDef         = 0;
        _sepFlipState           = false;
        ApplySepFlip(false);
        StopSkipTimer();
        StopBluescreen();
        _pendingBluescreen = false;

        _decoyAnnouncedActive = false;
        if (_decoyPatternActive)
        {
            _decoyPatternActive = false;
            SetEndTurnInteractable(true);
            ShowBossUI();
        }
        CleanupAllDecoys();

        // 그래픽-몰입: 공격력 +20
        virusData.AtkDmg += 20;

        UpdateData();
        if (PlayerManager.instance != null)
        {
            PlayerManager.instance.UpdateUI();
            PlayerManager.instance.RefreshHandVisuals(); // CardInfoHidingActive 해제 → 카드 정보 복원
        }
        enemyUIController?.state.OverrideDescriptions(Phase4Descriptions());

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

        // 디코이 패턴 재시도 중: 히트박스 이동만 하고 행동 없음
        if (_decoyPatternActive)
        {
            yield return new WaitForSeconds(0.5f);
            yield break;
        }

        // 디코이 패턴 종료 직후: 플레이어 턴 1회 보장 후 행동
        if (_skipBossTurnAfterDecoy)
        {
            _skipBossTurnAfterDecoy = false;
            yield return new WaitForSeconds(0.5f);
            yield break;
        }

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

    /// <summary>그래픽-혼란: 디코이 5기 생성, End Turn 비활성화, 보스 UI 숨김</summary>
    private IEnumerator CoGraphicChaos()
    {
        _decoyAnnouncedActive = false; // 예고 단계 종료, 실제 실행 단계로 전환

        if (!_firstChaosFired)
        {
            _firstChaosFired = true;
            _battleNotice?.Show("한 명만 가면을 안쓰고 있는 것 같아요!\n히트박스도 바뀐 것 같아요...", 10f);
        }

        CleanupAllDecoys();

        if (_decoyPrefab == null || _decoySpawnPoints == null || _decoySpawnPoints.Length < 5)
        {
            Debug.LogWarning("[BossUnrendered] 디코이 프리팹 또는 스폰 포인트(5개) 미설정");
            yield return new WaitForSeconds(0.5f);
            yield break;
        }

        _hitboxDecoyIndex = Random.Range(0, 5);

        // 스폰 루프 전에 패턴 활성화 — 루프 중 예외 발생 시에도 무적이 유지되어야 함
        _decoyPatternActive       = true;
        _decoyRetryCount          = 0;
        _hasClickedDecoyThisTurn  = false;
        _lastClickedDecoyIsHitbox = false;

        SetEndTurnInteractable(false);
        HideBossUI();

        for (int i = 0; i < 5; i++)
        {
            if (_decoySpawnPoints[i] == null)
            {
                Debug.LogWarning($"[BossUnrendered] 디코이 스폰 포인트[{i}] null — 건너뜀");
                continue;
            }
            GameObject obj = Instantiate(_decoyPrefab, _decoySpawnPoints[i].position, Quaternion.identity);
            UnrenderedDecoy decoy = obj.GetComponent<UnrenderedDecoy>();
            if (decoy == null) decoy = obj.AddComponent<UnrenderedDecoy>();

            bool isHitbox = (i == _hitboxDecoyIndex);
            decoy.Setup(this, isHitbox, GetCurrentPhaseSprite());
            _activeDecoys.Add(decoy);
        }

        Debug.Log($"[BossUnrendered] 그래픽-혼란: 디코이 5기 생성 (히트박스 인덱스 {_hitboxDecoyIndex})");
        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>픽셀-붕괴: 다음 플레이어 턴 드로우 후 카드 1장 모자이크 예약</summary>
    private IEnumerator CoPixelCollapse()
    {
        _pendingPixelCollapse = true;
        Debug.Log("[BossUnrendered] 픽셀-붕괴: 다음 플레이어 턴 카드 픽셀화 예정");
        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>PreparePlayerTurn 드로우 완료 후 호출 — 픽셀화 예약 및 붕괴 모자이크 적용</summary>
    private void HandleHandRefreshed()
    {
        if (_pendingPixelCollapse)
        {
            _pendingPixelCollapse = false;

            if (PlayerManager.instance != null)
            {
                PlayerCard target = PlayerManager.instance.GetRandomHandCard();
                if (target != null)
                {
                    PixelateEffect effect = target.gameObject.AddComponent<PixelateEffect>();
                    effect.Apply(8f);
                    _pixelatedCards.Add(effect);
                    Debug.Log($"[BossUnrendered] 픽셀-붕괴 적용: '{target.cardData.cardName}' 픽셀화");
                }
            }
        }

        // 붕괴 상태: 카드 드로우 완료 후 화면 전체 모자이크 (새로 드로우된 카드 포함)
        if (CollapseDebuffActive && _collapseScreenEffect != null)
        {
            if (_collapseScreenEffect.IsActive) _collapseScreenEffect.Remove();
            _collapseScreenEffect.Apply(12f);
        }
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

        int gainedAtk = Mathf.Max(0, currentAtk - _snapshotAtk);
        int gainedDef = Mathf.Max(0, currentDef - _snapshotDef);

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
        int baseDmg     = virusData.AtkDmg;
        int dmg         = Mathf.FloorToInt(baseDmg * multiplier);
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
            Debug.Log($"[BossUnrendered] 그래픽-과열: 카드 {cardCount}장 × 5 = {dmg} 피해");
        else
            Debug.Log("[BossUnrendered] 그래픽-과열: 사용 카드 없음, 피해 없음");

        if (dmg > 0)
            PlayerManager.instance.TakeDamage(dmg);

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

    /// <summary>그래픽-블루스크린: 다음 플레이어 턴에 블루스크린 패널 표시 후 5초 강제 턴 종료</summary>
    private IEnumerator CoGraphicBlueScreen()
    {
        _pendingBluescreen = true;
        Debug.Log("[BossUnrendered] 그래픽-블루스크린 예약 (다음 플레이어 턴 5초 강제 종료)");
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

    /// <summary>그래픽-시스템 페이크: 다음 플레이어 턴 손패에 페이크 카드 1장 추가.</summary>
    /// <remarks>페이크 카드를 조합에 포함하면 이번 턴 모든 카드 효과·공격력·방어도가 0이 됨.</remarks>
    private IEnumerator CoGraphicSystemFake()
    {
        if (PlayerManager.instance != null)
            PlayerManager.instance.pendingFakeCardCount += 1;

        Debug.Log("[BossUnrendered] 그래픽-시스템 페이크: 다음 플레이어 턴에 페이크 카드 1장 예약");
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

        int prevHP = PlayerManager.instance.currentHP;

        yield return CoAttack();

        int hpDamage = Mathf.Max(0, prevHP - PlayerManager.instance.currentHP);
        int selfDmg  = Mathf.FloorToInt(hpDamage * 0.5f);

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

        int hpDamage = Mathf.Max(0, prevHP - PlayerManager.instance.currentHP);

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
    /// 플레이어가 디코이 클릭 시 호출 — 마지막 클릭을 기록하고 End Turn 버튼 활성화
    /// </summary>
    public void OnDecoyClicked(UnrenderedDecoy decoy, bool isHitbox)
    {
        if (!_decoyPatternActive) return;

        _hasClickedDecoyThisTurn  = true;
        _lastClickedDecoyIsHitbox = isHitbox;

        SetEndTurnInteractable(true);

        Debug.Log($"[BossUnrendered] 디코이 클릭 — {(isHitbox ? "히트박스!" : "페이크")} (턴 종료 가능)");
    }

    /// <summary>매 보스 턴 히트박스 이동</summary>
    private void MoveDecoyHitbox()
    {
        if (_activeDecoys.Count == 0) return;

        foreach (var d in _activeDecoys)
            if (d != null && d.IsHitbox) d.SetAsNonHitbox();

        List<UnrenderedDecoy> alive = new List<UnrenderedDecoy>();
        foreach (var d in _activeDecoys)
            if (d != null) alive.Add(d);

        if (alive.Count > 0)
        {
            alive[Random.Range(0, alive.Count)].SetAsHitbox();
            Debug.Log("[BossUnrendered] 히트박스 이동 완료");
        }
    }

    /// <summary>디코이 패턴 종료 (성공 또는 최종 실패) — 디코이 제거, 버튼 복원, 보스 UI 복원</summary>
    private void FinishDecoyPattern()
    {
        _decoyAnnouncedActive     = false;
        _decoyPatternActive       = false;
        _hasClickedDecoyThisTurn  = false;
        _lastClickedDecoyIsHitbox = false;
        _decoyRetryCount          = 0;
        _skipBossTurnAfterDecoy   = true;
        CleanupAllDecoys();
        SetEndTurnInteractable(true);
        ShowBossUI();
    }

    /// <summary>모든 디코이 파괴</summary>
    private void CleanupAllDecoys()
    {
        foreach (var decoy in _activeDecoys)
        {
            if (decoy != null)
                Destroy(decoy.gameObject);
        }
        _activeDecoys.Clear();
        Debug.Log("[BossUnrendered] 모든 디코이 제거");
    }

    // ══════════════════════════════════════════════════════════════
    // UI 헬퍼
    // ══════════════════════════════════════════════════════════════

    private void SetEndTurnInteractable(bool interactable)
    {
        if (_turnChanger != null)
            _turnChanger.turnChangeBT.interactable = interactable;
    }

    private void HideBossUI()
    {
        if (enemyUIController != null)
            enemyUIController.panel.SetActive(false);
    }

    private void ShowBossUI()
    {
        if (enemyUIController != null)
            enemyUIController.panel.SetActive(true);
    }

    // ══════════════════════════════════════════════════════════════
    // 그래픽-스킵 (플레이어 턴 5초 타이머, 3페이즈~)
    // ══════════════════════════════════════════════════════════════

    private void StartSkipTimer()
    {
        StopSkipTimer();
        _skipCoroutine = StartCoroutine(CoSkipTimer());
        PlayerManager.instance?.UpdateUI(); // 디버프 아이콘 즉시 표시
    }

    private void StopSkipTimer()
    {
        if (_skipCoroutine != null)
        {
            StopCoroutine(_skipCoroutine);
            _skipCoroutine = null;
            PlayerManager.instance?.UpdateUI(); // 디버프 아이콘 즉시 제거
        }
    }

    /// <summary>
    /// 그래픽-스킵: 플레이어 턴 5초 이상 지속 시 고정 5 피해 (방어도 무시, currentHP 직접 감소)
    /// 5초마다 반복.
    /// </summary>
    private IEnumerator CoSkipTimer()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);

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
    // 그래픽-블루스크린
    // ══════════════════════════════════════════════════════════════

    private void StartBluescreen()
    {
        _bluescreenUI?.Show();
        BluescreenAttackBlocked = true; // 실제 블루스크린 턴에서도 반드시 재설정
        SetEndTurnInteractable(false);
        PlayerManager.instance?.SetHandCardsInteractable(false);

        if (_bluescreenCoroutine != null)
            StopCoroutine(_bluescreenCoroutine);
        _bluescreenCoroutine = StartCoroutine(CoBluescreenTimer());

        Debug.Log("[BossUnrendered] 블루스크린 활성 (5초 후 강제 턴 종료)");
    }

    private void StopBluescreen()
    {
        if (_bluescreenCoroutine != null)
        {
            StopCoroutine(_bluescreenCoroutine);
            _bluescreenCoroutine = null;
        }

        _bluescreenUI?.Hide();
        BluescreenAttackBlocked = false;
        SetEndTurnInteractable(true);
        PlayerManager.instance?.SetHandCardsInteractable(true);
        PlayerManager.instance?.UpdateUI(); // 조합 실행 버튼 상태 복원
    }

    /// <summary>블루스크린 5초 카운트다운 — 만료 시 강제 턴 종료</summary>
    private IEnumerator CoBluescreenTimer()
    {
        yield return new WaitForSeconds(5f);

        _bluescreenUI?.Hide();
        _bluescreenCoroutine = null;

        if (!GameManager.PlayerTurn) yield break;

        Debug.Log("[BossUnrendered] 블루스크린: 5초 경과 → 강제 턴 종료");
        GameManager.Instance.OnTrunButtonClick();
    }

    // ══════════════════════════════════════════════════════════════
    // 피해 처리
    // ══════════════════════════════════════════════════════════════

    public override int ApplyDamage(int damage)
    {
        // 디코이 예고/진행 중: 직접 공격 무효 (디코이 성공 피해는 예외)
        if (!_decoyHitActive && (_decoyAnnouncedActive || _decoyPatternActive))
        {
            Debug.Log("[BossUnrendered] 디코이 패턴 진행 중 — 피해 무효");
            return 0;
        }

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

        // 3페이즈~: 30% 확률로 사망 페이크
        if (_currentPhase >= 3 && remaining > 0 && Random.value < 0.30f)
            StartCoroutine(CoDeathFake());

        Debug.Log($"[BossUnrendered] 피해 {damage} → 체력: {virusData.CurHpCnt}");
        return remaining;
    }

    /// <summary>내부 자해용 피해 — HP 0 시 GameClear</summary>
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
            Debug.Log("[BossUnrendered] 체력 0 → 플레이어 승리!");
            CleanupEffects();
            GameManager.Instance.GameClear();
        }
    }

    // ══════════════════════════════════════════════════════════════
    // 클린업 / 사망
    // ══════════════════════════════════════════════════════════════

    private void CleanupPixelatedCards()
    {
        foreach (var eff in _pixelatedCards)
            if (eff != null) eff.Remove();
        _pixelatedCards.Clear();
        _pendingPixelCollapse = false;
    }

    private void CleanupEffects()
    {
        CardInfoHidingActive     = false;
        CardInfoDeceptionActive  = false;
        CollapseDebuffActive     = false;
        PermissionRecoveryActive = false;
        GraphicChangeLevel       = 0;
        PlayerHPHiddenActive     = false;
        _pendingBluescreen      = false;
        _sepFlipState           = false;
        ApplySepFlip(false);
        StopBluescreen(); // BluescreenAttackBlocked도 내부에서 false로 초기화
        _collapseScreenEffect?.Remove();

        _decoyAnnouncedActive = false;
        if (_decoyPatternActive)
        {
            _decoyPatternActive = false;
            SetEndTurnInteractable(true);
            ShowBossUI();
        }

        CleanupPixelatedCards();
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

    /// <summary>보스가 빨갛게 물들며 플레이어 위치까지 돌진했다 돌아오는 가짜 공격 모션.</summary>
    private IEnumerator CoFakeAttack()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Vector3 origin = transform.position;

        // 플레이어 실제 위치를 목표로 (없으면 왼쪽 3칸 fallback)
        Vector3 target = PlayerManager.instance != null
            ? PlayerManager.instance.transform.position
            : origin + new Vector3(-3f, 0f, 0f);

        // 이동 시작과 동시에 빨간색으로 변경
        if (sr != null) sr.color = new Color(1f, 0.3f, 0.3f, 1f);

        // 플레이어 위치까지 빠르게 돌진
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.15f;
            transform.position = Vector3.Lerp(origin, target, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        yield return new WaitForSeconds(0.05f);

        // 원위치로 복귀
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.22f;
            transform.position = Vector3.Lerp(target, origin, t);
            yield return null;
        }
        transform.position = origin;

        // 색상 복원
        if (sr != null) sr.color = Color.white;

        Debug.Log("[BossUnrendered] 그래픽-페이크 어택 발동");
    }

    /// <summary>보스가 쓰러지는 척하다 복귀하는 사망 페이크 모션.</summary>
    private IEnumerator CoDeathFake()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Vector3 originScale = transform.localScale;
        Vector3 originPos   = transform.position;

        // 페이드 + 기울기 (쓰러짐)
        float elapsed = 0f;
        float fallDur = 0.6f;
        while (elapsed < fallDur)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fallDur;
            if (sr != null) sr.color = Color.Lerp(Color.white, new Color(0.3f, 0.3f, 0.3f, 0.4f), t);
            transform.localScale = Vector3.Lerp(originScale, new Vector3(originScale.x * 0.6f, originScale.y * 0.2f, 1f), t);
            transform.position   = Vector3.Lerp(originPos, originPos + new Vector3(0f, -0.8f, 0f), t);
            yield return null;
        }

        yield return new WaitForSeconds(0.8f);

        // 복귀
        elapsed = 0f;
        float riseDur = 0.4f;
        while (elapsed < riseDur)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / riseDur;
            if (sr != null) sr.color = Color.Lerp(new Color(0.3f, 0.3f, 0.3f, 0.4f), Color.white, t);
            transform.localScale = Vector3.Lerp(new Vector3(originScale.x * 0.6f, originScale.y * 0.2f, 1f), originScale, t);
            transform.position   = Vector3.Lerp(originPos + new Vector3(0f, -0.8f, 0f), originPos, t);
            yield return null;
        }

        if (sr != null) sr.color = Color.white;
        transform.localScale = originScale;
        transform.position   = originPos;

        Debug.Log("[BossUnrendered] 그래픽-사망 페이크 발동");
    }

    /// <summary>3페이즈~: ATK·DEF·HP 수치 텍스트를 ???로 가림. HP 블록 바는 시각 유지.</summary>
    public override void UpdateData()
    {
        if (enemyUIController == null) { base.UpdateData(); return; }

        if (_currentPhase >= 3)
        {
            // HP 블록 바는 시각적으로 업데이트 (대략적 체력 변화는 보이게)
            enemyUIController.healthBar?.UpdateHPBar(virusData.CurHpCnt, virusData.HpCnt);
            // 수치 텍스트는 모두 ???로 덮어씀
            if (enemyUIController.atk  != null) enemyUIController.atk.text  = "???";
            if (enemyUIController.def  != null) enemyUIController.def.text  = "???";
            if (enemyUIController.hp   != null) enemyUIController.hp.text   = "???";
            if (enemyUIController.healthBar?.hpText != null)
                enemyUIController.healthBar.hpText.text = "???";
            enemyUIController.enemyStatusUI?.RefreshStatusUI();
        }
        else
        {
            base.UpdateData();
        }
    }

    /// <summary>3페이즈 패시브: Canvas Y 스케일을 반전/복원해 화면 위아래를 뒤집는다.</summary>
    private void ApplySepFlip(bool flip)
    {
        if (_battleUICanvas == null) return;
        _battleUICanvas.transform.localScale = flip
            ? new Vector3(1f, -1f, 1f)
            : Vector3.one;
    }

    private void ChangeSprite(Sprite sp)
    {
        if (sp == null) return;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.sprite = sp;
    }

    private Sprite GetCurrentPhaseSprite()
    {
        return _currentPhase switch
        {
            1 => phase1Sprite,
            2 => phase2Sprite,
            3 => phase3Sprite,
            _ => phase4Sprite,
        };
    }
}
