// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

#pragma warning disable IDE0005 // Using directive is unnecessary.
global using System;
global using System.Diagnostics;
global using System.Diagnostics.CodeAnalysis;
global using System.Runtime.CompilerServices;
global using System.Runtime.InteropServices;
global using Windows.Win32.Foundation;

// Try to direct as much as possible to Microsoft.IO on .NET Framework. Follow with explicit usings for types
// that are not defined in Microsoft.IO (exchange types).

#if NETFRAMEWORK
global using Microsoft.IO;
global using Microsoft.IO.Enumeration;
#else
global using System.IO;
global using System.IO.Enumeration;
#endif

global using Stream = System.IO.Stream;
global using FileNotFoundException = System.IO.FileNotFoundException;
global using DirectoryNotFoundException = System.IO.DirectoryNotFoundException;
global using PathTooLongException = System.IO.PathTooLongException;
global using DriveNotFoundException = System.IO.DriveNotFoundException;

global using Marshal = System.Runtime.InteropServices.Marshal;
global using FILETIME = Windows.Win32.Foundation.FILETIME;

#pragma warning restore IDE0005 // Using directive is unnecessary.
