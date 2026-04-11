using System.Collections;
using TMPro;
using UnityEngine;

public class Virus : MonoBehaviour
{
    [Header("SO������")]
    public VirusObjectSO virusObjectSO;

    [Header("���� ������")]
    public SpriteRenderer image;

    //public TextMeshProUGUI atkDmgText;
    //public TextMeshProUGUI hpCntText;

    [HideInInspector]
    public VirusData virusData;
    public int RandState;

    public int spawnNum;

    public bool virusState = false;

    public EnemyUIController enemyUIController;

    public State NextAction { get; protected set; }

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
        Ready,      // ���� �غ�
        Bomb,
        Debuf,
        Death
    }


    private void Awake()
    {
        _originPos = transform.position;
        _originScale = transform.localScale;
    }

    protected virtual void Start()
    {
        if (VirusMgr.instance == null)
        {
            Debug.LogError("CardMgr.instance�� �ʱ�ȭ���� �ʾҽ��ϴ�.");
            return;
        }
        
        Debug.Log($"바이러스 수: {VirusSpawn.instance.virusCnt}");

        InitData();
        
        if (enemyUIController != null)
        {
            enemyUIController.panel.SetActive(true); // Ȥ�� �������� UI �г� ���� Ȱ��ȭ
            UpdateData(); // ���� ü��, ���ݷ�, ������ UI �ؽ�Ʈ�� Ȯ���ϰ� ���� (�̹� �����ν� �Լ� Ȱ��!)
        }
        
        // ������ �� ������ ��������
        RollNextActionAndUpdateIcon();
    }

    //private void Update()
    //{
    //    if (virusData.CurHpCnt <= 0)
    //    {
    //        Debug.Log("���̷��� ��ġ");
    //        Destroy(enemyUIController.gameObject);
    //        Destroy(gameObject);
    //    }
    //}

    public void RollNextActionAndUpdateIcon()
    {
        RollNextAction();

        if (enemyUIController != null)
        {
            enemyUIController.state.UpdateStateImage(NextAction);
        }
    }

    //protected virtual void RollNextAction()
    //{
    //    GetRandState();
    //    NextAction = (State)RandState;
    //}
    protected virtual void RollNextAction()
    {
        // 1. �Ϲ����� ����(�θ� ������ �״�� ���� �ֵ�)�� �ൿ Ǯ
        State[] basicStates = { State.Atk, State.Def, State.Sup };

        // 2. �迭 �ȿ����� �������� ����
        int randomIndex = Random.Range(0, basicStates.Length);
        NextAction = basicStates[randomIndex];
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
        // ��ǥ: �÷��̾� ��ġ���� ���ٰ� ����
        Transform playerTr = PlayerManager.instance.transform;
        if (playerTr == null) yield break;

        Vector3 start = _originPos;
        Vector3 target = playerTr.position;

        // 2D Ⱦ�̵��� ���ϸ� y ����(�ʿ��ϸ� �ּ� ����)
         target.y = start.y;

        yield return LerpPos(start, target, atkMoveDuration);

        // ���� ������ ����
        PlayerManager.instance.TakeDamage(virusData.AtkDmg);

        if (atkPause > 0f) yield return new WaitForSeconds(atkPause);

        yield return LerpPos(target, start, atkMoveDuration);

        // ������ �׻� ���� ����
        transform.position = start;
    }

    protected IEnumerator CoSupport()
    {
        // ��ǥ: Ŀ���ٰ� �۾��� + ���ݷ� ����
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
        // ��ǥ: y������ �ö󰬴� ������ + ���� ����
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

        // �ൿ ����
        yield return CoRunStateAction(NextAction);
    }

    protected virtual IEnumerator CoRunStateAction(State s)
    {
        switch (s)
        {
            case State.Atk:
                yield return CoAttack();
                break;
            case State.Sup:
                yield return CoSupport();
                break;
            case State.Def:
                yield return CoDefend();
                break;
            case State.Death:
                Destroy(gameObject);
                yield break;
            default:
                // Idle/Death�� �׳� �Ѿ

                yield break;
        }
        //RollNextAction();
    }

    public int ApplyDamage(int damage)
    {
        int remaining = damage;
        
        // ���� ����
        if (virusData.DefCnt > 0)
        {
            int defUsed = Mathf.Min(virusData.DefCnt, remaining);
            virusData.DefCnt = Mathf.Max(0, virusData.DefCnt - defUsed);
            remaining -= defUsed;
        }
        
        // ���� ��������ŭ ü�� ����
        if (remaining > 0 && virusData.CurHpCnt > 0)
        {
            int hpUsed = Mathf.Min(virusData.CurHpCnt, remaining);
            virusData.CurHpCnt = Mathf.Max(0, virusData.CurHpCnt - hpUsed);
            remaining -= hpUsed;
        }
        
        UpdateData();
        
        Debug.Log($"[Enemy] ���� ������: {damage}, ���� ü��: {virusData.CurHpCnt}");

        if (virusData.CurHpCnt <= 0)
        {
            // Death state�� ���� ����
            OnDeath();
        }

        return remaining;
    }
    // ���� ó���� ����ϴ� �Լ� �߰�
    protected virtual void OnDeath()
    {
        VirusSpawn.instance.SetDiscountVirusCount();
        
        if (VirusSpawn.instance.virusCnt <= 0)
        {
            VirusSpawn.instance.StartCoroutine(VirusSpawn.instance.GetReward());
        }

        //gameObject.SetActive(false);


        // ���� UI�� ���� ���� �Ѵٸ�:
        if (enemyUIController != null) enemyUIController.panel.SetActive(false);

        Destroy(gameObject);
    }

    
}
