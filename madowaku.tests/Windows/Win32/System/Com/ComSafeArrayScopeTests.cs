// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using Windows.Win32.System.Variant;

namespace Windows.Win32.System.Com;

[TestClass]
public class ComSafeArrayScopeTests
{
    [TestMethod]
    public unsafe void Constructor_NullSafearrayPointer_IsNullTrue()
    {
        using ComSafeArrayScope<IUnknown> scope = new((SAFEARRAY*)null);
        scope.IsNull.Should().BeTrue();
    }

    [TestMethod]
    public unsafe void Constructor_WrongVarType_Throws()
    {
        using SafeArrayScope<int> source = new(1);
        SAFEARRAY* ptr = source.Value;
        FluentActions.Invoking(() => new ComSafeArrayScope<IUnknown>(ptr)).Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public unsafe void Constructor_VT_UNKNOWN_Array_Wraps()
    {
        // Build a 1-element VT_UNKNOWN SAFEARRAY manually and wrap it.
        SAFEARRAYBOUND bound = new() { cElements = 1, lLbound = 0 };
        SAFEARRAY* array = PInvokeMadowaku.SafeArrayCreate(VARENUM.VT_UNKNOWN, 1, &bound);
        (array is null).Should().BeFalse();

        using ComSafeArrayScope<IUnknown> scope = new(array);
        scope.IsNull.Should().BeFalse();
        scope.Length.Should().Be(1);
        scope.IsEmpty.Should().BeFalse();
    }

    [TestMethod]
    public unsafe void Length_NonEmptyArray_ReturnsCElements()
    {
        SAFEARRAYBOUND bound = new() { cElements = 3, lLbound = 0 };
        SAFEARRAY* array = PInvokeMadowaku.SafeArrayCreate(VARENUM.VT_UNKNOWN, 1, &bound);
        using ComSafeArrayScope<IUnknown> scope = new(array);
        scope.Length.Should().Be(3);
    }

    [TestMethod]
    public unsafe void Indexer_NullEntry_ReturnsNullComScope()
    {
        SAFEARRAYBOUND bound = new() { cElements = 1, lLbound = 0 };
        SAFEARRAY* array = PInvokeMadowaku.SafeArrayCreate(VARENUM.VT_UNKNOWN, 1, &bound);
        using ComSafeArrayScope<IUnknown> scope = new(array);

        // Default entry is null; the indexer should return a ComScope wrapping null.
        using ComScope<IUnknown> entry = scope[0];
        entry.IsNull.Should().BeTrue();
    }

    [TestMethod]
    public unsafe void ImplicitOperator_SafearrayDoublePointer_DereferencesToSafearrayPointer()
    {
        SAFEARRAYBOUND bound = new() { cElements = 1, lLbound = 0 };
        SAFEARRAY* array = PInvokeMadowaku.SafeArrayCreate(VARENUM.VT_UNKNOWN, 1, &bound);
        using ComSafeArrayScope<IUnknown> scope = new(array);
        SAFEARRAY** pp = scope;
        (pp is null).Should().BeFalse();
        (*pp == scope.Value).Should().BeTrue();
    }
}
