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

[TestClass]
public partial class VariantTests
{
    private static VARIANT MakeScalar<T>(VARENUM type, T value) where T : unmanaged
    {
        VARIANT v = new() { vt = type };
        Unsafe.As<VARIANT._Anonymous_e__Union._Anonymous_e__Struct._Anonymous_e__Union, T>(ref v.data) = value;
        return v;
    }

    [TestMethod]
    public void EmptyVariant_HasExpectedProperties()
    {
        VARIANT v = VARIANT.Empty;
        v.IsEmpty.Should().BeTrue();
        v.vt.Should().Be(VARENUM.VT_EMPTY);
        v.Type.Should().Be(VARENUM.VT_EMPTY);
        v.Byref.Should().BeFalse();
        v.GetManagedType().Should().BeNull();
    }

    [TestMethod]
    public void IntConversion_RoundTrip()
    {
        int value = 42;
        VARIANT v = (VARIANT)value;
        v.vt.Should().Be(VARENUM.VT_I4);
        ((int)v).Should().Be(value);
        v.GetManagedType().Should().Be(typeof(int));
    }

    [TestMethod]
    public void UIntConversion_RoundTrip()
    {
        uint value = 123u;
        VARIANT v = (VARIANT)value;
        v.vt.Should().Be(VARENUM.VT_UI4);
        ((uint)v).Should().Be(value);
        v.GetManagedType().Should().Be(typeof(uint));
    }

    [TestMethod]
    public void BoolConversion_RoundTrip()
    {
        VARIANT vTrue = (VARIANT)true;
        VARIANT vFalse = (VARIANT)false;
        vTrue.vt.Should().Be(VARENUM.VT_BOOL);
        ((bool)vTrue).Should().BeTrue();
        ((bool)vFalse).Should().BeFalse();
        vTrue.GetManagedType().Should().Be(typeof(bool));
    }

    [TestMethod]
    public void DecimalConversion_RoundTrip()
    {
        decimal value = 123.45m;
        VARIANT v = new();

        v.Anonymous.decVal = new(value);
        v.vt |= VARENUM.VT_DECIMAL;

        // BeApproximately with zero tolerance compares decimal value scale-insensitively
        // (the round-trip can change the decimal's scale on some target frameworks).
        v.ToObject().Should().BeOfType<decimal>().Which.Should().BeApproximately(value, 0m);
        v.GetManagedType().Should().Be(typeof(decimal));
    }

    [TestMethod]
    public void StringConversion_RoundTrip()
    {
        string s = "hello";
        using BSTR bstr = new(s);
        VARIANT v = (VARIANT)bstr;
        v.vt.Should().Be(VARENUM.VT_BSTR);
        ((string)v).Should().Be(s);
        v.GetManagedType().Should().Be(typeof(string));
    }

    [TestMethod]
    public void StringExplicitCast_RoundTrip()
    {
        VARIANT v = (VARIANT)"hello";
        v.vt.Should().Be(VARENUM.VT_BSTR);
        ((string)v).Should().Be("hello");
        v.Dispose();
    }

    [TestMethod]
    public void DoubleExplicitCast_ProducesR8Variant()
    {
        VARIANT v = (VARIANT)3.14;
        v.vt.Should().Be(VARENUM.VT_R8);
        v.data.dblVal.Should().Be(3.14);
    }

    [TestMethod]
    public unsafe void IDispatchPointer_NullRoundTrip()
    {
        VARIANT v = (VARIANT)(IDispatch*)null;
        v.vt.Should().Be(VARENUM.VT_DISPATCH);
        ((IDispatch*)v is null).Should().BeTrue();
    }

    [TestMethod]
    public unsafe void InvalidIDispatchCast_Throws()
    {
        VARIANT v = VARIANT.Empty;
        FluentActions.Invoking(() => { IDispatch* _ = (IDispatch*)v; }).Should().Throw<InvalidCastException>();
    }

    [TestMethod]
    public void InvalidCast_Throws()
    {
        VARIANT v = VARIANT.Empty;
        FluentActions.Invoking(() => (int)v).Should().Throw<InvalidCastException>();
        FluentActions.Invoking(() => (uint)v).Should().Throw<InvalidCastException>();
        FluentActions.Invoking(() => (bool)v).Should().Throw<InvalidCastException>();
        FluentActions.Invoking(() => (decimal)v).Should().Throw<InvalidCastException>();
        FluentActions.Invoking(() => (string)v).Should().Throw<InvalidCastException>();
    }

    [TestMethod]
    public void FromObject_Null_ReturnsEmpty()
    {
        VARIANT v = VARIANT.FromObject(null);
        v.IsEmpty.Should().BeTrue();
    }

    [TestMethod]
    public void FromObject_String_ReturnsBstrVariant()
    {
        VARIANT v = VARIANT.FromObject("text");
        v.vt.Should().Be(VARENUM.VT_BSTR);
        ((string)v).Should().Be("text");
        v.Dispose();
    }

    [TestMethod]
    public void FromObject_Int_ReturnsI4Variant()
    {
        VARIANT v = VARIANT.FromObject(123);
        v.vt.Should().Be(VARENUM.VT_I4);
        ((int)v).Should().Be(123);
    }

    [TestMethod]
    public void FromObject_UInt_ReturnsUI4Variant()
    {
        VARIANT v = VARIANT.FromObject(456u);
        v.vt.Should().Be(VARENUM.VT_UI4);
        ((uint)v).Should().Be(456u);
    }

    [TestMethod]
    public void FromObject_Short_ProducesI4VariantViaImplicitWidening()
    {
        // FromObject branches on `is short` but `(VARIANT)shortValue` has no short operator,
        // so the value widens to int and goes through the int operator → VT_I4.
        VARIANT v = VARIANT.FromObject((short)7);
        v.vt.Should().Be(VARENUM.VT_I4);
        ((int)v).Should().Be(7);
    }

    [TestMethod]
    public void FromObject_Bool_ReturnsBoolVariant()
    {
        VARIANT v = VARIANT.FromObject(true);
        v.vt.Should().Be(VARENUM.VT_BOOL);
        ((bool)v).Should().BeTrue();
    }

    [TestMethod]
    public void FromObject_Double_ReturnsR8Variant()
    {
        VARIANT v = VARIANT.FromObject(2.5);
        v.vt.Should().Be(VARENUM.VT_R8);
    }

    [TestMethod]
    public void FromObject_ViaMarshal_DateTime_ProducesDateOrR8Variant()
    {
        VARIANT v = VARIANT.FromObject(new DateTime(2025, 6, 1));
        try
        {
            // Marshal returns either VT_DATE or VT_R8 depending on the platform; either is acceptable.
            v.vt.Should().BeOneOf(VARENUM.VT_DATE, VARENUM.VT_R8);
        }
        finally
        {
            v.Dispose();
        }
    }

    [TestMethod]
    public void ToObject_Decimal_ReturnsDecimal()
    {
        VARIANT v = new();
        v.Anonymous.decVal = new(100.5m);
        v.vt |= VARENUM.VT_DECIMAL;
        v.ToObject().Should().BeOfType<decimal>().Which.Should().BeApproximately(100.5m, 0m);
    }

    [TestMethod]
    public void ToObject_Int_ReturnsInt()
    {
        VARIANT v = (VARIANT)42;
        v.ToObject().Should().Be(42);
    }

    [TestMethod]
    public void ToObject_Bool_ReturnsBool()
    {
        VARIANT v = (VARIANT)true;
        v.ToObject().Should().Be(true);
    }

    [TestMethod]
    public void ToObject_String_ReturnsString()
    {
        VARIANT v = (VARIANT)"abc";
        v.ToObject().Should().Be("abc");
        v.Dispose();
    }

