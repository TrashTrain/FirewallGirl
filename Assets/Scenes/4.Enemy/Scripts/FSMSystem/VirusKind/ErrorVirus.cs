using System.Collections;
using UnityEngine;

public class ErrorVirus : Virus
{
    // 기획서에 명시된 디버프 종류
    public enum DebuffType
    {
        Lag,            // 카드 쿨타임 증가 (1턴)
        Backdoor,       // 공격력 감소 (2턴)
        PacketLoss,     // 방어도 쌓기 불가 (1턴)
        CardLock        // 특정 카드 사용 불가 (1턴)
    }

    private DebuffType _nextDebuff;

    // 부모의 랜덤 행동 로직을 덮어씀
    protected override void RollNextAction()
    {
        // 기획서: "기본 공격 가능, 플레이어 방해 행위 위주"
        // 예시로 75% 확률로 디버프(Debuf 상태 활용), 25% 확률로 기본 공격(Atk) 설정
        if (Random.Range(0, 100) < 75)
        {
            NextAction = State.Debuf;
            // 4가지 디버프 중 랜덤 1개 선택
            _nextDebuff = (DebuffType)Random.Range(0, 4);
        }
        else
        {
            NextAction = State.Atk;
        }
    }

    // 행동 실행 로직
    protected override IEnumerator CoRunStateAction(State s)
    {
        if (s == State.Atk)
        {
            // 부모 클래스에 있는 공격 코루틴 그대로 사용 (ATK: 1)
            yield return CoAttack();
        }
        else if (s == State.Debuf)
        {
            // 디버프 전용 코루틴 실행
            yield return CoApplyDebuff();
        }
        else
        {
            // Idle 상태 등
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator CoApplyDebuff()
    {
        Debug.Log($"[ErrorVirus] 시스템 에러 발생! 디버프 시전: {_nextDebuff}");

        // 디버프 시전 연출 대기 시간
        yield return new WaitForSeconds(0.5f);

        // 플레이어 매니저가 존재하는지 확인
        if (PlayerManager.instance != null)
        {
            switch (_nextDebuff)
            {
                case DebuffType.Lag:
                    // [주의점 1 참고] 전투 카드 매니저와 연동 필요
                    PlayerManager.instance.lagDebuffTurns += PlayerManager.instance.lagDebuffValue + 1;
                    Debug.Log($"효과: 모든 카드의 쿨타임이 {PlayerManager.instance.lagDebuffValue} 증가 (1턴)");
                    break;

                case DebuffType.Backdoor:
                    // [주의점 2 참고] 다중 턴 유지 로직 필요     수정완료
                    PlayerManager.instance.AddMultiTurnStat(StatType.Attack, -1, 3);
                    Debug.Log("효과: 플레이어 공격력 감소 (2턴)");
                    break;

                case DebuffType.PacketLoss:
                    // [주의점 3 참고] 방어 불가 상태 추가 필요    수정완료
                    PlayerManager.instance.cannotGainDefenseTurns = 1;
                    Debug.Log("효과: 방어도 쌓기 불가 (1턴)");
                    break;

                case DebuffType.CardLock:
                    // [주의점 1 참고] 전투 중인 손패(Hand) 제어 필요
                    Debug.Log("효과: N번 카드 사용 불가 (1턴)");
                    break;
            }
        }

        yield return new WaitForSeconds(0.5f);
    }
}