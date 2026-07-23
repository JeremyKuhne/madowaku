// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

namespace Madowaku.Io;

/// <summary>
///  Thrown when a drive is not ready for access.
/// </summary>
public class DriveNotReadyException(string? message = null) : MadowakuIoException(WIN32_ERROR.ERROR_NOT_READY, message)
{
}
