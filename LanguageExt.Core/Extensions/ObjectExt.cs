﻿using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;
using LanguageExt.ClassInstances;

namespace LanguageExt;

#nullable disable   //TODO:  THIS IS TO RETAIN THE ORIGINAL CAPABILITY --- LEAVE IT DISABLED UNTIL YOU CAN PROVE IT WORKS OTHERWISE

public static class ObjectExt
{
    /// <summary>
    /// Returns true if the value is null, and does so without
    /// boxing of any value-types.  Value-types will always
    /// return false.
    /// </summary>
    /// <example>
    ///     int x = 0;
    ///     string y = null;
    ///     
    ///     x.IsNull()  // false
    ///     y.IsNull()  // true
    /// </example>
    /// <returns>True if the value is null, and does so without
    /// boxing of any value-types.  Value-types will always
    /// return false.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNull<A>(this A value) =>
        Check<A>.IsNull(value);
}

internal static class Check<A>
{
    static readonly bool IsReferenceType;
    static readonly bool IsNullable;

    static Check()
    {
        IsNullable      = Nullable.GetUnderlyingType(typeof(A)) != null;
        IsReferenceType = !typeof(A).GetTypeInfo().IsValueType;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsDefault(A value) =>
        EqDefault<A>.Equals(value, default);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsNull(A value) =>
        (IsReferenceType && ReferenceEquals(value, null)) || (IsNullable && value.Equals(default));
}
