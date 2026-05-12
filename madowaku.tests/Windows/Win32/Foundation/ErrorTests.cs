// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using System.ComponentModel;

namespace Windows.Win32.Foundation;

public class ErrorTests
{
    [Fact]
    public void ToHRESULT_Success_ReturnsZero()
    {
        HRESULT hr = WIN32_ERROR.ERROR_SUCCESS.ToHRESULT();
        Assert.Equal(0, hr.Value);
    }

    [Fact]
    public void ToHRESULT_NonSuccess_SetsFacilityWin32()
    {
        HRESULT hr = WIN32_ERROR.ERROR_ACCESS_DENIED.ToHRESULT();
        Assert.Equal(unchecked((int)0x80070005), hr.Value);
    }

    [Fact]
    public void ErrorToString_Success_StartsWithErrorSuccess()
    {
        string text = WIN32_ERROR.ERROR_SUCCESS.ErrorToString();
        Assert.StartsWith("ERROR_SUCCESS", text);
    }

    [Fact]
    public void ErrorToString_KnownError_IncludesEnumName()
    {
        string text = WIN32_ERROR.ERROR_FILE_NOT_FOUND.ErrorToString();
        Assert.Contains("ERROR_FILE_NOT_FOUND", text);
    }

    [Fact]
    public void GetException_FileNotFound_ReturnsFileNotFoundException()
    {
        Exception ex = WIN32_ERROR.ERROR_FILE_NOT_FOUND.GetException("foo.txt");
        Assert.IsType<FileNotFoundException>(ex);
    }

    [Fact]
    public void GetException_PathNotFound_ReturnsDirectoryNotFoundException()
    {
        Exception ex = WIN32_ERROR.ERROR_PATH_NOT_FOUND.GetException();
        Assert.IsType<DirectoryNotFoundException>(ex);
    }

    [Fact]
    public void GetException_AccessDenied_ReturnsUnauthorizedAccessException()
    {
        Exception ex = WIN32_ERROR.ERROR_ACCESS_DENIED.GetException();
        Assert.IsType<UnauthorizedAccessException>(ex);
    }

    [Fact]
    public void GetException_InvalidParameter_ReturnsArgumentException()
    {
        Exception ex = WIN32_ERROR.ERROR_INVALID_PARAMETER.GetException();
        Assert.IsType<ArgumentException>(ex);
    }

    [Fact]
    public void GetException_NotSupported_ReturnsNotSupportedException()
    {
        Exception ex = WIN32_ERROR.ERROR_NOT_SUPPORTED.GetException();
        Assert.IsType<NotSupportedException>(ex);
    }

    [Fact]
    public void Throw_AlwaysThrows()
    {
        Assert.Throws<FileNotFoundException>(() => WIN32_ERROR.ERROR_FILE_NOT_FOUND.Throw());
    }

    [Fact]
    public void ThrowIfFailed_Success_DoesNotThrow()
    {
        WIN32_ERROR.ERROR_SUCCESS.ThrowIfFailed();
    }

    [Fact]
    public void ThrowIfFailed_Failure_Throws()
    {
        Assert.Throws<FileNotFoundException>(() => WIN32_ERROR.ERROR_FILE_NOT_FOUND.ThrowIfFailed());
    }

    [Fact]
    public void GetException_FilenameExcedRange_ReturnsPathTooLongException()
    {
        Exception ex = WIN32_ERROR.ERROR_FILENAME_EXCED_RANGE.GetException();
        Assert.IsType<PathTooLongException>(ex);
    }

    [Fact]
    public void GetException_InvalidDrive_ReturnsDriveNotFoundException()
    {
        Exception ex = WIN32_ERROR.ERROR_INVALID_DRIVE.GetException();
        Assert.IsType<DriveNotFoundException>(ex);
    }

    [Fact]
    public void GetException_OperationAborted_ReturnsOperationCanceledException()
    {
        Exception ex = WIN32_ERROR.ERROR_OPERATION_ABORTED.GetException();
        Assert.IsType<OperationCanceledException>(ex);
    }

    [Fact]
    public void GetException_Cancelled_ReturnsOperationCanceledException()
    {
        Exception ex = WIN32_ERROR.ERROR_CANCELLED.GetException();
        Assert.IsType<OperationCanceledException>(ex);
    }

    [Fact]
    public void GetException_NetworkAccessDenied_ReturnsUnauthorizedAccessException()
    {
        Exception ex = WIN32_ERROR.ERROR_NETWORK_ACCESS_DENIED.GetException();
        Assert.IsType<UnauthorizedAccessException>(ex);
    }

    [Fact]
    public void GetException_NotReady_ReturnsWin32Exception()
    {
        Exception ex = WIN32_ERROR.ERROR_NOT_READY.GetException();
        Assert.IsType<Win32Exception>(ex);
    }

    [Fact]
    public void GetException_FileExists_ReturnsWin32Exception()
    {
        Exception ex = WIN32_ERROR.ERROR_FILE_EXISTS.GetException();
        Assert.IsType<Win32Exception>(ex);
    }

    [Fact]
    public void GetException_AlreadyExists_ReturnsWin32Exception()
    {
        Exception ex = WIN32_ERROR.ERROR_ALREADY_EXISTS.GetException();
        Assert.IsType<Win32Exception>(ex);
    }

    [Fact]
    public void GetException_NotSupportedInAppContainer_ReturnsNotSupportedException()
    {
        Exception ex = WIN32_ERROR.ERROR_NOT_SUPPORTED_IN_APPCONTAINER.GetException();
        Assert.IsType<NotSupportedException>(ex);
    }

    [Fact]
    public void GetException_UnknownError_ReturnsWin32Exception()
    {
        Exception ex = ((WIN32_ERROR)9999).GetException();
        Assert.IsType<Win32Exception>(ex);
    }

    [Fact]
    public void ThrowIfLastErrorNot_LastErrorMatches_DoesNotThrow()
    {
        // Read the current last-error so the test doesn't depend on prior state, then assert
        // ThrowIfLastErrorNot matches it.
        WIN32_ERROR current = Error.GetLastError();
        Error.ThrowIfLastErrorNot(current);
    }
}
