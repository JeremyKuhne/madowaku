// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

namespace Windows.Win32.Foundation;

/// <summary>
///  Represents a pointer to a wide string (null-terminated Unicode string).
/// </summary>
public unsafe partial struct PWSTR
{
    /// <summary>
    ///  <see langword="true"/> if the pointer is null.
    /// </summary>
    public bool IsNull => Value is null;

    /// <summary>
    ///  Calls <see cref="PInvokeMadowaku.LocalFree(HLOCAL)"/> on the pointer if it is not null.
    /// </summary>
    public void LocalFree()
    {
        if (Value is not null)
        {
            PInvokeMadowaku.LocalFree((HLOCAL)(nint)Value);
            Unsafe.AsRef(in this) = default;
        }
    }
}
