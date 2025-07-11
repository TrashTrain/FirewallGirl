using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class UpDownMgr : MonoBehaviour
{
    [Header("카드 UI")]
    public Button[] Card;

    [Header("카드 요소")]
    public TextMeshProUGUI[] PositiveSkillText;
    public TextMeshProUGUI[] NegativeSkillText;
    public TextMeshProUGUI[] PositiveUpDownText;
    public TextMeshProUGUI[] NegativeUpDownText;

    public Image[] PositiveSkillIcons;
    public Image[] NegativeSkillIcons;


    [Header("카드 Sprite")]
    public Sprite swordSprite; //공격력 = 방패
    public Sprite shieldSprite; //방어력 = 칼 
    public Sprite costUpSprite; //코스트 = 상승 그래프
    public Sprite costDownSprite; //코스트 = 상승 그래프
    public Sprite hpSprite; //체력 = 물약 
    public Sprite avoidanceSprite; //회피율 = 바람

    [Header("카드 Reload 버튼")]
    public Button ReloadBtn; //카드 리로드 버튼

    //카드 기억: 중복허용x 자료구조형으로
    private HashSet<int> recentValues = new HashSet<int>();
    private const int RECENT_HISTORY_LIMIT = 10; // 기억할 최근 값의 수

    void Start()
    {
        foreach (Button card in Card)
        {
            card.gameObject.SetActive(false);
        }
        UpDownSystem(); //처음에 리로드 시작하면서 시작
        //리로드 버튼 시작
        ReloadBtn.onClick.AddListener(ReloadBtnClick);

        for (int i = 0; i < Card.Length; i++)
        {
            int index = i; 
            Card[i].onClick.AddListener(() => OnCardClicked());
        }

    }

    //카드를 선택한 경우 배틀씬으로 
    void OnCardClicked()
    {
        Debug.Log("Clicked");
        // 필요하면 선택된 카드 인덱스를 저장
        // GameManager.selectedCardId = cardIndex;

        SceneManager.LoadScene("BattleScene");
    }

    Sprite GetSprite(string description, int value)
    {
        switch (description)
        {
            case "공격력": return swordSprite;
            case "방어력": return shieldSprite;
            case "코스트":
                return value > 0 ? costUpSprite : costDownSprite;
            case "체력": return hpSprite;
            case "회피율": return avoidanceSprite;
            default: return null;
        }
    }

    public struct UpDown
    {
        public int value;
        public string description;

        public UpDown(int value, string description)
        {
            this.value = value;
            this.description = description;
        }

        public override string ToString()
        {
            return $"{(value > 0 ? "+" : "")}{value}";
        }
    }

    UpDown GenerateRandomAugment()
    {
        //최종적으로 선택될 숫자 
        int value = 0;
        //시도 획수를 세는 변수
        int attempt = 0;

        //0 미포함
        while (value == 0 && attempt < 100) //루프를 100번만 돌도록 제한
        {
            //GenerateRandomAugment에서 랜덤으로 도출된 값
            int candidate = Random.Range(-10, 10);
            //루프를 ++
            attempt++;

            // 최근에 나온 값이 아니거나, 낮은 확률(30%)로 등장 허용
            if (candidate != 0 &&
                (!recentValues.Contains(candidate) || Random.value < 0.3f))
            {
                value = candidate;
                break;
            }
        }

        // value가 여전히 0이면 강제로 0을 제외한 다른 수를 뽑음
        if (value == 0)
        {
            do
            {
                value = Random.Range(-10, 10);
            } while (value == 0);
        }

        // 최근값 리스트에 저장
        recentValues.Add(value);
        if (recentValues.Count > RECENT_HISTORY_LIMIT)
        {
            recentValues.Clear();
        }

        string[] descriptions = { "공격력", "방어력", "코스트", "체력", "회피율" };
        string selected = descriptions[Random.Range(0, descriptions.Length)];

        return new UpDown(value, selected);
    }

    public void UpDownSystem()
    {
        // 카드 독립 중복방지를 위한 해시셋 
        HashSet<string> usedDescriptions = new HashSet<string>();

        for (int i = 0; i < 3; i++)
        {
            // ----------------- 긍정 효과 --------------------
            UpDown positive;
            int tryCount = 0;

            do
            {
                positive = GenerateRandomAugment();
                tryCount++;
            } while (usedDescriptions.Contains(positive.description) && tryCount < 20);

            usedDescriptions.Add(positive.description);

            PositiveSkillText[i].text = positive.description;
            PositiveUpDownText[i].text = positive.ToString();
            PositiveSkillIcons[i].sprite = GetSprite(positive.description, positive.value);

            // ----------------- 부정 효과 --------------------
            UpDown negative;
            tryCount = 0;

            do
            {
                negative = GenerateRandomAugment();
                tryCount++;
            } while (
                (usedDescriptions.Contains(negative.description) || negative.description == positive.description)
                && tryCount < 20
            );

            usedDescriptions.Add(negative.description);

            NegativeSkillText[i].text = negative.description;
            NegativeUpDownText[i].text = negative.ToString();
            NegativeSkillIcons[i].sprite = GetSprite(negative.description, negative.value);
        }

    }
    //카드 리로드
    public void ReloadBtnClick()
    {
        foreach (Button card in Card)
        {
            card.gameObject.SetActive(true);
        }
        UpDownSystem();
    }

}
