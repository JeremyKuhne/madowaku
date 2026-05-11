// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

namespace System.Runtime.InteropServices;

/// <summary>
///  Minimal polyfill of <see cref="ComWrappers"/> for .NET Framework 4.7.2.
/// </summary>
/// <remarks>
///  <para>
///   CsWin32 unconditionally emits a <c>ComHelpers.UnwrapCCW</c> helper that
///   references <see cref="ComInterfaceDispatch"/>.<c>GetInstance</c>. We do
///   not consume that helper on net472 (it's only used to unwrap managed
///   objects exposed as COM callable wrappers, a scenario that requires the
///   real CLR ComWrappers infrastructure that does not exist on .NET
///   Framework). This shim exists solely so the generated source compiles.
///   <see cref="ComInterfaceDispatch.GetInstance{T}(ComInterfaceDispatch*)"/>
///   always returns <see langword="null"/>, which causes the generated
///   <c>UnwrapCCW</c> helper to return <c>COR_E_OBJECTDISPOSED</c>; calling
///   into it is unsupported on .NET Framework.
///  </para>
/// </remarks>
internal abstract class ComWrappers
{
    /// <summary>
    ///  Polyfill of <c>ComWrappers.ComInterfaceDispatch</c>. See remarks on
    ///  <see cref="ComWrappers"/> for why this exists and what its members
    ///  do.
    /// </summary>
    public unsafe struct ComInterfaceDispatch
    {
        /// <summary>
        ///  Always returns <see langword="null"/> on .NET Framework. CCW
        ///  unwrapping is not supported in this polyfill.
        /// </summary>
        public static T? GetInstance<T>(ComInterfaceDispatch* dispatch) where T : class
        {
            _ = dispatch;
            return null;
        }
    }
}