    [TestMethod]
    public void ToObject_VT_NULL_ReturnsDBNull()
    {
        VARIANT v = new() { vt = VARENUM.VT_NULL };
        v.ToObject().Should().Be(Convert.DBNull);
    }

    [TestMethod]
    public void ToObject_VT_EMPTY_ReturnsNull()
    {
        VARIANT v = VARIANT.Empty;
        v.ToObject().Should().BeNull();
    }

    [TestMethod]
    public void ToObject_VT_I1_ReturnsSbyte()
    {
        VARIANT v = MakeScalar(VARENUM.VT_I1, (sbyte)-5);
        v.ToObject().Should().Be((sbyte)-5);
    }

    [TestMethod]
    public void ToObject_VT_UI1_ReturnsByte()
    {
        VARIANT v = MakeScalar(VARENUM.VT_UI1, (byte)200);
        v.ToObject().Should().Be((byte)200);
    }

    [TestMethod]
    public void ToObject_VT_I2_ReturnsShort()
    {
        VARIANT v = MakeScalar(VARENUM.VT_I2, (short)-1000);
        v.ToObject().Should().Be((short)-1000);
    }

    [TestMethod]
    public void ToObject_VT_UI2_ReturnsUshort()
    {
        VARIANT v = MakeScalar(VARENUM.VT_UI2, (ushort)50000);
        v.ToObject().Should().Be((ushort)50000);
    }

    [TestMethod]
    public void ToObject_VT_I8_ReturnsLong()
    {
        VARIANT v = MakeScalar(VARENUM.VT_I8, -1234567890123L);
        v.ToObject().Should().Be(-1234567890123L);
    }

    [TestMethod]
    public void ToObject_VT_UI8_ReturnsUlong()
    {
        VARIANT v = MakeScalar(VARENUM.VT_UI8, 9876543210123UL);
        v.ToObject().Should().Be(9876543210123UL);
    }

    [TestMethod]
    public void ToObject_VT_R4_ReturnsFloat()
    {
        VARIANT v = MakeScalar(VARENUM.VT_R4, 1.5f);
        v.ToObject().Should().Be(1.5f);
    }

    [TestMethod]
    public void ToObject_VT_R8_ReturnsDouble()
    {
        VARIANT v = MakeScalar(VARENUM.VT_R8, 2.25);
        v.ToObject().Should().Be(2.25);
    }

    [TestMethod]
    public void ToObject_VT_UINT_ReturnsUint()
    {
        VARIANT v = MakeScalar(VARENUM.VT_UINT, 42u);
        v.ToObject().Should().Be(42u);
    }

    [TestMethod]
    public void ToObject_VT_INT_ReturnsInt()
    {
        VARIANT v = MakeScalar(VARENUM.VT_INT, 7);
        v.ToObject().Should().Be(7);
    }

    [TestMethod]
    public void ToObject_VT_ERROR_ReturnsInt()
    {
        VARIANT v = MakeScalar(VARENUM.VT_ERROR, unchecked((int)0x80004005));
        v.ToObject().Should().Be(unchecked((int)0x80004005));
    }

    [TestMethod]
    public void ToObject_VT_DATE_ReturnsDateTime()
    {
        DateTime expected = new(2024, 3, 15);
        VARIANT v = MakeScalar(VARENUM.VT_DATE, expected.ToOADate());
        v.ToObject().Should().Be(expected);
    }

    [TestMethod]
    public void ToObject_VT_CY_ReturnsDecimal()
    {
        // OACurrency stores value * 10000 as Int64.
        VARIANT v = MakeScalar(VARENUM.VT_CY, 12345L);
        v.ToObject().Should().Be(1.2345m);
    }

    [TestMethod]
    public void ToObject_VT_VOID_ReturnsNull()
    {
        VARIANT v = new() { vt = VARENUM.VT_VOID };
        v.ToObject().Should().BeNull();
    }

    [TestMethod]
    public void ToObject_Invalid_HighVtBits_Throws()
    {
        VARIANT v = new() { vt = (VARENUM)0xFF };
        FluentActions.Invoking(() => v.ToObject()).Should().Throw<InvalidCastException>();
    }

    [TestMethod]
    public unsafe void ToObject_VT_CLSID_ReturnsGuid()
    {
        Guid expected = new("12345678-1234-1234-1234-1234567890ab");
        VARIANT v = new() { vt = VARENUM.VT_CLSID };
        v.data.puuid = &expected;
        v.ToObject().Should().Be(expected);
    }

