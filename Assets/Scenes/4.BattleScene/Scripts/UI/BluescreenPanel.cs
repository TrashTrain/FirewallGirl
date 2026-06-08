using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 4스테이지 보스 언렌더드.RAW 전용 블루스크린 패널.
/// 씬의 Canvas 하위 빈 GameObject에 이 컴포넌트를 추가하면 자동으로 레이아웃을 구성합니다.
/// BossUnrendered가 Start()에서 FindObjectOfType으로 이 컴포넌트를 탐색합니다.
/// Inspector의 Font 필드에 한글 TMP 폰트 에셋(NanumGothic SDF 등)을 연결하세요.
/// </summary>
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public class BluescreenPanel : MonoBehaviour
{
    [Header("폰트")]
    [Tooltip("한글을 지원하는 TMP 폰트 에셋을 연결하세요.")]
    [SerializeField] private TMP_FontAsset _font;

    private static readonly Color BgColor   = new Color(0.043f, 0.467f, 0.820f, 1f); // #0B77D1
    private static readonly Color TextWhite = Color.white;

    private void Awake()
    {
        SetupBackground();
        BuildLayout();
    }

    // ─── 배경 ────────────────────────────────────────────────────────

    private void SetupBackground()
    {
        // 화면 전체를 덮도록 RectTransform 설정
        RectTransform rt = GetComponent<RectTransform>();
        rt.anchorMin        = Vector2.zero;
        rt.anchorMax        = Vector2.one;
        rt.offsetMin        = Vector2.zero;
        rt.offsetMax        = Vector2.zero;

        // 파란색 배경 — raycastTarget=true 로 카드/버튼 입력 차단
        Image bg = GetComponent<Image>();
        bg.color          = BgColor;
        bg.raycastTarget  = true;
    }

    // ─── 텍스트 레이아웃 ─────────────────────────────────────────────

    private void BuildLayout()
    {
        // 이미 자식이 있으면 중복 생성 방지
        if (transform.childCount > 0) return;

        // ──────────────────────────────────────────────
        // :( 이모지  (화면 중앙보다 위)
        // ──────────────────────────────────────────────
        MakeText(
            name:      "Emoji",
            content:   ":(",
            fontSize:  120f,
            style:     FontStyles.Bold,
            pos:       new Vector2(0f, 130f),
            size:      new Vector2(400f, 160f)
        );

        // ──────────────────────────────────────────────
        // 메인 메시지
        // ──────────────────────────────────────────────
        MakeText(
            name:      "MainMsg",
            content:   "PC에 문제가 발생했습니다.",
            fontSize:  38f,
            style:     FontStyles.Bold,
            pos:       new Vector2(0f, 40f),
            size:      new Vector2(900f, 60f)
        );

        // ──────────────────────────────────────────────
        // 서브 메시지
        // ──────────────────────────────────────────────
        MakeText(
            name:      "SubMsg",
            content:   "그래픽 렌더링 오류가 감지되었습니다.\n" +
                       "오류 정보를 수집하는 중입니다. 잠시 후 자동으로 재개됩니다.",
            fontSize:  24f,
            style:     FontStyles.Normal,
            pos:       new Vector2(0f, -55f),
            size:      new Vector2(900f, 90f)
        );

        // ──────────────────────────────────────────────
        // 오류 코드
        // ──────────────────────────────────────────────
        MakeText(
            name:      "ErrorCode",
            content:   "오류 코드: UNRENDERED_GRAPHIC_OVERFLOW",
            fontSize:  20f,
            style:     FontStyles.Normal,
            pos:       new Vector2(0f, -155f),
            size:      new Vector2(900f, 36f)
        );

        // ──────────────────────────────────────────────
        // 하단 서명 (작은 글씨)
        // ──────────────────────────────────────────────
        MakeText(
            name:      "Footer",
            content:   "언렌더드.RAW",
            fontSize:  16f,
            style:     FontStyles.Normal,
            pos:       new Vector2(0f, -210f),
            size:      new Vector2(900f, 30f),
            alpha:     0.65f
        );
    }

    // ─── 공개 API ────────────────────────────────────────────────────

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);

    // ─── 헬퍼 ────────────────────────────────────────────────────────

    private void MakeText(string name, string content, float fontSize,
        FontStyles style, Vector2 pos, Vector2 size, float alpha = 1f)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(transform, false);

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text          = content;
        tmp.fontSize      = fontSize;
        tmp.fontStyle     = style;
        tmp.color         = new Color(TextWhite.r, TextWhite.g, TextWhite.b, alpha);
        tmp.alignment     = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        tmp.enableWordWrapping = true;

        if (_font != null)
            tmp.font = _font;

        RectTransform rt    = go.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;
    }
}
