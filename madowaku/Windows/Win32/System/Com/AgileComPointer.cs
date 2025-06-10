// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

// Based on https://github.com/dotnet/winforms/ sources
//
// Original header
// ---------------
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Windows.Win32.System.Com;

/// <summary>
///  Finalizable wrapper for COM pointers that gives agile access to the specified interface.
/// </summary>
public unsafe class AgileComPointer<TInterface> : IComPointer
    where TInterface : unmanaged, IComIID
{
    private readonly uint _cookie;
    private readonly TInterface* _originalHandle;

    /// <summary>
    ///  Initializes a new instance of the <see cref="AgileComPointer{TInterface}"/> class.
    /// </summary>
    /// <param name="interface">The COM interface pointer to register.</param>
    /// <param name="takeOwnership">If <see langword="true"/>, takes ownership of the reference count.</param>
    public AgileComPointer(TInterface* @interface, bool takeOwnership)
    {
        _originalHandle = @interface;
        _cookie = GlobalInterfaceTable.RegisterInterface(@interface);

        // We let the GlobalInterfaceTable maintain the ref count here
        if (takeOwnership)
        {
            uint count = ((IUnknown*)@interface)->Release();
        }
    }

    /// <summary>
    ///  Gets the default interface.
    /// </summary>
    public ComScope<TInterface> GetInterface()
    {
        var scope = GlobalInterfaceTable.GetInterface<TInterface>(_cookie, out HRESULT hr);
        hr.ThrowOnFailure();
        return scope;
    }

    /// <inheritdoc cref="IComPointer.GetInterface{TAsInterface}"/>
    public ComScope<TAsInterface> GetInterface<TAsInterface>()
        where TAsInterface : unmanaged, IComIID
    {
        var scope = TryGetInterface<TAsInterface>(out HRESULT hr);
        hr.ThrowOnFailure();
        return scope;
    }

    /// <summary>
    ///  Tries to get the default interface and returns the <see cref="HRESULT"/>.
    /// </summary>
    /// <param name="hr">The <see cref="HRESULT"/> from the operation.</param>
    /// <returns>A scope containing the interface.</returns>
    public ComScope<TInterface> TryGetInterface(out HRESULT hr)
        => GlobalInterfaceTable.GetInterface<TInterface>(_cookie, out hr);

    /// <inheritdoc cref="IComPointer.TryGetInterface{TAsInterface}(out HRESULT)"/>
    public ComScope<TAsInterface> TryGetInterface<TAsInterface>(out HRESULT hr)
        where TAsInterface : unmanaged, IComIID
    {
        var scope = GlobalInterfaceTable.GetInterface<TAsInterface>(_cookie, out hr);
        return scope;
    }

    /// <summary>
    ///  Finalizer that ensures the interface is revoked from the global interface table.
    /// </summary>
    ~AgileComPointer()
    {
        Debug.Fail($"Did not dispose {nameof(AgileComPointer<TInterface>)}");
        Dispose(disposing: false);
    }

    /// <summary>
    ///  Compares this pointer to another COM interface pointer.
    /// </summary>
    /// <param name="other">The pointer to compare against.</param>
    /// <returns><see langword="true"/> if the pointers are equal.</returns>
    public bool Equals(TInterface* other) => other == _originalHandle;

    /// <summary>
    ///  Disposes the agile COM pointer, revoking the interface from the global interface table.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///  Disposes the agile COM pointer, revoking the interface from the global interface table.
    /// </summary>
    /// <param name="disposing">
    ///  <see langword="true"/> if called from <see cref="Dispose()"/>;
    ///  <see langword="false"/> if called from the finalizer.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        HRESULT hr = GlobalInterfaceTable.RevokeInterface(_cookie);
        if (disposing)
        {
            // Don't assert from the finalizer thread.
            Debug.Assert(hr.Succeeded);
        }
    }
}