    [TestMethod]
    public unsafe void ToObject_VT_FILETIME_ReturnsDateTime()
    {
        DateTime expected = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc).ToLocalTime();
        FILETIME ft = (FILETIME)expected;
        VARIANT v = new() { vt = VARENUM.VT_FILETIME };
        Unsafe.As<VARIANT._Anonymous_e__Union._Anonymous_e__Struct._Anonymous_e__Union, FILETIME>(ref v.data) = ft;
        v.ToObject().Should().Be(expected);
    }

    [TestMethod]
    public unsafe void ToObject_VT_LPSTR_ReturnsString()
    {
        nint ansi = global::System.Runtime.InteropServices.Marshal.StringToCoTaskMemAnsi("ascii-text");
        try
        {
            VARIANT v = new() { vt = VARENUM.VT_LPSTR };
            v.data.pcVal = new PSTR((byte*)ansi);
            v.ToObject().Should().Be("ascii-text");
        }
        finally
        {
            global::System.Runtime.InteropServices.Marshal.FreeCoTaskMem(ansi);
        }
    }

    [TestMethod]
    public unsafe void ToObject_VT_VARIANT_NotByref_ThrowsArgument()
    {
        VARIANT v = new() { vt = VARENUM.VT_VARIANT };
        // Falling through the switch with no byref bit set yields the "Unsupported VARENUM" path.
        FluentActions.Invoking(() => v.ToObject()).Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public unsafe void ToObject_VT_ARRAY_VT_I4_ReturnsIntArray()
    {
        using SafeArrayScope<int> source = new([10, 20, 30]);

        VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_I4 };
        v.data.parray = source.Value;

        int[] array = v.ToObject().Should().BeOfType<int[]>().Subject;
        array.Should().Equal(10, 20, 30);
    }

    [TestMethod]
    public unsafe void ToObject_VT_ARRAY_VT_R8_ReturnsDoubleArray()
    {
        using SafeArrayScope<double> source = new([1.5, 2.5]);

        VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_R8 };
        v.data.parray = source.Value;

        double[] array = v.ToObject().Should().BeOfType<double[]>().Subject;
        array.Should().Equal(1.5, 2.5);
    }

    [TestMethod]
    public unsafe void ToObject_VT_ARRAY_VT_BSTR_ReturnsStringArray()
    {
        using SafeArrayScope<string> source = new(["alpha", "beta"]);

        VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_BSTR };
        v.data.parray = source.Value;

        string[] array = v.ToObject().Should().BeOfType<string[]>().Subject;
        array.Should().Equal("alpha", "beta");
    }

    [TestMethod]
    public unsafe void ToObject_VT_ARRAY_NullSafearray_ReturnsNull()
    {
        VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_I4 };
        v.data.parray = null;

        v.ToObject().Should().BeNull();
    }

    [TestMethod]
    public unsafe void ToObject_VT_ARRAY_2D_VT_I4_TransposesAndReturnsIntMatrix()
    {
        // 2x3 SAFEARRAY of VT_I4. Native SAFEARRAYs are column-major; CLR arrays are row-major,
        // so VARIANT.ToObject transposes. Build via SafeArrayCreate + SafeArrayPutElement.
        SAFEARRAYBOUND* bounds = stackalloc SAFEARRAYBOUND[2];
        bounds[0] = new SAFEARRAYBOUND { cElements = 2, lLbound = 0 };
        bounds[1] = new SAFEARRAYBOUND { cElements = 3, lLbound = 0 };

        SAFEARRAY* psa = PInvokeMadowaku.SafeArrayCreate(VARENUM.VT_I4, 2, bounds);
        (psa is null).Should().BeFalse();

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
            int[,] array = v.ToObject().Should().BeOfType<int[,]>().Subject;
            array.Rank.Should().Be(2);
            array.Length.Should().Be(6);
            (array.GetLength(0) * array.GetLength(1)).Should().Be(6);

            List<int> actual = [];
            for (int a = 0; a < array.GetLength(0); a++)
            {
                for (int b = 0; b < array.GetLength(1); b++)
                {
                    actual.Add(array[a, b]);
                }
            }

            int[] expected = [0, 1, 2, 10, 11, 12];
            actual.OrderBy(x => x).Should().Equal(expected.OrderBy(x => x));
        }
        finally
        {
            PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
        }
    }

    [TestMethod]
    public unsafe void Byref_BoolByRef_ReturnsTrue()
    {
        VARIANT_BOOL b = VARIANT_BOOL.VARIANT_TRUE;
        VARIANT v = new() { vt = VARENUM.VT_BOOL | VARENUM.VT_BYREF };
        v.data.pboolVal = &b;
        v.Byref.Should().BeTrue();
    }

    [TestMethod]
    public unsafe void ToObject_VT_BOOL_BYREF_ReturnsBool()
    {
        VARIANT_BOOL b = VARIANT_BOOL.VARIANT_TRUE;
        VARIANT v = new() { vt = VARENUM.VT_BOOL | VARENUM.VT_BYREF };
        v.data.pboolVal = &b;
        v.ToObject().Should().Be(true);
    }

    [TestMethod]
    public unsafe void ToObject_VT_I4_BYREF_ReturnsInt()
    {
        int value = 1234;
        VARIANT v = new() { vt = VARENUM.VT_I4 | VARENUM.VT_BYREF };
        v.data.pintVal = &value;
        v.ToObject().Should().Be(1234);
    }

    [TestMethod]
    public unsafe void ToObject_VT_R8_BYREF_ReturnsDouble()
    {
        double value = 3.5;
        VARIANT v = new() { vt = VARENUM.VT_R8 | VARENUM.VT_BYREF };
        v.data.pdblVal = &value;
        v.ToObject().Should().Be(3.5);
    }

    [TestMethod]
    public unsafe void ToObject_VT_VARIANT_BYREF_ReturnsNestedObject()
    {
        VARIANT inner = (VARIANT)42;
        VARIANT outer = new() { vt = VARENUM.VT_VARIANT | VARENUM.VT_BYREF };
        outer.data.pvarVal = &inner;
        outer.ToObject().Should().Be(42);
    }

    [TestMethod]
    public unsafe void ToObject_VT_EMPTY_BYREF_NullData_ReturnsZero()
    {
        VARIANT v = new() { vt = VARENUM.VT_EMPTY | VARENUM.VT_BYREF };
        // No data assigned — byref data pointer is null. Should yield 0 (uint/ulong),
        // not throw, per the VT_EMPTY|VT_BYREF special case.
        object? result = v.ToObject();
        result.Should().NotBeNull();
        (result is uint u && u == 0 || result is ulong ul && ul == 0).Should().BeTrue();
    }

    [TestMethod]
    public void GetManagedType_FromInstance_ReturnsBackingType()
    {
        VARIANT v = (VARIANT)123;
        v.GetManagedType().Should().Be(typeof(int));
    }

    [TestMethod]
    public void GetManagedType_FromEmptyInstance_ReturnsNull()
    {
        VARIANT.Empty.GetManagedType().Should().BeNull();
    }

    [TestMethod]
    public void GetManagedType_Static_ReturnsExpectedTypes()
    {
        VARIANT.GetManagedType(VARENUM.VT_I4).Should().Be(typeof(int));
        VARIANT.GetManagedType(VARENUM.VT_UI4).Should().Be(typeof(uint));
        VARIANT.GetManagedType(VARENUM.VT_BOOL).Should().Be(typeof(bool));
        VARIANT.GetManagedType(VARENUM.VT_BSTR).Should().Be(typeof(string));
        VARIANT.GetManagedType(VARENUM.VT_DECIMAL).Should().Be(typeof(decimal));
        VARIANT.GetManagedType(VARENUM.VT_UNKNOWN).Should().BeNull();
        VARIANT.GetManagedType(VARENUM.VT_ARRAY | VARENUM.VT_I4).Should().Be(typeof(int[]));
        VARIANT.GetManagedType((VARENUM)0xFFFF).Should().BeNull();
    }

    [TestMethod]
    public void GetManagedType_AllScalarTypes_ReturnsExpected()
    {
        VARIANT.GetManagedType(VARENUM.VT_I1).Should().Be(typeof(sbyte));
        VARIANT.GetManagedType(VARENUM.VT_UI1).Should().Be(typeof(byte));
        VARIANT.GetManagedType(VARENUM.VT_I2).Should().Be(typeof(short));
        VARIANT.GetManagedType(VARENUM.VT_UI2).Should().Be(typeof(ushort));
        VARIANT.GetManagedType(VARENUM.VT_INT).Should().Be(typeof(int));
        VARIANT.GetManagedType(VARENUM.VT_UINT).Should().Be(typeof(uint));
        VARIANT.GetManagedType(VARENUM.VT_I8).Should().Be(typeof(long));
        VARIANT.GetManagedType(VARENUM.VT_UI8).Should().Be(typeof(ulong));
        VARIANT.GetManagedType(VARENUM.VT_R4).Should().Be(typeof(float));
        VARIANT.GetManagedType(VARENUM.VT_R8).Should().Be(typeof(double));
        VARIANT.GetManagedType(VARENUM.VT_ERROR).Should().Be(typeof(int));
        VARIANT.GetManagedType(VARENUM.VT_CY).Should().Be(typeof(decimal));
        VARIANT.GetManagedType(VARENUM.VT_DATE).Should().Be(typeof(DateTime));
        VARIANT.GetManagedType(VARENUM.VT_FILETIME).Should().Be(typeof(DateTime));
        VARIANT.GetManagedType(VARENUM.VT_LPSTR).Should().Be(typeof(string));
        VARIANT.GetManagedType(VARENUM.VT_LPWSTR).Should().Be(typeof(string));
        VARIANT.GetManagedType(VARENUM.VT_VARIANT).Should().Be(typeof(VARIANT));
        VARIANT.GetManagedType(VARENUM.VT_CLSID).Should().Be(typeof(Guid));
        VARIANT.GetManagedType(VARENUM.VT_BLOB).Should().Be(typeof(byte[]));
        VARIANT.GetManagedType(VARENUM.VT_DISPATCH).Should().BeNull();
    }

    [TestMethod]
    public void GetManagedType_ArrayTypes_ReturnsArrayType()
    {
        VARIANT.GetManagedType(VARENUM.VT_ARRAY | VARENUM.VT_UI1).Should().Be(typeof(byte[]));
        VARIANT.GetManagedType(VARENUM.VT_ARRAY | VARENUM.VT_I2).Should().Be(typeof(short[]));
        VARIANT.GetManagedType(VARENUM.VT_ARRAY | VARENUM.VT_UI4).Should().Be(typeof(uint[]));
        VARIANT.GetManagedType(VARENUM.VT_ARRAY | VARENUM.VT_ERROR).Should().Be(typeof(int[]));
        VARIANT.GetManagedType(VARENUM.VT_ARRAY | VARENUM.VT_R8).Should().Be(typeof(double[]));
        VARIANT.GetManagedType(VARENUM.VT_ARRAY | VARENUM.VT_BOOL).Should().Be(typeof(bool[]));
        VARIANT.GetManagedType(VARENUM.VT_ARRAY | VARENUM.VT_BSTR).Should().Be(typeof(string[]));
        VARIANT.GetManagedType(VARENUM.VT_ARRAY | VARENUM.VT_CLSID).Should().Be(typeof(Guid[]));
        VARIANT.GetManagedType(VARENUM.VT_ARRAY | VARENUM.VT_DECIMAL).Should().Be(typeof(decimal[]));
        VARIANT.GetManagedType(VARENUM.VT_ARRAY | VARENUM.VT_DISPATCH).Should().BeNull();
    }

    [TestMethod]
    public void GetManagedType_StaticSpecialCases_ReturnExpectedTypes()
    {
        VARIANT.GetManagedType(VARENUM.VT_STREAM).Should().Be(typeof(Stream));
        VARIANT.GetManagedType(VARENUM.VT_CF).Should().BeNull();
        VARIANT.GetManagedType(VARENUM.VT_STREAMED_OBJECT).Should().BeNull();
        VARIANT.GetManagedType(VARENUM.VT_STORAGE).Should().BeNull();
        VARIANT.GetManagedType(VARENUM.VT_STORED_OBJECT).Should().BeNull();
        VARIANT.GetManagedType(VARENUM.VT_VERSIONED_STREAM).Should().BeNull();
    }

    [TestMethod]
    public void GetManagedType_VectorTypes_ReturnExpectedArrayTypes()
    {
        VARIANT.GetManagedType(VARENUM.VT_VECTOR | VARENUM.VT_I1).Should().Be(typeof(sbyte[]));
        VARIANT.GetManagedType(VARENUM.VT_VECTOR | VARENUM.VT_UI2).Should().Be(typeof(ushort[]));
        VARIANT.GetManagedType(VARENUM.VT_VECTOR | VARENUM.VT_I8).Should().Be(typeof(long[]));
        VARIANT.GetManagedType(VARENUM.VT_VECTOR | VARENUM.VT_UI8).Should().Be(typeof(ulong[]));
        VARIANT.GetManagedType(VARENUM.VT_VECTOR | VARENUM.VT_INT).Should().Be(typeof(int[]));
        VARIANT.GetManagedType(VARENUM.VT_VECTOR | VARENUM.VT_UINT).Should().Be(typeof(uint[]));
        VARIANT.GetManagedType(VARENUM.VT_VECTOR | VARENUM.VT_R4).Should().Be(typeof(float[]));
        VARIANT.GetManagedType(VARENUM.VT_VECTOR | VARENUM.VT_CY).Should().Be(typeof(decimal[]));
        VARIANT.GetManagedType(VARENUM.VT_VECTOR | VARENUM.VT_DATE).Should().Be(typeof(DateTime[]));
        VARIANT.GetManagedType(VARENUM.VT_VECTOR | VARENUM.VT_FILETIME).Should().Be(typeof(DateTime[]));
        VARIANT.GetManagedType(VARENUM.VT_VECTOR | VARENUM.VT_LPSTR).Should().Be(typeof(string[]));
        VARIANT.GetManagedType(VARENUM.VT_VECTOR | VARENUM.VT_LPWSTR).Should().Be(typeof(string[]));
        VARIANT.GetManagedType(VARENUM.VT_VECTOR | VARENUM.VT_VARIANT).Should().Be(typeof(VARIANT[]));
        VARIANT.GetManagedType(VARENUM.VT_VECTOR | VARENUM.VT_STREAM).Should().Be(typeof(Stream[]));
        VARIANT.GetManagedType(VARENUM.VT_VECTOR | VARENUM.VT_UNKNOWN).Should().BeNull();
        VARIANT.GetManagedType(VARENUM.VT_VECTOR | VARENUM.VT_DISPATCH).Should().BeNull();
    }

    [TestMethod]
    public void GetManagedType_ArraySpecialCases_ReturnExpectedTypes()
    {
        VARIANT.GetManagedType(VARENUM.VT_ARRAY | VARENUM.VT_STREAM).Should().Be(typeof(Stream[]));
        VARIANT.GetManagedType(VARENUM.VT_ARRAY | VARENUM.VT_BLOB).Should().BeNull();
        VARIANT.GetManagedType(VARENUM.VT_ARRAY | VARENUM.VT_CF).Should().BeNull();
        VARIANT.GetManagedType(VARENUM.VT_ARRAY | VARENUM.VT_STREAMED_OBJECT).Should().BeNull();
        VARIANT.GetManagedType(VARENUM.VT_ARRAY | VARENUM.VT_STORAGE).Should().BeNull();
        VARIANT.GetManagedType(VARENUM.VT_ARRAY | VARENUM.VT_STORED_OBJECT).Should().BeNull();
        VARIANT.GetManagedType(VARENUM.VT_ARRAY | VARENUM.VT_VERSIONED_STREAM).Should().BeNull();
        VARIANT.GetManagedType(VARENUM.VT_VOID).Should().BeNull();
    }

    [TestMethod]
    public void ByrefProperty_ReflectsVtFlag()
    {
        VARIANT v = (VARIANT)1;
        v.Byref.Should().BeFalse();
        v.vt |= VARENUM.VT_BYREF;
        v.Byref.Should().BeTrue();
    }

    [TestMethod]
    public void Dispose_ClearsVariant()
    {
        VARIANT v = (VARIANT)123;
        v.Dispose();
        v.IsEmpty.Should().BeTrue();
        v.vt.Should().Be(VARENUM.VT_EMPTY);
    }

    [TestMethod]
    public void Clear_ClearsVariant()
    {
        VARIANT v = (VARIANT)456;
        v.Clear();
        v.IsEmpty.Should().BeTrue();
        v.vt.Should().Be(VARENUM.VT_EMPTY);
    }

    private static unsafe SAFEARRAY* CreateSafeArray<T>(VARENUM vt, T[] values) where T : unmanaged
    {
        SAFEARRAYBOUND bound = new() { cElements = (uint)values.Length, lLbound = 0 };
        SAFEARRAY* psa = PInvokeMadowaku.SafeArrayCreate(vt, 1, &bound);
        (psa is null).Should().BeFalse();
        for (int i = 0; i < values.Length; i++)
        {
            T value = values[i];
            int index = i;
            PInvokeMadowaku.SafeArrayPutElement(psa, &index, &value).ThrowOnFailure();
        }

        return psa;
    }

    private static unsafe Array ConvertNonZeroLowerBoundArray<T>(VARENUM safeArrayType, VARENUM variantType, T[] values)
        where T : unmanaged
    {
        SAFEARRAYBOUND bounds = new() { cElements = (uint)values.Length, lLbound = 5 };
        SAFEARRAY* psa = PInvokeMadowaku.SafeArrayCreate(safeArrayType, 1, &bounds);
        (psa is null).Should().BeFalse();

        try
        {
            for (int i = 0; i < values.Length; i++)
            {
                int absoluteIndex = i + 5;
                T value = values[i];
                PInvokeMadowaku.SafeArrayPutElement(psa, &absoluteIndex, &value).ThrowOnFailure();
            }

            VARIANT variant = new() { vt = VARENUM.VT_ARRAY | variantType };
            variant.data.parray = psa;
            Array result = variant.ToObject().Should().BeAssignableTo<Array>().Subject;
            result.GetLowerBound(0).Should().Be(5);
            result.Length.Should().Be(values.Length);
            return result;
        }
        finally
        {
            if (psa is not null)
            {
                PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
            }
        }
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

    [TestMethod]
    public void Clear_NonEmptyBstr_ClearsAndFreesString()
    {
        VARIANT v = (VARIANT)"some-text";
        v.IsEmpty.Should().BeFalse();
        v.Clear();
        v.IsEmpty.Should().BeTrue();
        v.vt.Should().Be(VARENUM.VT_EMPTY);
    }

    [TestMethod]
    public void Clear_AlreadyEmpty_NoOps()
    {
        VARIANT v = VARIANT.Empty;
        v.Clear();
        v.IsEmpty.Should().BeTrue();
    }

    [TestMethod]
    public unsafe void StringCast_VT_LPWSTR_ReturnsString()
    {
        nint wide = InteropMarshal.StringToCoTaskMemUni("wide-text");
        try
        {
            VARIANT v = new() { vt = VARENUM.VT_LPWSTR };
            v.data.pcVal = new PSTR((byte*)wide);
            ((string)v).Should().Be("wide-text");
        }
        finally
        {
            InteropMarshal.FreeCoTaskMem(wide);
        }
    }

    [TestMethod]
    public unsafe void ToObject_VT_LPWSTR_ReturnsString()
    {
        nint wide = InteropMarshal.StringToCoTaskMemUni("wide-obj");
        try
        {
            VARIANT v = new() { vt = VARENUM.VT_LPWSTR };
            v.data.pcVal = new PSTR((byte*)wide);
            v.ToObject().Should().Be("wide-obj");
        }
        finally
        {
            InteropMarshal.FreeCoTaskMem(wide);
        }
    }

    [TestMethod]
    public unsafe void ToObject_VT_HRESULT_ReturnsInt()
    {
        VARIANT v = MakeScalar(VARENUM.VT_HRESULT, unchecked((int)0x80070005));
        v.ToObject().Should().Be(unchecked((int)0x80070005));
    }

    [TestMethod]
    public unsafe void ToObject_VT_UNKNOWN_NullPointer_ReturnsNull()
    {
        VARIANT v = new() { vt = VARENUM.VT_UNKNOWN };
        v.data.punkVal = null;
        v.ToObject().Should().BeNull();
    }

    [TestMethod]
    public unsafe void ToObject_VT_UNKNOWN_NonNullPointer_ReturnsComObject()
    {
        object source = new();
        nint unknown = InteropMarshal.GetIUnknownForObject(source);
        try
        {
            VARIANT v = new() { vt = VARENUM.VT_UNKNOWN };
            v.data.punkVal = (IUnknown*)unknown;
            v.ToObject().Should().NotBeNull();
        }
        finally
        {
            InteropMarshal.Release(unknown);
        }
    }

    [TestMethod]
    public unsafe void ToObject_VT_DECIMAL_BYREF_ReturnsDecimal()
    {
        DECIMAL value = new(42.125m);
        VARIANT v = new() { vt = VARENUM.VT_DECIMAL | VARENUM.VT_BYREF };
        v.data.byref = &value;
        v.ToObject().Should().Be(42.125m);
    }

    [TestMethod]
    public unsafe void ToObject_VT_DISPATCH_NullPointer_ReturnsNull()
    {
        VARIANT v = new() { vt = VARENUM.VT_DISPATCH };
        v.data.pdispVal = null;
        v.ToObject().Should().BeNull();
    }

    [TestMethod]
    public unsafe void ToObject_VT_VARIANT_BYREF_NestedByref_Throws()
    {
        VARIANT innerByref = new() { vt = VARENUM.VT_I4 | VARENUM.VT_BYREF };
        int value = 5;
        innerByref.data.pintVal = &value;
        VARIANT outer = new() { vt = VARENUM.VT_VARIANT | VARENUM.VT_BYREF };
        outer.data.pvarVal = &innerByref;
        FluentActions.Invoking(() => outer.ToObject()).Should().Throw<InvalidOleVariantTypeException>();
    }

    [TestMethod]
    public unsafe void ToObject_VT_CLSID_Byref_Throws()
    {
        Guid g = Guid.NewGuid();
        Guid* pg = &g;
        VARIANT v = new() { vt = VARENUM.VT_CLSID | VARENUM.VT_BYREF };
        v.data.byref = &pg;
        FluentActions.Invoking(() => v.ToObject()).Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public unsafe void ToObject_VT_FILETIME_Byref_Throws()
    {
        FILETIME ft = default;
        VARIANT v = new() { vt = VARENUM.VT_FILETIME | VARENUM.VT_BYREF };
        v.data.byref = &ft;
        FluentActions.Invoking(() => v.ToObject()).Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public unsafe void ToObject_VT_RECORD_NullRecInfo_Throws()
    {
        VARIANT v = new() { vt = VARENUM.VT_RECORD };
        FluentActions.Invoking(() => v.ToObject()).Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public unsafe void ToObject_Byref_NullData_NonEmpty_Throws()
    {
        VARIANT v = new() { vt = VARENUM.VT_I4 | VARENUM.VT_BYREF };
        v.data.pintVal = null;
        FluentActions.Invoking(() => v.ToObject()).Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public unsafe void ToObject_VT_NULL_BYREF_NullData_ReturnsDBNull()
    {
        VARIANT v = new() { vt = VARENUM.VT_NULL | VARENUM.VT_BYREF };
        v.ToObject().Should().Be(Convert.DBNull);
    }

    // ===== ToArray (VT_ARRAY) coverage =====

    [TestMethod]
    public unsafe void ToObject_Array_VT_I1_ReturnsSbyteArray()
    {
        sbyte[] values = [-1, 2, -3];
        SAFEARRAY* psa = CreateSafeArray(VARENUM.VT_I1, values);
        try
        {
            VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_I1 };
            v.data.parray = psa;
            v.ToObject().Should().BeOfType<sbyte[]>().Which.Should().Equal(values);
        }
        finally
        {
            PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
        }
    }

    [TestMethod]
    public unsafe void ToObject_Array_VT_UI1_ReturnsByteArray()
    {
        byte[] values = [1, 2, 3];
        SAFEARRAY* psa = CreateSafeArray(VARENUM.VT_UI1, values);
        try
        {
            VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_UI1 };
            v.data.parray = psa;
            v.ToObject().Should().BeOfType<byte[]>().Which.Should().Equal(values);
        }
        finally
        {
            PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
        }
    }

    [TestMethod]
    public unsafe void ToObject_Array_VT_I2_ReturnsShortArray()
    {
        short[] values = [-1, 2];
        SAFEARRAY* psa = CreateSafeArray(VARENUM.VT_I2, values);
        try
        {
            VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_I2 };
            v.data.parray = psa;
            v.ToObject().Should().BeOfType<short[]>().Which.Should().Equal(values);
        }
        finally
        {
            PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
        }
    }

    [TestMethod]
    public unsafe void ToObject_Array_VT_UI2_ReturnsUshortArray()
    {
        ushort[] values = [1, 2];
        SAFEARRAY* psa = CreateSafeArray(VARENUM.VT_UI2, values);
        try
        {
            VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_UI2 };
            v.data.parray = psa;
            v.ToObject().Should().BeOfType<ushort[]>().Which.Should().Equal(values);
        }
        finally
        {
            PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
        }
    }

    [TestMethod]
    public unsafe void ToObject_Array_VT_UI4_ReturnsUintArray()
    {
        uint[] values = [1u, 2u];
        SAFEARRAY* psa = CreateSafeArray(VARENUM.VT_UI4, values);
        try
        {
            VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_UI4 };
            v.data.parray = psa;
            v.ToObject().Should().BeOfType<uint[]>().Which.Should().Equal(values);
        }
        finally
        {
            PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
        }
    }

    [TestMethod]
    public unsafe void ToObject_Array_VT_I8_ReturnsLongArray()
    {
        long[] values = [-1L, 2L];
        SAFEARRAY* psa = CreateSafeArray(VARENUM.VT_I8, values);
        try
        {
            VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_I8 };
            v.data.parray = psa;
            v.ToObject().Should().BeOfType<long[]>().Which.Should().Equal(values);
        }
        finally
        {
            PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
        }
    }

    [TestMethod]
    public unsafe void ToObject_Array_VT_UI8_ReturnsUlongArray()
    {
        ulong[] values = [1UL, 2UL];
        SAFEARRAY* psa = CreateSafeArray(VARENUM.VT_UI8, values);
        try
        {
            VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_UI8 };
            v.data.parray = psa;
            v.ToObject().Should().BeOfType<ulong[]>().Which.Should().Equal(values);
        }
        finally
        {
            PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
        }
    }

    [TestMethod]
    public unsafe void ToObject_Array_VT_R4_ReturnsFloatArray()
    {
        float[] values = [1.5f, 2.5f];
        SAFEARRAY* psa = CreateSafeArray(VARENUM.VT_R4, values);
        try
        {
            VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_R4 };
            v.data.parray = psa;
            v.ToObject().Should().BeOfType<float[]>().Which.Should().Equal(values);
        }
        finally
        {
            PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
        }
    }

    [TestMethod]
    public unsafe void ToObject_Array_VT_BOOL_ReturnsBoolArray()
    {
        VARIANT_BOOL[] values = [VARIANT_BOOL.VARIANT_TRUE, VARIANT_BOOL.VARIANT_FALSE];
        SAFEARRAY* psa = CreateSafeArray(VARENUM.VT_BOOL, values);
        try
        {
            VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_BOOL };
            v.data.parray = psa;
            bool[] expected = [true, false];
            v.ToObject().Should().BeOfType<bool[]>().Which.Should().Equal(expected);
        }
        finally
        {
            PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
        }
    }

    [TestMethod]
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
            v.ToObject().Should().BeOfType<DateTime[]>().Which.Should().Equal(expected);
        }
        finally
        {
            PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
        }
    }

    [TestMethod]
    public unsafe void ToObject_Array_VT_CY_ReturnsDecimalArray()
    {
        long[] values = [12345L, 67890L];
        SAFEARRAY* psa = CreateSafeArray(VARENUM.VT_CY, values);
        try
        {
            VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_CY };
            v.data.parray = psa;
            decimal[] expected = [1.2345m, 6.789m];
            v.ToObject().Should().BeOfType<decimal[]>().Which.Should().Equal(expected);
        }
        finally
        {
            PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
        }
    }

    [TestMethod]
    public unsafe void ToObject_Array_VT_DECIMAL_ReturnsDecimalArray()
    {
        // Regression guard: VARIANT.ToObject used to short-circuit on `Type == VT_DECIMAL`
        // (which masks off VT_ARRAY/VT_VECTOR/VT_BYREF), returning a bogus scalar decimal
        // instead of dispatching to the SAFEARRAY path.
        DECIMAL[] values = [new(1.5m), new(2.5m)];
        SAFEARRAY* psa = CreateSafeArray(VARENUM.VT_DECIMAL, values);
        try
        {
            VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_DECIMAL };
            v.data.parray = psa;
            decimal[] expected = [1.5m, 2.5m];
            v.ToObject().Should().BeOfType<decimal[]>().Which.Should().Equal(expected);
        }
        finally
        {
            PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
        }
    }

    [TestMethod]
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
        FluentActions.Invoking(() => v481.ToObject()).Should().Throw<ArgumentException>();
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
        object?[] array = v.ToObject().Should().BeOfType<object?[]>().Subject;
        object?[] expected = [1, 2];
        array.Should().Equal(expected);
