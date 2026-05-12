// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using Windows.Win32.System.Variant;

namespace Windows.Win32.System.Com;

public class SafeArrayScopeTests
{
    [Fact]
    public void Constructor_IntSize_CreatesSafeArray()
    {
        using SafeArrayScope<int> array = new(3);
        unsafe { Assert.False(array.Value is null); }
    }

    [Fact]
    public void Indexer_Int_RoundTripsValues()
    {
        using SafeArrayScope<int> array = new(3);
        array[0] = 10;
        array[1] = 20;
        array[2] = 30;
        Assert.Equal(10, array[0]);
        Assert.Equal(20, array[1]);
        Assert.Equal(30, array[2]);
    }

    [Fact]
    public void Constructor_FromIntArray_PopulatesValues()
    {
        using SafeArrayScope<int> array = new([1, 2, 3, 4]);
        Assert.Equal(1, array[0]);
        Assert.Equal(4, array[3]);
    }

    [Fact]
    public void Indexer_String_RoundTripsValues()
    {
        using SafeArrayScope<string> array = new(2);
        array[0] = "alpha";
        array[1] = "beta";
        Assert.Equal("alpha", array[0]);
        Assert.Equal("beta", array[1]);
    }

    [Fact]
    public void Indexer_Double_RoundTripsValues()
    {
        using SafeArrayScope<double> array = new(2);
        array[0] = 1.5;
        array[1] = 2.5;
        Assert.Equal(1.5, array[0]);
        Assert.Equal(2.5, array[1]);
    }

    [Fact]
    public void Constructor_UnsupportedType_Throws()
    {
        Assert.Throws<ArgumentException>(() => new SafeArrayScope<byte>(1));
    }

    [Fact]
    public void Constructor_NintType_ThrowsWithComSafeArrayScopeMessage()
    {
        ArgumentException ex = Assert.Throws<ArgumentException>(() => new SafeArrayScope<nint>(1));
        Assert.Contains("ComSafeArrayScope", ex.Message);
    }

    [Fact]
    public unsafe void Constructor_NullSafearrayPointer_ValueIsNull()
    {
        using SafeArrayScope<int> array = new((SAFEARRAY*)null);
        Assert.True(array.Value is null);
    }

    [Fact]
    public unsafe void Constructor_WrapVT_I4_Succeeds()
    {
        using SafeArrayScope<int> source = new(2);
        source[0] = 7;
        source[1] = 8;

        // Wrap the existing VT_I4 SAFEARRAY in a new scope — must not throw.
        SafeArrayScope<int> wrapped = new(source.Value);
        Assert.Equal(7, wrapped[0]);
        Assert.Equal(8, wrapped[1]);
    }

    [Fact]
    public unsafe void Constructor_TypeMismatch_ForInt_Throws()
    {
        // VT_BSTR SAFEARRAY wrapped as SafeArrayScope<int> must throw.
        using SafeArrayScope<string> source = new(1);
        SAFEARRAY* ptr = source.Value;
        ArgumentException ex = Assert.Throws<ArgumentException>(
            () => new SafeArrayScope<int>(ptr));
        Assert.Contains("VarType=", ex.Message);
    }

    [Fact]
    public unsafe void Constructor_TypeMismatch_ForString_Throws()
    {
        using SafeArrayScope<int> source = new(1);
        SAFEARRAY* ptr = source.Value;
        Assert.Throws<ArgumentException>(() => new SafeArrayScope<string>(ptr));
    }

    [Fact]
    public unsafe void Constructor_TypeMismatch_ForDouble_Throws()
    {
        using SafeArrayScope<int> source = new(1);
        SAFEARRAY* ptr = source.Value;
        Assert.Throws<ArgumentException>(() => new SafeArrayScope<double>(ptr));
    }

    [Fact]
    public void Length_ReturnsElementCount()
    {
        using SafeArrayScope<int> array = new(5);
        Assert.Equal(5, array.Length);
    }

    [Fact]
    public void IsEmpty_LengthZero_ReturnsTrue()
    {
        using SafeArrayScope<int> array = new(0);
        Assert.True(array.IsEmpty);
    }

    [Fact]
    public void IsEmpty_NonEmpty_ReturnsFalse()
    {
        using SafeArrayScope<int> array = new(1);
        Assert.False(array.IsEmpty);
    }

    [Fact]
    public unsafe void IsNull_DefaultConstructed_ReturnsTrue()
    {
        using SafeArrayScope<int> array = new((SAFEARRAY*)null);
        Assert.True(array.IsNull);
    }

    [Fact]
    public unsafe void ImplicitOperator_SafearrayStar_NonNull()
    {
        using SafeArrayScope<int> array = new(2);
        SAFEARRAY* ptr = array;
        Assert.False(ptr is null);
    }

    [Fact]
    public void ImplicitOperator_Nint_NonZero()
    {
        using SafeArrayScope<int> array = new(2);
        nint value = array;
        Assert.NotEqual(0, value);
    }

    [Fact]
    public unsafe void ExplicitOperator_Variant_HasArrayFlag()
    {
        using SafeArrayScope<int> array = new(2);
        VARIANT v = (VARIANT)array;
        Assert.Equal(VARENUM.VT_ARRAY, v.vt & VARENUM.VT_ARRAY);
        Assert.Equal(VARENUM.VT_I4, v.vt & VARENUM.VT_TYPEMASK);
    }

    [Fact]
    public unsafe void Constructor_ObjectType_VariantSafearray_Roundtrips()
    {
        using SafeArrayScope<object> array = new(2);
        array[0] = 42;
        array[1] = "hi";
        Assert.Equal(42, array[0]);
        Assert.Equal("hi", array[1]);
    }

    [Fact]
    public unsafe void ImplicitOperator_VoidDoublePointer_NonNull()
    {
        using SafeArrayScope<int> array = new(2);
        void** pp = array;
        Assert.False(pp is null);
    }

    [Fact]
    public unsafe void ImplicitOperator_SafearrayDoublePointer_NonNull()
    {
        using SafeArrayScope<int> array = new(2);
        SAFEARRAY** pp = array;
        Assert.False(pp is null);
    }
}
