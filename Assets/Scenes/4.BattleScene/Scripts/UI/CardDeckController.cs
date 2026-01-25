using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardDeckController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
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
        defaultRotZ      = new float[n];

        linePositions    = new Vector2[n];

        targetPositions  = new Vector2[n];
        targetRotZ       = new float[n];

        // ✅ "부채꼴 레이아웃"은 자동 계산하지 않고,
        // 씬/프리팹에 배치된 현재 값을 그대로 저장
        for (int i = 0; i < n; i++)
        {
            Vector2 p = cardsRt[i].anchoredPosition;
            p = new Vector2(Mathf.Round(p.x), Mathf.Round(p.y));
            cardsRt[i].anchoredPosition = p;

            defaultPositions[i] = p;
            defaultRotZ[i] = cardsRt[i].localEulerAngles.z;

            targetPositions[i] = defaultPositions[i];
            targetRotZ[i] = defaultRotZ[i];
        }

        // ✅ 호버 시 "일렬 펼침" 위치만 계산
        BuildLineLayout();
    }

    private void Update()
    {
        int step = Mathf.Max(1, Mathf.RoundToInt(spreadSpeed * Time.deltaTime * 100f));

        for (int i = 0; i < cardsRt.Count; i++)
        {
            if (cards[i].isDragging) continue;

            // 위치 이동 (정수 스냅)
            Vector2 cur = cardsRt[i].anchoredPosition;
            cur = new Vector2(Mathf.Round(cur.x), Mathf.Round(cur.y));

            Vector2 tar = targetPositions[i];
            tar = new Vector2(Mathf.Round(tar.x), Mathf.Round(tar.y));

            float nx = Mathf.MoveTowards(cur.x, tar.x, step);
            float ny = Mathf.MoveTowards(cur.y, tar.y, step);

            cardsRt[i].anchoredPosition = new Vector2(Mathf.Round(nx), Mathf.Round(ny));

            // 회전 이동 (Z만)
            float cz = cardsRt[i].localEulerAngles.z;
            float tz = targetRotZ[i];
            float rz = Mathf.MoveTowardsAngle(cz, tz, spreadSpeed * 200f * Time.deltaTime);

            var e = cardsRt[i].localEulerAngles;
            e.z = rz;
            cardsRt[i].localEulerAngles = e;
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
