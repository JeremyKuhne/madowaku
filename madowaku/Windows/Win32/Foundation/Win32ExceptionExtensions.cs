// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using System.ComponentModel;
#if NETFRAMEWORK
using System.Reflection;
#endif
using Windows.Win32.System.Diagnostics.Debug;

namespace Windows.Win32.Foundation;

/// <summary>
///  Provides methods for creating <see cref="Win32Exception"/> instances with correctly
///  populated <see cref="Exception.HResult"/> and <see cref="Win32Exception.NativeErrorCode"/> values.
/// </summary>
/// <remarks>
///  <para>
///   <see cref="Win32Exception"/>'s constructors set <see cref="Win32Exception.NativeErrorCode"/>
///   but do not derive <see cref="Exception.HResult"/> from it, leaving the base
///   <see cref="Exception"/> default in place. These methods ensure both properties reflect the
///   given error.
///  </para>
/// </remarks>
public static class Win32ExceptionExtensions
{
    extension(Win32Exception)
    {
        /// <summary>
        ///  Creates a <see cref="Win32Exception"/> for the given <paramref name="error"/>.
        /// </summary>
        /// <param name="error">The Windows error code.</param>
        /// <param name="message">Optional message. If <see langword="null"/>, the system message for
        ///  <paramref name="error"/> is used.</param>
        public static Win32Exception Create(WIN32_ERROR error, string? message = null)
        {
            Win32Exception exception = message is null
                ? new Win32Exception((int)error)
                : new Win32Exception((int)error, message);

            SetHResult(exception, (int)error.ToHRESULT());
            return exception;
        }

        /// <summary>
        ///  Creates a <see cref="Win32Exception"/> for the given <paramref name="hr"/>.
        /// </summary>
        /// <param name="hr">The failing <see cref="HRESULT"/>.</param>
        /// <param name="message">Optional message. If <see langword="null"/>, the system message for
        ///  the native error code is used.</param>
        /// <remarks>
        ///  <para>
        ///   When <paramref name="hr"/> has the <see cref="FACILITY_CODE.FACILITY_WIN32"/> facility,
        ///   <see cref="Win32Exception.NativeErrorCode"/> is set to the encoded Win32 error code
        ///   ([HRESULT_CODE]). Otherwise it is set to the raw <see cref="HRESULT"/> value.
        ///  </para>
        /// </remarks>
        public static Win32Exception Create(HRESULT hr, string? message = null)
        {
            int nativeErrorCode = hr.Facility == FACILITY_CODE.FACILITY_WIN32 ? hr.Code : hr.Value;

            Win32Exception exception = message is null
                ? new Win32Exception(nativeErrorCode)
                : new Win32Exception(nativeErrorCode, message);

            SetHResult(exception, hr.Value);
            return exception;
        }
    }

#if NETFRAMEWORK
    // .NET Framework's Exception.HResult setter is protected, so it can't be assigned from here directly.
    private static readonly FieldInfo s_hResultField =
        typeof(Exception).GetField("_HResult", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private static void SetHResult(Exception exception, int hResult) =>
        s_hResultField.SetValue(exception, hResult);
#else
    // Exception.HResult's setter is public on modern .NET.
    private static void SetHResult(Exception exception, int hResult) =>
        exception.HResult = hResult;
#endif
}
