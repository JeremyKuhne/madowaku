// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

namespace Windows.Win32.System.Ole;

/// <summary>
///  Represents the IClassFactory COM interface, which is used to create instances of COM objects.
/// </summary>
public partial struct IClassFactory2 : IComIID
{
    readonly Guid IComIID.Guid => IID_Guid;
}
