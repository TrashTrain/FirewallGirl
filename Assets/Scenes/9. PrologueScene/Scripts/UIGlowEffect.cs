using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGlowEffect : MonoBehaviour
{
    [Header("Glow Settings")]
    public Image glowImage;         // 반짝일 이미지 컴포넌트
    public float speed = 2.0f;      // 반짝이는 속도
    public float minAlpha = 0.1f;   // 최소 투명도 (0~1)
    public float maxAlpha = 0.8f;   // 최대 투명도 (0~1)

    void Awake()
    {
        if (glowImage == null)
            glowImage = GetComponent<Image>();
    }

    void Update()
    {
        if (glowImage == null) return;

        // Mathf.PingPong을 이용해 시간이 지남에 따라 0과 1 사이를 왕복하게 만듭니다.
        float lerpFactor = Mathf.PingPong(Time.time * speed, 1f);
        
        // 투명도(Alpha) 값을 최소~최대 사이로 부드럽게 변경
        Color c = glowImage.color;
        c.a = Mathf.Lerp(minAlpha, maxAlpha, lerpFactor);
        glowImage.color = c;
    }
}
