using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 튜토리얼 하이라이트 시스템. 스포트라이트(4-panel 어두운 오버레이)와
/// 글로우 아웃라인(절차적 생성 테두리)을 관리한다.
/// Sort Order가 높은 별도 Canvas에 부착해야 한다 (예: Screen Space Overlay, Sort Order 100).
/// </summary>
[RequireComponent(typeof(Canvas))]
public class TutorialHighlightController : MonoBehaviour
{
    [Header("Spotlight")]
    [SerializeField] private Color spotlightColor = new Color(0f, 0.05f, 0.1f, 0.8f);

    [Header("Glow")]
    [SerializeField] private Color glowColor = new Color(0f, 1f, 0.8f, 1f);
    [SerializeField] private float glowBorderWidth = 3f;
    [SerializeField] private float glowPadding = 8f;

    private Canvas _canvas;
    private RectTransform _canvasRect;

    // 스포트라이트 4개 패널 (어두운 오버레이)
    private RectTransform _spotTop;
    private RectTransform _spotBottom;
    private RectTransform _spotLeft;
    private RectTransform _spotRight;

    private readonly List<GameObject> _activeGlows = new List<GameObject>();

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
        _canvasRect = GetComponent<RectTransform>();
        CreateSpotlightPanels();
        HideSpotlight();
    }

    // ── 퍼블릭 API ────────────────────────────────────────────────

    /// <summary>스포트라이트(배경 어둡게) + 글로우 아웃라인을 단일 대상에 표시한다.</summary>
    public void ShowSpotlightWithGlow(RectTransform target)
    {
        if (target == null) return;
        PositionSpotlight(target);
        ClearGlows();
        CreateGlowFor(target);
    }

    /// <summary>글로우 아웃라인만 여러 대상에 표시한다. 스포트라이트는 적용하지 않는다.</summary>
    public void ShowGlowOnly(RectTransform[] targets)
    {
        HideSpotlight();
        ClearGlows();
        foreach (var t in targets)
        {
            if (t != null) CreateGlowFor(t);
        }
    }

    /// <summary>모든 하이라이트(스포트라이트 + 글로우)를 제거한다.</summary>
    public void HideAll()
    {
        HideSpotlight();
        ClearGlows();
    }

    // ── 스포트라이트 ──────────────────────────────────────────────

    private void CreateSpotlightPanels()
    {
        _spotTop    = CreateSolidPanel("SpotTop",    spotlightColor);
        _spotBottom = CreateSolidPanel("SpotBottom", spotlightColor);
        _spotLeft   = CreateSolidPanel("SpotLeft",   spotlightColor);
        _spotRight  = CreateSolidPanel("SpotRight",  spotlightColor);
    }

    private RectTransform CreateSolidPanel(string panelName, Color color)
    {
        var go = new GameObject(panelName, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(transform, false);
        go.GetComponent<Image>().color = color;
        return go.GetComponent<RectTransform>();
    }

    private void PositionSpotlight(RectTransform target)
    {
        var corners = new Vector3[4];
        target.GetWorldCorners(corners);
        Camera cam = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;

        // 대상의 bottomLeft, topRight → 스크린 좌표 → 정규화 비율(0~1)
        var bl = RectTransformUtility.WorldToScreenPoint(cam, corners[0]);
        var tr = RectTransformUtility.WorldToScreenPoint(cam, corners[2]);

        float xMinN = bl.x / Screen.width;
        float yMinN = bl.y / Screen.height;
        float xMaxN = tr.x / Screen.width;
        float yMaxN = tr.y / Screen.height;

        // 각 패널의 anchor를 정규화 비율로 설정 → offsetMin/Max = zero 로 anchor 영역을 꽉 채움
        SetAnchors(_spotTop,    new Vector2(0f,    yMaxN), new Vector2(1f,    1f   ));
        SetAnchors(_spotBottom, new Vector2(0f,    0f   ), new Vector2(1f,    yMinN));
        SetAnchors(_spotLeft,   new Vector2(0f,    yMinN), new Vector2(xMinN, yMaxN));
        SetAnchors(_spotRight,  new Vector2(xMaxN, yMinN), new Vector2(1f,    yMaxN));

        _spotTop.gameObject.SetActive(true);
        _spotBottom.gameObject.SetActive(true);
        _spotLeft.gameObject.SetActive(true);
        _spotRight.gameObject.SetActive(true);
    }

    private static void SetAnchors(RectTransform rt, Vector2 min, Vector2 max)
    {
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private void HideSpotlight()
    {
        if (_spotTop    != null) _spotTop.gameObject.SetActive(false);
        if (_spotBottom != null) _spotBottom.gameObject.SetActive(false);
        if (_spotLeft   != null) _spotLeft.gameObject.SetActive(false);
        if (_spotRight  != null) _spotRight.gameObject.SetActive(false);
    }

    // ── 글로우 오버레이 ───────────────────────────────────────────

    private void CreateGlowFor(RectTransform target)
    {
        Camera cam = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;

        var corners = new Vector3[4];
        target.GetWorldCorners(corners);

        // 대상 모서리를 캔버스 로컬 좌표로 변환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect,
            RectTransformUtility.WorldToScreenPoint(cam, corners[0]),
            cam, out Vector2 blLocal);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect,
            RectTransformUtility.WorldToScreenPoint(cam, corners[2]),
            cam, out Vector2 trLocal);

        Vector2 center = (blLocal + trLocal) * 0.5f;
        Vector2 size   = (trLocal - blLocal) + new Vector2(glowPadding * 2f, glowPadding * 2f);

        var root = new GameObject("GlowOverlay", typeof(RectTransform));
        root.transform.SetParent(transform, false);

        var rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin        = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax        = new Vector2(0.5f, 0.5f);
        rootRect.pivot            = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = center;
        rootRect.sizeDelta        = size;

        // 4개 테두리 스트립으로 사각형 아웃라인 구성
        AddBorderStrip(root.transform, "GlowTop",    new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f  ), new Vector2(0f,            glowBorderWidth));
        AddBorderStrip(root.transform, "GlowBottom", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f  ), new Vector2(0f,            glowBorderWidth));
        AddBorderStrip(root.transform, "GlowLeft",   new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f,   0.5f), new Vector2(glowBorderWidth, 0f           ));
        AddBorderStrip(root.transform, "GlowRight",  new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f,   0.5f), new Vector2(glowBorderWidth, 0f           ));

        var pulse = root.AddComponent<GlowPulse>();
        pulse.Initialize(glowColor, root.GetComponentsInChildren<Image>());

        _activeGlows.Add(root);
    }

    private static void AddBorderStrip(Transform parent, string stripName,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta)
    {
        var go = new GameObject(stripName, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin        = anchorMin;
        rt.anchorMax        = anchorMax;
        rt.pivot            = pivot;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta        = sizeDelta;
        go.GetComponent<Image>().raycastTarget = false;
    }

    private void ClearGlows()
    {
        foreach (var glow in _activeGlows)
        {
            if (glow != null) Destroy(glow);
        }
        _activeGlows.Clear();
    }
}
