// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using Windows.Win32.Foundation;

namespace Windows.Win32.System.Variant;

public class VariantTests
{
    [Fact]
    public void EmptyVariant_HasExpectedProperties()
    {
        var v = VARIANT.Empty;
        Assert.True(v.IsEmpty);
        Assert.Equal(VARENUM.VT_EMPTY, v.vt);
        Assert.Equal(VARENUM.VT_EMPTY, v.Type);
        Assert.False(v.Byref);
        Assert.Null(v.GetManagedType());
    }

    [Fact]
    public void IntConversion_RoundTrip()
    {
        int value = 42;
        VARIANT v = (VARIANT)value;
        Assert.Equal(VARENUM.VT_I4, v.vt);
        Assert.Equal(value, (int)v);
        Assert.Equal(typeof(int), v.GetManagedType());
    }

    [Fact]
    public void UIntConversion_RoundTrip()
    {
        uint value = 123u;
        VARIANT v = (VARIANT)value;
        Assert.Equal(VARENUM.VT_UI4, v.vt);
        Assert.Equal(value, (uint)v);
        Assert.Equal(typeof(uint), v.GetManagedType());
    }

    [Fact]
    public void BoolConversion_RoundTrip()
    {
        VARIANT vTrue = (VARIANT)true;
        VARIANT vFalse = (VARIANT)false;
        Assert.Equal(VARENUM.VT_BOOL, vTrue.vt);
        Assert.True((bool)vTrue);
        Assert.False((bool)vFalse);
        Assert.Equal(typeof(bool), vTrue.GetManagedType());
    }

    [Fact]
    public void DecimalConversion_RoundTrip()
    {
        decimal value = 123.45m;
        VARIANT v = new();

        v.Anonymous.decVal = new(value);
        v.vt |= VARENUM.VT_DECIMAL;

        Assert.Equal(value, v.ToObject());
        Assert.Equal(typeof(decimal), v.GetManagedType());
    }

    [Fact]
    public void StringConversion_RoundTrip()
    {
        string s = "hello";
        using var bstr = new BSTR(s);
        VARIANT v = (VARIANT)bstr;
        Assert.Equal(VARENUM.VT_BSTR, v.vt);
        Assert.Equal(s, (string)v);
        Assert.Equal(typeof(string), v.GetManagedType());
    }

    [Fact]
    public void InvalidCast_Throws()
    {
        VARIANT v = VARIANT.Empty;
        Assert.Throws<InvalidCastException>(() => (int)v);
        Assert.Throws<InvalidCastException>(() => (uint)v);
        Assert.Throws<InvalidCastException>(() => (bool)v);
        Assert.Throws<InvalidCastException>(() => (decimal)v);
        Assert.Throws<InvalidCastException>(() => (string)v);
    }

    [Fact]
    public void GetManagedType_Static_ReturnsExpectedTypes()
    {
        Assert.Equal(typeof(int), VARIANT.GetManagedType(VARENUM.VT_I4));
        Assert.Equal(typeof(uint), VARIANT.GetManagedType(VARENUM.VT_UI4));
        Assert.Equal(typeof(bool), VARIANT.GetManagedType(VARENUM.VT_BOOL));
        Assert.Equal(typeof(string), VARIANT.GetManagedType(VARENUM.VT_BSTR));
        Assert.Equal(typeof(decimal), VARIANT.GetManagedType(VARENUM.VT_DECIMAL));
        Assert.Null(VARIANT.GetManagedType(VARENUM.VT_UNKNOWN));
        Assert.Equal(typeof(int[]), VARIANT.GetManagedType(VARENUM.VT_ARRAY | VARENUM.VT_I4));
        Assert.Null(VARIANT.GetManagedType((VARENUM)0xFFFF));
    }

    [Fact]
    public void ByrefProperty_ReflectsVtFlag()
    {
        VARIANT v = (VARIANT)1;
        Assert.False(v.Byref);
        v.vt |= VARENUM.VT_BYREF;
        Assert.True(v.Byref);
    }

    [Fact]
    public void Dispose_ClearsVariant()
    {
        VARIANT v = (VARIANT)123;
        v.Dispose();
        Assert.True(v.IsEmpty);
        Assert.Equal(VARENUM.VT_EMPTY, v.vt);
    }

    [Fact]
    public void Clear_ClearsVariant()
    {
        VARIANT v = (VARIANT)456;
        v.Clear();
        Assert.True(v.IsEmpty);
        Assert.Equal(VARENUM.VT_EMPTY, v.vt);
    }
}
