using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public interface IPriority
{
    int Priority { get; }
}
//public static class EnumUtils
//{
//	public static T Min<T>(T e1, T e2)
//		where T : struct, Enum
//	{
//		dynamic v1 = e1;
//		dynamic v2 = e2;
//		dynamic result = Math.Min((int)v1, (int)v2);
//		return (T)result;
//	}
//	public static T Max<T>(T e1, T e2)
//		where T : struct, Enum
//	{
//		dynamic v1 = e1;
//		dynamic v2 = e2;
//		dynamic result = Math.Max((int)v1, (int)v2);
//		return (T)result;
//	}
//	public static bool LessOrEqualThan<T>(this T e1, T e2)
//		where T : struct, Enum
//	{
//		dynamic v1 = e1;
//		dynamic v2 = e2;
//		return (int)v1 <= (int)v2;
//	}
//	public static bool MoreOrEqualThan<T>(this T e1, T e2)
//		where T : struct, Enum
//	{
//		dynamic v1 = e1;
//		dynamic v2 = e2;
//		return (int)v1 >= (int)v2;
//	}
//	public static bool IsBetween<T>(this T e, T l, T r)
//		where T : struct, Enum
//	{
//		dynamic v = e;
//		dynamic vl= l;
//		dynamic vr = r;
//		return (int)v >= (int)vl && (int)v <= (int)vr;
//	}
//}
public static class ListUtils
{
    public static T[] Resize<T>(this T[] array, int newSize)
    {
        var result = new T[newSize];
        var numElements = Mathf.Min(array is not null ? array.Length : 0, newSize);
        for (var i = 0; i < numElements; i++)
            result[i] = array[i];
        return result;
    }
    public static void UpdateAllElements<T>(this List<T> list, Func<T, T> update)
    {
        foreach (var i in list.Count)
            list[i] = update(list[i]);
    }
    public static IEnumerable<T> Inverse<T>(this List<T> list)
    {
        for (var i = list.Count - 1; i >= 0; i--)
            yield return list[i];
    }
    public static void Swap<T>(this List<T> list, int i1, int i2)
    {
        if (list.IsValidIndex(i1) && list.IsValidIndex(i2))
            (list[i1], list[i2]) = (list[i2], list[i1]);
    }
    public static T RemoveLast<T>(this List<T> list)
    {
        var result = list[^1];
        list.RemoveAt(list.Count - 1);
        return result;
    }
    public static bool TryRemoveLast<T>(this List<T> list, out T lastElement)
    {
        if (list.Count == 0)
        {
            lastElement = default;
            return false;
        }
        lastElement = list.RemoveLast();
        return true;
    }
    public static void RemoveAll<T>(this List<T> list, T element)
    {
        foreach (var i in -list.Count)
            if (EqualityComparer<T>.Default.Equals(element, list[i]))
                list.RemoveAt(i);
    }
    public static void SetRange<T>(this List<T> list, IEnumerable<T> other)
    {
        if (list == other)
            return;

        list.Clear();
        if (other is not null)
            list.AddRange(other);
    }
    public static void Resize<T>(
        this List<T> list,
        int newSize,
        Func<int, T> createNew,
        Action<T> processOld)
    {
        if (newSize < list.Count)
            for (var i = list.Count - 1; i >= newSize; i--)
            {
                processOld(list[i]);
                list.RemoveAt(i);
            }
        else if (newSize > list.Count)
        {
            for (var i = list.Count; i < newSize; i++)
                list.Add(createNew(i));
        }
    }
    public static void SortByPriority<T>(this List<T> list)
    {
        list.Sort(Compare);
        static int Compare(T left, T right)
        {
            var l = left is IPriority lp ? lp.Priority : 0;
            var r = right is IPriority rp ? rp.Priority : 0;
            return l.CompareTo(r);
        }
    }
    public static void SortByPriorityTyped<T>(this List<T> list)
        where T : IPriority
    {
        list.Sort(Compare);
        static int Compare(T left, T right)
            => left.Priority.CompareTo(right.Priority);
    }
    public static T GetLast<T>(this List<T> list, T defaultValue)
        => list.Count > 0 ? list[^1] : defaultValue;
    public static void RemoveRange<T>(this List<T> list, IEnumerable<T> other)
    {
        foreach (var item in other)
            list.Remove(item);
    }
    public static T AtIndexOrDefault<T>(this IReadOnlyList<T> list, int index)
    {
        if (list.Count <= index)
            return default;
        return list[index];
    }
    public static bool TryGetValue<T>(
        this IEnumerable<T> list, Predicate<T> predicate, out T element)
    {
        foreach (var item in list)
            if (predicate(item))
            {
                element = item;
                return true;
            }
        element = default;
        return false;
    }
    public static bool Contains<T>(this IEnumerable<T> list, Predicate<T> predicate)
        => list.TryGetValue(predicate, out _);
    public static bool HasOfType<T>(this IEnumerable list, out T element)
    {
        foreach (var e in list)
            if (e is T te)
            {
                element = te;
                return true;
            }
        element = default;
        return false;
    }
    public static bool IsNullOrEmpty<T>(this IEnumerable<T> list)
        => list is null || list.Count() == 0;
    public static bool ContainsAll<T1, T2>(this IEnumerable<T1> outer, IEnumerable<T2> inner)
        where T2 : T1
    {
        if (inner.IsNullOrEmpty())
            return true;
        if (outer.IsNullOrEmpty())
            return false;
        foreach (var item in inner)
            if (!outer.Contains(item))
                return false;
        return true;
    }
    public static List<T> UniqueUnion<T>(this List<T> list1, List<T> list2)
    {
        var result = new List<T>();
        if (list1 is not null)
            result.AddUniqueRange(list1);
        if (list2 is not null)
            result.AddUniqueRange(list2);
        return result;
    }
    public static bool AddUnique<T>(this IList<T> list, T element)
    {
        if (!list.Contains(element))
        {
            list.Add(element);
            return true;
        }
        return false;
    }
    public static void AddUniqueRange<T>(this List<T> list, IEnumerable<T> list2)
    {
        foreach (var item in list2)
            list.AddUnique(item);
    }
    public static bool IsValidIndex(this IList list, int index)
        => index >= 0 && index < list.Count;
    public static bool HasIndex<T>(this IList<T> list, int index, out T element)
    {
        var valid = index >= 0 && index < list.Count;
        element = valid ? list[index] : default;
        return valid;
    }
    public static int ClampIndex<T>(this IList<T> list, int index)
        => Mathf.Clamp(index, 0, list.Count - 1);
    public static TElement MinElement<TElement>(
        this IList<TElement> list,
        Func<TElement, float> comparer)
        => MinElementWithValue(list, comparer)._element;

    public static (TElement _element, float minValue) MinElementWithValue<TElement>(
        this IList<TElement> list,
        Func<TElement, float> comparer)
    {
        switch (list.Count)
        {
            case 0:
                return default;
            case 1:
                var minValue = comparer(list[0]);
                return (list[0], minValue);
            default:
                minValue = comparer(list[0]);
                var element = list[0];
                for (var i = 1; i < list.Count; i++)
                {
                    var newValue = comparer(list[i]);
                    if (newValue < minValue)
                    {
                        minValue = newValue;
                        element = list[i];
                    }
                }
                return (element, minValue);
        }
    }
    public static bool TryFindIndex<T>(
        this IList<T> list,
        Predicate<T> predicate,
        out int index)
    {
        for (var i = 0; i < list.Count; i++)
        {
            if (predicate(list[i]))
            {
                index = i;
                return true;
            }
        }
        index = 0;
        return false;
    }
    public static bool Remove<T>(this List<T> list, Predicate<T> predicate)
    {
        for (var i = 0; i < list.Count; i++)
            if (predicate(list[i]))
            {
                list.RemoveAt(i);
                return true;
            }
        return false;
    }
    public static T Peek<T>(this List<T> list)
        => list.Count > 0 ? list[^1] : default;
}
