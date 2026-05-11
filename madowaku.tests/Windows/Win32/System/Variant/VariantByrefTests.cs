// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using Windows.Win32.Foundation;

namespace Windows.Win32.System.Variant;

public class VariantByrefTests
{
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
    public unsafe void FromObject_ViaMarshal_DateTime_ProducesDateTimeVariant()
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
}
