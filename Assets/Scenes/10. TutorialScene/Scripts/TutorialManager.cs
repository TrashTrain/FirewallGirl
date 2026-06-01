using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 튜토리얼 전체 흐름을 상태 머신으로 관리.
/// DeckBuildingUI → BattleUI 두 단계로 구성되며, 단일 씬 안에서 패널을 전환한다.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance;

    // ── UI 패널 참조 ──────────────────────────────────────────────
    [Header("Tutorial UI")]
    [SerializeField] private TutorialDialogPanel dialogPanel;
    [SerializeField] private TutorialInputBlocker inputBlocker;

    [Header("Scene Panels")]
    [SerializeField] private GameObject deckBuildingUI;   // DeckBuildingScene 오브젝트들
    [SerializeField] private GameObject battleUI;         // IntegratedScene 오브젝트들

    // ── 튜토리얼 전용 적 프리팹 ───────────────────────────────────
    [Header("Tutorial Enemy Prefabs")]
    [SerializeField] private GameObject tutorialVirusPrefab;   // 4~6단계 기본 적
    [SerializeField] private GameObject errorVirusPrefab;      // 7단계 상태이상 적

    // ── 튜토리얼 전용 스탯 (SO 값을 덮어씀) ──────────────────────
    [Header("Tutorial Virus Stats")]
    [SerializeField] private int tutorialVirusHp = 2;
    [SerializeField] private int tutorialVirusAtk = 1;
    [SerializeField] private int tutorialVirusDef = 0;

    [Header("Error Virus Stats")]
    [SerializeField] private int errorVirusHp = 10;
    [SerializeField] private int errorVirusAtk = 0;
    [SerializeField] private int errorVirusDef = 5;

    // ── 7단계 디버프 설정 ─────────────────────────────────────────
    [Header("Debuff Learn Settings")]
    [SerializeField] private TutorialDebuffType forcedDebuffType = TutorialDebuffType.Backdoor;

    // ── UI 활성화 제어 (CanvasGroup) ──────────────────────────────
    [Header("UI Interactability Control")]
    [SerializeField] private CanvasGroup battleCardHandGroup;  // 전투 손패 영역
    [SerializeField] private CanvasGroup battleCombineGroup;   // 전투 조합 영역 (Use + 효과 적용)
    [SerializeField] private CanvasGroup battleTurnEndGroup;   // 전투 턴 종료 버튼
    [SerializeField] private CanvasGroup deckCollectionGroup;  // 덱빌딩 컬렉션 영역
    [SerializeField] private CanvasGroup deckConfirmGroup;     // 덱빌딩 확인 버튼

    [Header("Tutorial Highlight")]
    [SerializeField] private TutorialHighlightController _highlight;
    [SerializeField] private RectTransform augmentOptionsPanel; // 증강체 선택 옵션 패널 루트

    // ── 튜토리얼 전용 고정 핸드 ──────────────────────────────────
    [Header("Tutorial Hand")]
    [SerializeField] private CardObject[] tutorialHandCards;   // Inspector에서 3장 지정

    // ── BattleUI 위치 참조 ────────────────────────────────────────
    [Header("BattleUI Target References")]
    [SerializeField] private RectTransform hpBarTarget;        // 체력바
    [SerializeField] private RectTransform attackPowerUITarget;      // 공격력
    [SerializeField] private RectTransform defensePowerUITarget;      // 방어력
    [SerializeField] private RectTransform costUITarget;       // 코스트
    [SerializeField] private RectTransform cardPanelTarget;    // 손패 카드 패널
    [SerializeField] private RectTransform cardDetailsTarget;    // 카드 디테일뷰 패널
    [SerializeField] private RectTransform combineInfoTarget;  // 조합 정보 패널
    [SerializeField] private RectTransform enemyAreaTarget;    // 적 UI 영역
    [SerializeField] private RectTransform debuffAreaTarget;   // 버프/디버프 아이콘 영역

    // ── 캐릭터 스프라이트 ─────────────────────────────────────────
    [Header("Character Sprites")]
    [SerializeField] private Sprite aiDefaultSprite;
    [SerializeField] private Sprite aiSmileSprite;
    [SerializeField] private Sprite aiThinkingSprite;

    // ── 이벤트 수신 플래그 ─────────────────────────────────────────
    private bool _deckConfirmed;
    private bool _anyCardClicked;
    private bool _deckFull;
    private bool _cardClicked;
    private bool _useClicked;
    private bool _combinationExecuted;
    private bool _playerTurnEnded;
    private bool _allEnemiesDefeated;
    private bool _augmentSelected;
    private bool _dialogDone;

    // ── 현재 단계 ─────────────────────────────────────────────────
    private TutorialPhase _currentPhase = TutorialPhase.DeckBuilding;

    private readonly WaitForSeconds _waitRespawn = new WaitForSeconds(1.5f);
    private readonly WaitForSeconds _waitFrame = new WaitForSeconds(0.1f);

    // ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // 스프라이트를 Inspector에 할당하지 않은 경우 Resources에서 로드
        if (aiDefaultSprite == null)
            aiDefaultSprite = Resources.Load<Sprite>("Characters/AI_Default");
        if (aiSmileSprite == null)
            aiSmileSprite = Resources.Load<Sprite>("Characters/AI_Smile");
        if (aiThinkingSprite == null)
            aiThinkingSprite = Resources.Load<Sprite>("Characters/AI_Thinking");
    }

    private void OnEnable()
    {
        DeckManager.OnDeckConfirmed += HandleDeckConfirmed;
        DeckManager.OnAnyCardClicked += HandleAnyCardClicked;
        DeckManager.OnDeckFull += HandleDeckFull;
        CardController.OnCardClicked += HandleCardClicked;
        PlayerCard.OnUseClicked += HandleUseClicked;
        PlayerManager.OnCombinationExecuted += HandleCombinationExecuted;
        PlayerManager.OnPlayerTurnEnded += HandlePlayerTurnEnded;
        VirusSpawn.OnAllEnemiesDefeated += HandleAllEnemiesDefeated;
        UpDownMgr.OnAugmentSelected += HandleAugmentSelected;
    }

    private void OnDisable()
    {
        DeckManager.OnDeckConfirmed -= HandleDeckConfirmed;
        DeckManager.OnAnyCardClicked -= HandleAnyCardClicked;
        DeckManager.OnDeckFull -= HandleDeckFull;
        CardController.OnCardClicked -= HandleCardClicked;
        PlayerCard.OnUseClicked -= HandleUseClicked;
        PlayerManager.OnCombinationExecuted -= HandleCombinationExecuted;
        PlayerManager.OnPlayerTurnEnded -= HandlePlayerTurnEnded;
        VirusSpawn.OnAllEnemiesDefeated -= HandleAllEnemiesDefeated;
        UpDownMgr.OnAugmentSelected -= HandleAugmentSelected;
    }

    private void Start()
    {
        // BattleUI는 처음에 비활성. DeckBuildingUI만 표시.
        if (battleUI != null) battleUI.SetActive(false);
        if (deckBuildingUI != null) deckBuildingUI.SetActive(true);

        inputBlocker.Unlock();
        StartCoroutine(RunTutorial());
    }

    // ── 이벤트 핸들러 ─────────────────────────────────────────────

    private void HandleDeckConfirmed() => _deckConfirmed = true;
    private void HandleAnyCardClicked() => _anyCardClicked = true;
    private void HandleDeckFull() => _deckFull = true;
    private void HandleCardClicked() => _cardClicked = true;
    private void HandleUseClicked() => _useClicked = true;
    

    private void HandleCombinationExecuted()
    {
        if (_currentPhase == TutorialPhase.FirstBattle ||
            _currentPhase == TutorialPhase.CombineLearn)
            _combinationExecuted = true;
    }

    private void HandlePlayerTurnEnded()
    {
        if (_currentPhase >= TutorialPhase.FirstBattle &&
            _currentPhase <= TutorialPhase.DebuffLearn)
            _playerTurnEnded = true;
    }

    private void HandleAllEnemiesDefeated()
    {
        if (_currentPhase == TutorialPhase.FinalBattle)
        {
            _allEnemiesDefeated = true;
        }
        else
        {
            // 4~7단계에서 적을 조기 처치했을 경우 → 보상 억제 후 재소환
            if (VirusSpawn.instance != null)
                VirusSpawn.instance.suppressReward = true;
            StartCoroutine(RespawnCurrentEnemy());
        }
    }

    private void HandleAugmentSelected() => _augmentSelected = true;

    // ── 헬퍼 메서드 ──────────────────────────────────────────────

    private void LockInput() => inputBlocker.Lock();
    private void UnlockInput() => inputBlocker.Unlock();

    private void DrawTutorialHand()
    {
        if (PlayerManager.instance == null || tutorialHandCards == null || tutorialHandCards.Length == 0) return;
        PlayerManager.instance.DrawTutorialHand(new System.Collections.Generic.List<CardObject>(tutorialHandCards));
    }

    private IEnumerator ShowDialogsAndWait(DialogLine[] lines)
    {
        _dialogDone = false;
        LockInput();
        dialogPanel.ShowDialogs(lines, () => _dialogDone = true);
        yield return new WaitUntil(() => _dialogDone);
        UnlockInput();
    }

    private IEnumerator WaitForEnemyTurnComplete()
    {
        // PlayerTurn이 false가 될 때까지(적 턴 시작), 그 후 true가 될 때까지(적 턴 종료) 대기
        yield return new WaitUntil(() => !GameManager.PlayerTurn);
        yield return new WaitUntil(() => GameManager.PlayerTurn);
    }

    private void SetGroupInteractable(CanvasGroup cg, bool interactable)
    {
        if (cg == null) return;
        cg.interactable = interactable;
        cg.blocksRaycasts = interactable;
    }

    /// <summary>전투 UI 영역별 상호작용 허용 여부를 설정.</summary>
    private void SetBattleUI(bool hand, bool combine, bool turnEnd, bool cardUse = true)
    {
        SetGroupInteractable(battleCardHandGroup, hand);
        SetGroupInteractable(battleCombineGroup, combine);
        SetGroupInteractable(battleTurnEndGroup, turnEnd);

        // CanvasGroup.interactable은 IPointerXxxHandler를 막지 못하므로 직접 잠금
        if (battleCardHandGroup != null)
        {
            foreach (CardController cc in battleCardHandGroup.GetComponentsInChildren<CardController>(true))
                cc.isLockedByTutorial = !hand;

            foreach (PlayerCard pc in battleCardHandGroup.GetComponentsInChildren<PlayerCard>(true))
                pc.SetHoverUseBtnInteractable(cardUse);
        }
    }

    /// <summary>덱빌딩 UI 영역별 상호작용 허용 여부를 설정.</summary>
    private void SetDeckBuildingUI(bool collection, bool confirm)
    {
        SetGroupInteractable(deckCollectionGroup, collection);
        SetGroupInteractable(deckConfirmGroup, confirm);
    }

    private RectTransform GetRect(CanvasGroup group) =>
        group != null ? group.GetComponent<RectTransform>() : null;

    private RectTransform[] GetChildRects(Transform parent)
    {
        if (parent == null) return System.Array.Empty<RectTransform>();
        var list = new System.Collections.Generic.List<RectTransform>();
        foreach (Transform child in parent)
        {
            if (child.gameObject.activeSelf && child.GetComponent<TMPro.TMP_Text>() == null)
                list.Add(child.GetComponent<RectTransform>());
        }
        return list.ToArray();
    }

    /// <summary>forcedDebuffType에 지정된 디버프를 플레이어에게 즉시 부여. 7단계 전용.</summary>
    private void ApplyForcedDebuff()
    {
        if (PlayerManager.instance == null || forcedDebuffType == TutorialDebuffType.None) return;
        switch (forcedDebuffType)
        {
            case TutorialDebuffType.Backdoor:
                PlayerManager.instance.AddMultiTurnStat(StatType.Attack, -1, 3, "공격력 감소");
                break;
            case TutorialDebuffType.PacketLoss:
                PlayerManager.instance.cannotGainDefenseTurns = 2;
                PlayerManager.instance.UpdateUI();
                break;
            case TutorialDebuffType.Lag:
                PlayerManager.instance.lagDebuffTurns += PlayerManager.instance.lagDebuffValue + 1;
                PlayerManager.instance.UpdateUI();
                break;
        }
    }

    /// <summary>지정 슬롯의 바이러스 스탯을 SO 기본값에서 tutorialVirus/errorVirus 값으로 덮어씀.</summary>
    private void ApplyVirusStats(int spawnIdx, int hp, int atk, int def)
    {
        if (VirusSpawn.instance == null || spawnIdx >= VirusSpawn.instance.spawns.Count) return;
        Virus virus = VirusSpawn.instance.spawns[spawnIdx].GetComponentInChildren<Virus>();
        if (virus == null || virus.virusData == null) return;
        virus.virusData.HpCnt = hp;
        virus.virusData.CurHpCnt = hp;
        virus.virusData.AtkDmg = atk;
        virus.virusData.DefCnt = def;
        virus.UpdateData();
    }

    private void SpawnPhase46Enemy()
    {
        if (VirusSpawn.instance == null) return;
        VirusSpawn.instance.ResetVirusCount();
        VirusSpawn.instance.SpawnVirus(0, tutorialVirusPrefab);
        ApplyVirusStats(0, tutorialVirusHp, tutorialVirusAtk, tutorialVirusDef);
        VirusSpawn.instance.SpawnVirus(1, tutorialVirusPrefab);
        ApplyVirusStats(1, tutorialVirusHp, tutorialVirusAtk, tutorialVirusDef);
        VirusSpawn.instance.SpawnVirus(2, tutorialVirusPrefab);
        ApplyVirusStats(2, tutorialVirusHp, tutorialVirusAtk, tutorialVirusDef);
        VirusSpawn.instance.virusCnt = 3;
    }

    private IEnumerator RespawnCurrentEnemy()
    {
        yield return _waitRespawn;
        if (VirusSpawn.instance == null) yield break;
        VirusSpawn.instance.CleanVirus();
        yield return null;
        SpawnPhase46Enemy();
        if (PlayerManager.instance != null) PlayerManager.instance.PreparePlayerTurn();
        DrawTutorialHand();
    }

    // ── 메인 튜토리얼 코루틴 ─────────────────────────────────────

    private IEnumerator RunTutorial()
    {
        // ═══════════════════════════════════════
        // 1단계: 덱 빌딩
        // ═══════════════════════════════════════
        _currentPhase = TutorialPhase.DeckBuilding;

        yield return ShowDialogsAndWait(new[]
        {
            MakeLine("자, 본격적으로 시작하기 전에 우리가 싸울 때 쓸 카드들을 먼저 골라야 해요.", aiSmileSprite),
            MakeLine("화면에 보이는 카드들 중에서 마음에 드는 걸 눌러보세요!", aiSmileSprite),
        });
        // 카드 클릭만 허용, 확인 버튼 차단
        SetDeckBuildingUI(collection: true, confirm: false);
        _anyCardClicked = false;
        // _highlight?.ShowGlowOnly(GetChildRects(deckCollectionGroup.transform));
        _highlight?.ShowSpotlightWithGlow(GetRect(deckCollectionGroup));
        yield return new WaitUntil(() => _anyCardClicked);
        _highlight?.HideAll();
        SetDeckBuildingUI(collection: true, confirm: true);

        yield return ShowDialogsAndWait(new[]
        {
            MakeLine("선택한 카드는 아래쪽에 추가돼요. 다시 누르면 제거할 수 있죠!", aiSmileSprite),
            MakeLine("딱 10장을 골라야 해요. 모자라도 안 되고, 넘쳐도 안 돼요!", aiSmileSprite),
            MakeLine("각 카드에는 백신, 패치, 루트 속성이 있어요. 나중에 전투에서 꽤 중요하니까 확인해두세요!", aiDefaultSprite),
            MakeLine("10장의 카드를 고른 뒤 '덱 확정' 버튼을 눌러주세요!", aiSmileSprite),
        });

        // 10장 모두 선택할 때까지 컬렉션 카드 글로우
        _deckFull = false;
        // _highlight?.ShowGlowOnly(GetChildRects(deckCollectionGroup.transform));
        _highlight?.ShowSpotlightWithGlow(GetRect(deckCollectionGroup));
        yield return new WaitUntil(() => _deckFull);
        _highlight?.HideAll();

        _deckConfirmed = false;
        _highlight?.ShowSpotlightWithGlow(GetRect(deckConfirmGroup));
        yield return new WaitUntil(() => _deckConfirmed);
        _highlight?.HideAll();

        // ═══════════════════════════════════════
        // DeckBuildingUI → BattleUI 전환
        // ═══════════════════════════════════════
        deckBuildingUI.SetActive(false);
        battleUI.SetActive(true);

        // BattleUI의 Start() 메서드들이 실행될 때까지 대기
        yield return null;
        yield return null;

        // DeckBuilding에서 SetCurrentDeck()으로 DB에만 저장된 덱을 drawPile에 로드
        // PlayerManager는 DontDestroyOnLoad이므로 Start()가 재실행되지 않아 수동 호출 필요
        if (PlayerManager.instance != null)
        {
            PlayerManager.instance.isTutorialMode = true; // 자동 재드로우 차단
            PlayerManager.instance.InitializeBattleDeck();
            DrawTutorialHand(); // BattleUI 첫 진입 시 지정 카드 지급
        }

        // 자동 스폰된 적 정리 후 튜토리얼용 적 1마리 소환
        if (VirusSpawn.instance != null)
        {
            VirusSpawn.instance.suppressReward = true;
            VirusSpawn.instance.CleanVirus();
            yield return null;
            SpawnPhase46Enemy();
        }

        // ═══════════════════════════════════════
        // 2단계: 전투 UI 소개
        // ═══════════════════════════════════════
        _currentPhase = TutorialPhase.BattleUIIntro;

        yield return ShowDialogsAndWait(new[]
        {
            MakeLine("자, 드디어 전투 시작이에요! 일단 화면을 쭉 살펴봐주세요.", aiSmileSprite),
        });
        
        dialogPanel.MoveToTarget(hpBarTarget);
        _highlight?.ShowSpotlightWithGlow(hpBarTarget);
        yield return ShowDialogsAndWait(new[]
        {
            MakeLine("이건 제 체력이에요. 체력이 전부 사라지면... 우리의 여정은 그걸로 끝나는 거죠.", aiDefaultSprite),
        });

        dialogPanel.MoveToTarget(attackPowerUITarget);
        _highlight?.ShowSpotlightWithGlow(attackPowerUITarget);
        yield return ShowDialogsAndWait(new[]
        {
            MakeLine("이 숫자들은 공격력과 방어력이에요.", aiDefaultSprite),
            MakeLine("턴이 끝나면 공격력만큼 제가 적을 공격할 거에요!", aiSmileSprite),
        });
        
        dialogPanel.MoveToTarget(defensePowerUITarget);
        _highlight?.ShowSpotlightWithGlow(defensePowerUITarget);
        yield return ShowDialogsAndWait(new[]
        {
            MakeLine("방어력은 적의 공격을 먼저 막아줘요. 일종의 배리어 역할을 한다고 볼 수 있죠.", aiDefaultSprite),
        });

        dialogPanel.MoveToTarget(costUITarget, PanelCorner.BottomRight, PanelCorner.TopLeft);
        _highlight?.ShowSpotlightWithGlow(costUITarget);
        yield return ShowDialogsAndWait(new[]
        {
            MakeLine("이 숫자는 코스트예요. 카드를 쓸 때 소비하고, 매 턴 시작마다 다시 채워져요!", aiSmileSprite),
        });

        // ═══════════════════════════════════════
        // 3단계: 카드 이해
        // ═══════════════════════════════════════
        _currentPhase = TutorialPhase.CardLearn;

        dialogPanel.MoveToTarget(cardPanelTarget, PanelCorner.BottomLeft, PanelCorner.TopRight);
        _highlight?.ShowSpotlightWithGlow(cardPanelTarget);
        yield return ShowDialogsAndWait(new[]
        {
            MakeLine("아래쪽에 있는 카드를 한 번 눌러보세요!", aiSmileSprite),
        });
        
        // 손패 카드 클릭만 허용
        SetBattleUI(hand: true, combine: false, turnEnd: false, cardUse: false);
        _cardClicked = false;
        _highlight?.ShowGlowOnly(GetChildRects(battleCardHandGroup.transform));
        yield return new WaitUntil(() => _cardClicked);
        _highlight?.HideAll();
        SetBattleUI(hand: true, combine: true, turnEnd: true);

        yield return ShowDialogsAndWait(new[]
        {
            MakeLine("카드에는 두 가지 효과가 동시에 존재해요. 올라가는 수치와 내려가는 수치요.", aiDefaultSprite),
            MakeLine("공격력 +3, 방어력 -2 이런 식이죠. 등가교환! 세상의 이치랄까요?", aiThinkingSprite),
            MakeLine("스탯 종류는 공격력, 방어력, 코스트, 체력 이렇게 네 가지예요.", aiDefaultSprite),
            MakeLine("카드 위에 마우스를 올리면 간단 설명이 나오고, 클릭하면 상세 정보를 볼 수 있어요!", aiSmileSprite),
        });

        // ═══════════════════════════════════════
        // 4단계: 첫 번째 전투 — 카드 1장 사용
        // ═══════════════════════════════════════
        _currentPhase = TutorialPhase.FirstBattle;

        dialogPanel.MoveToTarget(combineInfoTarget);
        yield return ShowDialogsAndWait(new[]
        {
            MakeLine("그럼 이제 직접 카드를 써볼까요? '적용' 버튼을 눌러보세요!", aiSmileSprite),
        });
        
        // 카드 + Use 버튼 허용, 턴 종료 차단
        SetBattleUI(hand: true, combine: true, turnEnd: false);
        _useClicked = false;
        _highlight?.ShowGlowOnly(GetChildRects(battleCardHandGroup.transform));
        yield return new WaitUntil(() => _useClicked);
        _highlight?.HideAll();

        yield return ShowDialogsAndWait(new[]
        {
            MakeLine("좋아요! 이제 왼쪽에 있는 '효과 적용' 버튼을 눌러보세요!", aiSmileSprite),
        });
        // 효과 적용 버튼만 허용
        SetBattleUI(hand: false, combine: true, turnEnd: false);
        _combinationExecuted = false;
        _highlight?.ShowSpotlightWithGlow(GetRect(battleCombineGroup));
        yield return new WaitUntil(() => _combinationExecuted);
        _highlight?.HideAll();

        dialogPanel.MoveToTarget(hpBarTarget);
        yield return ShowDialogsAndWait(new[]
        {
            MakeLine("카드 효과가 적용됐어요! 공격력이나 방어력이 변한 거 보이시죠?", aiSmileSprite),
            MakeLine("이제 공격할 준비가 되었어요! '턴 종료' 버튼을 눌러볼까요?", aiDefaultSprite),
        });
        // 턴 종료 버튼만 허용
        SetBattleUI(hand: false, combine: false, turnEnd: true);
        _playerTurnEnded = false;
        _highlight?.ShowSpotlightWithGlow(GetRect(battleTurnEndGroup));
        yield return new WaitUntil(() => _playerTurnEnded);
        _highlight?.HideAll();
        SetBattleUI(hand: false, combine: false, turnEnd: false); // 적 턴 중 전체 차단
        yield return WaitForEnemyTurnComplete();

        yield return ShowDialogsAndWait(new[]
        {
            MakeLine("공격력만큼 적에게 데미지가 들어갔어요!", aiSmileSprite),
            MakeLine("저기 적 위에 있는 아이콘 보이시죠? 다음 턴에 적이 뭘 할지 미리 알려주는 거예요. 잘 활용하세요!", aiDefaultSprite),
        });

        // ═══════════════════════════════════════
        // 5단계: 조합 시스템
        // ═══════════════════════════════════════
        _currentPhase = TutorialPhase.CombineLearn;
        
        if (PlayerManager.instance != null)
        {
            PlayerManager.instance.isTutorialMode = true; // 자동 재드로우 차단
            PlayerManager.instance.InitializeBattleDeck();
            DrawTutorialHand(); // BattleUI 첫 진입 시 지정 카드 지급
        }

        dialogPanel.MoveToTarget(combineInfoTarget);
        yield return ShowDialogsAndWait(new[]
        {
            MakeLine("이번엔 카드를 여러 장 써볼게요! 최대 3장까지 동시에 조합할 수 있어요.", aiSmileSprite),
            MakeLine("백신 카드와 패치 카드를 각각 하나씩 골라서 조합해보세요!", aiSmileSprite),
        });

        // 카드 선택 + 조합 허용, 턴 종료 차단
        SetBattleUI(hand: true, combine: true, turnEnd: false);
        _combinationExecuted = false;
        var combineLearnTargets = new System.Collections.Generic.List<RectTransform>(GetChildRects(battleCardHandGroup.transform));
        if (battleCombineGroup != null) combineLearnTargets.Add(battleCombineGroup.GetComponent<RectTransform>());
        _highlight?.ShowGlowOnly(combineLearnTargets.ToArray());
        yield return new WaitUntil(() => _combinationExecuted);
        _highlight?.HideAll();

        dialogPanel.MoveToTarget(hpBarTarget);
        yield return ShowDialogsAndWait(new[]
        {
            MakeLine("보셨어요?! 백신이랑 패치 카드를 같이 쓰면 효과가 1.5배가 돼요! 훨씬 강하죠?", aiSmileSprite),
            MakeLine("루트 카드는 조금 특별해요. 어떻게 조합하느냐에 따라 배율이 달라지니까 나중에 직접 실험해봐요!", aiThinkingSprite),
            MakeLine("코스트는 선택한 카드들의 합산이에요. 코스트가 부족하면 효과 적용 버튼이 빨간색으로 바뀌니까 잘 보세요!", aiDefaultSprite),
            MakeLine("그리고 효과 적용은 한 턴에 딱 한 번만 쓸 수 있어요. 신중하게 조합을 맞춰서 사용해야 해요!", aiDefaultSprite),
        });

        // ═══════════════════════════════════════
        // 6단계: 방어 전략
        // ═══════════════════════════════════════
        _currentPhase = TutorialPhase.DefenseLearn;

        dialogPanel.MoveToTarget(enemyAreaTarget);
        yield return ShowDialogsAndWait(new[]
        {
            MakeLine("저기 적 위의 아이콘을 자세히 보세요. 어떤 행동을 할 것 같나요?", aiDefaultSprite),
            MakeLine("검 모양은 공격, 방패 모양은 방어, 위쪽 화살표는 자기 강화예요!", aiDefaultSprite),
            MakeLine("적이 공격 예고를 하고 있으면 방어력을 올리는 카드를 쓰는 게 유리해요. 방어력이 먼저 깎이고 그 뒤에 체력이 깎이거든요!", aiSmileSprite),
            MakeLine("방어력이 먼저 깎이고 그 뒤에 체력이 깎이거든요.", aiDefaultSprite),
            MakeLine("방어력이 있으면 그만큼 잘 버틸 수 있으니까요!", aiSmileSprite),
            MakeLine("다시 '턴 종료' 버튼을 눌러 턴을 진행해보세요.", aiSmileSprite),
        });

        // 턴 종료 버튼만 허용
        SetBattleUI(hand: false, combine: false, turnEnd: true);
        _playerTurnEnded = false;
        _highlight?.ShowSpotlightWithGlow(GetRect(battleTurnEndGroup));
        yield return new WaitUntil(() => _playerTurnEnded);
        _highlight?.HideAll();
        SetBattleUI(hand: false, combine: false, turnEnd: false); // 적 턴 중 전체 차단
        yield return WaitForEnemyTurnComplete();

        // ═══════════════════════════════════════
        // 7단계: 상태이상 체험
        // ═══════════════════════════════════════
        _currentPhase = TutorialPhase.DebuffLearn;

        if (VirusSpawn.instance != null)
        {
            VirusSpawn.instance.CleanVirus();
            yield return null;
            VirusSpawn.instance.ResetVirusCount();
            VirusSpawn.instance.SpawnVirus(0, errorVirusPrefab);
            ApplyVirusStats(0, errorVirusHp, errorVirusAtk, errorVirusDef);
            VirusSpawn.instance.virusCnt = 1;
        }
        if (PlayerManager.instance != null) PlayerManager.instance.PreparePlayerTurn();
        DrawTutorialHand();

        dialogPanel.MoveToTarget(debuffAreaTarget);
        yield return ShowDialogsAndWait(new[]
        {
            MakeLine("이제 조금 특수한 적을 만나볼게요. 적한테 맞은 뒤 어떤 변화가 생기는지 한번 보세요!", aiDefaultSprite),
        });

        // 자유롭게 플레이 후 턴 종료
        SetBattleUI(hand: false, combine: false, turnEnd: true);
        _playerTurnEnded = false;
        _highlight?.ShowSpotlightWithGlow(GetRect(battleTurnEndGroup));
        yield return new WaitUntil(() => _playerTurnEnded);
        _highlight?.HideAll();
        SetBattleUI(hand: false, combine: false, turnEnd: false); // 적 턴 중 전체 차단
        yield return WaitForEnemyTurnComplete();
        ApplyForcedDebuff();

        yield return ShowDialogsAndWait(new[]
        {
            MakeLine("상태이상 아이콘이 생겼죠? 저 아이콘에 마우스를 올리면 설명이 나와요!", aiDefaultSprite),
            MakeLine("백도어는 공격력이 2턴 동안 줄어드는 효과예요.", aiDefaultSprite),
            MakeLine("패킷 로스는 그 턴 동안 방어력 획득이 완전히 막혀버려요! 공격 예고가 있을 때 걸리면 꽤 위험하다구요!", aiDefaultSprite),
        });

        // ═══════════════════════════════════════
        // 8단계: 최종 전투 → 증강체 선택
        // ═══════════════════════════════════════
        _currentPhase = TutorialPhase.FinalBattle;
        dialogPanel.SetPosition(Vector2.zero);

        if (VirusSpawn.instance != null)
        {
            VirusSpawn.instance.suppressReward = false;
            VirusSpawn.instance.CleanVirus();
            yield return null;
            VirusSpawn.instance.ResetVirusCount();
            VirusSpawn.instance.SpawnVirus(0, tutorialVirusPrefab);
            ApplyVirusStats(0, tutorialVirusHp, tutorialVirusAtk, tutorialVirusDef);
            VirusSpawn.instance.SpawnVirus(1, tutorialVirusPrefab);
            ApplyVirusStats(1, tutorialVirusHp, tutorialVirusAtk, tutorialVirusDef);
            VirusSpawn.instance.virusCnt = 2;
        }
        if (PlayerManager.instance != null) PlayerManager.instance.PreparePlayerTurn();
        DrawTutorialHand();

        yield return ShowDialogsAndWait(new[]
        {
            MakeLine("이제 배운 걸 총동원해서 싸워볼까요? 할 수 있겠죠?", aiSmileSprite),
        });

        // 이 시점부터 매 턴 일반 드로우 활성화
        if (PlayerManager.instance != null) PlayerManager.instance.isTutorialMode = false;
        SetBattleUI(hand: true, combine: true, turnEnd: true);
        _allEnemiesDefeated = false;
        yield return new WaitUntil(() => _allEnemiesDefeated);

        yield return ShowDialogsAndWait(new[]
        {
            MakeLine("이겼어요! 수고했어요~", aiSmileSprite),
            MakeLine("전투에서 이기면 세 가지 증강체 중 하나를 고를 수 있어요. 한번 골라보세요!", aiSmileSprite),
        });

        _augmentSelected = false;
        if (augmentOptionsPanel != null) _highlight?.ShowGlowOnly(GetChildRects(augmentOptionsPanel));
        yield return new WaitUntil(() => _augmentSelected);
        _highlight?.HideAll();

        yield return ShowDialogsAndWait(new[]
        {
            MakeLine("증강체는 덱의 카드 스탯이나 저의 영구 스탯을 바꿔줘요.", aiDefaultSprite),
            MakeLine("효과가 즉시 적용되니까 덱 전략에 잘 맞는 걸 골라야 해요!", aiSmileSprite),
        });

        // ═══════════════════════════════════════
        // 9단계: 스테이지 맵 안내
        // ═══════════════════════════════════════
        _currentPhase = TutorialPhase.StageIntro;

        yield return ShowDialogsAndWait(new[]
        {
            MakeLine("자, 이제 기본은 다 배웠어요!", aiSmileSprite),
            MakeLine("그럼 가볼까요? 이 컴퓨터를 지키러 출발하죠!", aiSmileSprite),
        });

        // ═══════════════════════════════════════
        // 완료 → 덱 빌딩 씬으로 이동
        // ═══════════════════════════════════════
        _currentPhase = TutorialPhase.Complete;
        if (PlayerManager.instance != null)
        {
            PlayerManager.instance.isTutorialMode = false;
            PlayerManager.instance.ResetForNewRun();
        }
        instance = null;
        SceneManager.LoadScene("DeckBuildingScene");
    }

    // ── 유틸리티 ──────────────────────────────────────────────────

    private DialogLine MakeLine(string text, Sprite sprite, string speaker = "아이 (A.I.)")
    {
        return new DialogLine
        {
            speakerName = speaker,
            text = text,
            characterSprite = sprite
        };
    }
}

public enum TutorialDebuffType
{
    None,
    Backdoor,    // 공격력 -1, 2턴
    PacketLoss,  // 방어 불가, 1턴
    Lag          // 쿨타임 증가, 1턴
}

public enum TutorialPhase
{
    DeckBuilding,   // 1단계: 덱 빌딩
    BattleUIIntro,  // 2단계: 전투 UI 소개
    CardLearn,      // 3단계: 카드 이해
    FirstBattle,    // 4단계: 첫 번째 전투 (카드 1장)
    CombineLearn,   // 5단계: 조합 시스템
    DefenseLearn,   // 6단계: 방어 전략
    DebuffLearn,    // 7단계: 상태이상 체험
    FinalBattle,    // 8단계: 최종 전투 → 증강체 선택
    StageIntro,     // 9단계: 스테이지 안내
    Complete        // 완료
}
