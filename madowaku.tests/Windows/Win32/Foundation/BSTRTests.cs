// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

namespace Windows.Win32.Foundation;

[TestClass]
public class BSTRTests
{
    [TestMethod]
    public void Constructor_ManagedString_RoundTripsValue()
    {
        using BSTR bstr = new("hello");
        bstr.IsNull.Should().BeFalse();
        bstr.ToString().Should().Be("hello");
    }

    [TestMethod]
    public void IsNull_DefaultInstance_ReturnsTrue()
    {
        BSTR bstr = default;
        bstr.IsNull.Should().BeTrue();
    }

    [TestMethod]
    public void ToStringAndFree_ReturnsValueAndFrees()
    {
        BSTR bstr = new("disposable");
        string result = bstr.ToStringAndFree();
        result.Should().Be("disposable");
        bstr.IsNull.Should().BeTrue();
    }

    [TestMethod]
    public void Dispose_ClearsPointer()
    {
        BSTR bstr = new("clear me");
        bstr.Dispose();
        bstr.IsNull.Should().BeTrue();
    }

    [TestMethod]
    public void TryFormat_DestinationLargeEnough_WritesAllChars()
    {
        using BSTR bstr = new("abc");
        Span<char> buffer = stackalloc char[8];
        bool result = bstr.TryFormat(buffer, out int written, default, null);
        result.Should().BeTrue();
        written.Should().Be(3);
        buffer[..written].ToString().Should().Be("abc");
    }

    [TestMethod]
    public void TryFormat_DestinationTooSmall_ReturnsFalse()
    {
        using BSTR bstr = new("abcdef");
        Span<char> buffer = stackalloc char[3];
        bool result = bstr.TryFormat(buffer, out int written, default, null);
        result.Should().BeFalse();
        written.Should().Be(0);
    }

    [TestMethod]
    public void ToStringWithFormat_IgnoresFormatAndProvider()
    {
        using BSTR bstr = new("value");
        bstr.ToString("X", null).Should().Be("value");
    }
}
