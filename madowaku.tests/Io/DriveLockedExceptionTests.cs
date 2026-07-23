// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using Windows.Win32.Foundation;

namespace Madowaku.Io;

[TestClass]
public class DriveLockedExceptionTests
{
    [TestMethod]
    public void Constructor_SetsHResultToFveLockedVolume()
    {
        DriveLockedException exception = new();
        exception.HResult.Should().Be(HRESULT.FVE_E_LOCKED_VOLUME.Value);
    }

    [TestMethod]
    public void Constructor_NullMessage_UsesDescription()
    {
        DriveLockedException exception = new();
        exception.Message.Should().Be(HRESULT.FVE_E_LOCKED_VOLUME.ToStringWithDescription());
    }

    [TestMethod]
    public void Constructor_WithMessage_UsesGivenMessage()
    {
        DriveLockedException exception = new("drive is locked");
        exception.Message.Should().Be("drive is locked");
    }
}
