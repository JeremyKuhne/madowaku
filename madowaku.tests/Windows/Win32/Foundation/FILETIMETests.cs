// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

namespace Windows.Win32.Foundation;

[TestClass]
public class FILETIMETests
{
    [TestMethod]
    public void ExplicitCast_DateTimeRoundTrip_PreservesValue()
    {
        DateTime original = new DateTime(2025, 5, 11, 13, 45, 30, DateTimeKind.Utc).ToLocalTime();
        FILETIME ft = (FILETIME)original;
        DateTime roundTripped = (DateTime)ft;
        roundTripped.Should().Be(original);
    }

    [TestMethod]
    public void ExplicitCast_FromDateTime_PopulatesLowAndHighParts()
    {
        DateTime value = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToLocalTime();
        FILETIME ft = (FILETIME)value;
        long combined = ((long)ft.dwHighDateTime << 32) | ft.dwLowDateTime;
        combined.Should().Be(value.ToFileTime());
    }
}
