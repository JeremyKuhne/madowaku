// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

namespace Windows.Win32.Foundation;

public unsafe partial struct FILETIME
{
    /// <summary>
    ///  Converts a <see cref="DateTime"/> value to a <see cref="FILETIME"/> structure.
    /// </summary>
    /// <param name="value">The <see cref="DateTime"/> value to convert.</param>
    /// <returns>A <see cref="FILETIME"/> structure representing the specified <see cref="DateTime"/>.</returns>
    public static explicit operator FILETIME(DateTime value)
    {
        long ft = value.ToFileTime();

        return new FILETIME()
        {
            dwLowDateTime = (uint)(ft & 0xFFFFFFFF),
            dwHighDateTime = (uint)(ft >> 32)
        };
    }

    /// <summary>
    ///  Converts a <see cref="FILETIME"/> structure to a <see cref="DateTime"/> value.
    /// </summary>
    /// <param name="value">The <see cref="FILETIME"/> structure to convert.</param>
    /// <returns>A <see cref="DateTime"/> value representing the specified <see cref="FILETIME"/>.</returns>
    public static explicit operator DateTime(FILETIME value)
        => DateTime.FromFileTime(((long)value.dwHighDateTime << 32) + value.dwLowDateTime);
}
