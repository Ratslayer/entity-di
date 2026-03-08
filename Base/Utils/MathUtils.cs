using System;
public static class MathUtils
{
    const float _epsilon = 0.00001f;
    public static bool Approximately(this float a, float b) => Math.Abs(a - b) <= _epsilon;
    public static bool Approximately(this double a, double b) => Math.Abs(a - b) <= double.Epsilon;
    public static float Clamp(this float f, float min, float max)
        => Math.Clamp(f, min, max);
    public static double Clamp(this double d, double min, double max)
        => Math.Clamp(d, min, max);
    public static int Clamp(this int n, int min, int max)
        => Math.Clamp(n, min, max);
    public static double Clamp01(this double d)
        => d.Clamp(0, 1);
    public static float Clamp01(this float f)
        => f.Clamp(0, 1);
    public static double Min0(this double d) => Math.Max(d, 0);
    public static int Min0(this int d) => Math.Max(d, 0);
    public static double Min(this double d, double v) => Math.Max(d, v);
    public static double Max0(this double d) => Math.Min(d, 0);
    public static double Max(this double d, double v) => Math.Min(d, v);

    public static double MapToRange(this double d, double min, double max)
        => min + (max - min) * d;
    public static double ClampToRange(this double d, double min, double max)
        => d.Clamp01().MapToRange(min, max);
    public static float MapToRange(this float f, float min, float max)
        => min + (max - min) * f;
    public static float ClampToRange(this float f, float min, float max)
        => f.Clamp01().MapToRange(min, max);
    public static float ClampAngle(this float f, float min = -180, float max = 180)
    {
        var numFullCircles = Math.Floor(f / 360);
        var angle = f - (float)numFullCircles * 360;
        if (angle > 180)
            angle -= 360;
        return angle.Clamp(min, max);
    }
    public static bool IsBetween(this double d, double min, double max)
        => d >= min && d <= max;
    public static bool IsZero(this float value) => value.Approximately(0);
    public static bool IsZero(this double value) => value.Approximately(0);
    public static bool NotZero(this double value) => !value.IsZero();
    public static bool NotZero(this float value) => !value.IsZero();
    public static bool IsValid(this double value) => value is not double.NaN;
    public static bool LessThanOrEquals(this float v1, float v2)
        => v1 <= v2 + _epsilon;
    public static bool LessThanOrEquals(this double v1, double v2)
        => v1 <= v2 + _epsilon;
    public static bool LessThan(this float v1, float v2)
        => v1 <= v2 - _epsilon;
    public static bool LessThan(this double v1, double v2)
        => v1 <= v2 - _epsilon;
    public static bool GreaterThanOrEquals(this float v1, float v2)
        => v1 >= v2 - _epsilon;
    public static bool GreaterThanOrEquals(this double v1, double v2)
        => v1 >= v2 - _epsilon;
    public static bool GreaterThan(this float v1, float v2)
        => v1 >= v2 + _epsilon;
    public static bool GreaterThan(this double v1, double v2)
        => v1 >= v2 + _epsilon;

    public static float SignOrZero(float value, float threshold)
        => Math.Abs(value) < threshold ? 0f : Math.Sign(value);
    public static float Min0(this float value) => Math.Max(value, 0);
    public static bool IsPositive(this double value)
        => value.IsValid() && value >= _epsilon;

    public static bool IsOdd(this int n) => n % 2 == 1;
    public static bool IsEven(this int n) => n % 2 == 0;
}
