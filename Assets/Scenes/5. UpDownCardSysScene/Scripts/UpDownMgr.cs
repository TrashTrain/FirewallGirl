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
    public TextMeshProUGUI[] SkillText;
    public TextMeshProUGUI[] UpDownText;
    public Image[] SkillIcons;

    [Header("카드 Sprite")]
    public Sprite swordSprite; //공격력 = 방패
    public Sprite shieldSprite; //방어력 = 칼 
    public Sprite costUpSprite; //코스트 = 상승 그래프
    public Sprite costDownSprite; //코스트 = 상승 그래프
    public Sprite hpSprite; //체력 = 물약 
    public Sprite avoidanceSprite; //회피율 = 바람

    [Header("카드 Reload 버튼")]
    public Button ReloadBtn; //카드 리로드 버튼

    //카드 기억 
    private List<int> recentValues = new List<int>();
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
        int value = 0;
        int attempt = 0;

        //0 미포함
        while (value == 0 && attempt < 100)
        {
            int candidate = Random.Range(-10, 10);
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
            recentValues.RemoveAt(0);
        }

        string[] descriptions = { "공격력", "방어력", "코스트", "체력", "회피율" };
        string selected = descriptions[Random.Range(0, descriptions.Length)];

        return new UpDown(value, selected);
    }

    public void UpDownSystem()
    {
        for (int i = 0; i < 3; i++)
        {
            UpDown randomAugment = GenerateRandomAugment();
            SkillText[i].text = randomAugment.description;
            UpDownText[i].text = randomAugment.ToString();

            Sprite icon = GetSprite(randomAugment.description, randomAugment.value);
            SkillIcons[i].sprite = icon;
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
