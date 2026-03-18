using System.Collections;
using TMPro;
using UnityEngine;

public class Virus : MonoBehaviour
{
    [Header("SO데이터")]
    public VirusObjectSO virusObjectSO;

    [Header("내부 데이터")]
    public SpriteRenderer image;

    //public TextMeshProUGUI atkDmgText;
    //public TextMeshProUGUI hpCntText;

    [HideInInspector]
    public VirusData virusData;
    public int RandState;

    public int spawnNum;

    public bool virusState = false;

    public EnemyUIController enemyUIController;

    public State NextAction { get; private set; }

    private Coroutine _actionCo;
    private Vector3 _originPos;
    private Vector3 _originScale;

    [Header("Action Params")]
    private float atkMoveDuration = 0.3f;
    private float atkPause = 0.1f;
    private float supScaleMul = 1.15f;
    private float supDuration = 0.5f;
    private float defYOffset = 0.20f;
    private float defDuration = 0.3f;

    public void InitData()
    {
        virusData = new VirusData(virusObjectSO.virusIndex, virusObjectSO.virusImage, virusObjectSO.virusName, virusObjectSO.virusAtk, virusObjectSO.virusDef, virusObjectSO.virusHp, virusObjectSO.virusHp);
        gameObject.GetComponent<SpriteRenderer>().sprite = virusData.VirusImage;
        //atkDmgText.text = virusData.AtkDmg.ToString();
        //hpCntText.text = virusData.HpCnt.ToString();

        if (transform.parent.name == "Spawn1")
            spawnNum = 1;
        else if (transform.parent.name == "Spawn2")
            spawnNum = 2;
        else if (transform.parent.name == "Spawn3")
            spawnNum = 3;
        else
            spawnNum = 0;
    }

    
    public void UpdateData()
    {
        enemyUIController.atk.text = virusData.AtkDmg.ToString();
        enemyUIController.def.text = virusData.DefCnt.ToString();
        enemyUIController.healthBar.UpdateHPBar(virusData.CurHpCnt, virusData.HpCnt);
    }
    public enum State
    {
        Idle,
        Atk,
        Def,
        Sup,
        Death
    }


    private void Awake()
    {
        _originPos = transform.position;
        _originScale = transform.localScale;
    }

    private void Start()
    {
        if (VirusMgr.instance == null)
        {
            Debug.LogError("CardMgr.instance가 초기화되지 않았습니다.");
            return;
        }

        InitData();
        Debug.Log("asdfasdf");
        // 시작할 때 아이콘 나오도록
        RollNextActionAndUpdateIcon();
    }

    //private void Update()
    //{
    //    if (virusData.CurHpCnt <= 0)
    //    {
    //        Debug.Log("바이러스 퇴치");
    //        Destroy(enemyUIController.gameObject);
    //        Destroy(gameObject);
    //    }
    //}

    public void RollNextActionAndUpdateIcon()
    {
        RollNextAction();
        enemyUIController.state.UpdateStateImage(NextAction);
    }

    private void RollNextAction()
    {
        GetRandState();
        NextAction = (State)RandState;
    }

    public void GetRandState()
    {
        RandState = ChangeStateRand((int)State.Death);
    }

    public int ChangeStateRand(int endNum)
    {
        int check = Random.Range(1, endNum);
        return check;
    }

    public void ChangeAtkValue(int atk)
    {
        virusData.AtkDmg += atk;
    }
    
    public void ChangeDefenseValue(int def)
    {
        virusData.DefCnt += def;
    }

    protected IEnumerator CoAttack()
    {
        // 목표: 플레이어 위치까지 갔다가 복귀
        Transform playerTr = PlayerManager.instance.transform;
        if (playerTr == null) yield break;

        Vector3 start = _originPos;
        Vector3 target = playerTr.position;

        // 2D 횡이동만 원하면 y 고정(필요하면 주석 해제)
         target.y = start.y;

        yield return LerpPos(start, target, atkMoveDuration);

        // 도착 시점에 피해
        PlayerManager.instance.TakeDamage(virusData.AtkDmg);

        if (atkPause > 0f) yield return new WaitForSeconds(atkPause);

        yield return LerpPos(target, start, atkMoveDuration);

        // 끝나면 항상 원복 보정
        transform.position = start;
    }

