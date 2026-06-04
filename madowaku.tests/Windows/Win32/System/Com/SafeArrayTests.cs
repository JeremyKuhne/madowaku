// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using Windows.Win32.System.Variant;

namespace Windows.Win32.System.Com;

[TestClass]
public class SafeArrayTests
{
    [TestMethod]
    public unsafe void VarType_IntArray_ReturnsVT_I4()
    {
        using SafeArrayScope<int> array = new(2);
        array.Value->VarType.Should().Be(VARENUM.VT_I4);
    }

    [TestMethod]
    public unsafe void VarType_StringArray_ReturnsVT_BSTR()
    {
        using SafeArrayScope<string> array = new(1);
        array.Value->VarType.Should().Be(VARENUM.VT_BSTR);
    }

    [TestMethod]
    public unsafe void VarType_DoubleArray_ReturnsVT_R8()
    {
        using SafeArrayScope<double> array = new(1);
        array.Value->VarType.Should().Be(VARENUM.VT_R8);
    }

    [TestMethod]
    public unsafe void VarType_ObjectArray_ReturnsVT_VARIANT()
    {
        using SafeArrayScope<object> array = new(1);
        array.Value->VarType.Should().Be(VARENUM.VT_VARIANT);
    }

    [TestMethod]
    public unsafe void GetBounds_OneDimensionalArray_ReturnsExpectedBound()
    {
        using SafeArrayScope<int> array = new(5);
        SAFEARRAYBOUND bound = array.Value->GetBounds();
        bound.cElements.Should().Be(5u);
        bound.lLbound.Should().Be(0);
    }

    [TestMethod]
    public unsafe void GetValue_OneDimensional_ReadsBackInOrder()
    {
        using SafeArrayScope<int> array = new([10, 20, 30]);
        Span<int> indices0 = [0];
        Span<int> indices1 = [1];
        Span<int> indices2 = [2];
        array.Value->GetValue<int>(indices0).Should().Be(10);
        array.Value->GetValue<int>(indices1).Should().Be(20);
        array.Value->GetValue<int>(indices2).Should().Be(30);
    }
}
