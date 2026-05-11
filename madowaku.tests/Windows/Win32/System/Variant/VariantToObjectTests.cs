// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using System.Runtime.CompilerServices;
using Windows.Win32.System.Com;

namespace Windows.Win32.System.Variant;

public class VariantToObjectTests
{
    private static VARIANT MakeScalar<T>(VARENUM type, T value) where T : unmanaged
    {
        VARIANT v = new() { vt = type };
        Unsafe.As<VARIANT._Anonymous_e__Union._Anonymous_e__Struct._Anonymous_e__Union, T>(ref v.data) = value;
        return v;
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
}
