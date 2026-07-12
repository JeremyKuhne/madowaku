// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

namespace Windows.Win32.Foundation;

/// <summary>
///  Represents an NT status code.
/// </summary>
public partial struct NTSTATUS
{
    /// <summary>
    ///  Throws an exception when this status represents an error or warning.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void ThrowIfFailed()
    {
        if (SeverityCode is Severity.Error or Severity.Warning)
        {
            throw this;
        }
    }

    /// <summary>
    ///  Creates the exception that corresponds to this status.
    /// </summary>
    /// <param name="path">The optional path associated with the failure.</param>
    /// <returns>The corresponding exception.</returns>
    public Exception GetException(string? path = null)
    {
        WIN32_ERROR error = (WIN32_ERROR)this;

        string message = path is null
            ? $"{Error.ErrorToString(error)} {{NTSTATUS: {Value:x8}}}"
            : $"{Error.ErrorToString(error)} {{NTSTATUS: {Value:x8}}} '{path}'";

        return Error.WindowsErrorToException(error, message, path);
    }

    /// <summary>
    ///  Converts an NT status to its corresponding exception.
    /// </summary>
    /// <param name="result">The status to convert.</param>
    public static implicit operator Exception(NTSTATUS result) => result.GetException();

    /// <summary>
    ///  Converts an NT status to its corresponding Win32 error.
    /// </summary>
    /// <param name="status">The status to convert.</param>
    public static explicit operator WIN32_ERROR(NTSTATUS status)
        => (WIN32_ERROR)PInvoke.LsaNtStatusToWinError(status);
}