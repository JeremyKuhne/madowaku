// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using System.ComponentModel;

namespace Windows.Win32.Foundation;

/// <summary>
///  Represents a native Windows handle.
/// </summary>
public readonly partial struct HANDLE : IDisposable
{
    /// <summary>
    ///  Closes the handle when it is neither null nor <c>INVALID_HANDLE_VALUE</c>.
    /// </summary>
    /// <exception cref="Win32Exception"><c>CloseHandle</c> failed.</exception>
    public unsafe void Dispose()
    {
        nint value = (nint)Value;
        if (value is not 0 and not -1)
        {
            PInvoke.CloseHandle(this).ThrowLastErrorIfFalse();
        }

        Unsafe.AsRef(in this) = default;
    }
}