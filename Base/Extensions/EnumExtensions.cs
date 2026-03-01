using System;

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
