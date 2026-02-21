using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
public static class EnumExtensions
{
    public static bool FlagsIntersect<T>(this T? t1, T? t2)
        where T : struct, Enum
    {
        if (t1 is null)
            return false;
        if (t2 is null)
            return false;

        var i1 = Convert.ToInt32(t1);
        var i2 = Convert.ToInt32(t2);
        var intersection = i1 & i2;
        return intersection != 0;
    }
}
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
        if (string.IsNullOrWhiteSpace(str))
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
    public static string[] SplitByCapitalWords(this string s)
        => string.IsNullOrWhiteSpace(s)
        ? Array.Empty<string>()
        : Regex.Replace(s, "([A-Z\\d])", " $1").Trim().Split();
}