#endif
    }

    [TestMethod]
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
            FluentActions.Invoking(() => v.ToObject()).Should().Throw<ArgumentException>();
        }
        finally
        {
            PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
        }
    }

    [TestMethod]
    public unsafe void ToObject_Array_NonZeroLowerBound_PreservesBounds()
    {
        SAFEARRAYBOUND bound = new() { cElements = 3, lLbound = 5 };
        SAFEARRAY* psa = PInvokeMadowaku.SafeArrayCreate(VARENUM.VT_I4, 1, &bound);
        (psa is null).Should().BeFalse();

        try
        {
            for (int i = 0; i < 3; i++)
            {
                int abs = i + 5;
                int value = i * 10;
                PInvokeMadowaku.SafeArrayPutElement(psa, &abs, &value).ThrowOnFailure();
            }

            VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_I4 };
            v.data.parray = psa;
            Array array = v.ToObject().Should().BeAssignableTo<Array>().Subject;
            array.Rank.Should().Be(1);
            array.GetLowerBound(0).Should().Be(5);
            array.Length.Should().Be(3);
            array.GetValue(5).Should().Be(0);
            array.GetValue(6).Should().Be(10);
            array.GetValue(7).Should().Be(20);
        }
        finally
        {
            if (psa is not null)
            {
                PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
            }
        }
    }

    [TestMethod]
    public unsafe void ToObject_Array_NonZeroLowerBound_VariousScalarTypes_RoundTrip()
    {
        Array i1 = ConvertNonZeroLowerBoundArray(VARENUM.VT_I1, VARENUM.VT_I1, (sbyte[])[-1, 2]);
        i1.GetValue(5).Should().Be((sbyte)-1);
        i1.GetValue(6).Should().Be((sbyte)2);

        Array ui1 = ConvertNonZeroLowerBoundArray(VARENUM.VT_UI1, VARENUM.VT_UI1, (byte[])[1, 2]);
        ui1.GetValue(5).Should().Be((byte)1);
        ui1.GetValue(6).Should().Be((byte)2);

        Array i2 = ConvertNonZeroLowerBoundArray(VARENUM.VT_I2, VARENUM.VT_I2, (short[])[-2, 3]);
        i2.GetValue(5).Should().Be((short)-2);
        i2.GetValue(6).Should().Be((short)3);

        Array ui2 = ConvertNonZeroLowerBoundArray(VARENUM.VT_UI2, VARENUM.VT_UI2, (ushort[])[2, 4]);
        ui2.GetValue(5).Should().Be((ushort)2);
        ui2.GetValue(6).Should().Be((ushort)4);

        Array i8 = ConvertNonZeroLowerBoundArray(VARENUM.VT_I8, VARENUM.VT_I8, (long[])[-3, 6]);
        i8.GetValue(5).Should().Be(-3L);
        i8.GetValue(6).Should().Be(6L);

        Array ui8 = ConvertNonZeroLowerBoundArray(VARENUM.VT_UI8, VARENUM.VT_UI8, (ulong[])[3, 6]);
        ui8.GetValue(5).Should().Be(3UL);
        ui8.GetValue(6).Should().Be(6UL);

        Array r4 = ConvertNonZeroLowerBoundArray(VARENUM.VT_R4, VARENUM.VT_R4, (float[])[1.25f, 2.5f]);
        r4.GetValue(5).Should().Be(1.25f);
        r4.GetValue(6).Should().Be(2.5f);
    }

    [TestMethod]
    public unsafe void ToObject_Array_NonZeroLowerBound_VT_DATE_BehaviorByTfm()
    {
        DateTime d1 = new(2024, 1, 1);
        DateTime d2 = new(2025, 1, 1);
        double[] values = [d1.ToOADate(), d2.ToOADate()];

#if NETFRAMEWORK
        FluentActions.Invoking(() => ConvertNonZeroLowerBoundArray(VARENUM.VT_DATE, VARENUM.VT_DATE, values))
            .Should().Throw<ArgumentException>();
#else
        Array dateArray = ConvertNonZeroLowerBoundArray(VARENUM.VT_DATE, VARENUM.VT_DATE, values);
        dateArray.GetValue(5).Should().Be(d1);
        dateArray.GetValue(6).Should().Be(d2);
#endif
    }

    [TestMethod]
    public unsafe void ToObject_Array_TypeMismatch_Throws()
    {
        double[] values = [1.0];
        SAFEARRAY* psa = CreateSafeArray(VARENUM.VT_R8, values);
        try
        {
            VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_I4 };
            v.data.parray = psa;
            FluentActions.Invoking(() => v.ToObject()).Should().Throw<SafeArrayTypeMismatchException>();
        }
        finally
        {
            PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
        }
    }

    [TestMethod]
    public unsafe void ToObject_Array_VT_EMPTY_ThrowsInvalidOleVariantTypeException()
    {
        int[] values = [1];
        SAFEARRAY* psa = CreateSafeArray(VARENUM.VT_I4, values);
        try
        {
            VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_EMPTY };
            v.data.parray = psa;
            FluentActions.Invoking(() => v.ToObject()).Should().Throw<InvalidOleVariantTypeException>();
        }
        finally
        {
            PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
        }
    }

    [TestMethod]
    public unsafe void ToObject_Array_VT_UNKNOWN_EmptySafeArray_ReturnsEmptyObjectArray()
    {
        SAFEARRAYBOUND bounds = new() { cElements = 0, lLbound = 0 };
        SAFEARRAY* psa = PInvokeMadowaku.SafeArrayCreate(VARENUM.VT_UNKNOWN, 1, &bounds);
        (psa is null).Should().BeFalse();

        try
        {
            VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_UNKNOWN };
            v.data.parray = psa;
            v.ToObject().Should().BeOfType<object?[]>().Which.Should().BeEmpty();
        }
        finally
        {
            if (psa is not null)
            {
                PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
            }
        }
    }

    [TestMethod]
    public unsafe void ToObject_Array_VT_DISPATCH_EmptySafeArray_ReturnsEmptyObjectArray()
    {
        SAFEARRAYBOUND bounds = new() { cElements = 0, lLbound = 0 };
        SAFEARRAY* psa = PInvokeMadowaku.SafeArrayCreate(VARENUM.VT_DISPATCH, 1, &bounds);
        (psa is null).Should().BeFalse();

        try
        {
            VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_DISPATCH };
            v.data.parray = psa;
            v.ToObject().Should().BeOfType<object?[]>().Which.Should().BeEmpty();
        }
        finally
        {
            if (psa is not null)
            {
                PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
            }
        }
    }

    [TestMethod]
    public unsafe void ToObject_Array_VT_INT_From_VT_I4_SafeArray_Succeeds()
    {
        int[] values = [7, 8];
        SAFEARRAY* psa = CreateSafeArray(VARENUM.VT_I4, values);
        try
        {
            VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_INT };
            v.data.parray = psa;
            v.ToObject().Should().BeOfType<int[]>().Which.Should().Equal(values);
        }
        finally
        {
            PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
        }
    }

    [TestMethod]
    public unsafe void ToObject_Array_VT_I4_From_VT_INT_SafeArray_Succeeds()
    {
        int[] values = [9, 10];
        SAFEARRAY* psa = CreateSafeArray(VARENUM.VT_INT, values);
        try
        {
            VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_I4 };
            v.data.parray = psa;
            v.ToObject().Should().BeOfType<int[]>().Which.Should().Equal(values);
        }
        finally
        {
            PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
        }
    }

    [TestMethod]
    public unsafe void ToObject_Array_VT_UINT_From_VT_UI4_SafeArray_Succeeds()
    {
        uint[] values = [11u, 12u];
        SAFEARRAY* psa = CreateSafeArray(VARENUM.VT_UI4, values);
        try
        {
            VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_UINT };
            v.data.parray = psa;
            v.ToObject().Should().BeOfType<uint[]>().Which.Should().Equal(values);
        }
        finally
        {
            PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
        }
    }

    [TestMethod]
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
            string[,] array = v.ToObject().Should().BeOfType<string[,]>().Subject;
            array.Length.Should().Be(4);
            List<string> items = [];
            for (int a = 0; a < array.GetLength(0); a++)
            {
                for (int b = 0; b < array.GetLength(1); b++)
                {
                    items.Add(array[a, b]);
                }
            }

            string[] expected = ["0,0", "0,1", "1,0", "1,1"];
            items.OrderBy(x => x).Should().Equal(expected);
        }
        finally
        {
            PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
        }
    }

    [TestMethod]
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
            double[,,] array = v.ToObject().Should().BeOfType<double[,,]>().Subject;
            array.Length.Should().Be(8);

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
            actual.OrderBy(x => x).Should().Equal(expected.OrderBy(x => x));
        }
        finally
        {
            PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
        }
    }

    [TestMethod]
    public unsafe void ToObject_Array_2D_VT_UINT_Transposes()
    {
        SAFEARRAYBOUND* bounds = stackalloc SAFEARRAYBOUND[2];
        bounds[0] = new SAFEARRAYBOUND { cElements = 2, lLbound = 0 };
        bounds[1] = new SAFEARRAYBOUND { cElements = 2, lLbound = 0 };
        SAFEARRAY* psa = PInvokeMadowaku.SafeArrayCreate(VARENUM.VT_UI4, 2, bounds);
        (psa is null).Should().BeFalse();

        try
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    uint value = (uint)(i * 10 + j);
                    Span<int> idx = [i, j];
                    fixed (int* p = idx)
                    {
                        PInvokeMadowaku.SafeArrayPutElement(psa, p, &value).ThrowOnFailure();
                    }
                }
            }

            VARIANT v = new() { vt = VARENUM.VT_ARRAY | VARENUM.VT_UINT };
            v.data.parray = psa;
            uint[,] array = v.ToObject().Should().BeOfType<uint[,]>().Subject;

            uint[] expected = [0u, 1u, 10u, 11u];
            List<uint> actual = [];
            for (int a = 0; a < array.GetLength(0); a++)
            {
                for (int b = 0; b < array.GetLength(1); b++)
                {
                    actual.Add(array[a, b]);
                }
            }

            actual.OrderBy(x => x).Should().Equal(expected.OrderBy(x => x));
        }
        finally
        {
            if (psa is not null)
            {
                PInvokeMadowaku.SafeArrayDestroy(psa).ThrowOnFailure();
            }
        }
    }

    // ===== ToVector (VT_VECTOR) coverage =====

    [TestMethod]
    public unsafe void ToObject_Vector_VT_I1_ReturnsSbyteArray()
    {
        sbyte[] values = [-1, 2];
        VARIANT v = MakeVector(VARENUM.VT_I1, values);
        try
        {
            v.ToObject().Should().BeOfType<sbyte[]>().Which.Should().Equal(values);
        }
        finally
        {
            FreeVector(ref v);
        }
    }

    [TestMethod]
    public unsafe void ToObject_Vector_VT_UI1_ReturnsByteArray()
    {
        byte[] values = [1, 2, 3];
        VARIANT v = MakeVector(VARENUM.VT_UI1, values);
        try
        {
            v.ToObject().Should().BeOfType<byte[]>().Which.Should().Equal(values);
        }
        finally
        {
            FreeVector(ref v);
        }
    }

    [TestMethod]
    public unsafe void ToObject_Vector_VT_I2_ReturnsShortArray()
    {
        short[] values = [-1, 2];
        VARIANT v = MakeVector(VARENUM.VT_I2, values);
        try
        {
            v.ToObject().Should().BeOfType<short[]>().Which.Should().Equal(values);
        }
        finally
        {
            FreeVector(ref v);
        }
    }

    [TestMethod]
    public unsafe void ToObject_Vector_VT_UI2_ReturnsUshortArray()
    {
        ushort[] values = [1, 2];
        VARIANT v = MakeVector(VARENUM.VT_UI2, values);
        try
        {
            v.ToObject().Should().BeOfType<ushort[]>().Which.Should().Equal(values);
        }
        finally
        {
            FreeVector(ref v);
        }
    }

    [TestMethod]
    public unsafe void ToObject_Vector_VT_I4_ReturnsIntArray()
    {
        int[] values = [1, 2, 3];
        VARIANT v = MakeVector(VARENUM.VT_I4, values);
        try
        {
            v.ToObject().Should().BeOfType<int[]>().Which.Should().Equal(values);
        }
        finally
        {
            FreeVector(ref v);
        }
    }

    [TestMethod]
    public unsafe void ToObject_Vector_VT_INT_ReturnsIntArray()
    {
        int[] values = [4, 5];
        VARIANT v = MakeVector(VARENUM.VT_INT, values);
        try
        {
            v.ToObject().Should().BeOfType<int[]>().Which.Should().Equal(values);
        }
        finally
        {
            FreeVector(ref v);
        }
    }

    [TestMethod]
    public unsafe void ToObject_Vector_VT_UI4_ReturnsUintArray()
    {
        uint[] values = [1u, 2u];
        VARIANT v = MakeVector(VARENUM.VT_UI4, values);
        try
        {
            v.ToObject().Should().BeOfType<uint[]>().Which.Should().Equal(values);
        }
        finally
        {
            FreeVector(ref v);
        }
    }

    [TestMethod]
    public unsafe void ToObject_Vector_VT_UINT_ReturnsUintArray()
    {
        uint[] values = [3u, 4u];
        VARIANT v = MakeVector(VARENUM.VT_UINT, values);
        try
        {
            v.ToObject().Should().BeOfType<uint[]>().Which.Should().Equal(values);
        }
        finally
        {
            FreeVector(ref v);
        }
    }

    [TestMethod]
    public unsafe void ToObject_Vector_VT_ERROR_ReturnsUintArray()
    {
        uint[] values = [5u];
        VARIANT v = MakeVector(VARENUM.VT_ERROR, values);
        try
        {
            v.ToObject().Should().BeOfType<uint[]>().Which.Should().Equal(values);
        }
        finally
        {
            FreeVector(ref v);
        }
    }

    [TestMethod]
    public unsafe void ToObject_Vector_VT_I8_ReturnsLongArray()
    {
        long[] values = [1L, 2L];
        VARIANT v = MakeVector(VARENUM.VT_I8, values);
        try
        {
            v.ToObject().Should().BeOfType<long[]>().Which.Should().Equal(values);
        }
        finally
        {
            FreeVector(ref v);
        }
    }

    [TestMethod]
    public unsafe void ToObject_Vector_VT_UI8_ReturnsUlongArray()
    {
        ulong[] values = [1UL, 2UL];
        VARIANT v = MakeVector(VARENUM.VT_UI8, values);
        try
        {
            v.ToObject().Should().BeOfType<ulong[]>().Which.Should().Equal(values);
        }
        finally
        {
            FreeVector(ref v);
        }
    }

    [TestMethod]
    public unsafe void ToObject_Vector_VT_R4_ReturnsFloatArray()
    {
        float[] values = [1.5f, 2.5f];
        VARIANT v = MakeVector(VARENUM.VT_R4, values);
        try
        {
            v.ToObject().Should().BeOfType<float[]>().Which.Should().Equal(values);
        }
        finally
        {
            FreeVector(ref v);
        }
    }

    [TestMethod]
    public unsafe void ToObject_Vector_VT_R8_ReturnsDoubleArray()
    {
        double[] values = [1.5, 2.5];
        VARIANT v = MakeVector(VARENUM.VT_R8, values);
        try
        {
            v.ToObject().Should().BeOfType<double[]>().Which.Should().Equal(values);
        }
        finally
        {
            FreeVector(ref v);
        }
    }

    [TestMethod]
    public unsafe void ToObject_Vector_VT_BOOL_ReturnsBoolArray()
    {
        VARIANT_BOOL[] values = [VARIANT_BOOL.VARIANT_TRUE, VARIANT_BOOL.VARIANT_FALSE];
        VARIANT v = MakeVector(VARENUM.VT_BOOL, values);
        try
        {
            bool[] expected = [true, false];
            v.ToObject().Should().BeOfType<bool[]>().Which.Should().Equal(expected);
        }
        finally
        {
            FreeVector(ref v);
        }
    }

    [TestMethod]
    public unsafe void ToObject_Vector_VT_CY_ReturnsDecimalArray()
    {
        long[] values = [12345L];
        VARIANT v = MakeVector(VARENUM.VT_CY, values);
        try
        {
            decimal[] expected = [1.2345m];
            v.ToObject().Should().BeOfType<decimal[]>().Which.Should().Equal(expected);
        }
        finally
        {
            FreeVector(ref v);
        }
    }

    [TestMethod]
    public unsafe void ToObject_Vector_VT_DATE_ReturnsDateTimeArray()
    {
        DateTime d = new(2024, 1, 1);
        double[] values = [d.ToOADate()];
        VARIANT v = MakeVector(VARENUM.VT_DATE, values);
        try
        {
            DateTime[] expected = [d];
            v.ToObject().Should().BeOfType<DateTime[]>().Which.Should().Equal(expected);
        }
        finally
        {
            FreeVector(ref v);
        }
    }

    [TestMethod]
    public unsafe void ToObject_Vector_VT_FILETIME_ReturnsDateTimeArray()
    {
        DateTime expected = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc).ToLocalTime();
        FILETIME ft = (FILETIME)expected;
        FILETIME[] values = [ft];
        VARIANT v = MakeVector(VARENUM.VT_FILETIME, values);
        try
        {
            DateTime[] expectedArray = [expected];
            v.ToObject().Should().BeOfType<DateTime[]>().Which.Should().Equal(expectedArray);
        }
        finally
        {
            FreeVector(ref v);
        }
    }

    [TestMethod]
    public unsafe void ToObject_Vector_VT_CLSID_ReturnsGuidArray()
    {
        Guid g1 = Guid.NewGuid();
        Guid g2 = Guid.NewGuid();
        Guid[] values = [g1, g2];
        VARIANT v = MakeVector(VARENUM.VT_CLSID, values);
        try
        {
            v.ToObject().Should().BeOfType<Guid[]>().Which.Should().Equal(values);
        }
        finally
        {
            FreeVector(ref v);
        }
    }

    [TestMethod]
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
                string?[] array = v.ToObject().Should().BeOfType<string?[]>().Subject;
                string?[] expected = ["alpha", "beta"];
                array.Should().Equal(expected);
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

    [TestMethod]
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
                string?[] array = v.ToObject().Should().BeOfType<string?[]>().Subject;
                string?[] expected = ["one", "two"];
                array.Should().Equal(expected);
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

    [TestMethod]
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
                string?[] array = v.ToObject().Should().BeOfType<string?[]>().Subject;
                string?[] expected = ["one", "two"];
                array.Should().Equal(expected);
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

    [TestMethod]
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
            FluentActions.Invoking(() => v481.ToObject()).Should().Throw<ArgumentException>();
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
            object?[] array = v.ToObject().Should().BeOfType<object?[]>().Subject;
            object?[] expected = [1, 2];
            array.Should().Equal(expected);
        }
        finally
        {
            FreeVector(ref v);
        }
#endif
    }

    [TestMethod]
    public unsafe void ToObject_Vector_UnsupportedType_Throws()
    {
        int[] values = [0];
        VARIANT v = MakeVector(VARENUM.VT_CF, values);
        try
        {
            FluentActions.Invoking(() => v.ToObject()).Should().Throw<ArgumentException>();
        }
        finally
        {
            FreeVector(ref v);
        }
    }
}