    protected IEnumerator CoSupport()
    {
        // 목표: 커졌다가 작아짐 + 공격력 증가
        ChangeAtkValue(3);
        UpdateData();

        Vector3 start = _originScale;
        Vector3 big = start * supScaleMul;

        yield return LerpScale(start, big, supDuration);
        yield return LerpScale(big, start, supDuration);

        transform.localScale = start;
    }

    protected IEnumerator CoDefend()
    {
        // 목표: y축으로 올라갔다 내려옴 + 방어력 증가
        ChangeDefenseValue(3);
        UpdateData();
        
        Vector3 start = _originPos;
        Vector3 up = start + new Vector3(0f, defYOffset, 0f);

        yield return LerpPos(start, up, defDuration);
        yield return LerpPos(up, start, defDuration);

        transform.position = start;
    }

    private IEnumerator LerpPos(Vector3 start, Vector3 target, float dur)
    {
        float t = 0f;
        dur = Mathf.Max(0.0001f, dur);

        while (t < 1f)
        {
            Debug.Log($"pos now: {transform.position}");
            t += Time.deltaTime / dur;
            float easedT = Mathf.SmoothStep(0f, 1f, t);
            transform.position = Vector3.Lerp(start, target, easedT);
            yield return null;
        }
        transform.position = target;
    }

    private IEnumerator LerpScale(Vector3 a, Vector3 b, float dur)
    {
        float t = 0f;
        dur = Mathf.Max(0.0001f, dur);
        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            transform.localScale = Vector3.Lerp(a, b, t);
            yield return null;
        }
        transform.localScale = b;
    }

    public IEnumerator CoDoOneAction()
    {

        // 행동 시작
        yield return StartCoroutine(CoRunStateAction(NextAction));
    }

    private IEnumerator CoRunStateAction(State s)
    {
        switch (s)
        {
            case State.Atk:
                yield return StartCoroutine(CoAttack());
                break;
            case State.Sup:
                yield return StartCoroutine(CoSupport());
                break;
            case State.Def:
                yield return StartCoroutine(CoDefend());
                break;
            case State.Death:
                Destroy(gameObject);
                yield break;
            default:
                // Idle/Death면 그냥 넘어감

                yield break;
        }
        //RollNextAction();
    }

    public int ApplyDamage(int damage)
    {
        int remaining = damage;
        
        // 방어력 감소
        if (virusData.DefCnt > 0)
        {
            int defUsed = Mathf.Min(virusData.DefCnt, remaining);
            virusData.DefCnt = Mathf.Max(0, virusData.DefCnt - defUsed);
            remaining -= defUsed;
        }
        
        // 남은 데미지만큼 체력 감소
        if (remaining > 0 && virusData.CurHpCnt > 0)
        {
            int hpUsed = Mathf.Min(virusData.CurHpCnt, remaining);
            virusData.CurHpCnt = Mathf.Max(0, virusData.CurHpCnt - hpUsed);
            remaining -= hpUsed;
        }
        
        UpdateData();
        
        Debug.Log($"[Enemy] 받은 데미지: {damage}, 남은 체력: {virusData.CurHpCnt}");

        if (virusData.CurHpCnt <= 0)
        {
            // Death state로 변경 로직
            OnDeath();
        }

        return remaining;
    }
    // 죽음 처리를 담당하는 함수 추가
    private void OnDeath()
    {
        VirusSpawn.instance.SetDiscountVirusCount();
        
        if (VirusSpawn.instance.virusCnt <= 0)
        {
            VirusSpawn.instance.StartCoroutine(VirusSpawn.instance.GetReward());
        }
        gameObject.SetActive(false);

        // 만약 UI도 같이 꺼야 한다면:
        if (enemyUIController != null) enemyUIController.gameObject.SetActive(false);
        
    }

    
}
