// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using Windows.Win32;
using Windows.Win32.System.Memory;

namespace Madowaku;

/// <summary>
///  Native helpers for span types.
/// </summary>
public static unsafe class SpanExtensions
{
    extension<T>(ReadOnlySpan<T> span) where T : unmanaged
    {
        /// <summary>
        ///  Copies the contents of the span to newly allocated global (native) memory.
        /// </summary>
        /// <param name="nullTerminate">
        ///  <see langword="true"/> to allocate room for and write an additional default
        ///  <typeparamref name="T"/> value as a null terminator after the copied data.
        /// </param>
        /// <param name="flags">
        ///  The flags to pass to <see cref="PInvoke.GlobalAlloc(GLOBAL_ALLOC_FLAGS, nuint)"/>.
        /// </param>
        /// <returns>
        ///  A handle to the allocated global memory containing a copy of the span's data.
        /// </returns>
        /// <exception cref="System.ComponentModel.Win32Exception">
        ///  Thrown if the native allocation, lock, or unlock operation fails.
        /// </exception>
        public HGLOBAL CopyToNative(
            bool nullTerminate = false,
            GLOBAL_ALLOC_FLAGS flags = GLOBAL_ALLOC_FLAGS.GMEM_MOVEABLE)
        {
            int sizeofT = sizeof(T);
            int length = span.Length;
            if (nullTerminate)
            {
                length = checked(length + 1);
            }

            nuint size = checked((nuint)length * (nuint)sizeofT);

            HGLOBAL global = PInvoke.GlobalAlloc(flags, size);
            if (global.IsNull)
            {
                Error.ThrowLastError();
            }

            using var lockScope = global.Lock();
            Span<T> destinationSpan = new Span<T>(lockScope, length);
            span.CopyTo(destinationSpan);

            if (nullTerminate)
            {
                destinationSpan[^1] = default;
            }

            return global;
        }

        /// <summary>
        ///  Copies the contents of the span into existing global (native) memory.
        /// </summary>
        /// <param name="destination">
        ///  The handle to the existing global memory to copy into.
        /// </param>
        /// <param name="nullTerminate">
        ///  <see langword="true"/> to write an additional default <typeparamref name="T"/>
        ///  value as a null terminator after the copied data.
        /// </param>
        /// <exception cref="System.ArgumentException">
        ///  Thrown if <paramref name="destination"/> is not large enough to hold the copied data.
        /// </exception>
        /// <exception cref="System.ComponentModel.Win32Exception">
        ///  Thrown if the native size query, lock, or unlock operation fails.
        /// </exception>
        public void CopyToNative(HGLOBAL destination, bool nullTerminate = false)
        {
            int sizeofT = sizeof(T);
            int length = span.Length;
            if (nullTerminate)
            {
                length = checked(length + 1);
            }

            nuint size = checked((nuint)length * (nuint)sizeofT);

            nuint destinationSize = PInvoke.GlobalSize(destination);
            if (destinationSize < size)
            {
                throw new ArgumentException("Destination is not large enough to hold the copied data.", nameof(destination));
            }

            using var lockScope = destination.Lock();
            Span<T> destinationSpan = new Span<T>(lockScope, length);
            span.CopyTo(destinationSpan);

            if (nullTerminate)
            {
                destinationSpan[^1] = default;
            }
        }
    }

    extension<T>(Span<T> span) where T : unmanaged
    {
        /// <inheritdoc cref="SpanExtensions.CopyToNative{T}(ReadOnlySpan{T}, bool, GLOBAL_ALLOC_FLAGS)"/>
        public HGLOBAL CopyToNative(
            bool nullTerminate = false,
            GLOBAL_ALLOC_FLAGS flags = GLOBAL_ALLOC_FLAGS.GMEM_MOVEABLE)
        {
            return ((ReadOnlySpan<T>)span).CopyToNative(nullTerminate, flags);
        }

        /// <inheritdoc cref="SpanExtensions.CopyToNative{T}(ReadOnlySpan{T}, HGLOBAL, bool)"/>
        public void CopyToNative(HGLOBAL destination, bool nullTerminate = false)
        {
            ((ReadOnlySpan<T>)span).CopyToNative(destination, nullTerminate);
        }
    }
}
