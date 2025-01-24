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
	public static float Clamp01(this double d)
		=> (float)d.Clamp(0, 1);
	public static double Min0(this double d) => Math.Max(d, 0);
	public static int Min0(this int d) => Math.Max(d, 0);
	public static double Min(this double d, double v) => Math.Max(d, v);
	public static double Max0(this double d) => Math.Min(d, 0);
	public static double Max(this double d, double v) => Math.Min(d, v);
	public static bool IsBetween(this double d, double min, double max)
		=> d >= min && d <= max;
	public static bool IsZero(this float value) => value.Approximately(0);
	public static bool IsZero(this double value) => value.Approximately(0);

	public static bool LessThanOrEquals(this float v1, float v2)
		=> v1 <= v2 + _epsilon;
	public static bool LessThanOrEquals(this double v1, double v2)
		=> v1 <= v2 + _epsilon;

	public static float SignOrZero(float value, float threshold)
		=> Math.Abs(value) < threshold ? 0f : Math.Sign(value);
	public static float Min0(this float value) => Math.Max(value, 0);
	public static bool IsPositive(this float value)
		=> value >= _epsilon;
}
