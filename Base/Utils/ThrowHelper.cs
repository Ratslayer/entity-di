using System;

public static class ThrowHelper
{
    public static void IfNull(object obj, string message = "")
    {
        if (obj is null)
            throw new NullReferenceException(message);
    }
}
