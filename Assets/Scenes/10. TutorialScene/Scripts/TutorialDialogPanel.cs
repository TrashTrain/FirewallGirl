using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum PanelCorner { TopLeft, TopRight, BottomLeft, BottomRight }

[System.Serializable]
public struct DialogLine
{
    public string speakerName;
    public string text;
    public Sprite characterSprite;
}

/// <summary>
/// 방화벽 소녀(아이) 캐릭터 이미지 + 대사창 표시.
/// 마우스 클릭으로 대사를 넘기거나 타이핑을 즉시 완성.
/// Canvas 최상단에 배치하여 InputBlocker 위에 위치시킬 것.
/// </summary>
public class TutorialDialogPanel : MonoBehaviour
{
    [SerializeField] private Image characterImage;
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private GameObject dialogBox;

    private const float TypeSpeed = 0.035f;
    private const string DefaultSpeakerName = "아이 (A.I.)";

    private DialogLine[] _currentLines;
    private int _currentIndex;
    private Action _onComplete;
    private bool _isTyping;
    private bool _skipTyping;
    private Coroutine _typeCoroutine;

    private readonly WaitForSeconds _typeWait = new WaitForSeconds(TypeSpeed);

    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        if (characterImage != null) characterImage.gameObject.SetActive(false);
        if (dialogBox != null) dialogBox.SetActive(false);
        if (dialogText != null) dialogText.gameObject.SetActive(false);
    }

    /// <summary>
    /// 패널의 지정한 모서리(panelCorner)를 target의 위치에 맞춤.
    /// targetCorner가 null이면 target.position(피벗 중심)을 사용하고,
    /// 값이 있으면 target의 해당 모서리 좌표를 사용한다.
    /// </summary>
    public void MoveToTarget(RectTransform target, PanelCorner panelCorner = PanelCorner.TopLeft, PanelCorner? targetCorner = null)
    {
        if (_rectTransform == null || target == null) return;

        Vector3[] pCorners = new Vector3[4];
        _rectTransform.GetWorldCorners(pCorners);
        // GetWorldCorners 순서: [0]=BottomLeft, [1]=TopLeft, [2]=TopRight, [3]=BottomRight
        Vector3 myCorner = panelCorner switch
        {
            PanelCorner.BottomLeft  => pCorners[0],
            PanelCorner.TopLeft     => pCorners[1],
            PanelCorner.TopRight    => pCorners[2],
            PanelCorner.BottomRight => pCorners[3],
            _                       => pCorners[1],
        };

        Vector3 targetPos;
        if (targetCorner.HasValue)
        {
            Vector3[] tCorners = new Vector3[4];
            target.GetWorldCorners(tCorners);
            targetPos = targetCorner.Value switch
            {
                PanelCorner.BottomLeft  => tCorners[0],
                PanelCorner.TopLeft     => tCorners[1],
                PanelCorner.TopRight    => tCorners[2],
                PanelCorner.BottomRight => tCorners[3],
                _                       => tCorners[1],
            };
        }
        else
        {
            targetPos = target.position;
        }

        _rectTransform.position += targetPos - myCorner;
    }

    /// <summary>패널을 절대 anchoredPosition으로 이동.</summary>
    public void SetPosition(Vector2 anchoredPosition)
    {
        if (_rectTransform == null) return;
        _rectTransform.anchoredPosition = anchoredPosition;
    }

    private void Update()
    {
        if (dialogBox == null || !dialogBox.activeSelf) return;
        if (!Input.GetMouseButtonDown(0)) return;

        if (_isTyping)
            _skipTyping = true;
        else
            AdvanceLine();
    }

    /// <summary>순차적으로 대사를 표시하고, 모두 완료되면 onComplete 콜백 호출.</summary>
    public void ShowDialogs(DialogLine[] lines, Action onComplete)
    {
        _currentLines = lines;
        _currentIndex = 0;
        _onComplete = onComplete;
        if (characterImage != null) characterImage.gameObject.SetActive(true);
        dialogBox.SetActive(true);
        dialogText.gameObject.SetActive(true);
        ShowCurrentLine();
    }

    public void Hide()
    {
        if (characterImage != null) characterImage.gameObject.SetActive(false);
        if (dialogBox != null) dialogBox.SetActive(false);
        if (dialogText != null) dialogText.gameObject.SetActive(false);
        if (_typeCoroutine != null) StopCoroutine(_typeCoroutine);
    }

    private void ShowCurrentLine()
    {
        if (_currentIndex >= _currentLines.Length)
        {
            if (characterImage != null) characterImage.gameObject.SetActive(false);
            dialogBox.SetActive(false);
            dialogText.gameObject.SetActive(false);
            _onComplete?.Invoke();
            return;
        }

        DialogLine line = _currentLines[_currentIndex];

        if (characterImage != null && line.characterSprite != null)
            characterImage.sprite = line.characterSprite;

        if (_typeCoroutine != null) StopCoroutine(_typeCoroutine);
        _typeCoroutine = StartCoroutine(TypeText(line.text));
    }

    private IEnumerator TypeText(string text)
    {
        _isTyping = true;
        _skipTyping = false;
        dialogText.text = string.Empty;

        foreach (char c in text)
        {
            if (_skipTyping)
            {
                dialogText.text = text;
                break;
            }
            dialogText.text += c;
            yield return _typeWait;
        }

        _isTyping = false;
    }

    private void AdvanceLine()
    {
        _currentIndex++;
        ShowCurrentLine();
    }
}
