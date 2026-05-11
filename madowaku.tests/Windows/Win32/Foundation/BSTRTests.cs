// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

namespace Windows.Win32.Foundation;

public class BSTRTests
{
    [Fact]
    public void Constructor_ManagedString_RoundTripsValue()
    {
        using BSTR bstr = new("hello");
        Assert.False(bstr.IsNull);
        Assert.Equal("hello", bstr.ToString());
    }

    [Fact]
    public void IsNull_DefaultInstance_ReturnsTrue()
    {
        BSTR bstr = default;
        Assert.True(bstr.IsNull);
    }

    [Fact]
    public void ToStringAndFree_ReturnsValueAndFrees()
    {
        BSTR bstr = new("disposable");
        string result = bstr.ToStringAndFree();
        Assert.Equal("disposable", result);
        Assert.True(bstr.IsNull);
    }

    [Fact]
    public void Dispose_ClearsPointer()
    {
        BSTR bstr = new("clear me");
        bstr.Dispose();
        Assert.True(bstr.IsNull);
    }

    [Fact]
    public void TryFormat_DestinationLargeEnough_WritesAllChars()
    {
        using BSTR bstr = new("abc");
        Span<char> buffer = stackalloc char[8];
        bool result = bstr.TryFormat(buffer, out int written, default, null);
        Assert.True(result);
        Assert.Equal(3, written);
        Assert.Equal("abc", buffer[..written].ToString());
    }

    [Fact]
    public void TryFormat_DestinationTooSmall_ReturnsFalse()
    {
        using BSTR bstr = new("abcdef");
        Span<char> buffer = stackalloc char[3];
        bool result = bstr.TryFormat(buffer, out int written, default, null);
        Assert.False(result);
        Assert.Equal(0, written);
    }

    [Fact]
    public void ToStringWithFormat_IgnoresFormatAndProvider()
    {
        using BSTR bstr = new("value");
        Assert.Equal("value", bstr.ToString("X", null));
    }
}
