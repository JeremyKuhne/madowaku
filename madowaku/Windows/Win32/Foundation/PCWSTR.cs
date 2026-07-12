// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

namespace Windows.Win32.Foundation;

/// <summary>
///  Represents a pointer to a constant null-terminated UTF-16 string.
/// </summary>
public unsafe partial struct PCWSTR
{
    /// <summary>
    ///  Gets a value indicating whether the pointer is null.
    /// </summary>
    public bool IsNull => Value is null;

    /// <summary>
    ///  Frees the pointer with <c>LocalFree</c> when it is not null.
    /// </summary>
    public void LocalFree()
    {
        if (Value is not null)
        {
            PInvoke.LocalFree((HLOCAL)(nint)Value);
            Unsafe.AsRef(in this) = default;
        }
    }
}