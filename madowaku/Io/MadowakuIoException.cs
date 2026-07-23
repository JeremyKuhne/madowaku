// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

namespace Madowaku.Io;

/// <summary>
///  Base class for IO exceptions thrown by Madowaku.
/// </summary>
public class MadowakuIoException : IOException
{
    /// <summary>
    ///  Constructs a new <see cref="MadowakuIoException"/>.
    /// </summary>
    public MadowakuIoException()
        : base() { }

    /// <summary>
    ///  Constructs a new <see cref="MadowakuIoException"/> with the specified error and message.
    /// </summary>
    public MadowakuIoException(HRESULT hr, string? message = null)
        : base(message ?? hr.ToStringWithDescription(), hresult: hr) { }

    /// <summary>
    ///  Constructs a new <see cref="MadowakuIoException"/> with the specified error and message.
    /// </summary>
    public MadowakuIoException(WIN32_ERROR error, string? message = null)
        : base(message ?? error.ErrorToString(), (int)error.ToHRESULT()) { }
}
