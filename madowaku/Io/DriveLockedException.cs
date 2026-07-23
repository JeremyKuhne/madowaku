// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

namespace Madowaku.Io;

/// <summary>
///  Thrown when a drive is locked.
/// </summary>
public class DriveLockedException(string? message = null) : MadowakuIoException(HRESULT.FVE_E_LOCKED_VOLUME, message)
{
}
