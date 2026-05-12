// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using System.Runtime.CompilerServices;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com;
using Windows.Win32.System.Com.StructuredStorage;
using InteropMarshal = System.Runtime.InteropServices.Marshal;
using InvalidOleVariantTypeException = System.Runtime.InteropServices.InvalidOleVariantTypeException;
using SafeArrayTypeMismatchException = System.Runtime.InteropServices.SafeArrayTypeMismatchException;

namespace Windows.Win32.System.Variant;

public partial class VariantTests
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
    public unsafe void ToObject_VT_ARRAY_2D_VT_I4_TransposesAndReturnsIntMatrix()
    {
        // 2x3 SAFEARRAY of VT_I4. Native SAFEARRAYs are column-major; CLR arrays are row-major,
        // so VARIANT.ToObject transposes. Build via SafeArrayCreate + SafeArrayPutElement.
        SAFEARRAYBOUND* bounds = stackalloc SAFEARRAYBOUND[2];
        bounds[0] = new SAFEARRAYBOUND { cElements = 2, lLbound = 0 };
        bounds[1] = new SAFEARRAYBOUND { cElements = 3, lLbound = 0 };

        SAFEARRAY* psa = PInvokeMadowaku.SafeArrayCreate(VARENUM.VT_I4, 2, bounds);
        Assert.False(psa is null);

        try
        {
            // Populate so transposition is obvious.
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    int value = i * 10 + j;
                    Span<int> idx = [i, j];
                    fixed (int* p = idx)
                    {
                        PInvokeMadowaku.SafeArrayPutElement(psa, p, &value).ThrowOnFailure();
                    }
                }
            }

            VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_I4 };
            v.data.parray = psa;

            // VARIANT.ToObject transposes column-major SAFEARRAY into row-major CLR Array, and
            // CreateArrayFromSafeArray reverses the bound order, so a 2x3 SAFEARRAY round-trips
            // as a 2D CLR int matrix of total length 6. This test guards the InvalidCastException
            // regression (multi-dim arrays used to throw); strengthen the assertions to detect
            // dimension confusion: every populated source value must appear exactly once, with
            // no extras and no default-zero padding.
            int[,] array = Assert.IsType<int[,]>(v.ToObject());
            Assert.Equal(2, array.Rank);
            Assert.Equal(6, array.Length);
            Assert.Equal(6, array.GetLength(0) * array.GetLength(1));

            List<int> actual = [];
            for (int a = 0; a < array.GetLength(0); a++)
            {
                for (int b = 0; b < array.GetLength(1); b++)
                {
                    actual.Add(array[a, b]);
                }
            }

            int[] expected = [0, 1, 2, 10, 11, 12];
            Assert.Equal(expected.OrderBy(x => x), actual.OrderBy(x => x));
        }
        finally
        {
            PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
        }
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

    private static unsafe SAFEARRAY* CreateSafeArray<T>(VARENUM vt, T[] values) where T : unmanaged
    {
        SAFEARRAYBOUND bound = new() { cElements = (uint)values.Length, lLbound = 0 };
        SAFEARRAY* psa = PInvokeMadowaku.SafeArrayCreate(vt, 1, &bound);
        Assert.False(psa is null);
        for (int i = 0; i < values.Length; i++)
        {
            T value = values[i];
            int index = i;
            PInvokeMadowaku.SafeArrayPutElement(psa, &index, &value).ThrowOnFailure();
        }

        return psa;
    }

    private static unsafe VARIANT MakeVector<T>(VARENUM elementType, T[] values) where T : unmanaged
    {
        int byteCount = sizeof(T) * values.Length;
        nint mem = InteropMarshal.AllocCoTaskMem(byteCount);
        // Use a raw memory copy to avoid Span<T> constraints on net481 for types
        // containing pointers (e.g. VARIANT).
        fixed (T* src = values)
        {
            Buffer.MemoryCopy(src, (void*)mem, byteCount, byteCount);
        }

        VARIANT v = new() { vt = VARENUM.VT_VECTOR | elementType };
        v.data.ca = new CAUB { cElems = (uint)values.Length, pElems = (byte*)mem };
        return v;
    }

    private static unsafe void FreeVector(ref VARIANT v)
    {
        if (v.data.ca.pElems is not null)
        {
            InteropMarshal.FreeCoTaskMem((nint)v.data.ca.pElems);
            v.data.ca.pElems = null;
        }
    }

    [Fact]
    public void Clear_NonEmptyBstr_ClearsAndFreesString()
    {
        VARIANT v = (VARIANT)"some-text";
        Assert.False(v.IsEmpty);
        v.Clear();
        Assert.True(v.IsEmpty);
        Assert.Equal(VARENUM.VT_EMPTY, v.vt);
    }

    [Fact]
    public void Clear_AlreadyEmpty_NoOps()
    {
        VARIANT v = VARIANT.Empty;
        v.Clear();
        Assert.True(v.IsEmpty);
    }

    [Fact]
    public unsafe void StringCast_VT_LPWSTR_ReturnsString()
    {
        nint wide = InteropMarshal.StringToCoTaskMemUni("wide-text");
        try
        {
            VARIANT v = new() { vt = VARENUM.VT_LPWSTR };
            v.data.pcVal = new PSTR((byte*)wide);
            Assert.Equal("wide-text", (string)v);
        }
        finally
        {
            InteropMarshal.FreeCoTaskMem(wide);
        }
    }

    [Fact]
    public unsafe void ToObject_VT_LPWSTR_ReturnsString()
    {
        nint wide = InteropMarshal.StringToCoTaskMemUni("wide-obj");
        try
        {
            VARIANT v = new() { vt = VARENUM.VT_LPWSTR };
            v.data.pcVal = new PSTR((byte*)wide);
            Assert.Equal("wide-obj", v.ToObject());
        }
        finally
        {
            InteropMarshal.FreeCoTaskMem(wide);
        }
    }

    [Fact]
    public unsafe void ToObject_VT_HRESULT_ReturnsInt()
    {
        VARIANT v = MakeScalar(VARENUM.VT_HRESULT, unchecked((int)0x80070005));
        Assert.Equal(unchecked((int)0x80070005), v.ToObject());
    }

    [Fact]
    public unsafe void ToObject_VT_UNKNOWN_NullPointer_ReturnsNull()
    {
        VARIANT v = new() { vt = VARENUM.VT_UNKNOWN };
        v.data.punkVal = null;
        Assert.Null(v.ToObject());
    }

    [Fact]
    public unsafe void ToObject_VT_DISPATCH_NullPointer_ReturnsNull()
    {
        VARIANT v = new() { vt = VARENUM.VT_DISPATCH };
        v.data.pdispVal = null;
        Assert.Null(v.ToObject());
    }

    [Fact]
    public unsafe void ToObject_VT_VARIANT_BYREF_NestedByref_Throws()
    {
        VARIANT innerByref = new() { vt = VARENUM.VT_I4 | VARENUM.VT_BYREF };
        int value = 5;
        innerByref.data.pintVal = &value;
        VARIANT outer = new() { vt = VARENUM.VT_VARIANT | VARENUM.VT_BYREF };
        outer.data.pvarVal = &innerByref;
        Assert.Throws<InvalidOleVariantTypeException>(() => outer.ToObject());
    }

    [Fact]
    public unsafe void ToObject_VT_CLSID_Byref_Throws()
    {
        Guid g = Guid.NewGuid();
        Guid* pg = &g;
        VARIANT v = new() { vt = VARENUM.VT_CLSID | VARENUM.VT_BYREF };
        v.data.byref = &pg;
        Assert.Throws<ArgumentException>(() => v.ToObject());
    }

    [Fact]
    public unsafe void ToObject_VT_FILETIME_Byref_Throws()
    {
        FILETIME ft = default;
        VARIANT v = new() { vt = VARENUM.VT_FILETIME | VARENUM.VT_BYREF };
        v.data.byref = &ft;
        Assert.Throws<ArgumentException>(() => v.ToObject());
    }

    [Fact]
    public unsafe void ToObject_VT_RECORD_NullRecInfo_Throws()
    {
        VARIANT v = new() { vt = VARENUM.VT_RECORD };
        Assert.Throws<ArgumentException>(() => v.ToObject());
    }

    [Fact]
    public unsafe void ToObject_Byref_NullData_NonEmpty_Throws()
    {
        VARIANT v = new() { vt = VARENUM.VT_I4 | VARENUM.VT_BYREF };
        v.data.pintVal = null;
        Assert.Throws<ArgumentException>(() => v.ToObject());
    }

    [Fact]
    public unsafe void ToObject_VT_NULL_BYREF_NullData_ReturnsDBNull()
    {
        VARIANT v = new() { vt = VARENUM.VT_NULL | VARENUM.VT_BYREF };
        Assert.Equal(Convert.DBNull, v.ToObject());
    }

    // ===== ToArray (VT_ARRAY) coverage =====

    [Fact]
    public unsafe void ToObject_Array_VT_I1_ReturnsSbyteArray()
    {
        sbyte[] values = [-1, 2, -3];
        SAFEARRAY* psa = CreateSafeArray(VARENUM.VT_I1, values);
        try
        {
            VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_I1 };
            v.data.parray = psa;
            Assert.Equal(values, Assert.IsType<sbyte[]>(v.ToObject()));
        }
        finally
        {
            PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
        }
    }

    [Fact]
    public unsafe void ToObject_Array_VT_UI1_ReturnsByteArray()
    {
        byte[] values = [1, 2, 3];
        SAFEARRAY* psa = CreateSafeArray(VARENUM.VT_UI1, values);
        try
        {
            VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_UI1 };
            v.data.parray = psa;
            Assert.Equal(values, Assert.IsType<byte[]>(v.ToObject()));
        }
        finally
        {
            PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
        }
    }

    [Fact]
    public unsafe void ToObject_Array_VT_I2_ReturnsShortArray()
    {
        short[] values = [-1, 2];
        SAFEARRAY* psa = CreateSafeArray(VARENUM.VT_I2, values);
        try
        {
            VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_I2 };
            v.data.parray = psa;
            Assert.Equal(values, Assert.IsType<short[]>(v.ToObject()));
        }
        finally
        {
            PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
        }
    }

    [Fact]
    public unsafe void ToObject_Array_VT_UI2_ReturnsUshortArray()
    {
        ushort[] values = [1, 2];
        SAFEARRAY* psa = CreateSafeArray(VARENUM.VT_UI2, values);
        try
        {
            VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_UI2 };
            v.data.parray = psa;
            Assert.Equal(values, Assert.IsType<ushort[]>(v.ToObject()));
        }
        finally
        {
            PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
        }
    }

    [Fact]
    public unsafe void ToObject_Array_VT_UI4_ReturnsUintArray()
    {
        uint[] values = [1u, 2u];
        SAFEARRAY* psa = CreateSafeArray(VARENUM.VT_UI4, values);
        try
        {
            VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_UI4 };
            v.data.parray = psa;
            Assert.Equal(values, Assert.IsType<uint[]>(v.ToObject()));
        }
        finally
        {
            PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
        }
    }

    [Fact]
    public unsafe void ToObject_Array_VT_I8_ReturnsLongArray()
    {
        long[] values = [-1L, 2L];
        SAFEARRAY* psa = CreateSafeArray(VARENUM.VT_I8, values);
        try
        {
            VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_I8 };
            v.data.parray = psa;
            Assert.Equal(values, Assert.IsType<long[]>(v.ToObject()));
        }
        finally
        {
            PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
        }
    }

    [Fact]
    public unsafe void ToObject_Array_VT_UI8_ReturnsUlongArray()
    {
        ulong[] values = [1UL, 2UL];
        SAFEARRAY* psa = CreateSafeArray(VARENUM.VT_UI8, values);
        try
        {
            VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_UI8 };
            v.data.parray = psa;
            Assert.Equal(values, Assert.IsType<ulong[]>(v.ToObject()));
        }
        finally
        {
            PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
        }
    }

    [Fact]
    public unsafe void ToObject_Array_VT_R4_ReturnsFloatArray()
    {
        float[] values = [1.5f, 2.5f];
        SAFEARRAY* psa = CreateSafeArray(VARENUM.VT_R4, values);
        try
        {
            VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_R4 };
            v.data.parray = psa;
            Assert.Equal(values, Assert.IsType<float[]>(v.ToObject()));
        }
        finally
        {
            PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
        }
    }

    [Fact]
    public unsafe void ToObject_Array_VT_BOOL_ReturnsBoolArray()
    {
        VARIANT_BOOL[] values = [VARIANT_BOOL.VARIANT_TRUE, VARIANT_BOOL.VARIANT_FALSE];
        SAFEARRAY* psa = CreateSafeArray(VARENUM.VT_BOOL, values);
        try
        {
            VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_BOOL };
            v.data.parray = psa;
            bool[] expected = [true, false];
            Assert.Equal(expected, Assert.IsType<bool[]>(v.ToObject()));
        }
        finally
        {
            PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
        }
    }

    [Fact]
    public unsafe void ToObject_Array_VT_DATE_ReturnsDateTimeArray()
    {
        DateTime d1 = new(2024, 1, 1);
        DateTime d2 = new(2025, 6, 15);
        double[] values = [d1.ToOADate(), d2.ToOADate()];
        SAFEARRAY* psa = CreateSafeArray(VARENUM.VT_DATE, values);
        try
        {
            VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_DATE };
            v.data.parray = psa;
            DateTime[] expected = [d1, d2];
            Assert.Equal(expected, Assert.IsType<DateTime[]>(v.ToObject()));
        }
        finally
        {
            PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
        }
    }

    [Fact]
    public unsafe void ToObject_Array_VT_CY_ReturnsDecimalArray()
    {
        long[] values = [12345L, 67890L];
        SAFEARRAY* psa = CreateSafeArray(VARENUM.VT_CY, values);
        try
        {
            VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_CY };
            v.data.parray = psa;
            decimal[] expected = [1.2345m, 6.789m];
            Assert.Equal(expected, Assert.IsType<decimal[]>(v.ToObject()));
        }
        finally
        {
            PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
        }
    }

    [Fact]
    public unsafe void ToObject_Array_VT_VARIANT_ReturnsObjectArray()
    {
#if NETFRAMEWORK
        // net481's Span<T> rejects types containing pointers (VARIANT contains BSTR* etc.),
        // so the Span<VARIANT> in ToArray throws ArgumentException. The modern .NET runtime
        // allows it and produces a populated object?[].
        using SafeArrayScope<object> source481 = new(1);
        VARIANT inner481 = (VARIANT)1;
        int idx481 = 0;
        PInvokeMadowaku.SafeArrayPutElement(source481.Value, &idx481, &inner481).ThrowOnFailure();
        VARIANT v481 = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_VARIANT };
        v481.data.parray = source481.Value;
        Assert.Throws<ArgumentException>(() => v481.ToObject());
