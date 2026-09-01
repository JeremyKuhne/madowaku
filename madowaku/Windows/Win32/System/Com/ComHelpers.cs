// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

#if NET
namespace Windows.Win32;

/// <summary>
///  Provides helpers for generated COM interop.
/// </summary>
public static unsafe partial class ComHelpers
{
    static partial void PopulateIUnknownImpl<TComInterface>(System.Com.IUnknown.Vtbl* vtable)
        where TComInterface : unmanaged
    {
        IUnknownVtableProvider.Populate(vtable);
    }
}
#endif