// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using Windows.Win32.Foundation;

namespace Madowaku.Io;

[TestClass]
public class MadowakuIoExceptionTests
{
    [TestMethod]
    public void Constructor_Default_DoesNotThrow()
    {
        MadowakuIoException exception = new();
        exception.Should().NotBeNull();
    }

    [TestMethod]
    public void Constructor_HRESULT_SetsHResult()
    {
        MadowakuIoException exception = new(HRESULT.COR_E_OBJECTDISPOSED);
        exception.HResult.Should().Be(HRESULT.COR_E_OBJECTDISPOSED.Value);
    }

    [TestMethod]
    public void Constructor_HRESULT_NullMessage_UsesDescription()
    {
        MadowakuIoException exception = new(HRESULT.COR_E_OBJECTDISPOSED);
        exception.Message.Should().Be(HRESULT.COR_E_OBJECTDISPOSED.ToStringWithDescription());
    }

    [TestMethod]
    public void Constructor_HRESULT_WithMessage_UsesGivenMessage()
    {
        MadowakuIoException exception = new(HRESULT.COR_E_OBJECTDISPOSED, "custom message");
        exception.Message.Should().Be("custom message");
    }

    [TestMethod]
    public void Constructor_WIN32_ERROR_SetsHResultFromHResultForWin32()
    {
        MadowakuIoException exception = new(WIN32_ERROR.ERROR_FILE_NOT_FOUND);
        exception.HResult.Should().Be(unchecked((int)0x80070002));
    }

    [TestMethod]
    public void Constructor_WIN32_ERROR_NullMessage_UsesErrorToString()
    {
        MadowakuIoException exception = new(WIN32_ERROR.ERROR_FILE_NOT_FOUND);
        exception.Message.Should().Be(WIN32_ERROR.ERROR_FILE_NOT_FOUND.ErrorToString());
    }

    [TestMethod]
    public void Constructor_WIN32_ERROR_WithMessage_UsesGivenMessage()
    {
        MadowakuIoException exception = new(WIN32_ERROR.ERROR_FILE_NOT_FOUND, "custom message");
        exception.Message.Should().Be("custom message");
    }
}
