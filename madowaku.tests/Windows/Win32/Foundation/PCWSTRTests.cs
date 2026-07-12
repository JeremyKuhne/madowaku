// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using Windows.Win32.System.Diagnostics.Debug;

namespace Windows.Win32.Foundation;

[TestClass]
public class PCWSTRTests
{
    [TestMethod]
    public void IsNull_DefaultInstance_ReturnsTrue()
    {
        PCWSTR value = default;

        value.IsNull.Should().BeTrue();
    }

    [TestMethod]
    public unsafe void LocalFree_FormatMessageBuffer_ClearsPointerAndIsIdempotent()
    {
        PCWSTR value = default;

        try
        {
            uint length = PInvoke.FormatMessage(
                dwFlags: FORMAT_MESSAGE_OPTIONS.FORMAT_MESSAGE_ALLOCATE_BUFFER
                    | FORMAT_MESSAGE_OPTIONS.FORMAT_MESSAGE_FROM_SYSTEM
                    | FORMAT_MESSAGE_OPTIONS.FORMAT_MESSAGE_IGNORE_INSERTS,
                lpSource: null,
                dwMessageId: (uint)WIN32_ERROR.ERROR_FILE_NOT_FOUND,
                dwLanguageId: 0,
                lpBuffer: (PWSTR)(void*)(&value),
                nSize: 0,
                Arguments: null);

            length.Should().BeGreaterThan(0);
            value.IsNull.Should().BeFalse();

            value.LocalFree();
            value.IsNull.Should().BeTrue();

            value.LocalFree();
        }
        finally
        {
            value.LocalFree();
        }
    }
}