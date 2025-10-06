using UnityEngine;

/// <summary>
/// Pomoćna klasa: konverzija HSV -> RGB (Unity već nudi Color.HSVToRGB,
/// ali ovde je wrap sa clamp i eksplicitnim parametrima, radi jasnoće).
/// </summary>
public static class HSVUtil
{
    /// <param name="h">Hue u [0,1] (0->0°, 1->360°)</param>
    /// <param name="s">Saturation u [0,1]</param>
    /// <param name="v">Value u [0,1]</param>
    /// <returns>UnityEngine.Color</returns>
    public static Color HSV01(float h, float s, float v)
    {
        h = Mathf.Repeat(h, 1f);
        s = Mathf.Clamp01(s);
        v = Mathf.Clamp01(v);
        return Color.HSVToRGB(h, s, v);
    }
}
