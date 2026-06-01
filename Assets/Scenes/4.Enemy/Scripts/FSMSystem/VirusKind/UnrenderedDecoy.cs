using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 언렌더드.RAW 1~3페이즈 분신 — '그래픽-혼란' 디코이
/// 히트박스 1기 + 비히트박스 4기로 구성.
/// 히트박스: 황금/노란색 glow (SpriteRenderer.color = 노란색)
/// 비히트박스: 기본 색상
/// 매 보스 턴 히트박스 이동. 턴 종료 시 CleanupAllDecoys()로 전체 파괴.
/// </summary>
public class UnrenderedDecoy : MonoBehaviour
{
    // 히트박스 여부
    public bool IsHitbox { get; private set; }

    private BossUnrendered _boss;
    private SpriteRenderer _sr;

    private static readonly Color HitboxColor    = new Color(1f, 0.85f, 0f, 1f); // 황금/노란색 glow
    private static readonly Color NonHitboxColor = Color.white;

    public void Setup(BossUnrendered boss, bool isHitbox)
    {
        _boss    = boss;
        IsHitbox = isHitbox;
        _sr      = GetComponent<SpriteRenderer>();

        if (_sr != null)
            _sr.color = isHitbox ? HitboxColor : NonHitboxColor;

        // OnMouseDown 작동에 Collider 필요 — 없으면 자동 추가
        if (GetComponent<Collider2D>() == null && GetComponent<Collider>() == null)
        {
            if (_sr != null)
            {
                var col = gameObject.AddComponent<BoxCollider2D>();
                col.size = _sr.bounds.size;
            }
            else
            {
                gameObject.AddComponent<BoxCollider2D>();
            }
        }
    }

    /// <summary>비히트박스로 전환 (히트박스 이동 시 기존 히트박스 호출)</summary>
    public void SetAsNonHitbox()
    {
        IsHitbox = false;
        if (_sr != null) _sr.color = NonHitboxColor;
    }

    /// <summary>히트박스로 전환</summary>
    public void SetAsHitbox()
    {
        IsHitbox = true;
        if (_sr != null) _sr.color = HitboxColor;
    }

    private void OnMouseDown()
    {
        if (_boss != null)
            _boss.OnDecoyClicked(this, IsHitbox);
    }
}
