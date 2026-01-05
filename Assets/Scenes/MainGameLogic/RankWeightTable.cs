using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RankWeight
{
    public AugmentRank rank;
    [Min(0f)] public float weight = 1f;
}

[CreateAssetMenu(menuName = "Augments/Rank Weight Table")]
public class RankWeightTable : ScriptableObject
{
    public List<RankWeight> weights = new List<RankWeight>()
    {
        new RankWeight { rank = AugmentRank.Common, weight = 40f },
        new RankWeight { rank = AugmentRank.Rare,   weight = 30f },
        new RankWeight { rank = AugmentRank.Unique, weight = 20f  },
        new RankWeight { rank = AugmentRank.Epic,   weight = 10f  },
    };
}
