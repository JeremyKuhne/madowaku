// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using System.Threading;

namespace Windows.Win32.System.Com;

/// <summary>
///  Lifetime management helper for a COM callable wrapper. It holds the created <typeparamref name="TObject"/>
///  wrapper with he given <typeparamref name="TVTable"/>.
/// </summary>
/// <remarks>
///  <para>
///   This should not be created directly. Instead use <see cref="Lifetime{TVTable, TObject}.Allocate"/>.
///  </para>
///  <para>
///   A COM object's memory layout is a virtual function table (vtable) pointer followed by instance data. We're
///   effectively manually creating a COM "object" here that contains instance data of a GCHandle to the related
///   managed object and a ref count.
///  </para>
/// </remarks>
public unsafe struct Lifetime<TVTable, TObject> where TVTable : unmanaged
{
    private TVTable* _vtable;
    private IUnknown* _handle;
    private int _refCount;

    /// <inheritdoc cref="IUnknown.AddRef"/>
    public static unsafe uint AddRef(IUnknown* @this)
        => (uint)Interlocked.Increment(ref ((Lifetime<TVTable, TObject>*)@this)->_refCount);

    /// <inheritdoc cref="IUnknown.Release"/>
    public static unsafe uint Release(IUnknown* @this)
    {
        var lifetime = (Lifetime<TVTable, TObject>*)@this;
        Debug.Assert(lifetime->_refCount > 0);
        int count = Interlocked.Decrement(ref lifetime->_refCount);
        if (count <= 0)
        {
            GCHandle.FromIntPtr((IntPtr)lifetime->_handle).Free();
            PInvokeMadowaku.CoTaskMemFree(lifetime);
        }

        return (uint)count;
    }

    /// <summary>
    ///  Allocate a lifetime wrapper for the given <paramref name="object"/> with the given
    ///  <paramref name="vtable"/>.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   This creates a <see cref="GCHandle"/> to root the <paramref name="object"/> until ref
    ///   counting has gone to zero.
    ///  </para>
    ///  <para>
    ///   The <paramref name="vtable"/> should be fixed, typically as a static. Com calls always
    ///   include the "this" pointer as the first argument.
    ///  </para>
    /// </remarks>
    public static unsafe Lifetime<TVTable, TObject>* Allocate(TObject @object, TVTable* vtable)
    {
        // Manually allocate a native instance of this struct.
        var wrapper = (Lifetime<TVTable, TObject>*)PInvokeMadowaku.CoTaskMemAlloc((nuint)sizeof(Lifetime<TVTable, TObject>));

        // Assign a pointer to the vtable, allocate a GCHandle for the related object, and set the initial ref count.
        wrapper->_vtable = vtable;
        wrapper->_handle = (IUnknown*)GCHandle.ToIntPtr(GCHandle.Alloc(@object));
        wrapper->_refCount = 1;

        return wrapper;
    }

    /// <summary>
    ///  Gets the object wrapped by a lifetime wrapper.
    /// </summary>
    public static TObject? GetObject(IUnknown* @this)
    {
        var lifetime = (Lifetime<TVTable, TObject>*)@this;
        return (TObject?)GCHandle.FromIntPtr((IntPtr)lifetime->_handle).Target;
    }
}