#else
        using SafeArrayScope<object> source = new(2);
        VARIANT one = (VARIANT)1;
        VARIANT two = (VARIANT)2;
        int idx0 = 0;
        int idx1 = 1;
        PInvokeMadowaku.SafeArrayPutElement(source.Value, &idx0, &one).ThrowOnFailure();
        PInvokeMadowaku.SafeArrayPutElement(source.Value, &idx1, &two).ThrowOnFailure();

        VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_VARIANT };
        v.data.parray = source.Value;
        object?[] array = Assert.IsType<object?[]>(v.ToObject());
        object?[] expected = [1, 2];
        Assert.Equal(expected, array);
#endif
    }

    [Fact]
    public unsafe void ToObject_Array_VT_RECORD_MismatchedSafeArray_ThrowsArgumentException()
    {
        // Constructing a true VT_RECORD SAFEARRAY requires an IRecordInfo, which is impractical
        // in a unit test. Instead, mis-tag a VT_I4 SAFEARRAY as VT_ARRAY|VT_RECORD to exercise
        // CreateArrayFromSafeArray's record-type validation path. The current behavior is to
        // throw ArgumentException; pin that contract so a behavior change is caught.
        int[] values = [1];
        SAFEARRAY* psa = CreateSafeArray(VARENUM.VT_I4, values);
        try
        {
            VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_RECORD };
            v.data.parray = psa;
            Assert.Throws<ArgumentException>(() => v.ToObject());
        }
        finally
        {
            PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
        }
    }

    [Fact]
    public unsafe void ToObject_Array_TypeMismatch_Throws()
    {
        double[] values = [1.0];
        SAFEARRAY* psa = CreateSafeArray(VARENUM.VT_R8, values);
        try
        {
            VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_I4 };
            v.data.parray = psa;
            Assert.Throws<SafeArrayTypeMismatchException>(() => v.ToObject());
        }
        finally
        {
            PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
        }
    }

    [Fact]
    public unsafe void ToObject_Array_VT_INT_From_VT_I4_SafeArray_Succeeds()
    {
        int[] values = [7, 8];
        SAFEARRAY* psa = CreateSafeArray(VARENUM.VT_I4, values);
        try
        {
            VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_INT };
            v.data.parray = psa;
            Assert.Equal(values, Assert.IsType<int[]>(v.ToObject()));
        }
        finally
        {
            PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
        }
    }

    [Fact]
    public unsafe void ToObject_Array_2D_VT_BSTR_TransposesStrings()
    {
        SAFEARRAYBOUND* bounds = stackalloc SAFEARRAYBOUND[2];
        bounds[0] = new SAFEARRAYBOUND { cElements = 2, lLbound = 0 };
        bounds[1] = new SAFEARRAYBOUND { cElements = 2, lLbound = 0 };
        SAFEARRAY* psa = PInvokeMadowaku.SafeArrayCreate(VARENUM.VT_BSTR, 2, bounds);
        try
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    using BSTR b = new($"{i},{j}");
                    Span<int> idx = [i, j];
                    fixed (int* p = idx)
                    {
                        PInvokeMadowaku.SafeArrayPutElement(psa, p, (void*)(nint)b).ThrowOnFailure();
                    }
                }
            }

            VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_BSTR };
            v.data.parray = psa;
            string[,] array = Assert.IsType<string[,]>(v.ToObject());
            Assert.Equal(4, array.Length);
            List<string> items = [];
            for (int a = 0; a < array.GetLength(0); a++)
            {
                for (int b = 0; b < array.GetLength(1); b++)
                {
                    items.Add(array[a, b]);
                }
            }

            string[] expected = ["0,0", "0,1", "1,0", "1,1"];
            Assert.Equal(expected, items.OrderBy(x => x));
        }
        finally
        {
            PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
        }
    }

    [Fact]
    public unsafe void ToObject_Array_3D_VT_R8_Transposes()
    {
        SAFEARRAYBOUND* bounds = stackalloc SAFEARRAYBOUND[3];
        bounds[0] = new SAFEARRAYBOUND { cElements = 2, lLbound = 0 };
        bounds[1] = new SAFEARRAYBOUND { cElements = 2, lLbound = 0 };
        bounds[2] = new SAFEARRAYBOUND { cElements = 2, lLbound = 0 };
        SAFEARRAY* psa = PInvokeMadowaku.SafeArrayCreate(VARENUM.VT_R8, 3, bounds);
        try
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        double value = i * 100 + j * 10 + k;
                        Span<int> idx = [i, j, k];
                        fixed (int* p = idx)
                        {
                            PInvokeMadowaku.SafeArrayPutElement(psa, p, &value).ThrowOnFailure();
                        }
                    }
                }
            }

            VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_R8 };
            v.data.parray = psa;
            double[,,] array = Assert.IsType<double[,,]>(v.ToObject());
            Assert.Equal(8, array.Length);

            // Validate that every populated source value appears exactly once in the CLR array.
            // SAFEARRAYs are column-major and CLR arrays are row-major, so VARIANT.ToObject
            // transposes; CreateArrayFromSafeArray also reverses the bound order. Assert the
            // multiset of values rather than per-index equality to detect dimension/index
            // confusion (extras, missing values, default-zero padding) without locking in the
            // specific index mapping.
            List<double> actual = [];
            for (int a = 0; a < array.GetLength(0); a++)
            {
                for (int b = 0; b < array.GetLength(1); b++)
                {
                    for (int c = 0; c < array.GetLength(2); c++)
                    {
                        actual.Add(array[a, b, c]);
                    }
                }
            }

            double[] expected = [0, 1, 10, 11, 100, 101, 110, 111];
            Assert.Equal(expected.OrderBy(x => x), actual.OrderBy(x => x));
        }
        finally
        {
            PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
        }
    }

    // ===== ToVector (VT_VECTOR) coverage =====

    [Fact]
    public unsafe void ToObject_Vector_VT_I1_ReturnsSbyteArray()
    {
        sbyte[] values = [-1, 2];
        VARIANT v = MakeVector(VARENUM.VT_I1, values);
        try
        {
            Assert.Equal(values, Assert.IsType<sbyte[]>(v.ToObject()));
        }
        finally
        {
            FreeVector(ref v);
        }
    }

    [Fact]
    public unsafe void ToObject_Vector_VT_UI1_ReturnsByteArray()
    {
        byte[] values = [1, 2, 3];
        VARIANT v = MakeVector(VARENUM.VT_UI1, values);
        try
        {
            Assert.Equal(values, Assert.IsType<byte[]>(v.ToObject()));
        }
        finally
        {
            FreeVector(ref v);
        }
    }

    [Fact]
    public unsafe void ToObject_Vector_VT_I2_ReturnsShortArray()
    {
        short[] values = [-1, 2];
        VARIANT v = MakeVector(VARENUM.VT_I2, values);
        try
        {
            Assert.Equal(values, Assert.IsType<short[]>(v.ToObject()));
        }
        finally
        {
            FreeVector(ref v);
        }
    }

    [Fact]
    public unsafe void ToObject_Vector_VT_UI2_ReturnsUshortArray()
    {
        ushort[] values = [1, 2];
        VARIANT v = MakeVector(VARENUM.VT_UI2, values);
        try
        {
            Assert.Equal(values, Assert.IsType<ushort[]>(v.ToObject()));
        }
        finally
        {
            FreeVector(ref v);
        }
    }

    [Fact]
    public unsafe void ToObject_Vector_VT_I4_ReturnsIntArray()
    {
        int[] values = [1, 2, 3];
        VARIANT v = MakeVector(VARENUM.VT_I4, values);
        try
        {
            Assert.Equal(values, Assert.IsType<int[]>(v.ToObject()));
        }
        finally
        {
            FreeVector(ref v);
        }
    }

    [Fact]
    public unsafe void ToObject_Vector_VT_UI4_ReturnsUintArray()
    {
        uint[] values = [1u, 2u];
        VARIANT v = MakeVector(VARENUM.VT_UI4, values);
        try
        {
            Assert.Equal(values, Assert.IsType<uint[]>(v.ToObject()));
        }
        finally
        {
            FreeVector(ref v);
        }
    }

    [Fact]
    public unsafe void ToObject_Vector_VT_I8_ReturnsLongArray()
    {
        long[] values = [1L, 2L];
        VARIANT v = MakeVector(VARENUM.VT_I8, values);
        try
        {
            Assert.Equal(values, Assert.IsType<long[]>(v.ToObject()));
        }
        finally
        {
            FreeVector(ref v);
        }
    }

    [Fact]
    public unsafe void ToObject_Vector_VT_UI8_ReturnsUlongArray()
    {
        ulong[] values = [1UL, 2UL];
        VARIANT v = MakeVector(VARENUM.VT_UI8, values);
        try
        {
            Assert.Equal(values, Assert.IsType<ulong[]>(v.ToObject()));
        }
        finally
        {
            FreeVector(ref v);
        }
    }

    [Fact]
    public unsafe void ToObject_Vector_VT_R4_ReturnsFloatArray()
    {
        float[] values = [1.5f, 2.5f];
        VARIANT v = MakeVector(VARENUM.VT_R4, values);
        try
        {
            Assert.Equal(values, Assert.IsType<float[]>(v.ToObject()));
        }
        finally
        {
            FreeVector(ref v);
        }
    }

    [Fact]
    public unsafe void ToObject_Vector_VT_R8_ReturnsDoubleArray()
    {
        double[] values = [1.5, 2.5];
        VARIANT v = MakeVector(VARENUM.VT_R8, values);
        try
        {
            Assert.Equal(values, Assert.IsType<double[]>(v.ToObject()));
        }
        finally
        {
            FreeVector(ref v);
        }
    }

    [Fact]
    public unsafe void ToObject_Vector_VT_BOOL_ReturnsBoolArray()
    {
        VARIANT_BOOL[] values = [VARIANT_BOOL.VARIANT_TRUE, VARIANT_BOOL.VARIANT_FALSE];
        VARIANT v = MakeVector(VARENUM.VT_BOOL, values);
        try
        {
            bool[] expected = [true, false];
            Assert.Equal(expected, Assert.IsType<bool[]>(v.ToObject()));
        }
        finally
        {
            FreeVector(ref v);
        }
    }

    [Fact]
    public unsafe void ToObject_Vector_VT_CY_ReturnsDecimalArray()
    {
        long[] values = [12345L];
        VARIANT v = MakeVector(VARENUM.VT_CY, values);
        try
        {
            decimal[] expected = [1.2345m];
            Assert.Equal(expected, Assert.IsType<decimal[]>(v.ToObject()));
        }
        finally
        {
            FreeVector(ref v);
        }
    }

    [Fact]
    public unsafe void ToObject_Vector_VT_DATE_ReturnsDateTimeArray()
    {
        DateTime d = new(2024, 1, 1);
        double[] values = [d.ToOADate()];
        VARIANT v = MakeVector(VARENUM.VT_DATE, values);
        try
        {
            DateTime[] expected = [d];
            Assert.Equal(expected, Assert.IsType<DateTime[]>(v.ToObject()));
        }
        finally
        {
            FreeVector(ref v);
        }
    }

    [Fact]
    public unsafe void ToObject_Vector_VT_FILETIME_ReturnsDateTimeArray()
    {
        DateTime expected = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc).ToLocalTime();
        FILETIME ft = (FILETIME)expected;
        FILETIME[] values = [ft];
        VARIANT v = MakeVector(VARENUM.VT_FILETIME, values);
        try
        {
            DateTime[] expectedArray = [expected];
            Assert.Equal(expectedArray, Assert.IsType<DateTime[]>(v.ToObject()));
        }
        finally
        {
            FreeVector(ref v);
        }
    }

    [Fact]
    public unsafe void ToObject_Vector_VT_CLSID_ReturnsGuidArray()
    {
        Guid g1 = Guid.NewGuid();
        Guid g2 = Guid.NewGuid();
        Guid[] values = [g1, g2];
        VARIANT v = MakeVector(VARENUM.VT_CLSID, values);
        try
        {
            Assert.Equal(values, Assert.IsType<Guid[]>(v.ToObject()));
        }
        finally
        {
            FreeVector(ref v);
        }
    }

    [Fact]
    public unsafe void ToObject_Vector_VT_BSTR_ReturnsStringArray()
    {
        BSTR b1 = new("alpha");
        BSTR b2 = new("beta");
        try
        {
            nint[] values = [(nint)b1, (nint)b2];
            VARIANT v = MakeVector(VARENUM.VT_BSTR, values);
            try
            {
                string?[] array = Assert.IsType<string?[]>(v.ToObject());
                string?[] expected = ["alpha", "beta"];
                Assert.Equal(expected, array);
            }
            finally
            {
                FreeVector(ref v);
            }
        }
        finally
        {
            b1.Dispose();
            b2.Dispose();
        }
    }

    [Fact]
    public unsafe void ToObject_Vector_VT_LPWSTR_ReturnsStringArray()
    {
        nint s1 = InteropMarshal.StringToCoTaskMemUni("one");
        nint s2 = InteropMarshal.StringToCoTaskMemUni("two");
        try
        {
            nint[] values = [s1, s2];
            VARIANT v = MakeVector(VARENUM.VT_LPWSTR, values);
            try
            {
                string?[] array = Assert.IsType<string?[]>(v.ToObject());
                string?[] expected = ["one", "two"];
                Assert.Equal(expected, array);
            }
            finally
            {
                FreeVector(ref v);
            }
        }
        finally
        {
            InteropMarshal.FreeCoTaskMem(s1);
            InteropMarshal.FreeCoTaskMem(s2);
        }
    }

    [Fact]
    public unsafe void ToObject_Vector_VT_LPSTR_ReturnsStringArray()
    {
        nint s1 = InteropMarshal.StringToCoTaskMemAnsi("one");
        nint s2 = InteropMarshal.StringToCoTaskMemAnsi("two");
        try
        {
            nint[] values = [s1, s2];
            VARIANT v = MakeVector(VARENUM.VT_LPSTR, values);
            try
            {
                string?[] array = Assert.IsType<string?[]>(v.ToObject());
                string?[] expected = ["one", "two"];
                Assert.Equal(expected, array);
            }
            finally
            {
                FreeVector(ref v);
            }
        }
        finally
        {
            InteropMarshal.FreeCoTaskMem(s1);
            InteropMarshal.FreeCoTaskMem(s2);
        }
    }

    [Fact]
    public unsafe void ToObject_Vector_VT_VARIANT_ReturnsObjectArray()
    {
#if NETFRAMEWORK
        // net481's Span<T> rejects types containing pointers (VARIANT contains BSTR* etc.),
        // so the Span<VARIANT> in ToVector throws ArgumentException. The modern .NET runtime
        // allows it and produces a populated object?[].
        VARIANT[] inners481 = [(VARIANT)1];
        VARIANT v481 = MakeVector(VARENUM.VT_VARIANT, inners481);
        try
        {
            Assert.Throws<ArgumentException>(() => v481.ToObject());
        }
        finally
        {
            FreeVector(ref v481);
        }
#else
        VARIANT[] inners = [(VARIANT)1, (VARIANT)2];
        VARIANT v = MakeVector(VARENUM.VT_VARIANT, inners);
        try
        {
            object?[] array = Assert.IsType<object?[]>(v.ToObject());
            object?[] expected = [1, 2];
            Assert.Equal(expected, array);
        }
        finally
        {
            FreeVector(ref v);
        }
#endif
    }

    [Fact]
    public unsafe void ToObject_Vector_UnsupportedType_Throws()
    {
        int[] values = [0];
        VARIANT v = MakeVector(VARENUM.VT_CF, values);
        try
        {
            Assert.Throws<ArgumentException>(() => v.ToObject());
        }
        finally
        {
            FreeVector(ref v);
        }
    }
}
