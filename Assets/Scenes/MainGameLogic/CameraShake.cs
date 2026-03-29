using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    // 어디서든 쉽게 접근할 수 있도록 싱글톤 패턴 적용
    public static CameraShake Instance;

    private Vector3 _originalPos;
    private Coroutine _shakeCo;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this.gameObject);
    }

    private void Start()
    {
        // 평상시 카메라의 위치를 저장해둡니다. (매우 중요)
        _originalPos = transform.localPosition;
    }

    /// <summary>
    /// 화면을 흔듭니다.
    /// </summary>
    /// <param name="duration">흔들릴 시간(초)</param>
    /// <param name="magnitude">흔들림의 강도</param>
    public void Shake(float duration, float magnitude)
    {
        // 이미 흔들리고 있다면 기존 코루틴을 중지하고 새 것으로 시작 (중첩 방지)
        if (_shakeCo != null) StopCoroutine(_shakeCo);
        _shakeCo = StartCoroutine(CoShake(duration, magnitude));
    }

    private IEnumerator CoShake(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // 시간 경과에 따라 0f~1f로 변하는 t
            float t = elapsed / duration;

            // 흔들림 강도를 서서히 줄여서 자연스럽게 멈추도록 (Optional)
            // linearFadeout을 true로 하면 흔들림이 끝나갈수록 약해집니다.
            bool linearFadeout = true;
            float currentMagnitude = linearFadeout ? Mathf.Lerp(magnitude, 0f, t) : magnitude;

            // -1~1 사이의 랜덤한 벡터에 강도를 곱해서 랜덤 좌표 생성
            Vector3 randomOffset = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                0f // 2D 게임이면 z축은 흔들지 않음
            );

            // 카메라의 localPosition을 랜덤 좌표로 순간이동
            transform.localPosition = _originalPos + (randomOffset * currentMagnitude);

            // 시간 업데이트
            elapsed += Time.deltaTime;

            // 다음 프레임까지 대기
            yield return null;
        }

        // 흔들림이 끝나면 반드시 원래 위치로 원복
        transform.localPosition = _originalPos;
        _shakeCo = null;
    }
}