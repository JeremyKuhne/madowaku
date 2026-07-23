// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using System.ComponentModel;

namespace Windows.Win32.Foundation;

[TestClass]
public class Win32ExceptionExtensionsTests
{
    [TestMethod]
    public void Create_FromWin32Error_SetsNativeErrorCode()
    {
        Win32Exception exception = Win32ExceptionExtensions.Create(WIN32_ERROR.ERROR_FILE_NOT_FOUND);
        exception.NativeErrorCode.Should().Be((int)WIN32_ERROR.ERROR_FILE_NOT_FOUND);
    }

    [TestMethod]
    public void Create_FromWin32Error_SetsHResultFromHResultForWin32()
    {
        Win32Exception exception = Win32ExceptionExtensions.Create(WIN32_ERROR.ERROR_FILE_NOT_FOUND);
        exception.HResult.Should().Be(unchecked((int)0x80070002));
    }

    [TestMethod]
    public void Create_FromWin32Error_Success_SetsHResultZero()
    {
        Win32Exception exception = Win32ExceptionExtensions.Create(WIN32_ERROR.ERROR_SUCCESS);
        exception.HResult.Should().Be(0);
    }

    [TestMethod]
    public void Create_FromWin32Error_NullMessage_UsesSystemMessage()
    {
        Win32Exception exception = Win32ExceptionExtensions.Create(WIN32_ERROR.ERROR_FILE_NOT_FOUND);
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [TestMethod]
    public void Create_FromWin32Error_WithMessage_UsesGivenMessage()
    {
        Win32Exception exception = Win32ExceptionExtensions.Create(WIN32_ERROR.ERROR_FILE_NOT_FOUND, "custom message");
        exception.Message.Should().Be("custom message");
    }

    [TestMethod]
    public void Create_FromHRESULT_Win32Facility_SetsNativeErrorCodeToHResultCode()
    {
        HRESULT hr = (HRESULT)WIN32_ERROR.ERROR_FILE_NOT_FOUND;
        Win32Exception exception = Win32ExceptionExtensions.Create(hr);
        exception.NativeErrorCode.Should().Be((int)WIN32_ERROR.ERROR_FILE_NOT_FOUND);
    }

    [TestMethod]
    public void Create_FromHRESULT_Win32Facility_SetsHResultToRawValue()
    {
        HRESULT hr = (HRESULT)WIN32_ERROR.ERROR_FILE_NOT_FOUND;
        Win32Exception exception = Win32ExceptionExtensions.Create(hr);
        exception.HResult.Should().Be(hr.Value);
    }

    [TestMethod]
    public void Create_FromHRESULT_NonWin32Facility_SetsNativeErrorCodeToRawValue()
    {
        Win32Exception exception = Win32ExceptionExtensions.Create(HRESULT.COR_E_OBJECTDISPOSED);
        exception.NativeErrorCode.Should().Be(HRESULT.COR_E_OBJECTDISPOSED.Value);
    }

    [TestMethod]
    public void Create_FromHRESULT_NonWin32Facility_SetsHResultToRawValue()
    {
        Win32Exception exception = Win32ExceptionExtensions.Create(HRESULT.COR_E_OBJECTDISPOSED);
        exception.HResult.Should().Be(HRESULT.COR_E_OBJECTDISPOSED.Value);
    }

    [TestMethod]
    public void Create_FromHRESULT_WithMessage_UsesGivenMessage()
    {
        Win32Exception exception = Win32ExceptionExtensions.Create(HRESULT.COR_E_OBJECTDISPOSED, "custom message");
        exception.Message.Should().Be("custom message");
    }

    [TestMethod]
    public void Create_FromHRESULT_NullMessage_UsesSystemMessage()
    {
        Win32Exception exception = Win32ExceptionExtensions.Create((HRESULT)WIN32_ERROR.ERROR_FILE_NOT_FOUND);
        exception.Message.Should().NotBeNullOrEmpty();
    }
}
