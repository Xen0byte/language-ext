﻿using LanguageExt.Attributes;
using System.Diagnostics.Contracts;

namespace LanguageExt.Traits;

[Trait("Ord*")]
public interface Ord<in A> : Eq<A> 
{
    /// <summary>
    /// Compare two values
    /// </summary>
    /// <param name="x">Left hand side of the compare operation</param>
    /// <param name="y">Right hand side of the compare operation</param>
    /// <returns>
    /// if x greater than y : 1
    /// 
    /// if x less than y    : -1
    /// 
    /// if x equals y       : 0
    /// </returns>
    [Pure]
    public static abstract int Compare(A x, A y);
}
