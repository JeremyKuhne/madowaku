// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

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
}
