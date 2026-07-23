// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using System.ComponentModel;
using Madowaku.Io;
using Windows.Win32.System.Diagnostics.Debug;

namespace Windows.Win32.Foundation;

/// <summary>
///  Provides methods for working with Windows errors.
/// </summary>
public static unsafe class Error
{
    /// <inheritdoc cref="Marshal.GetLastWin32Error"/>
    public static WIN32_ERROR GetLastError() => (WIN32_ERROR)Marshal.GetLastWin32Error();

    // Throws prevent inlining of methods. Try to force methods that throw to not get inlined
    // to ensure callers can be inlined.

    /// <summary>
    ///  Throws the specified error code as an exception.
    /// </summary>
    /// <param name="path">Optional path or other input detail.</param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    [DoesNotReturn]
    public static void Throw(this WIN32_ERROR error, string? path = null) => throw error.GetException(path);

    /// <summary>
    ///  Throws the specified error code as an exception if it is not <see cref="WIN32_ERROR.ERROR_SUCCESS"/>.
    /// </summary>
    /// <param name="path">Optional path or other input detail.</param>
    public static void ThrowIfFailed(this WIN32_ERROR error, string? path = null)
    {
        if (error != WIN32_ERROR.ERROR_SUCCESS)
        {
            error.Throw(path);
        }
    }

    /// <summary>
    ///  Throws the last error code from Windows as an exception.
    /// </summary>
    /// <param name="path">Optional path or other input detail.</param>
    [DoesNotReturn]
    public static void ThrowLastError(string? path = null) => Throw(GetLastError(), path);

    /// <summary>
    ///  Throw the last error code from Windows if <paramref name="result"/> is false.
    /// </summary>
    /// <param name="path">Optional path or other input detail.</param>
    internal static void ThrowLastErrorIfFalse(this bool result, string? path = null)
    {
        if (!result)
        {
            GetLastError().Throw(path);
        }
    }

    /// <summary>
    ///  Throw the last error code from Windows if <paramref name="result"/> is false.
    /// </summary>
    /// <param name="path">Optional path or other input detail.</param>
    internal static void ThrowLastErrorIfFalse(this BOOL result, string? path = null)
    {
        if (!result)
        {
            GetLastError().Throw(path);
        }
    }

    /// <summary>
    ///  Throw the last error code from Windows if it isn't <paramref name="error"/>.
    /// </summary>
    /// <param name="path">Optional path or other input detail.</param>
    public static void ThrowIfLastErrorNot(WIN32_ERROR error, string? path = null)
    {
        WIN32_ERROR lastError = GetLastError();
        if (lastError != error)
        {
            throw lastError.GetException(path);
        }
    }

    /// <summary>
    ///  Convert a Windows error to an HRESULT. [HRESULT_FROM_WIN32]
    /// </summary>
    public static HRESULT ToHRESULT(this WIN32_ERROR error)
    {
        // https://learn.microsoft.com/windows/win32/api/winerror/nf-winerror-hresult_from_win32
        // return (HRESULT)(x) <= 0 ? (HRESULT)(x) : (HRESULT) (((x) & 0x0000FFFF) | (FACILITY_WIN32 << 16) | 0x80000000);
        return (HRESULT)(int)((int)error <= 0 ? (int)error : (int)error & 0x0000FFFF | (int)FACILITY_CODE.FACILITY_WIN32 << 16 | 0x80000000);
    }

    /// <summary>
    ///  Turns Windows errors into the appropriate exception (that maps with existing .NET behavior as much as possible).
    /// </summary>
    public static Exception GetException(this WIN32_ERROR error, string? path = null)
    {
        // http://referencesource.microsoft.com/#mscorlib/system/io/__error.cs,142

        string message = path is null
            ? $"{ErrorToString(error)}"
            : $"{ErrorToString(error)} '{path}'";

        return WindowsErrorToException(error, message, path);
    }

    /// <summary>
    ///  Create a descriptive string for the error.
    /// </summary>
    public static string ErrorToString(this WIN32_ERROR error)
    {
        string message = FormatMessage(
            messageId: (uint)error,
            source: HINSTANCE.Null);

        // There are a few defintions for '0', we'll always use ERROR_SUCCESS
        return error == WIN32_ERROR.ERROR_SUCCESS
            ? $"ERROR_SUCCESS ({(uint)error}): {message}"
            : Enum.IsDefined(error)
                ? $"{error} ({(uint)error}): {message}"
                : $"Error {error}: {message}";
    }

