// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

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
}
