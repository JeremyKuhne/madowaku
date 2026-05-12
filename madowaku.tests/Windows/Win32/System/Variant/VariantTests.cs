// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using System.Runtime.CompilerServices;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com;

namespace Windows.Win32.System.Variant;

public class VariantTests
{
    private static VARIANT MakeScalar<T>(VARENUM type, T value) where T : unmanaged
    {
        VARIANT v = new() { vt = type };
        Unsafe.As<VARIANT._Anonymous_e__Union._Anonymous_e__Struct._Anonymous_e__Union, T>(ref v.data) = value;
        return v;
    }

    [Fact]
    public void EmptyVariant_HasExpectedProperties()
    {
        VARIANT v = VARIANT.Empty;
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
        using BSTR bstr = new(s);
        VARIANT v = (VARIANT)bstr;
        Assert.Equal(VARENUM.VT_BSTR, v.vt);
        Assert.Equal(s, (string)v);
        Assert.Equal(typeof(string), v.GetManagedType());
    }

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
    public void FromObject_ViaMarshal_DateTime_ProducesDateOrR8Variant()
    {
        VARIANT v = VARIANT.FromObject(new DateTime(2025, 6, 1));
        try
        {
            // Marshal returns either VT_DATE or VT_R8 depending on the platform; either is acceptable.
            Assert.True(v.vt is VARENUM.VT_DATE or VARENUM.VT_R8);
        }
        finally
        {
            v.Dispose();
        }
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
    public void ToObject_VT_NULL_ReturnsDBNull()
    {
        VARIANT v = new() { vt = VARENUM.VT_NULL };
        Assert.Equal(Convert.DBNull, v.ToObject());
    }

    [Fact]
    public void ToObject_VT_I1_ReturnsSbyte()
    {
        VARIANT v = MakeScalar(VARENUM.VT_I1, (sbyte)-5);
        Assert.Equal((sbyte)-5, v.ToObject());
    }

    [Fact]
    public void ToObject_VT_UI1_ReturnsByte()
    {
        VARIANT v = MakeScalar(VARENUM.VT_UI1, (byte)200);
        Assert.Equal((byte)200, v.ToObject());
    }

    [Fact]
    public void ToObject_VT_I2_ReturnsShort()
    {
        VARIANT v = MakeScalar(VARENUM.VT_I2, (short)-1000);
        Assert.Equal((short)-1000, v.ToObject());
    }

    [Fact]
    public void ToObject_VT_UI2_ReturnsUshort()
    {
        VARIANT v = MakeScalar(VARENUM.VT_UI2, (ushort)50000);
        Assert.Equal((ushort)50000, v.ToObject());
    }

    [Fact]
    public void ToObject_VT_I8_ReturnsLong()
    {
        VARIANT v = MakeScalar(VARENUM.VT_I8, -1234567890123L);
        Assert.Equal(-1234567890123L, v.ToObject());
    }

    [Fact]
    public void ToObject_VT_UI8_ReturnsUlong()
    {
        VARIANT v = MakeScalar(VARENUM.VT_UI8, 9876543210123UL);
        Assert.Equal(9876543210123UL, v.ToObject());
    }

    [Fact]
    public void ToObject_VT_R4_ReturnsFloat()
    {
        VARIANT v = MakeScalar(VARENUM.VT_R4, 1.5f);
        Assert.Equal(1.5f, v.ToObject());
    }

    [Fact]
    public void ToObject_VT_R8_ReturnsDouble()
    {
        VARIANT v = MakeScalar(VARENUM.VT_R8, 2.25);
        Assert.Equal(2.25, v.ToObject());
    }

    [Fact]
    public void ToObject_VT_UINT_ReturnsUint()
    {
        VARIANT v = MakeScalar(VARENUM.VT_UINT, 42u);
        Assert.Equal(42u, v.ToObject());
    }

    [Fact]
    public void ToObject_VT_INT_ReturnsInt()
    {
        VARIANT v = MakeScalar(VARENUM.VT_INT, 7);
        Assert.Equal(7, v.ToObject());
    }

    [Fact]
    public void ToObject_VT_ERROR_ReturnsInt()
    {
        VARIANT v = MakeScalar(VARENUM.VT_ERROR, unchecked((int)0x80004005));
        Assert.Equal(unchecked((int)0x80004005), v.ToObject());
    }

    [Fact]
    public void ToObject_VT_DATE_ReturnsDateTime()
    {
        DateTime expected = new(2024, 3, 15);
        VARIANT v = MakeScalar(VARENUM.VT_DATE, expected.ToOADate());
        Assert.Equal(expected, v.ToObject());
    }

    [Fact]
    public void ToObject_VT_CY_ReturnsDecimal()
    {
        // OACurrency stores value * 10000 as Int64.
        VARIANT v = MakeScalar(VARENUM.VT_CY, 12345L);
        Assert.Equal(1.2345m, v.ToObject());
    }

    [Fact]
    public void ToObject_VT_VOID_ReturnsNull()
    {
        VARIANT v = new() { vt = VARENUM.VT_VOID };
        Assert.Null(v.ToObject());
    }

    [Fact]
    public void ToObject_Invalid_HighVtBits_Throws()
    {
        VARIANT v = new() { vt = (VARENUM)0xFF };
        Assert.Throws<InvalidCastException>(() => v.ToObject());
    }

    [Fact]
    public unsafe void ToObject_VT_CLSID_ReturnsGuid()
    {
        Guid expected = new("12345678-1234-1234-1234-1234567890ab");
        VARIANT v = new() { vt = VARENUM.VT_CLSID };
        v.data.puuid = &expected;
        Assert.Equal(expected, v.ToObject());
    }

    [Fact]
    public unsafe void ToObject_VT_FILETIME_ReturnsDateTime()
    {
        DateTime expected = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc).ToLocalTime();
        FILETIME ft = (FILETIME)expected;
        VARIANT v = new() { vt = VARENUM.VT_FILETIME };
        Unsafe.As<VARIANT._Anonymous_e__Union._Anonymous_e__Struct._Anonymous_e__Union, FILETIME>(ref v.data) = ft;
        Assert.Equal(expected, v.ToObject());
    }

