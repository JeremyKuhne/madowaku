// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using Windows.Win32.System.Diagnostics.Debug;

namespace Windows.Win32.Foundation;

/// <summary>
///  Represents an HRESULT value.
/// </summary>
public partial struct HRESULT
{
    /// <summary>
    ///  Convert a Windows error to an <see cref="HRESULT"/>. [HRESULT_FROM_WIN32]
    /// </summary>
    /// <param name="error">The Windows error to convert.</param>
    /// <returns>The corresponding <see cref="HRESULT"/>.</returns>
    public static explicit operator HRESULT(WIN32_ERROR error)
    {
        // https://learn.microsoft.com/windows/win32/api/winerror/nf-winerror-hresult_from_win32
        // return (HRESULT)(x) <= 0 ? (HRESULT)(x) : (HRESULT) (((x) & 0x0000FFFF) | (FACILITY_WIN32 << 16) | 0x80000000);
        return (HRESULT)(int)((int)error <= 0 ? (int)error : (((int)error & 0x0000FFFF) | ((int)FACILITY_CODE.FACILITY_WIN32 << 16) | 0x80000000));
    }

    /// <summary>
    ///  Extracts the code portion of the HRESULT. [HRESULT_CODE]
    /// </summary>
    public int Code =>
        // https://learn.microsoft.com/windows/win32/api/winerror/nf-winerror-hresult_code
        // #define HRESULT_CODE(hr)    ((hr) & 0xFFFF)
        Value & 0xFFFF;

    /// <summary>
    ///  Extracts the facility code of the HRESULT. [HRESULT_FACILITY]
    /// </summary>
    public FACILITY_CODE Facility =>
        // https://learn.microsoft.com/windows/win32/api/winerror/nf-winerror-hresult_facility
        // #define HRESULT_FACILITY(hr)  (((hr) >> 16) & 0x1fff)
        (FACILITY_CODE)((Value >> 16) & 0x1fff);

    // COR_* HRESULTs are .NET HRESULTs
#pragma warning disable TOUKI0041 // Naming rule violation
#pragma warning disable IDE0055
    /// <summary>
    ///  The HRESULT corresponding to <see cref="ArgumentException"/>.
    /// </summary>
    public static readonly HRESULT COR_E_ARGUMENT               = (HRESULT)unchecked((int)0x80070057);

    /// <summary>
    ///  The HRESULT indicating that a type library is not registered.
    /// </summary>
    public static readonly HRESULT TLBX_E_LIBNOTREGISTERED      = (HRESULT)unchecked((int)0x80131165);

    /// <summary>
    ///  The HRESULT corresponding to <see cref="MissingFieldException"/>.
    /// </summary>
    public static readonly HRESULT COR_E_MISSINGFIELD           = (HRESULT)unchecked((int)0x80131511);

    /// <summary>
    ///  The HRESULT corresponding to <see cref="MissingMemberException"/>.
    /// </summary>
    public static readonly HRESULT COR_E_MISSINGMEMBER          = (HRESULT)unchecked((int)0x80131512);

    /// <summary>
    ///  The HRESULT corresponding to <see cref="MissingMethodException"/>.
    /// </summary>
    public static readonly HRESULT COR_E_MISSINGMETHOD          = (HRESULT)unchecked((int)0x80131513);

    /// <summary>
    ///  The HRESULT corresponding to <see cref="NotSupportedException"/>.
    /// </summary>
    public static readonly HRESULT COR_E_NOTSUPPORTED           = (HRESULT)unchecked((int)0x80131515);

    /// <summary>
    ///  The HRESULT corresponding to <see cref="OverflowException"/>.
    /// </summary>
    public static readonly HRESULT COR_E_OVERFLOW               = (HRESULT)unchecked((int)0x80131516);

    /// <summary>
    ///  The HRESULT corresponding to <see cref="InvalidOleVariantTypeException"/>.
    /// </summary>
    public static readonly HRESULT COR_E_INVALIDOLEVARIANTTYPE  = (HRESULT)unchecked((int)0x80131531);

    /// <summary>
    ///  The HRESULT corresponding to <see cref="SafeArrayTypeMismatchException"/>.
    /// </summary>
    public static readonly HRESULT COR_E_SAFEARRAYTYPEMISMATCH  = (HRESULT)unchecked((int)0x80131533);

    /// <summary>
    ///  The HRESULT corresponding to <see cref="global::System.Reflection.TargetInvocationException"/>.
    /// </summary>
    public static readonly HRESULT COR_E_TARGETINVOCATION       = (HRESULT)unchecked((int)0x80131604);

    /// <summary>
    ///  The HRESULT corresponding to <see cref="ObjectDisposedException"/>.
    /// </summary>
    public static readonly HRESULT COR_E_OBJECTDISPOSED         = (HRESULT)unchecked((int)0x80131622);
#pragma warning restore TOUKI0041 // Naming rule violation
#pragma warning restore IDE0055

    // The .NET runtime host uses a FACILITY_NULL facility code when failing to launch (0x8000????).
    // https://github.com/dotnet/runtime/blob/main/docs/design/features/host-error-codes.md

    /// <summary>
    ///  Implicitly converts an <see cref="HRESULT"/> to an <see cref="Exception"/>.
    /// </summary>
    /// <param name="result">The HRESULT to convert.</param>
    /// <returns>The corresponding exception.</returns>
    public static implicit operator Exception(HRESULT result) =>
        Marshal.GetExceptionForHR(result) ?? new InvalidOperationException("Not a failing result.");

    /// <summary>
    ///  Format an <see cref="HRESULT"/> with a message.
    /// </summary>
    /// <returns>A string containing the HRESULT value and its message.</returns>
    public string ToStringWithDescription()
    {
        bool win32error = Facility == FACILITY_CODE.FACILITY_WIN32;

        string message = Error.FormatMessage(win32error ? (uint)Code : (uint)Value);
        return win32error
            ? $"HRESULT 0x{Value:X8} [{(WIN32_ERROR)Code} ({(uint)Code:D})]: {message}"
            : $"HRESULT 0x{Value:X8} [{Value:D}]: {message}";
    }
}
