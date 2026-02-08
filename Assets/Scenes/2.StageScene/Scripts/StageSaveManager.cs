using UnityEngine;

public static class StageSaveManager
{
    // 보스 스테이지를 열기 위해 클리어해야 하는 일반 스테이지의 개수
    // (예: 1번~5번 스테이지를 깨야 한다면 5로 설정)
    private const int TOTAL_NORMAL_STAGES = 6;
    public static int CurrentStageIdx = 0;

    // --- 저장 관련 핵심 기능 ---

    // 스테이지 클리어 시 호출 (stageID: 1, 2, 3...)
    public static void ClearStage(int stageID)
    {
        // "Stage_1", "Stage_2" 같은 키로 1(True)을 저장
        if (PlayerPrefs.GetInt($"Stage_{stageID}", 0) == 0)
        {
            PlayerPrefs.SetInt($"Stage_{stageID}", 1);
            PlayerPrefs.Save(); // 저장 확정
            Debug.Log($"스테이지 {stageID} 클리어 저장 완료!");
        }
    }

    // 스테이지 초기화
    public static void ResetStage()
    {
        for (int i = 0; i < TOTAL_NORMAL_STAGES; i++)
        {
            if (IsStageCleared(i))
            {
                PlayerPrefs.SetInt($"Stage_{i}", 0);
            }
        }
        PlayerPrefs.Save(); // 저장 확정
        Debug.Log($"스테이지 클리어 초기화 완료!");
    }

    // 특정 스테이지를 깼는지 확인
    public static bool IsStageCleared(int stageID)
    {
        // 값이 1이면 깬 것, 0이면 안 깬 것
        return PlayerPrefs.GetInt($"Stage_{stageID}", 0) == 1;
    }

    // --- 보스 스테이지 해금 조건 확인 ---

    // 보스 스테이지에 입장 가능한지 검사하는 함수
    public static bool CanEnterBossStage()
    {
        // 1번부터 마지막 일반 스테이지까지 다 깼는지 확인
        for (int i = 0; i < TOTAL_NORMAL_STAGES; i++)
        {
            // 하나라도 안 깬 게 있다면 false 반환 (보스 잠김)
            if (!IsStageCleared(i))
            {
                return false;
            }
        }

        // 반복문을 무사히 통과했다면 모든 스테이지를 깬 것 (보스 열림)
        return true;
    }
}