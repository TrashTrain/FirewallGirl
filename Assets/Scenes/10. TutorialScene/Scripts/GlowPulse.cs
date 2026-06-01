using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 글로우 오버레이 Image들의 알파값을 주기적으로 변화시켜 펄싱 효과를 구현한다.
/// TutorialHighlightController에서 절차적으로 생성되는 GlowOverlay 루트에 부착된다.
/// AddComponent 직후 반드시 Initialize()를 호출해야 한다.
/// </summary>
public class GlowPulse : MonoBehaviour
{
    private Image[] _images;
    private Color _baseColor;
    private readonly WaitForSeconds _wait = new WaitForSeconds(0.05f);

    /// <summary>색상과 대상 Image 배열을 설정하고 펄스 코루틴을 시작한다.</summary>
    public void Initialize(Color color, Image[] images)
    {
        _baseColor = color;
        _images = images;
        foreach (var img in _images)
            img.color = _baseColor;
        StartCoroutine(Pulse());
    }

    private IEnumerator Pulse()
    {
        float t = 0f;
        bool increasing = true;
        while (true)
        {
            t = increasing ? t + 0.1f : t - 0.1f;
            if (t >= 1f) { t = 1f; increasing = false; }
            if (t <= 0f) { t = 0f; increasing = true;  }

            float alpha = Mathf.Lerp(0.4f, 1.0f, t);
            Color c = new Color(_baseColor.r, _baseColor.g, _baseColor.b, alpha);
            foreach (var img in _images)
            {
                if (img != null) img.color = c;
            }
            yield return _wait;
        }
    }
}
