// CardSceneInitializer.cs

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardSceneInitializer : MonoBehaviour
{
    // 인스펙터에 UpDownSysScene에 있는 모든 UI 컴포넌트를 연결해야 합니다.
    [Header("UI 연결 (UpDownSysScene)")]
    public GameObject CardParentPanel;
    public Button[] Cards;
    public TextMeshProUGUI[] PositiveSkillText;
    public TextMeshProUGUI[] NegativeSkillText;
    public TextMeshProUGUI[] PositiveUpDownText;
    public TextMeshProUGUI[] NegativeUpDownText;
    public Image[] PositiveSkillIcons;
    public Image[] NegativeSkillIcons;

    void Start()
    {
        // 씬 로드 시 UpDownMgr가 있는지 확인
        if (UpDownMgr.instance != null)
        {
            // 씬에 있는 UI 컴포넌트들을 UpDownMgr에 전달하여 재연결 및 초기화 명령
            UpDownMgr.instance.InitializeUI(
                Cards,
                PositiveSkillText,
                NegativeSkillText,
                PositiveUpDownText,
                NegativeUpDownText,
                PositiveSkillIcons,
                NegativeSkillIcons,
                CardParentPanel
            );
        }
    }
}