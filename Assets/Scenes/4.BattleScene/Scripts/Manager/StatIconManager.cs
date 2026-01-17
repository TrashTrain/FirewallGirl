using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatIconManager : MonoBehaviour
{
    public static StatIconManager Instance { get; private set; }

    [Header("효과 스프라이트")]
    [SerializeField] private Sprite attackIcon;
    [SerializeField] private Sprite defenseIcon;
    [SerializeField] private Sprite costIcon;
    [SerializeField] private Sprite healthIcon;
    [SerializeField] private Sprite evasionIcon;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // UI 씬이 바뀌어도 유지하고 싶으면 사용
        DontDestroyOnLoad(gameObject);
    }

    public Sprite GetIcon(StatType stat)
    {
        switch (stat)
        {
            case StatType.Attack:  return attackIcon;
            case StatType.Defense: return defenseIcon;
            case StatType.Cost:    return costIcon;
            case StatType.Health:  return healthIcon;
            case StatType.Evasion: return evasionIcon;
            default:
                Debug.LogWarning($"StatIconManager: Unknown StatType = {stat}");
                return null;
        }
    }
}
