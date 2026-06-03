using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // 씬 전환 감지를 위해 추가

[CreateAssetMenu(menuName = "Augments/CostBalance_Independent")]
public class CostBalanceAugment : AugmentBase
{
    [Header("남은 코스트가 있을 때 (버프)")]
    public int defensePerRemainingCost = 2;

    [Header("남은 코스트가 없을 때 (디버프)")]
    public int attackDecreaseAmount = 2;

    public override void OnEquip(BattleContext context)
    {
        CreateMonitor(context.player);
    }

    public override void OnBattleStart(BattleContext context)
    {
        CreateMonitor(context.player);
    }

    private void CreateMonitor(PlayerManager player)
    {
        if (player == null) return;

        // 중복 생성 방지
        GameObject oldMonitor = GameObject.Find("CostBalanceMonitor");
        if (oldMonitor != null) return;

        GameObject monitorObj = new GameObject("CostBalanceMonitor");

        // ?? [핵심 변경] 보상 씬에서 만들어진 감시자가 전투 씬까지 삭제되지 않고 따라가도록 설정합니다.
        DontDestroyOnLoad(monitorObj);

        var monitor = monitorObj.AddComponent<TurnEndMonitor>();
        monitor.Initialize(player, defensePerRemainingCost, attackDecreaseAmount, augmentName);

        Debug.Log($"<color=cyan>[증강체 생성 완료]</color> {augmentName} 감시자가 DontDestroyOnLoad 상태로 배치되었습니다.");
    }
}

public class TurnEndMonitor : MonoBehaviour
{
    private PlayerManager _player;
    private int _defCheck;
    private int _atkCheck;
    private string _augName;

    private int _appliedDefenseThisRound = 0;

    public void Initialize(PlayerManager player, int def, int atk, string name)
    {
        _player = player;
        _defCheck = def;
        _atkCheck = atk;
        _augName = name;
        _appliedDefenseThisRound = 0;

        StartCoroutine(CoTurnTrackingLoop());

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private IEnumerator CoTurnTrackingLoop()
    {
        
        Debug.Log($"<color=yellow>[감시 룹 시작]</color> {_augName} 추적 시작. 현재 PlayerTurn = {GameManager.PlayerTurn}");

        while (_player != null)
        {
            // 1. 플레이어 턴이 될 때까지 대기
            while (!GameManager.PlayerTurn)
            {
                if (_player == null) yield break;
                yield return null;
            }

            // 플레이어 턴 시작 시 지난 턴에 줬던 임시 방어력 회수
            if (_appliedDefenseThisRound > 0)
            {
                int currentBaseDef = _player.GetBaseStat(StatType.Defense);
                _player.SetBaseStat(StatType.Defense, Mathf.Max(0, currentBaseDef - _appliedDefenseThisRound));
                Debug.Log($"<color=green>[턴 시작 청소]</color> 이전 임시 방어력 {_appliedDefenseThisRound} 반환.");
                _appliedDefenseThisRound = 0;
                ForceRefreshInGameUI();
            }

            // 2. 플레이어 턴이 끝날 때까지 대기
            while (GameManager.PlayerTurn)
            {
                if (_player == null) yield break;
                yield return null;
            }

            // 턴 종료 시점 포착 로직 실행
            ExecuteTurnEndLogic();
        }
    }

    private void ExecuteTurnEndLogic()
    {
        if (_player == null) return;

        // ?? 현재 전투 씬에 실재하는 PlayerManager.instance의 실시간 코스트를 가져옵니다.
        int remainingCost = _player.currentCost;
        Debug.Log($"<color=orange>[턴 종료 포착]</color> 남은 코스트: {remainingCost}");

        if (remainingCost > 0)
        {
            _appliedDefenseThisRound = remainingCost * _defCheck;
            int newDef = _player.GetBaseStat(StatType.Defense) + _appliedDefenseThisRound;
            _player.SetBaseStat(StatType.Defense, newDef);

            Debug.Log($"<color=emerald>[방어력 부여]</color> 방어력 +{_appliedDefenseThisRound} 적용 (최종: {_player.DefensePower})");
        }
        else
        {
            _player.AddPermanentStat(StatType.Attack, -_atkCheck);
            Debug.Log($"<color=red>[공격력 페널티]</color> 공격력 {_atkCheck} 영구 감소.");
        }

        ForceRefreshInGameUI();
    }

    private void ForceRefreshInGameUI()
    {
        if (_player == null) return;

        _player.UpdateUI();

        if (_player.powerUI != null)
        {
            _player.powerUI.UpdateAttackPowerUI(_player.AttackPower);
            _player.powerUI.UpdateDefensePowerUI(_player.DefensePower);
        }

        if (_player.costUI != null)
        {
            _player.costUI.UpdateCostUI(_player.currentCost, _player.TotalCost);
        }

        if (PlayerStatusUI.instance != null)
        {
            PlayerStatusUI.instance.RefreshStatusUI();
        }
    }

    // 전투가 끝나고 다시 보상 씬("UpDownSysScene")으로 돌아오면 이 감시자는 임무를 다한 것이므로 파괴
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "UpDownSysScene")
        {
            Debug.Log($"<color=white>[감시자 퇴근]</color> 보상 씬으로 돌아왔으므로 감시 오브젝트를 제거합니다.");
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제 (메모리 누수 방지)
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (_player != null && _appliedDefenseThisRound > 0)
        {
            int currentBaseDef = _player.GetBaseStat(StatType.Defense);
            _player.SetBaseStat(StatType.Defense, Mathf.Max(0, currentBaseDef - _appliedDefenseThisRound));
        }
    }
}