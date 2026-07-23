// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

namespace Madowaku.Io;

/// <summary>
///  Thrown when a file already exists.
/// </summary>
public class FileExistsException(WIN32_ERROR error, string? message = null) : MadowakuIoException(error, message)
{
}
