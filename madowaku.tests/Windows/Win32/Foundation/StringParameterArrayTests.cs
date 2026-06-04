// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

namespace Windows.Win32.Foundation;

[TestClass]
public class StringParameterArrayTests
{
    [TestMethod]
    public unsafe void Constructor_NullArray_ProducesNullPointer()
    {
        using StringParameterArray array = new(null);
        char** ptr = array;
        (ptr is null).Should().BeTrue();
    }

    [TestMethod]
    public unsafe void Constructor_EmptyArray_ProducesNullPointer()
    {
        using StringParameterArray array = new([]);
        char** ptr = array;
        (ptr is null).Should().BeTrue();
    }

    [TestMethod]
    public unsafe void Constructor_PopulatedArray_PointersPointToPinnedStrings()
    {
        string[] values = ["alpha", "beta", "gamma"];
        using StringParameterArray array = new(values);
        char** ptr = array;
        (ptr is null).Should().BeFalse();

        for (int i = 0; i < values.Length; i++)
        {
            string actual = new(ptr[i]);
            actual.Should().Be(values[i]);
        }
    }

    [TestMethod]
    public unsafe void ImplicitOperator_SbyteDoublePointer_NonNullForPopulated()
    {
        string[] values = ["x"];
        using StringParameterArray array = new(values);
        sbyte** ptr = array;
        (ptr is null).Should().BeFalse();
    }

    [TestMethod]
    public unsafe void ImplicitOperator_SbyteDoublePointer_NullForEmpty()
    {
        using StringParameterArray array = new(null);
        sbyte** ptr = array;
        (ptr is null).Should().BeTrue();
    }

    [TestMethod]
    public void Dispose_NullArray_DoesNotThrow()
    {
        StringParameterArray array = new(null);
        array.Dispose();
    }
}
