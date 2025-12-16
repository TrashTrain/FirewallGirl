using System.Collections.Generic;
using UnityEngine;

public class CardPrint : MonoBehaviour
{
    [Header("CSV Reader")]
    [SerializeField] private string csv_FileName = "CardSheets"; // Resources/CardSheets.csv 필요
    private List<Dictionary<string, object>> cardData;

    void Start()
    {
        // CSV 데이터 읽기
        cardData = CSVReader.Read(csv_FileName);

        if (cardData == null || cardData.Count == 0)
        {
            Debug.LogError("CSV 파일을 읽을 수 없거나 데이터가 없습니다.");
            return;
        }

        // 모든 카드 출력
        for (int i = 0; i < cardData.Count; i++)
        {
            PrintCardData(i);
        }
    }

    void PrintCardData(int index)
    {
        if (index < 0 || index >= cardData.Count) return;

        var data = cardData[index];

        string cardIDText = data.ContainsKey("카드ID") ? data["카드ID"].ToString() : "";
        string cardNameText = data.ContainsKey("카드명") ? data["카드명"].ToString() : "";
        string cardTypeText = data.ContainsKey("속성") ? data["속성"].ToString() : "";
        string cardEffectTypeText = data.ContainsKey("효과타입") ? data["효과타입"].ToString() : "";
        string cardEffectValueText = data.ContainsKey("효과값") ? data["효과값"].ToString() : "";
        string cardTimeText = data.ContainsKey("지속시간") ? data["지속시간"].ToString() : "";
        string cardCostText = data.ContainsKey("코스트소모") ? data["코스트소모"].ToString() : "";

        Debug.Log($"[카드 {index}] " +
                  $"이름: {cardNameText}, " +
                  $"속성: {cardTypeText}, " +
                  $"효과: {cardEffectTypeText}, " +
                  $"값: {cardEffectValueText}, " +
                  $"지속시간: {cardTimeText}, " +
                  $"코스트: {cardCostText}");
    }
}
