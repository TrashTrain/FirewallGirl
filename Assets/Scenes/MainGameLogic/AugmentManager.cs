using System.Collections.Generic;
using UnityEngine;

public class AugmentManager : MonoBehaviour
{
    public static AugmentManager Instance { get; private set; }

    [Header("Config")]
    public RankWeightTable rankWeightTable;

    [Header("Base Augments (set in Inspector)")]
    public List<AugmentData> baseAugments = new List<AugmentData>();

    private AugmentPool pool = new AugmentPool();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 런 시작 시 호출
    public void StartRun()
    {
        pool.Clear();

        foreach (var a in baseAugments)
            pool.Add(a);
    }

    // 런타임 생성 증강 추가
    public bool AddRuntimeAugment(AugmentData data)
        => pool.Add(data);

    // 랜덤 뽑기 (중복 비허용: 풀에서 제거됨)
    public bool TryDrawAugment(out AugmentData data)
        => pool.TryDraw(rankWeightTable, out data);
}