    [Fact]
    public unsafe void ToObject_VT_LPSTR_ReturnsString()
    {
        nint ansi = global::System.Runtime.InteropServices.Marshal.StringToCoTaskMemAnsi("ascii-text");
        try
        {
            VARIANT v = new() { vt = VARENUM.VT_LPSTR };
            v.data.pcVal = new PSTR((byte*)ansi);
            Assert.Equal("ascii-text", v.ToObject());
        }
        finally
        {
            global::System.Runtime.InteropServices.Marshal.FreeCoTaskMem(ansi);
        }
    }

    [Fact]
    public unsafe void ToObject_VT_VARIANT_NotByref_ThrowsArgument()
    {
        VARIANT v = new() { vt = VARENUM.VT_VARIANT };
        // Falling through the switch with no byref bit set yields the "Unsupported VARENUM" path.
        Assert.Throws<ArgumentException>(() => v.ToObject());
    }

    [Fact]
    public unsafe void ToObject_VT_ARRAY_VT_I4_ReturnsIntArray()
    {
        using SafeArrayScope<int> source = new([10, 20, 30]);

        VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_I4 };
        v.data.parray = source.Value;

        int[] array = Assert.IsType<int[]>(v.ToObject());
        Assert.Equal([10, 20, 30], array);
    }

    [Fact]
    public unsafe void ToObject_VT_ARRAY_VT_R8_ReturnsDoubleArray()
    {
        using SafeArrayScope<double> source = new([1.5, 2.5]);

        VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_R8 };
        v.data.parray = source.Value;

        double[] array = Assert.IsType<double[]>(v.ToObject());
        Assert.Equal([1.5, 2.5], array);
    }

    [Fact]
    public unsafe void ToObject_VT_ARRAY_VT_BSTR_ReturnsStringArray()
    {
        using SafeArrayScope<string> source = new(["alpha", "beta"]);

        VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_BSTR };
        v.data.parray = source.Value;

        string[] array = Assert.IsType<string[]>(v.ToObject());
        Assert.Equal(["alpha", "beta"], array);
    }

    [Fact]
    public unsafe void ToObject_VT_ARRAY_NullSafearray_ReturnsNull()
    {
        VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_I4 };
        v.data.parray = null;

        Assert.Null(v.ToObject());
    }

    [Fact]
    public unsafe void Byref_BoolByRef_ReturnsTrue()
    {
        VARIANT_BOOL b = VARIANT_BOOL.VARIANT_TRUE;
        VARIANT v = new() { vt = VARENUM.VT_BOOL | VARENUM.VT_BYREF };
        v.data.pboolVal = &b;
        Assert.True(v.Byref);
    }

    [Fact]
    public unsafe void ToObject_VT_BOOL_BYREF_ReturnsBool()
    {
        VARIANT_BOOL b = VARIANT_BOOL.VARIANT_TRUE;
        VARIANT v = new() { vt = VARENUM.VT_BOOL | VARENUM.VT_BYREF };
        v.data.pboolVal = &b;
        Assert.Equal(true, v.ToObject());
    }

    [Fact]
    public unsafe void ToObject_VT_I4_BYREF_ReturnsInt()
    {
        int value = 1234;
        VARIANT v = new() { vt = VARENUM.VT_I4 | VARENUM.VT_BYREF };
        v.data.pintVal = &value;
        Assert.Equal(1234, v.ToObject());
    }

    [Fact]
    public unsafe void ToObject_VT_R8_BYREF_ReturnsDouble()
    {
        double value = 3.5;
        VARIANT v = new() { vt = VARENUM.VT_R8 | VARENUM.VT_BYREF };
        v.data.pdblVal = &value;
        Assert.Equal(3.5, v.ToObject());
    }

    [Fact]
    public unsafe void ToObject_VT_VARIANT_BYREF_ReturnsNestedObject()
    {
        VARIANT inner = (VARIANT)42;
        VARIANT outer = new() { vt = VARENUM.VT_VARIANT | VARENUM.VT_BYREF };
        outer.data.pvarVal = &inner;
        Assert.Equal(42, outer.ToObject());
    }

    [Fact]
    public unsafe void ToObject_VT_EMPTY_BYREF_NullData_ReturnsZero()
    {
        VARIANT v = new() { vt = VARENUM.VT_EMPTY | VARENUM.VT_BYREF };
        // No data assigned — byref data pointer is null. Should yield 0 (uint/ulong),
        // not throw, per the VT_EMPTY|VT_BYREF special case.
        object? result = v.ToObject();
        Assert.NotNull(result);
        Assert.True(result is uint u && u == 0 || result is ulong ul && ul == 0);
    }

    [Fact]
    public void GetManagedType_FromInstance_ReturnsBackingType()
    {
        VARIANT v = (VARIANT)123;
        Assert.Equal(typeof(int), v.GetManagedType());
    }

    [Fact]
    public void GetManagedType_FromEmptyInstance_ReturnsNull()
    {
        Assert.Null(VARIANT.Empty.GetManagedType());
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
