using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 전투 중 일시적 알림 텍스트를 표시하는 컴포넌트.
/// Canvas 하위 비활성 GameObject에 추가하면 Awake에서 레이아웃을 자동 구성합니다.
/// BossUnrendered 등에서 FindObjectOfType으로 탐색 후 Show()를 호출합니다.
/// </summary>
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class BattleNoticeUI : MonoBehaviour
{
    private TextMeshProUGUI _text;
    private CanvasGroup _group;
    private Coroutine _showCoroutine;

    [Header("폰트")]
    [Tooltip("한글 TMP 폰트 에셋을 연결하세요.")]
    [SerializeField] private TMP_FontAsset _font;

    private void Awake()
    {
        _group = GetComponent<CanvasGroup>();
        SetupBackground();
        BuildText();
    }

    private void SetupBackground()
    {
        RectTransform rt = GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0f, 0.1f);
        rt.anchorMax        = new Vector2(1f, 0.35f);
        rt.offsetMin        = Vector2.zero;
        rt.offsetMax        = Vector2.zero;

        Image bg = GetComponent<Image>();
        if (bg == null) bg = gameObject.AddComponent<Image>();
        bg.color         = new Color(0f, 0f, 0f, 0.72f);
        bg.raycastTarget = false;
    }

    private void BuildText()
    {
        if (transform.childCount > 0) return;

        GameObject go = new GameObject("NoticeText", typeof(RectTransform));
        go.transform.SetParent(transform, false);

        _text = go.AddComponent<TextMeshProUGUI>();
        _text.fontSize        = 28f;
        _text.color           = Color.white;
        _text.alignment       = TextAlignmentOptions.Center;
        _text.enableWordWrapping = true;
        _text.raycastTarget   = false;

        if (_font != null) _text.font = _font;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin        = Vector2.zero;
        rt.anchorMax        = Vector2.one;
        rt.offsetMin        = new Vector2(40f, 10f);
        rt.offsetMax        = new Vector2(-40f, -10f);
    }

    // ─── 공개 API ────────────────────────────────────────────────

    public void Show(string message, float duration = 3f)
    {
        if (_text == null) BuildText();
        _text.text = message;

        if (_showCoroutine != null) StopCoroutine(_showCoroutine);
        _showCoroutine = StartCoroutine(CoShow(duration));
    }

    // ─── 내부 ────────────────────────────────────────────────────

    private IEnumerator CoShow(float duration)
    {
        _group.alpha = 0f;
        gameObject.SetActive(true);

        // 페이드 인
        yield return Fade(0f, 1f, 0.3f);

        // 표시 유지
        yield return new WaitForSeconds(duration);

        // 페이드 아웃
        yield return Fade(1f, 0f, 0.5f);

        gameObject.SetActive(false);
        _showCoroutine = null;
    }

    private IEnumerator Fade(float from, float to, float dur)
    {
        float elapsed = 0f;
        while (elapsed < dur)
        {
            elapsed += Time.unscaledDeltaTime;
            _group.alpha = Mathf.Lerp(from, to, elapsed / dur);
            yield return null;
        }
        _group.alpha = to;
    }
}
