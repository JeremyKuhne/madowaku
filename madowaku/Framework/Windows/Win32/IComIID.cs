// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

namespace Windows.Win32
{
    /// <summary>
    ///  Common interface for COM interface wrapping structs.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   On .NET 6 and later, this is provided by CSWin32 as an abstract static.
    ///  </para>
    /// </remarks>
    public interface IComIID
    {
        /// <summary>
        ///  The identifier (IID) GUID for this interface.
        /// </summary>
        Guid Guid { get; }
    }
}
