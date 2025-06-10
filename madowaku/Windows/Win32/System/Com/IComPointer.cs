// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

namespace Windows.Win32.System.Com;

/// <summary>
///  COM pointer interface.
/// </summary>
public interface IComPointer : IDisposable
{
    /// <summary>
    ///  Gets the interface cast to the specified type.
    /// </summary>
    /// <typeparam name="TAsInterface">The target interface type.</typeparam>
    /// <returns>A scope containing the requested interface.</returns>
    ComScope<TAsInterface> GetInterface<TAsInterface>() where TAsInterface : unmanaged, IComIID;

    /// <summary>
    ///  Tries to get the interface cast to the specified type and returns the <see cref="HRESULT"/>.
    /// </summary>
    /// <typeparam name="TAsInterface">The target interface type.</typeparam>
    /// <param name="hr">The <see cref="HRESULT"/> from the operation.</param>
    /// <returns>A scope containing the requested interface.</returns>
    ComScope<TAsInterface> TryGetInterface<TAsInterface>(out HRESULT hr) where TAsInterface : unmanaged, IComIID;
}
