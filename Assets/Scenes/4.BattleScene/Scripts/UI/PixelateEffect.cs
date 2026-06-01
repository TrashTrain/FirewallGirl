using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 카드의 모든 뷰(미니·호버·상세)에 모자이크 효과를 적용/해제합니다.
/// - Image  : Custom/PixelateUI 셰이더로 블록 픽셀화
/// - TMP 텍스트 : 블록 문자(█ ▓ ▒ ░)로 치환 (SDF 특성상 셰이더 픽셀화 불가)
/// Remove() 또는 OnDestroy() 시 원본 material·텍스트 완전 복원됩니다.
/// </summary>
public class PixelateEffect : MonoBehaviour
{
    private readonly List<Image>               _imgTargets   = new List<Image>();
    private readonly List<Material>            _imgOriginals = new List<Material>();
    private readonly Dictionary<TMP_Text, string> _textOriginals = new Dictionary<TMP_Text, string>();

    private Material _pixelateMat;

    private static readonly char[] BlockChars = { '█', '▓', '▒', '░' };

    public bool IsActive { get; private set; }

    // ── 공개 API ──────────────────────────────────────────────────

    public void Apply(float blockSize = 8f)
    {
        if (IsActive) return;

        var shader = Shader.Find("Custom/PixelateUI");
        if (shader == null)
        {
            Debug.LogWarning("[PixelateEffect] Custom/PixelateUI 셰이더를 찾을 수 없습니다.");
            return;
        }

        _pixelateMat = new Material(shader);
        _pixelateMat.SetFloat("_BlockSize", blockSize);

        // 모든 Image에 픽셀화 셰이더 적용 (HoverView·DetailView 포함)
        _imgTargets.Clear();
        _imgOriginals.Clear();
        foreach (Image img in GetComponentsInChildren<Image>(true))
        {
            _imgTargets.Add(img);
            _imgOriginals.Add(img.material);
            img.material = _pixelateMat;
        }

        // 모든 TMP_Text를 블록 문자로 치환
        _textOriginals.Clear();
        foreach (TMP_Text tmp in GetComponentsInChildren<TMP_Text>(true))
        {
            _textOriginals[tmp] = tmp.text;
            tmp.text = ScrambleToBlocks(tmp.text);
        }

        IsActive = true;
    }

    public void Remove()
    {
        if (!IsActive) return;

        for (int i = 0; i < _imgTargets.Count; i++)
            if (_imgTargets[i] != null)
                _imgTargets[i].material = _imgOriginals[i];
        _imgTargets.Clear();
        _imgOriginals.Clear();

        foreach (var kv in _textOriginals)
            if (kv.Key != null) kv.Key.text = kv.Value;
        _textOriginals.Clear();

        if (_pixelateMat != null) { Destroy(_pixelateMat); _pixelateMat = null; }
        IsActive = false;
    }

    // ── 내부 ──────────────────────────────────────────────────────

    private void OnDestroy() => Remove();

    private static string ScrambleToBlocks(string input)
    {
        var sb = new StringBuilder(input.Length);
        foreach (char c in input)
        {
            if (c == '\n' || c == ' ')
                sb.Append(c);
            else
                sb.Append(BlockChars[Random.Range(0, BlockChars.Length)]);
        }
        return sb.ToString();
    }
}
