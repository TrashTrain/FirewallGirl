using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 상태(스탯·증강체·덱)를 JSON 파일로 저장/불러오기하는 순수 C# 싱글톤.
/// MonoBehaviour가 아니므로 씬에 배치할 필요 없이 첫 접근 시 자동 생성된다.
/// </summary>
public class PlayerStateSaveManager
{
    private static PlayerStateSaveManager _instance;
    public static PlayerStateSaveManager instance => _instance ??= new PlayerStateSaveManager();

    private const string SaveFileName = "player_save.json";
    private const int TotalNormalStages = 6;

    private static string SavePath => Application.persistentDataPath + "/" + SaveFileName;

    public PlayerSaveData StageEntrySnapshot { get; private set; }
    public bool IsLoadingFromSave { get; set; }
    public PlayerSaveData PendingRestoreData { get; private set; }
    public void SetPendingRestore(PlayerSaveData data) => PendingRestoreData = data;
    public void ClearPendingRestore()                  => PendingRestoreData = null;

    // ─── 파일 입출력 ───────────────────────────────────────────────────────────

    public void Save(PlayerSaveData data)
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
            Debug.Log("[PlayerStateSaveManager] 저장 완료: " + SavePath);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[PlayerStateSaveManager] 저장 실패: " + e.Message);
        }
    }

    public PlayerSaveData Load()
    {
        try
        {
            if (!File.Exists(SavePath)) return null;
            string json = File.ReadAllText(SavePath);
            return JsonUtility.FromJson<PlayerSaveData>(json);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[PlayerStateSaveManager] 불러오기 실패: " + e.Message);
            return null;
        }
    }

    public bool Exists() => File.Exists(SavePath);

    public void Delete()
    {
        try
        {
            if (File.Exists(SavePath)) File.Delete(SavePath);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[PlayerStateSaveManager] 삭제 실패: " + e.Message);
        }
    }

    // ─── 스냅샷 / 저장 데이터 생성 ────────────────────────────────────────────

    /// <summary>현재 PlayerManager 상태를 스테이지 진입 시점 스냅샷으로 기록한다.</summary>
    public void TakeSnapshot()
    {
        StageEntrySnapshot = BuildSaveData(StageSaveManager.CurrentStageIdx);
        Debug.Log("[PlayerStateSaveManager] 스냅샷 저장 (스테이지 " + StageSaveManager.CurrentStageIdx + ")");
    }

    /// <summary>현재 상태를 기반으로 PlayerSaveData를 생성한다.</summary>
    public PlayerSaveData BuildSaveData(int resumeStageIndex)
    {
        PlayerManager pm = PlayerManager.instance;
        if (pm == null) return null;

        var data = new PlayerSaveData
        {
            currentHP = pm.currentHP,
            fullHP = pm.MaxHP,
            baseStats = pm.GetBaseStats(),
            resumeStageIndex = resumeStageIndex
        };

        // 클리어된 스테이지 목록 수집 (PlayerPrefs가 이미 갱신된 상태)
        for (int i = 0; i < TotalNormalStages; i++)
        {
            if (StageSaveManager.IsStageCleared(i))
                data.clearedStageIds.Add(i);
        }

        // 증강체 ID 수집 (파일명 기반, " (Clone)" 제거)
        foreach (var aug in pm.activeAugments)
        {
            string id = aug.name.Replace(" (Clone)", "").Trim();
            data.augmentIds.Add(id);
        }

        // 카드 덱 수집 (PlayerManager.masterDeck의 현재 스탯)
        foreach (var card in pm.masterDeck)
        {
            data.deck.Add(new CardSaveData
            {
                cardIndex = card.cardIndex,
                positiveStatValue = card.positiveStatValue,
                negativeStatValue = card.negativeStatValue,
                cost = card.cost,
                coolTime = card.coolTime
            });
        }

        return data;
    }
}
