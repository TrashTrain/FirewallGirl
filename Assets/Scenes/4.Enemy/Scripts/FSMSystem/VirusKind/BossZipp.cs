using System.Collections;
using UnityEngine;

public class BossZipp : Virus
{
    [Header("보스 소환 설정")]
    [Tooltip("소환할 일반 바이러스 프리팹들을 넣어주세요.")]
    public GameObject[] randomVirusPrefabs;

    [Header("페이즈별 외형 이미지")]
    public Sprite phase1Sprite; // 평범한 zip 파일
    public Sprite phase2Sprite; // 사람과 비슷한 형태
    public Sprite phase3Sprite; // 지퍼가 열린 검은색 형체

    private int _currentPhase = 1;
    private int _turnInPhase = 0; // 현재 페이즈에서 진행된 턴 수
    private int _phase3DotDamage = 1; // 3페이즈 누적 지속 피해량

    private void Start()
    {
        if (VirusMgr.instance == null) return;
        InitData();

        // 💡 1페이즈 기본 이미지 적용
        ChangeSprite(phase1Sprite);

        // 보스 스폰 위치 강제 확인
        if (spawnNum != 3)
        {
            Debug.LogError("[BossZipp] 보스는 반드시 Spawn3 (3번 슬롯)에 스폰되어야 합니다! 현재 위치: " + transform.parent.name);
        }

        RollNextActionAndUpdateIcon();
    }

    protected override void RollNextAction()
    {
        CheckPhaseTransition(); // 페이즈가 넘어갔는지 확인
        _turnInPhase++;

        if (_currentPhase == 1)
        {
            // 3턴째, 6턴째, 9턴째... 에 소환 (첫 2턴 대기 조건 충족)
            if (_turnInPhase % 3 == 0)
                NextAction = State.Sup;
            else
                NextAction = State.Idle;
        }
        else if (_currentPhase == 2)
        {
            if (_turnInPhase <= 2)
            {
                NextAction = State.Idle; // 진입 후 2턴간 대기
            }
            else
            {
                // 3가지 행동 완전 랜덤
                int rand = Random.Range(0, 3);
                if (rand == 0) NextAction = State.Atk;
                else if (rand == 1) NextAction = State.Sup;
                else NextAction = State.Def;
            }
        }
        else if (_currentPhase == 3)
        {
            // 공격 60%, 용량 확보(버프) 40%
            int rand = Random.Range(0, 100);
            if (rand < 60) NextAction = State.Atk;
            else NextAction = State.Debuf;
        }
    }

    // 턴이 돌아올 때마다 체력 비율을 체크해 페이즈를 넘기고 외형을 변경합니다.
    private void CheckPhaseTransition()
    {
        float hpRatio = (float)virusData.CurHpCnt / virusData.HpCnt;

        if (_currentPhase == 1 && hpRatio <= 0.5f)
        {
            _currentPhase = 2;
            _turnInPhase = 0;
            ChangeSprite(phase2Sprite); // 💡 2페이즈 이미지로 변경
            Debug.Log("보스 2페이즈 진입! 외형이 사람과 비슷한 형태로 변합니다.");
        }
        else if (_currentPhase == 2 && hpRatio <= 0.25f)
        {
            _currentPhase = 3;
            _turnInPhase = 0;
            ChangeSprite(phase3Sprite); // 💡 3페이즈 이미지로 변경
            Debug.Log("보스 3페이즈 진입! 지퍼가 개방되며 검은색 형체로 변합니다.");
        }
    }

    // 이미지를 교체하는 공통 함수
    private void ChangeSprite(Sprite newSprite)
    {
        if (newSprite != null)
        {
            // 부모 클래스(Virus)에 있는 SpriteRenderer에 접근하여 이미지를 바꿉니다.
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = newSprite;
            }
        }
    }

    protected override IEnumerator CoRunStateAction(State s)
    {
        // 3페이즈 지속 피해 로직
        if (_currentPhase == 3)
        {
            Debug.Log($"[바이러스 침투] 플레이어에게 {_phase3DotDamage}의 지속 피해를 입힙니다.");
            PlayerManager.instance.TakeDamage(_phase3DotDamage);
            _phase3DotDamage++;

            yield return new WaitForSeconds(0.5f);
        }

        // 상태에 따른 실제 행동 실행
        switch (s)
        {
            case State.Atk:
                yield return CoAttack();
                break;
            case State.Sup:
                yield return CoUnzipSummon();
                break;
            case State.Def:
                yield return CoZipHeal();
                break;
            case State.Debuf:
                yield return CoCapacitySecure();
                break;
            default:
                yield return new WaitForSeconds(0.5f);
                break;
        }
    }

    // ==========================================
    // 보스 전용 특수 패턴 코루틴들 (이전과 동일)
    // ==========================================

    private IEnumerator CoUnzipSummon()
    {
        Debug.Log("[보스 패턴] 압축 풀기! 몬스터를 소환합니다.");

        int summonCount = (_currentPhase == 1) ? 2 : 1;
        int spawned = 0;

        Transform spawn1 = GameObject.Find("Spawn1")?.transform;
        Transform spawn2 = GameObject.Find("Spawn2")?.transform;

        if (spawn1 != null && spawn1.childCount == 0 && spawned < summonCount)
        {
            SpawnRandomVirus(spawn1);
            spawned++;
        }

        if (spawn2 != null && spawn2.childCount == 0 && spawned < summonCount)
        {
            SpawnRandomVirus(spawn2);
            spawned++;
        }

        if (spawned == 0)
        {
            Debug.Log("모든 스폰 공간이 차있어 소환을 건너뜁니다.");
        }

        yield return new WaitForSeconds(0.5f);
    }

    private void SpawnRandomVirus(Transform parentSlot)
    {
        if (randomVirusPrefabs == null || randomVirusPrefabs.Length == 0) return;

        int rand = Random.Range(0, randomVirusPrefabs.Length);
        Instantiate(randomVirusPrefabs[rand], parentSlot);
    }

    private IEnumerator CoZipHeal()
    {
        int healAmt = Random.Range(10, 21);

        virusData.CurHpCnt = Mathf.Min(virusData.HpCnt, virusData.CurHpCnt + healAmt);

        Debug.Log($"[보스 패턴] 압축 하기! 체력을 {healAmt} 회복했습니다. (현재 체력: {virusData.CurHpCnt})");
        UpdateData();

        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator CoCapacitySecure()
    {
        int consumeHp = virusData.CurHpCnt / 10;

        if (consumeHp < 1) consumeHp = 1;
        if (consumeHp >= virusData.CurHpCnt) consumeHp = virusData.CurHpCnt - 1;

        virusData.CurHpCnt -= consumeHp;

        ChangeAtkValue(5);

        int addDef = Mathf.Max(0, consumeHp - 1);
        if (addDef > 0)
        {
            ChangeDefenseValue(addDef);
        }

        Debug.Log($"[보스 패턴] 용량 확보! 체력을 {consumeHp} 소모하여 공격력 5, 방어력 {addDef} 증가!");
        UpdateData();

        yield return new WaitForSeconds(0.5f);
    }
}