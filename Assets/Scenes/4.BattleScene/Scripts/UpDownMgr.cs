using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class UpDownMgr : MonoBehaviour
{
    [Header("카드 UI")]
    public GameObject[] Card;

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

    void Start()
    {
        foreach (GameObject card in Card)
        {
            card.SetActive(false);
        }
        UpDownSystem(); //처음에 리로드 시작하면서 시작
        //리로드 버튼 시작
        ReloadBtn.onClick.AddListener(ReloadBtnClick);
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
        //0 미포함
        while (value == 0)
        {
            value = Random.Range(-10, 10);
        }
        string[] descriptions = { "공격력", "방어력", "코스트", "체력", "회피율" };
        string selected = descriptions[Random.Range(0, descriptions.Length)];

        return new UpDown(value, selected);
    }

    public void UpDownSystem()
    {   
        for (int i=0; i<3; i++)
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
        foreach (GameObject card in Card)
        {
            card.SetActive(true);
        }
        UpDownSystem();
    }
}
