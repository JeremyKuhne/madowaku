// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using System.ComponentModel;

namespace Windows.Win32.Foundation;

[TestClass]
public class ErrorTests
{
    [TestMethod]
    public void ToHRESULT_Success_ReturnsZero()
    {
        HRESULT hr = WIN32_ERROR.ERROR_SUCCESS.ToHRESULT();
        hr.Value.Should().Be(0);
    }

    [TestMethod]
    public void ToHRESULT_NonSuccess_SetsFacilityWin32()
    {
        HRESULT hr = WIN32_ERROR.ERROR_ACCESS_DENIED.ToHRESULT();
        hr.Value.Should().Be(unchecked((int)0x80070005));
    }

    [TestMethod]
    public void ErrorToString_Success_StartsWithErrorSuccess()
    {
        string text = WIN32_ERROR.ERROR_SUCCESS.ErrorToString();
        text.Should().StartWith("ERROR_SUCCESS");
    }

    [TestMethod]
    public void ErrorToString_KnownError_IncludesEnumName()
    {
        string text = WIN32_ERROR.ERROR_FILE_NOT_FOUND.ErrorToString();
        text.Should().Contain("ERROR_FILE_NOT_FOUND");
    }

    [TestMethod]
    public void GetException_FileNotFound_ReturnsFileNotFoundException()
    {
        Exception ex = WIN32_ERROR.ERROR_FILE_NOT_FOUND.GetException("foo.txt");
        ex.Should().BeOfType<FileNotFoundException>();
    }

    [TestMethod]
    public void GetException_PathNotFound_ReturnsDirectoryNotFoundException()
    {
        Exception ex = WIN32_ERROR.ERROR_PATH_NOT_FOUND.GetException();
        ex.Should().BeOfType<DirectoryNotFoundException>();
    }

    [TestMethod]
    public void GetException_AccessDenied_ReturnsUnauthorizedAccessException()
    {
        Exception ex = WIN32_ERROR.ERROR_ACCESS_DENIED.GetException();
        ex.Should().BeOfType<UnauthorizedAccessException>();
    }

    [TestMethod]
    public void GetException_InvalidParameter_ReturnsArgumentException()
    {
        Exception ex = WIN32_ERROR.ERROR_INVALID_PARAMETER.GetException();
        ex.Should().BeOfType<ArgumentException>();
    }

    [TestMethod]
    public void GetException_NotSupported_ReturnsNotSupportedException()
    {
        Exception ex = WIN32_ERROR.ERROR_NOT_SUPPORTED.GetException();
        ex.Should().BeOfType<NotSupportedException>();
    }

    [TestMethod]
    public void Throw_AlwaysThrows()
    {
        FluentActions.Invoking(() => WIN32_ERROR.ERROR_FILE_NOT_FOUND.Throw()).Should().Throw<FileNotFoundException>();
    }

    [TestMethod]
    public void ThrowIfFailed_Success_DoesNotThrow()
    {
        WIN32_ERROR.ERROR_SUCCESS.ThrowIfFailed();
    }

    [TestMethod]
    public void ThrowIfFailed_Failure_Throws()
    {
        FluentActions.Invoking(() => WIN32_ERROR.ERROR_FILE_NOT_FOUND.ThrowIfFailed()).Should().Throw<FileNotFoundException>();
    }

    [TestMethod]
    public void GetException_FilenameExcedRange_ReturnsPathTooLongException()
    {
        Exception ex = WIN32_ERROR.ERROR_FILENAME_EXCED_RANGE.GetException();
        ex.Should().BeOfType<PathTooLongException>();
    }

    [TestMethod]
    public void GetException_InvalidDrive_ReturnsDriveNotFoundException()
    {
        Exception ex = WIN32_ERROR.ERROR_INVALID_DRIVE.GetException();
        ex.Should().BeOfType<DriveNotFoundException>();
    }

    [TestMethod]
    public void GetException_OperationAborted_ReturnsOperationCanceledException()
    {
        Exception ex = WIN32_ERROR.ERROR_OPERATION_ABORTED.GetException();
        ex.Should().BeOfType<OperationCanceledException>();
    }

    [TestMethod]
    public void GetException_Cancelled_ReturnsOperationCanceledException()
    {
        Exception ex = WIN32_ERROR.ERROR_CANCELLED.GetException();
        ex.Should().BeOfType<OperationCanceledException>();
    }

    [TestMethod]
    public void GetException_NetworkAccessDenied_ReturnsUnauthorizedAccessException()
    {
        Exception ex = WIN32_ERROR.ERROR_NETWORK_ACCESS_DENIED.GetException();
        ex.Should().BeOfType<UnauthorizedAccessException>();
    }

    [TestMethod]
    public void GetException_NotReady_ReturnsWin32Exception()
    {
        Exception ex = WIN32_ERROR.ERROR_NOT_READY.GetException();
        ex.Should().BeOfType<Win32Exception>();
    }

    [TestMethod]
    public void GetException_FileExists_ReturnsWin32Exception()
    {
        Exception ex = WIN32_ERROR.ERROR_FILE_EXISTS.GetException();
        ex.Should().BeOfType<Win32Exception>();
    }

    [TestMethod]
    public void GetException_AlreadyExists_ReturnsWin32Exception()
    {
        Exception ex = WIN32_ERROR.ERROR_ALREADY_EXISTS.GetException();
        ex.Should().BeOfType<Win32Exception>();
    }

    [TestMethod]
    public void GetException_NotSupportedInAppContainer_ReturnsNotSupportedException()
    {
        Exception ex = WIN32_ERROR.ERROR_NOT_SUPPORTED_IN_APPCONTAINER.GetException();
        ex.Should().BeOfType<NotSupportedException>();
    }

    [TestMethod]
    public void GetException_UnknownError_ReturnsWin32Exception()
    {
        Exception ex = ((WIN32_ERROR)9999).GetException();
        ex.Should().BeOfType<Win32Exception>();
    }

    [TestMethod]
    public void ThrowIfLastErrorNot_LastErrorMatches_DoesNotThrow()
    {
        // Read the current last-error so the test doesn't depend on prior state, then assert
        // ThrowIfLastErrorNot matches it.
        WIN32_ERROR current = Error.GetLastError();
        Error.ThrowIfLastErrorNot(current);
    }
}
