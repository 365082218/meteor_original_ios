using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using System;

public class Assert
{
    public static void NotNull(object argument, string message)
    {
        if (argument == null)
            throw new System.ArgumentNullException(string.Format(CultureInfo.InvariantCulture, "Argument '{0}' cannot be null", message));
    }

    public static void True(bool v)
    {
        IsTrue(v);
    }
    public static void IsTrue(bool v)
    {
        if (!v)
        {
            throw new Exception("not true");
        }
    }

    public static void AreEqual(object a, object b, string extra = "")
    {
        if (!object.Equals(a, b))
        {
            throw new Exception(a + " != " + b + ", " + extra);
        }
    }

    public static void AreNotEqual(object a, object b)
    {
        if (object.Equals(a, b))
        {
            throw new Exception(a + " == " + b);
        }
    }
}
