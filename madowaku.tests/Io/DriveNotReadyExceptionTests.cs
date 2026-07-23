// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using Windows.Win32.Foundation;

namespace Madowaku.Io;

[TestClass]
public class DriveNotReadyExceptionTests
{
    [TestMethod]
    public void Constructor_SetsHResultForErrorNotReady()
    {
        DriveNotReadyException exception = new();
        exception.HResult.Should().Be((int)WIN32_ERROR.ERROR_NOT_READY.ToHRESULT());
    }

    [TestMethod]
    public void Constructor_NullMessage_UsesErrorToString()
    {
        DriveNotReadyException exception = new();
        exception.Message.Should().Be(WIN32_ERROR.ERROR_NOT_READY.ErrorToString());
    }

    [TestMethod]
    public void Constructor_WithMessage_UsesGivenMessage()
    {
        DriveNotReadyException exception = new("drive is not ready");
        exception.Message.Should().Be("drive is not ready");
    }
}
