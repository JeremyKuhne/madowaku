// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

namespace Windows.Win32.Foundation;

/// <summary>
///  Global memory handle.
/// </summary>
public unsafe partial struct HGLOBAL
{
    /// <summary>
    ///  Locks the global memory handle and returns a pointer to the memory.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   Use the result in a <c>using</c> statement to ensure the memory is unlocked when done.
    ///  </para>
    /// </remarks>
    public LockScope Lock() => new LockScope(this);

    /// <summary>
    ///  Returns the current size of the global memory handle. Returns 0 if the handle is invalid.
    /// </summary>
    public nuint Size => PInvoke.GlobalSize(this);

    /// <summary>
    ///  Returns a value indicating whether the handle is null or invalid.
    /// </summary>
    public bool IsValid => !IsNull && PInvoke.GlobalFlags(this) != PInvoke.GMEM_INVALID_HANDLE;

    /// <summary>
    ///  Locks the global memory handle and returns a pointer to the memory.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   Use in a <c>using</c> statement to ensure the memory is unlocked when done.
    ///  </para>
    /// </remarks>
    public unsafe ref struct LockScope
    {
        private readonly HGLOBAL _global;
        private void* _memory;

        /// <summary>
        ///  Constructs a new <see cref="LockScope"/> for the specified global memory handle.
        /// </summary>
        public LockScope(HGLOBAL global)
        {
            _global = global;
            _memory = PInvoke.GlobalLock(global);
            if (_memory is null)
            {
                Error.ThrowLastError();
            }
        }

        /// <summary>
        ///  The pointer to the locked memory.
        /// </summary>
        public readonly void* Memory => _memory;

        /// <summary>
        ///  Implicit converter to the pointer to the locked memory.
        /// </summary>
        public static implicit operator void*(LockScope scope) => scope.Memory;

        /// <inheritdoc cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            if (_memory is null)
            {
                return;
            }

            if (!PInvoke.GlobalUnlock(_global))
            {
                Error.ThrowIfLastErrorNot(WIN32_ERROR.ERROR_SUCCESS);
            }

            _memory = null;
        }
    }
}
