// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using Windows.Win32.System.Variant;

namespace Windows.Win32.System.Com;

public class SafeArrayTests
{
    [Fact]
    public unsafe void VarType_IntArray_ReturnsVT_I4()
    {
        using SafeArrayScope<int> array = new(2);
        Assert.Equal(VARENUM.VT_I4, array.Value->VarType);
    }

    [Fact]
    public unsafe void VarType_StringArray_ReturnsVT_BSTR()
    {
        using SafeArrayScope<string> array = new(1);
        Assert.Equal(VARENUM.VT_BSTR, array.Value->VarType);
    }

    [Fact]
    public unsafe void VarType_DoubleArray_ReturnsVT_R8()
    {
        using SafeArrayScope<double> array = new(1);
        Assert.Equal(VARENUM.VT_R8, array.Value->VarType);
    }

    [Fact]
    public unsafe void VarType_ObjectArray_ReturnsVT_VARIANT()
    {
        using SafeArrayScope<object> array = new(1);
        Assert.Equal(VARENUM.VT_VARIANT, array.Value->VarType);
    }

    [Fact]
    public unsafe void GetBounds_OneDimensionalArray_ReturnsExpectedBound()
    {
        using SafeArrayScope<int> array = new(5);
        SAFEARRAYBOUND bound = array.Value->GetBounds();
        Assert.Equal(5u, bound.cElements);
        Assert.Equal(0, bound.lLbound);
    }

    [Fact]
    public unsafe void GetValue_OneDimensional_ReadsBackInOrder()
    {
        using SafeArrayScope<int> array = new([10, 20, 30]);
        Span<int> indices0 = [0];
        Span<int> indices1 = [1];
        Span<int> indices2 = [2];
        Assert.Equal(10, array.Value->GetValue<int>(indices0));
        Assert.Equal(20, array.Value->GetValue<int>(indices1));
        Assert.Equal(30, array.Value->GetValue<int>(indices2));
    }
}
