// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

namespace Windows.Win32.System.Com;

/// <summary>
///  Untyped equivalent of <see cref="ComScope{T}"/>. Prefer <see cref="ComScope{T}"/>.
/// </summary>
public readonly unsafe ref struct ComScope
{
    // Keeping internal as nint allows us to use Unsafe methods to get significantly better generated code.
    private readonly nint _value;

    /// <summary>
    ///  The underlying COM pointer.
    /// </summary>
    public void* Value => (void*)_value;

    /// <summary>
    ///  Creates a new <see cref="ComScope"/> from the given <paramref name="value"/>.
    /// </summary>
    public ComScope(void* value) => _value = (nint)value;

    /// <summary>
    ///  Implicitly converts a <see cref="ComScope"/> to its pointer.
    /// </summary>
    public static implicit operator void*(in ComScope scope) => (void*)scope._value;

    /// <summary>
    ///  Implicitly converts a <see cref="ComScope"/> to a pointer to a pointer (T**).
    ///  Used to get an out parameter for COM methods that take a pointer to a pointer.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator void**(in ComScope scope) => (void**)Unsafe.AsPointer(ref Unsafe.AsRef(in scope._value));

    /// <summary>
    ///  <see langword="true"/> if the underlying COM pointer is null (0).
    /// </summary>
    public bool IsNull => _value == 0;

    /// <inheritdoc cref="IDisposable.Dispose()"/>
    public void Dispose()
    {
        IUnknown* unknown = (IUnknown*)_value;

        // Really want this to be null after disposal to avoid double releases, but we also want
        // to maintain the readonly state of the struct to allow passing as `in` without creating implicit
        // copies (which would break the T** and void** operators).
        *(void**)this = null;
        if (unknown is not null)
        {
            unknown->Release();
        }
    }
}
