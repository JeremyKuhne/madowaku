// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using Windows.Win32.System.Variant;

namespace Windows.Win32.System.Com;

public class ComSafeArrayScopeTests
{
    [Fact]
    public unsafe void Constructor_NullSafearrayPointer_IsNullTrue()
    {
        using ComSafeArrayScope<IUnknown> scope = new((SAFEARRAY*)null);
        Assert.True(scope.IsNull);
    }

    [Fact]
    public unsafe void Constructor_WrongVarType_Throws()
    {
        using SafeArrayScope<int> source = new(1);
        SAFEARRAY* ptr = source.Value;
        Assert.Throws<ArgumentException>(() => new ComSafeArrayScope<IUnknown>(ptr));
    }

    [Fact]
    public unsafe void Constructor_VT_UNKNOWN_Array_Wraps()
    {
        // Build a 1-element VT_UNKNOWN SAFEARRAY manually and wrap it.
        SAFEARRAYBOUND bound = new() { cElements = 1, lLbound = 0 };
        SAFEARRAY* array = PInvokeMadowaku.SafeArrayCreate(VARENUM.VT_UNKNOWN, 1, &bound);
        Assert.False(array is null);

        using ComSafeArrayScope<IUnknown> scope = new(array);
        Assert.False(scope.IsNull);
        Assert.Equal(1, scope.Length);
        Assert.False(scope.IsEmpty);
    }

    [Fact]
    public unsafe void Length_NonEmptyArray_ReturnsCElements()
    {
        SAFEARRAYBOUND bound = new() { cElements = 3, lLbound = 0 };
        SAFEARRAY* array = PInvokeMadowaku.SafeArrayCreate(VARENUM.VT_UNKNOWN, 1, &bound);
        using ComSafeArrayScope<IUnknown> scope = new(array);
        Assert.Equal(3, scope.Length);
    }

    [Fact]
    public unsafe void Indexer_NullEntry_ReturnsNullComScope()
    {
        SAFEARRAYBOUND bound = new() { cElements = 1, lLbound = 0 };
        SAFEARRAY* array = PInvokeMadowaku.SafeArrayCreate(VARENUM.VT_UNKNOWN, 1, &bound);
        using ComSafeArrayScope<IUnknown> scope = new(array);

        // Default entry is null; the indexer should return a ComScope wrapping null.
        using ComScope<IUnknown> entry = scope[0];
        Assert.True(entry.IsNull);
    }

    [Fact]
    public unsafe void ImplicitOperator_SafearrayDoublePointer_DereferencesToSafearrayPointer()
    {
        SAFEARRAYBOUND bound = new() { cElements = 1, lLbound = 0 };
        SAFEARRAY* array = PInvokeMadowaku.SafeArrayCreate(VARENUM.VT_UNKNOWN, 1, &bound);
        using ComSafeArrayScope<IUnknown> scope = new(array);
        SAFEARRAY** pp = scope;
        Assert.False(pp is null);
        Assert.True(*pp == scope.Value);
    }
}
