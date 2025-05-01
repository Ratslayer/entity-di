using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

public static class StringExtensions
{
	static readonly StringBuilder _builder = new();
	public static string SafeToString<T>(T value)
	{
		if (value == null)
			return "NULL";
		return value.ToString();
	}
	public static bool IsInvalid(this string str) => string.IsNullOrWhiteSpace(str);
	public static string DefaultTo(this string str, string defaultString)
	{
		if(string.IsNullOrWhiteSpace(str))
			return defaultString;
		return str;
	}
	public static bool IsValid(this string str) => !IsInvalid(str);
	public static string Join<T>(string separator, IEnumerable<T> collection, Func<T, string> strGetter)
	{
		_builder.Clear();
		foreach (var item in collection)
		{
			_builder.Append(strGetter(item));
			_builder.Append(separator);
		}
		_builder.Trim(separator);
		return _builder.ToString();
	}
	public static void Trim(this StringBuilder builder, int numChars)
	{
		if (builder.Length < numChars)
			builder.Clear();
		builder.Remove(builder.Length - numChars, numChars);
	}
	public static void Trim(this StringBuilder builder, string str)
		=> builder.Trim(str.Length);
	public static string MatchWithRegex(this string s, string regex)
	{
		var match = Regex.Match(s, regex);
		if (!match.Success)
			return "";
		return match.Groups[^1].Value;
	}
}
