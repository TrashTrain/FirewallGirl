using System.Collections.Generic;
using UnityEngine;

public class AugmentPool
{
    private readonly Dictionary<AugmentRank, List<AugmentData>> byRank = new();
    private readonly HashSet<string> ids = new(); // 전체 중복 방지

    public int Count
    {
        get
        {
            int sum = 0;
            foreach (var kv in byRank) sum += kv.Value.Count;
            return sum;
        }
    }

    public void Clear()
    {
        byRank.Clear();
        ids.Clear();
    }

    public bool Add(AugmentData data)
    {
        if (data == null) return false;
        if (string.IsNullOrEmpty(data.id)) return false; // id 없으면 거부 (정책상 안전)
        if (ids.Contains(data.id)) return false;          // 중복이면 거부

        ids.Add(data.id);

        if (!byRank.TryGetValue(data.augmentRank, out var list))
        {
            list = new List<AugmentData>();
            byRank[data.augmentRank] = list;
        }
        list.Add(data);
        return true;
    }

    public bool TryDraw(RankWeightTable table, out AugmentData drawn)
    {
        drawn = null;
        if (table == null || Count == 0) return false;

        // 1) 현재 풀에 존재하는 등급만 대상으로 가중치 합산
        float total = 0f;
        foreach (var rw in table.weights)
        {
            if (rw.weight <= 0f) continue;
            if (byRank.TryGetValue(rw.rank, out var list) && list.Count > 0)
                total += rw.weight;
        }
        if (total <= 0f) return false;

        // 2) 가중치로 등급 선택
        float r = Random.value * total;
        AugmentRank chosenRank = AugmentRank.Common;
        bool found = false;

        foreach (var rw in table.weights)
        {
            if (rw.weight <= 0f) continue;
            if (!byRank.TryGetValue(rw.rank, out var list) || list.Count == 0) continue;

            r -= rw.weight;
            if (r <= 0f)
            {
                chosenRank = rw.rank;
                found = true;
                break;
            }
        }
        if (!found) return false;

        // 3) 해당 등급 풀에서 1개 뽑고 제거(중복 비허용)
        var candidates = byRank[chosenRank];
        int idx = Random.Range(0, candidates.Count);
        drawn = candidates[idx];
        candidates.RemoveAt(idx);

        // 리스트가 비면 키 정리(선택)
        if (candidates.Count == 0)
            byRank.Remove(chosenRank);

        ids.Remove(drawn.id); // “풀에서 제거”이므로 id도 제거 (이번 런에서 재등장 방지 목적이면 제거하지 말 것)

        return true;
    }
}
