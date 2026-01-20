using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardChoicePanel : MonoBehaviour
{
    public GameObject panel; // CardSelectPanel
    public RectTransform centerCard;
    public RectTransform leftCard;
    public RectTransform rightCard;

    public Vector2 centerPosition = new Vector2(0, 0);
    public Vector2 leftOffset = new Vector2(-300, 0);
    public Vector2 rightOffset = new Vector2(300, 0);
    public float centerRiseY = -600f;

    public float moveDuration = 0.25f;
    public float delayBetween = 0.15f;
    
    public void ShowPanel()
    {
        panel.SetActive(true);
        
        centerCard.SetAsLastSibling();

        // 초기 위치 세팅
        centerCard.anchoredPosition = new Vector2(0, centerRiseY);
        leftCard.anchoredPosition = centerPosition;
        rightCard.anchoredPosition = centerPosition;

        StartCoroutine(AnimateCards());
    }
    
    IEnumerator AnimateCards()
    {
        // 1. 센터 카드 올라오기
        float t = 0f;
        Vector2 start = centerCard.anchoredPosition;
        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            centerCard.anchoredPosition = Vector2.Lerp(start, centerPosition, t);
            yield return null;
        }
        centerCard.anchoredPosition = centerPosition;

        // 2. 살짝 딜레이
        yield return new WaitForSeconds(delayBetween);

        // 3. 좌우 카드 이동
        t = 0f;
        Vector2 leftStart = centerPosition;
        Vector2 rightStart = centerPosition;
        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            leftCard.anchoredPosition = Vector2.Lerp(leftStart, centerPosition + leftOffset, t);
            rightCard.anchoredPosition = Vector2.Lerp(rightStart, centerPosition + rightOffset, t);
            yield return null;
        }
        leftCard.anchoredPosition = centerPosition + leftOffset;
        rightCard.anchoredPosition = centerPosition + rightOffset;
    }

    private void SaveStageClear()
    {
        StageSaveManager.ClearStage(StageMgr.Instance.clearStageCnt++);
    }
}
