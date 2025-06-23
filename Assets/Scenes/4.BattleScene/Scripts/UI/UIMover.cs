using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMover : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 2f, 0);
    public Camera mainCamera;
    public Image fillImage;
    
    [Range(0f, 1f)]
    public float hpPercent = 1f;
    
    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;   
        }
    }
    
    void Update()
    {
        if (target != null)
        {
            Vector3 worldPos = target.position + offset;
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
            transform.position = screenPos;
        }

        fillImage.fillAmount = hpPercent;
    }
    
    // 외부에서 체력 조정용
    public void SetHP(float current, float max)
    {
        hpPercent = Mathf.Clamp01(current / max);
    }
}