    internal static Exception WindowsErrorToException(WIN32_ERROR error, string? message, string? path) => error switch
    {
        WIN32_ERROR.ERROR_FILE_NOT_FOUND => new FileNotFoundException(message, path),
        WIN32_ERROR.ERROR_PATH_NOT_FOUND => new DirectoryNotFoundException(message),
        WIN32_ERROR.ERROR_ACCESS_DENIED or WIN32_ERROR.ERROR_NETWORK_ACCESS_DENIED => new UnauthorizedAccessException(message),
        WIN32_ERROR.ERROR_FILENAME_EXCED_RANGE => new PathTooLongException(message),
        // Not available in Portable libraries
        WIN32_ERROR.ERROR_INVALID_DRIVE => new DriveNotFoundException(message),
        WIN32_ERROR.ERROR_OPERATION_ABORTED or WIN32_ERROR.ERROR_CANCELLED => new OperationCanceledException(message),
        WIN32_ERROR.ERROR_NOT_READY => new DriveNotReadyException(message),
        // File or directory already exists
        WIN32_ERROR.ERROR_FILE_EXISTS or WIN32_ERROR.ERROR_ALREADY_EXISTS => new FileExistsException(error, message),
        WIN32_ERROR.ERROR_INVALID_PARAMETER => new ArgumentException(message),
        WIN32_ERROR.ERROR_NOT_SUPPORTED or WIN32_ERROR.ERROR_NOT_SUPPORTED_IN_APPCONTAINER => new NotSupportedException(message),
        _ => error == (WIN32_ERROR)(int)HRESULT.FVE_E_LOCKED_VOLUME
            // Drive locked
            ? new DriveLockedException(message)
            : Win32Exception.Create(error, message),
    };

    /// <inheritdoc cref="FormatMessage(uint, HINSTANCE, ReadOnlySpan{string})"/>
    public static string FormatMessage(
        uint messageId,
        HINSTANCE source = default,
        params string[] args) => FormatMessage(messageId, source, args.AsSpan());

    /// <summary>
    ///  Formats a Windows error code into a string using FormatMessage.
    /// </summary>
    /// <param name="messageId">The message ID to format.</param>
    /// <param name="source">The source of the message. If <see langword="null"/>, the system message table is used.</param>
    /// <param name="args">Optional arguments to format into the message.</param>
    /// <remarks>
    ///  <para>
    ///   .NET's Win32Exception impements the error code lookup on FormatMessage using FORMAT_MESSAGE_FROM_SYSTEM.
    ///   It won't handle Network Errors (NERR_BASE..MAX_NERR), which come from NETMSG.DLL.
    ///  </para>
    /// </remarks>
    public static string FormatMessage(
        uint messageId,
        HINSTANCE source = default,
        params ReadOnlySpan<string> args)
    {
        FORMAT_MESSAGE_OPTIONS flags =
            // Let the API allocate the buffer
            FORMAT_MESSAGE_OPTIONS.FORMAT_MESSAGE_ALLOCATE_BUFFER
            | FORMAT_MESSAGE_OPTIONS.FORMAT_MESSAGE_FROM_SYSTEM;

        if (args.Length == 0)
        {
            flags.SetFlags(FORMAT_MESSAGE_OPTIONS.FORMAT_MESSAGE_IGNORE_INSERTS);
        }
        else
        {
            flags.SetFlags(FORMAT_MESSAGE_OPTIONS.FORMAT_MESSAGE_ARGUMENT_ARRAY);
        }

        if (!source.IsNull)
        {
            flags.SetFlags(FORMAT_MESSAGE_OPTIONS.FORMAT_MESSAGE_FROM_HMODULE);
        }

        // Don't use line breaks
        flags.SetFlags((FORMAT_MESSAGE_OPTIONS)0xFF); // FORMAT_MESSAGE_MAX_WIDTH_MASK

        using StringParameterArray strings = new(args);

        sbyte** sargs = strings;

        PWSTR buffer = default;
        uint result = PInvoke.FormatMessage(
            dwFlags: flags,
            lpSource: source.Value,
            dwMessageId: messageId,
            // Do the default language lookup
            dwLanguageId: 0,
            lpBuffer: (PWSTR)(char*)(&buffer),
            nSize: 0,
            Arguments: sargs);

        if (result == 0 || buffer.IsNull)
        {
            buffer.LocalFree();

            WIN32_ERROR error = GetLastError();
            if (error == WIN32_ERROR.ERROR_MR_MID_NOT_FOUND)
            {
                HRESULT hr = (HRESULT)messageId;
                if (hr.Failed && hr.Facility == FACILITY_CODE.FACILITY_URT)
                {
                    // .NET HRESULT, extract the message (pass -1 to ignore whatever random IErrorInfo is on the thread)
                    string? dotNetMessage = Marshal.GetExceptionForHR((int)hr, (nint)(-1))?.Message;
                    if (dotNetMessage is not null)
                    {
                        return dotNetMessage;
                    }
                }
            }

            return $"The message for id 0x{messageId:x8} was not found.";
        }

        string message = new(buffer, 0, (int)result);
        buffer.LocalFree();
        return message;
    }
}
