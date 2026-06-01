using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyUIController : MonoBehaviour
{
    public EnemyHPControl healthBar;
    public StateImageChange state;
    public GameObject panel;
    public TextMeshProUGUI atk;
    public TextMeshProUGUI def;
    public TextMeshProUGUI hp;

    [Header("Boss Status Effects (optional)")]
    public EnemyStatusUI enemyStatusUI; // 보스 전용 상태 효과 UI — 일반 몬스터는 null로 두면 됨
}
