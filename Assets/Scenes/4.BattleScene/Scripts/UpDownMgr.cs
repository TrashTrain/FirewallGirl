using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class UpDownMgr : MonoBehaviour
{

    public Button UpDownTestBtn;

    void Start()
    {
        UpDownTestBtn.onClick.AddListener(UpDownTestBtnClick);
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
            return $"{(value > 0 ? "+" : "")}{value} {description}";
        }
    }

    UpDown GenerateRandomAugment()
    {
        int value = 0;
        //0 미포함
        while (value == 0)
        {
            value = Random.Range(-3, 3);
        }
        string[] descriptions = { "공격력", "방어력", "코스트", "체력" };
        string selected = descriptions[Random.Range(0, descriptions.Length)];

        return new UpDown(value, selected);
    }

    public void UpDownTestBtnClick()
    {
        UpDown randomAugment = GenerateRandomAugment();
        Debug.Log("증강 결과: " + randomAugment.ToString());
    }
}
