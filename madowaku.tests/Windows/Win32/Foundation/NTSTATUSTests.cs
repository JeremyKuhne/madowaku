// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using System.ComponentModel;

namespace Windows.Win32.Foundation;

[TestClass]
public class NTSTATUSTests
{
    private static readonly NTSTATUS s_accessDenied = (NTSTATUS)unchecked((int)0xC0000022);
    private static readonly NTSTATUS s_bufferOverflow = (NTSTATUS)unchecked((int)0x80000005);

    [TestMethod]
    public void ThrowIfFailed_Success_DoesNotThrow()
    {
        ((NTSTATUS)0).ThrowIfFailed();
    }

    [TestMethod]
    public void ThrowIfFailed_Informational_DoesNotThrow()
    {
        ((NTSTATUS)0x40000000).ThrowIfFailed();
    }

    [TestMethod]
    public void ThrowIfFailed_Warning_Throws()
    {
        FluentActions.Invoking(s_bufferOverflow.ThrowIfFailed).Should().Throw<Win32Exception>();
    }

    [TestMethod]
    public void ThrowIfFailed_Error_ThrowsMappedException()
    {
        FluentActions.Invoking(s_accessDenied.ThrowIfFailed).Should().Throw<UnauthorizedAccessException>();
    }

    [TestMethod]
    public void GetException_Error_ReturnsMappedExceptionWithStatusAndPath()
    {
        Exception exception = s_accessDenied.GetException("test-path");

        exception.Should().BeOfType<UnauthorizedAccessException>();
        exception.Message.Should().Contain("{NTSTATUS: c0000022}").And.Contain("'test-path'");
    }

    [TestMethod]
    public void ExplicitCastToWin32Error_AccessDenied_ReturnsAccessDenied()
    {
        ((WIN32_ERROR)s_accessDenied).Should().Be(WIN32_ERROR.ERROR_ACCESS_DENIED);
    }

    [TestMethod]
    public void ImplicitCastToException_AccessDenied_ReturnsMappedException()
    {
        Exception exception = s_accessDenied;

        exception.Should().BeOfType<UnauthorizedAccessException>();
    }
}