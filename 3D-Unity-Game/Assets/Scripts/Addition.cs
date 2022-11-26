using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Addition
{
    public static bool Contains<T>(this IEnumerable<T> lst, T val, Func<T, T, bool> check)
    {
        return check != default && lst.Any(item => check(item, val));
    }

    public static bool ContainsKey<T, J>(this Dictionary<T, J> dct, T key, Func<T, T, bool> check)
    {
        return check != default && dct.Keys.Any(item => check(item, key));
    }
}
