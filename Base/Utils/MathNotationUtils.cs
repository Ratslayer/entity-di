using System;

public static class MathNotationUtils
{
	public static string Nicify(this double value, bool forceSign = false)
	{
		var name = ToScientificNotation(value);
		if (forceSign && value > 0)
			name = $"+{name}";
		return name;
	}
	public static string ToScientificNotation(double value)
	{
		var abs = Math.Abs(value);
		return abs > 1000 ? string.Format($"{value:#.##E+00}")
		: abs < 0.001 ? "0"
		: Math.Abs(Math.Round(abs, 2) - Math.Round(abs, 0)) < 0.005
		? string.Format($"{value:F0}") : string.Format($"{value:F2}");
	}

}