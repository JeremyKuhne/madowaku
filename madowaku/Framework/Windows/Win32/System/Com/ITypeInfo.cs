// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

namespace Windows.Win32.System.Com;

/// <summary>
///  COM ITypeInfo interface wrapping struct.
/// </summary>
public unsafe partial struct ITypeInfo : IComIID
{
    readonly Guid IComIID.Guid => IID_Guid;
}
