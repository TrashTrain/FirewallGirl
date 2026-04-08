using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class CardDeckController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject cardPrefab;
    public float cardScale = 0.7f;
    
    public float fanSpacing = 80f;     // 부채꼴 좌우 간격
    public float fanYDrop = 15f;       // 부채꼴 가장자리 아래로 떨어지는 정도
    public float fanAngleSpacing = 5f; // 부채꼴 회전 각도
    
    [Header("Line Spread (Hover)")]
    public float lineSpacing = 165f;   // 덱에 마우스를 올렸을 때 일렬 간격
    public float spreadSpeed = 10f;

    private readonly List<RectTransform> cardsRt = new List<RectTransform>();
    private readonly List<CardController> cards = new List<CardController>();

    // ✅ 기본(부채) 상태: "게임 시작 전 세팅 그대로" 저장
    private Vector2[] defaultPositions;
    private float[] defaultRotZ;

    // ✅ 호버(일렬) 상태
    private Vector2[] linePositions;

    // ✅ 현재 목표값
    private Vector2[] targetPositions;
    private float[] targetRotZ;

    public bool isSpread = false;     // true = 호버로 일렬 펼침 상태
    private bool isAnimating = false;
    private float snapEpsilon = 0.5f;

    void Start()
    {
        // 1. DB에서 카드를 불러와 동적으로 생성합니다.
        SpawnCardsFromDatabase();
        
        // 자식 카드 모으기
        foreach (Transform child in transform)
        {
            var cc = child.GetComponent<CardController>();
            var rt = child as RectTransform;

            if (cc != null && rt != null)
            {
                cards.Add(cc);
                cardsRt.Add(rt);
            }
        }

        int n = cardsRt.Count;

        defaultPositions = new Vector2[n];
        defaultRotZ = new float[n];
        linePositions = new Vector2[n];
        targetPositions = new Vector2[n];
        targetRotZ = new float[n];

        // // ✅ "부채꼴 레이아웃"은 자동 계산하지 않고,
        // // 씬/프리팹에 배치된 현재 값을 그대로 저장
        // for (int i = 0; i < n; i++)
        // {
        //     Vector2 p = cardsRt[i].anchoredPosition;
        //     p = new Vector2(Mathf.Round(p.x), Mathf.Round(p.y));
        //     cardsRt[i].anchoredPosition = p;
        //
        //     defaultPositions[i] = p;
        //     defaultRotZ[i] = cardsRt[i].localEulerAngles.z;
        //
        //     targetPositions[i] = defaultPositions[i];
        //     targetRotZ[i] = defaultRotZ[i];
        // }
        
        // 부채꼴 레이아웃 자동 계산
        BuildFanLayout();

        // ✅ 호버 시 "일렬 펼침" 위치만 계산
        BuildLineLayout();
    }
    
    private void SpawnCardsFromDatabase()
    {
        if (CardDatabaseManager.instance == null) return;

        List<CardObject> deck = CardDatabaseManager.instance.GetCurrentDeck();

        foreach (CardObject cardData in deck)
        {
            if (cardData != null)
            {
                // 프리팹 생성
                GameObject newCard = Instantiate(cardPrefab, transform);
                newCard.transform.localScale = Vector3.one * cardScale;

                // 배틀 모드로 설정
                CardController cc = newCard.GetComponent<CardController>();
                if (cc != null) cc.currentMode = CardController.CardMode.Battle;

                // 데이터 주입 (미리 클론된 데이터가 들어가므로 스탯 유지됨!)
                PlayerCard pc = newCard.GetComponent<PlayerCard>();
                if (pc != null) pc.cardData = cardData;

                // UI 비주얼 갱신
                UpdateCardVisuals(newCard, cardData);
            }
        }
    }
    
    private void UpdateCardVisuals(GameObject cardObj, CardObject data)
    {
        Image icon = cardObj.transform.Find("Icon")?.GetComponent<Image>();
        if (icon == null) icon = cardObj.transform.Find("Front/Icon")?.GetComponent<Image>();
        if (icon != null) icon.sprite = data.cardImage;

        TextMeshProUGUI nameText = FindChild<TextMeshProUGUI>(cardObj.transform, "NameText");
        if (nameText != null) nameText.text = data.cardName;

        TextMeshProUGUI costText = FindChild<TextMeshProUGUI>(cardObj.transform, "CostText");
        if (costText != null) costText.text = data.cost.ToString();

        TextMeshProUGUI descText = FindChild<TextMeshProUGUI>(cardObj.transform, "DescText");
        if (descText != null)
        {
            string dynamicDesc = data.description
                .Replace("{0}", data.positiveStatValue.ToString("+#;-#;0"))
                .Replace("{1}", data.negativeStatValue.ToString("+#;-#;0"));
            
            descText.text = dynamicDesc;
        }
    }
    
    private T FindChild<T>(Transform parent, string name) where T : Component
    {
        Transform child = parent.Find(name);
        if (child != null) return child.GetComponent<T>();
        return null;
    }

    private void Update()
    {
        if (cardsRt.Count == 0) return;
        
        int step = Mathf.Max(1, Mathf.RoundToInt(spreadSpeed * Time.deltaTime * 100f));

        for (int i = 0; i < cardsRt.Count; i++)
        {
            if (cards[i].isDragging) continue;
            
            cardsRt[i].anchoredPosition = Vector2.Lerp(
                cardsRt[i].anchoredPosition, 
                targetPositions[i], 
                spreadSpeed * Time.deltaTime
            );

            // 2. 회전 부드럽게 이동
            float currentZ = cardsRt[i].localEulerAngles.z;
            float targetZ = targetRotZ[i];
            
            // 각도 보간은 Mathf.LerpAngle을 쓰는 것이 좋습니다
            float newZ = Mathf.LerpAngle(currentZ, targetZ, spreadSpeed * Time.deltaTime);
            
            var e = cardsRt[i].localEulerAngles;
            e.z = newZ;
            cardsRt[i].localEulerAngles = e;

            // // 위치 이동 (정수 스냅)
            // Vector2 cur = cardsRt[i].anchoredPosition;
            // cur = new Vector2(Mathf.Round(cur.x), Mathf.Round(cur.y));
            //
            // Vector2 tar = targetPositions[i];
            // tar = new Vector2(Mathf.Round(tar.x), Mathf.Round(tar.y));
            //
            // float nx = Mathf.MoveTowards(cur.x, tar.x, step);
            // float ny = Mathf.MoveTowards(cur.y, tar.y, step);
            //
            // cardsRt[i].anchoredPosition = new Vector2(Mathf.Round(nx), Mathf.Round(ny));
            //
            // // 회전 이동 (Z만)
            // float cz = cardsRt[i].localEulerAngles.z;
            // float tz = targetRotZ[i];
            // float rz = Mathf.MoveTowardsAngle(cz, tz, spreadSpeed * 200f * Time.deltaTime);
            //
            // var e = cardsRt[i].localEulerAngles;
            // e.z = rz;
            // cardsRt[i].localEulerAngles = e;
        }

        // 애니메이션 종료 판정
        if (isAnimating)
        {
            bool allReached = true;

            for (int i = 0; i < cardsRt.Count; i++)
            {
                if (cards[i].isDragging) continue;

                Vector2 cur = cardsRt[i].anchoredPosition;
                Vector2 tar = targetPositions[i];
                float dz = Mathf.DeltaAngle(cardsRt[i].localEulerAngles.z, targetRotZ[i]);

                if ((cur - tar).sqrMagnitude > snapEpsilon * snapEpsilon) { allReached = false; break; }
                if (Mathf.Abs(dz) > 0.5f) { allReached = false; break; }
            }

            if (allReached) isAnimating = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerEnter != null && eventData.pointerEnter.CompareTag("CardDeck"))
        {
            if (!isSpread)
            {
                ApplyLineTargets();   // ✅ 일렬 + z=0
                isSpread = true;
                isAnimating = true;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isSpread)
        {
            if (isAnimating) return;

            ApplyDefaultTargets();    // ✅ 시작 전(부채) 세팅 그대로 복귀
            isSpread = false;
            isAnimating = true;
        }
    }

    private void ApplyDefaultTargets()
    {
        for (int i = 0; i < cardsRt.Count; i++)
        {
            targetPositions[i] = defaultPositions[i];
            targetRotZ[i] = defaultRotZ[i];
        }
    }

    private void ApplyLineTargets()
    {
        for (int i = 0; i < cardsRt.Count; i++)
        {
            targetPositions[i] = new Vector2(
                Mathf.Round(linePositions[i].x),
                Mathf.Round(linePositions[i].y)
            );
            targetRotZ[i] = 0f; // ✅ 요구사항: 펼칠 때 z rotation 모두 0
        }
    }
    
    private void BuildFanLayout()
    {
        int n = cardsRt.Count;
        if (n == 0) return;

        float mid = (n - 1) * 0.5f;

        for (int i = 0; i < n; i++)
        {
            float offset = i - mid; 
            
            // 중앙을 기준으로 퍼지는 위치 및 회전 계산
            float x = offset * fanSpacing;
            // 이차함수(포물선) 형태로 Y값을 내려서 둥근 부채꼴 모양을 만듦
            float y = -(offset * offset) * fanYDrop; 
            float z = -offset * fanAngleSpacing; 

            Vector2 p = new Vector2(Mathf.Round(x), Mathf.Round(y));

            defaultPositions[i] = p;
            defaultRotZ[i] = z;

            // 시작 위치를 기본 위치로 즉시 설정
            cardsRt[i].anchoredPosition = p;
            
            var e = cardsRt[i].localEulerAngles;
            e.z = z;
            cardsRt[i].localEulerAngles = e;

            targetPositions[i] = defaultPositions[i];
            targetRotZ[i] = defaultRotZ[i];
        }
    }

    private void BuildLineLayout()
    {
        int n = cardsRt.Count;
        if (n == 0) return;

        float mid = (n - 1) * 0.5f;

        // 일렬 펼침의 기준 Y: 기본(부채) 상태의 평균 Y 사용
        float baseY = 0f;
        for (int i = 0; i < n; i++)
            baseY += defaultPositions[i].y;
        baseY /= n;

        for (int i = 0; i < n; i++)
        {
            float x = (i - mid) * lineSpacing;
            linePositions[i] = new Vector2(x, baseY);
        }
    }
}
