// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

namespace Windows.Win32.System.Ole;

/// <summary>
///  Represents the IRecordInfo COM interface, which provides information about a record type.
/// </summary>
public partial struct IRecordInfo : IComIID
{
    readonly ref readonly Guid IComIID.Guid
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Unsafe.AsRef(in IID_Guid);
    }
}
