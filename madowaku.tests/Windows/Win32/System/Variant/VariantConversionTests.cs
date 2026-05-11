// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using Windows.Win32.System.Com;

namespace Windows.Win32.System.Variant;

public class VariantConversionTests
{
    [Fact]
    public void StringExplicitCast_RoundTrip()
    {
        VARIANT v = (VARIANT)"hello";
        Assert.Equal(VARENUM.VT_BSTR, v.vt);
        Assert.Equal("hello", (string)v);
        v.Dispose();
    }

    [Fact]
    public void DoubleExplicitCast_ProducesR8Variant()
    {
        VARIANT v = (VARIANT)3.14;
        Assert.Equal(VARENUM.VT_R8, v.vt);
        Assert.Equal(3.14, v.data.dblVal);
    }

    [Fact]
    public unsafe void IDispatchPointer_NullRoundTrip()
    {
        VARIANT v = (VARIANT)(IDispatch*)null;
        Assert.Equal(VARENUM.VT_DISPATCH, v.vt);
        Assert.True((IDispatch*)v is null);
    }

    [Fact]
    public unsafe void InvalidIDispatchCast_Throws()
    {
        VARIANT v = VARIANT.Empty;
        Assert.Throws<InvalidCastException>(() => { IDispatch* _ = (IDispatch*)v; });
    }

    [Fact]
    public void FromObject_Null_ReturnsEmpty()
    {
        VARIANT v = VARIANT.FromObject(null);
        Assert.True(v.IsEmpty);
    }

    [Fact]
    public void FromObject_String_ReturnsBstrVariant()
    {
        VARIANT v = VARIANT.FromObject("text");
        Assert.Equal(VARENUM.VT_BSTR, v.vt);
        Assert.Equal("text", (string)v);
        v.Dispose();
    }

    [Fact]
    public void FromObject_Int_ReturnsI4Variant()
    {
        VARIANT v = VARIANT.FromObject(123);
        Assert.Equal(VARENUM.VT_I4, v.vt);
        Assert.Equal(123, (int)v);
    }

    [Fact]
    public void FromObject_UInt_ReturnsUI4Variant()
    {
        VARIANT v = VARIANT.FromObject(456u);
        Assert.Equal(VARENUM.VT_UI4, v.vt);
        Assert.Equal(456u, (uint)v);
    }

    [Fact]
    public void FromObject_Short_ProducesI4VariantViaImplicitWidening()
    {
        // FromObject branches on `is short` but `(VARIANT)shortValue` has no short operator,
        // so the value widens to int and goes through the int operator → VT_I4.
        VARIANT v = VARIANT.FromObject((short)7);
        Assert.Equal(VARENUM.VT_I4, v.vt);
        Assert.Equal(7, (int)v);
    }

    [Fact]
    public void FromObject_Bool_ReturnsBoolVariant()
    {
        VARIANT v = VARIANT.FromObject(true);
        Assert.Equal(VARENUM.VT_BOOL, v.vt);
        Assert.True((bool)v);
    }

    [Fact]
    public void FromObject_Double_ReturnsR8Variant()
    {
        VARIANT v = VARIANT.FromObject(2.5);
        Assert.Equal(VARENUM.VT_R8, v.vt);
    }

    [Fact]
    public void ToObject_Decimal_ReturnsDecimal()
    {
        VARIANT v = new();
        v.Anonymous.decVal = new(100.5m);
        v.vt |= VARENUM.VT_DECIMAL;
        Assert.Equal(100.5m, v.ToObject());
    }

    [Fact]
    public void ToObject_Int_ReturnsInt()
    {
        VARIANT v = (VARIANT)42;
        Assert.Equal(42, v.ToObject());
    }

    [Fact]
    public void ToObject_Bool_ReturnsBool()
    {
        VARIANT v = (VARIANT)true;
        Assert.Equal(true, v.ToObject());
    }

    [Fact]
    public void ToObject_String_ReturnsString()
    {
        VARIANT v = (VARIANT)"abc";
        Assert.Equal("abc", v.ToObject());
        v.Dispose();
    }

    [Fact]
    public void GetManagedType_AllScalarTypes_ReturnsExpected()
    {
        Assert.Equal(typeof(sbyte), VARIANT.GetManagedType(VARENUM.VT_I1));
        Assert.Equal(typeof(byte), VARIANT.GetManagedType(VARENUM.VT_UI1));
        Assert.Equal(typeof(short), VARIANT.GetManagedType(VARENUM.VT_I2));
        Assert.Equal(typeof(ushort), VARIANT.GetManagedType(VARENUM.VT_UI2));
        Assert.Equal(typeof(long), VARIANT.GetManagedType(VARENUM.VT_I8));
        Assert.Equal(typeof(ulong), VARIANT.GetManagedType(VARENUM.VT_UI8));
        Assert.Equal(typeof(float), VARIANT.GetManagedType(VARENUM.VT_R4));
        Assert.Equal(typeof(double), VARIANT.GetManagedType(VARENUM.VT_R8));
        Assert.Equal(typeof(int), VARIANT.GetManagedType(VARENUM.VT_ERROR));
        Assert.Equal(typeof(decimal), VARIANT.GetManagedType(VARENUM.VT_CY));
        Assert.Equal(typeof(DateTime), VARIANT.GetManagedType(VARENUM.VT_DATE));
        Assert.Equal(typeof(DateTime), VARIANT.GetManagedType(VARENUM.VT_FILETIME));
        Assert.Equal(typeof(string), VARIANT.GetManagedType(VARENUM.VT_LPSTR));
        Assert.Equal(typeof(string), VARIANT.GetManagedType(VARENUM.VT_LPWSTR));
        Assert.Equal(typeof(VARIANT), VARIANT.GetManagedType(VARENUM.VT_VARIANT));
        Assert.Equal(typeof(Guid), VARIANT.GetManagedType(VARENUM.VT_CLSID));
        Assert.Equal(typeof(byte[]), VARIANT.GetManagedType(VARENUM.VT_BLOB));
        Assert.Null(VARIANT.GetManagedType(VARENUM.VT_DISPATCH));
    }

    [Fact]
    public void GetManagedType_ArrayTypes_ReturnsArrayType()
    {
        Assert.Equal(typeof(byte[]), VARIANT.GetManagedType(VARENUM.VT_ARRAY | VARENUM.VT_UI1));
        Assert.Equal(typeof(short[]), VARIANT.GetManagedType(VARENUM.VT_ARRAY | VARENUM.VT_I2));
        Assert.Equal(typeof(double[]), VARIANT.GetManagedType(VARENUM.VT_ARRAY | VARENUM.VT_R8));
        Assert.Equal(typeof(bool[]), VARIANT.GetManagedType(VARENUM.VT_ARRAY | VARENUM.VT_BOOL));
        Assert.Equal(typeof(string[]), VARIANT.GetManagedType(VARENUM.VT_ARRAY | VARENUM.VT_BSTR));
        Assert.Equal(typeof(Guid[]), VARIANT.GetManagedType(VARENUM.VT_ARRAY | VARENUM.VT_CLSID));
        Assert.Equal(typeof(decimal[]), VARIANT.GetManagedType(VARENUM.VT_ARRAY | VARENUM.VT_DECIMAL));
        Assert.Null(VARIANT.GetManagedType(VARENUM.VT_ARRAY | VARENUM.VT_DISPATCH));
    }
}
