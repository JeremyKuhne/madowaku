// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

namespace Windows.Win32.Foundation;

public class StringParameterArrayTests
{
    [Fact]
    public unsafe void Constructor_NullArray_ProducesNullPointer()
    {
        using StringParameterArray array = new(null);
        char** ptr = array;
        Assert.True(ptr is null);
    }

    [Fact]
    public unsafe void Constructor_EmptyArray_ProducesNullPointer()
    {
        using StringParameterArray array = new([]);
        char** ptr = array;
        Assert.True(ptr is null);
    }

    [Fact]
    public unsafe void Constructor_PopulatedArray_PointersPointToPinnedStrings()
    {
        string[] values = ["alpha", "beta", "gamma"];
        using StringParameterArray array = new(values);
        char** ptr = array;
        Assert.False(ptr is null);

        for (int i = 0; i < values.Length; i++)
        {
            string actual = new(ptr[i]);
            Assert.Equal(values[i], actual);
        }
    }

    [Fact]
    public unsafe void ImplicitOperator_SbyteDoublePointer_NonNullForPopulated()
    {
        string[] values = ["x"];
        using StringParameterArray array = new(values);
        sbyte** ptr = array;
        Assert.False(ptr is null);
    }

    [Fact]
    public unsafe void ImplicitOperator_SbyteDoublePointer_NullForEmpty()
    {
        using StringParameterArray array = new(null);
        sbyte** ptr = array;
        Assert.True(ptr is null);
    }

    [Fact]
    public void Dispose_NullArray_DoesNotThrow()
    {
        StringParameterArray array = new(null);
        array.Dispose();
    }
}
