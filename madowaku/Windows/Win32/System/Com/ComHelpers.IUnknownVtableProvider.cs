// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

#if NET
using System.Collections;

namespace Windows.Win32;

public static unsafe partial class ComHelpers
{
    private sealed class IUnknownVtableProvider : ComWrappers
    {
        /// <summary>
        ///  Populates the specified IUnknown vtable with the runtime implementations.
        /// </summary>
        /// <param name="vtable">The vtable to populate.</param>
        public static void Populate(System.Com.IUnknown.Vtbl* vtable)
        {
            GetIUnknownImpl(out nint queryInterface, out nint addRef, out nint release);
            vtable->QueryInterface_1 =
                (delegate* unmanaged[Stdcall]<System.Com.IUnknown*, Guid*, void**, Foundation.HRESULT>)queryInterface;

            vtable->AddRef_2 = (delegate* unmanaged[Stdcall]<System.Com.IUnknown*, uint>)addRef;
            vtable->Release_3 = (delegate* unmanaged[Stdcall]<System.Com.IUnknown*, uint>)release;
        }

        protected override ComInterfaceEntry* ComputeVtables(
            object obj,
            CreateComInterfaceFlags flags,
            out int count) =>
                throw new NotSupportedException();

        protected override object CreateObject(nint externalComObject, CreateObjectFlags flags) =>
            throw new NotSupportedException();

        protected override void ReleaseObjects(IEnumerable objects) =>
            throw new NotSupportedException();
    }
}
#endif