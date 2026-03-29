using System.Collections;
using UnityEngine;

public class BombVirus : Virus
{
    [Header("자폭 몬스터 설정")]
    public int explosionDamage = 50; // 폭발 시 데미지

    private int _turnCount = 0; // 경과 턴 수

    // 부모의 랜덤 행동 로직을 무시하고 고정된 카운트다운 적용
    protected override void RollNextAction()
    {
        if (_turnCount == 0)
        {
            // 1턴째: 폭발 준비 (아무 행동도 하지 않거나, 전용 '준비' 애니메이션/UI 표기 권장)
            NextAction = State.Ready;
        }
        else if (_turnCount == 1)
        {
            // 2턴째: 폭발 카운트다운 진행 중
            NextAction = State.Ready;
        }
        else if (_turnCount >= 2)
        {
            // 3턴째: 마침내 폭발
            NextAction = State.Bomb;
        }
    }

    // 부모의 행동 실행 로직을 덮어써서 자폭 전용 흐름 적용
    protected override IEnumerator CoRunStateAction(State s)
    {
        if (s == State.Bomb && _turnCount >= 2)
        {
            // 폭발 턴이 되면 자폭 코루틴 실행
            yield return CoExplosion();
        }
        else
        {
            // 폭발 턴이 아닐 때는 가만히 대기하며 턴 넘김
            Debug.Log($"[BombVirus] 자폭까지 남은 턴: {2 - _turnCount}");
            yield return new WaitForSeconds(0.5f);
        }

        _turnCount++;
    }

    // 자폭 전용 로직
    private IEnumerator CoExplosion()
    {
        Debug.Log("[BombVirus] 폭발합니다!");

        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.8f, 2.0f);
        }

        // 1. 플레이어에게 데미지 가함
        if (PlayerManager.instance != null)
        {
            /* * 기획서의 다른 데미지 옵션을 사용하려면 아래처럼 변경 가능합니다.
             * 플레이어 현재 체력의 1/2: PlayerManager.instance.curHp / 2
             * 플레이어 전체 체력의 1/2: PlayerManager.instance.maxHp / 2
             */
            PlayerManager.instance.TakeDamage(explosionDamage);
        }

        // 폭발 이펙트를 보여줄 여유 시간 대기
        yield return new WaitForSeconds(0.5f);

        // 2. 데미지를 준 후 몬스터 본인 사망 처리
        ApplyDamage(9999);
    }
}