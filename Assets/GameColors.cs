using UnityEngine;

public static class GameColors
{
    // 읽기 전용으로 미리 정의
    public static Color MainTheme = new Color32(255, 128, 0, 255); // 주황색

    // Hex 코드를 바로 쓰고 싶다면 이렇게 함수화
    public static Color FromHex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color color);
        return color;
    }
}