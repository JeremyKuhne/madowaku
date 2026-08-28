// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

namespace Windows.Win32.System.Com;

/// <summary>
///  Represents CLSIDs for various COM components.
/// </summary>
public static class CLSID
{
    /// <summary>
    ///  Gets the CLSID for the Visual Studio Setup Configuration 2 component.
    /// </summary>
    public static Guid SetupConfiguration2 { get; } = new(0x42843719, 0xdb4c, 0x46c2, 0x8e, 0x7c, 0x64, 0xf1, 0x81, 0x6e, 0xfd, 0x5b);

    /// <summary>
    ///  Gets the CLSID for the Visual Studio Setup Configuration component.
    /// </summary>
    public static Guid SetupConfiguration { get; } = new(0x177f0c4a, 0x1cd3, 0x4de7, 0xa3, 0x2c, 0x71, 0xdb, 0xbb, 0x9f, 0xa3, 0x6d);

    /// <summary>
    ///  Gets the CLSID for the shell autocomplete component.
    /// </summary>
    public static Guid AutoComplete { get; } = new(0x00bb2763, 0x6a77, 0x11d0, 0xa5, 0x35, 0x00, 0xc0, 0x4f, 0xd7, 0xd0, 0x62);

    /// <summary>
    ///  Gets the CLSID for the shell drag-and-drop helper component.
    /// </summary>
    public static Guid DragDropHelper { get; } = new(0x4657278a, 0x411b, 0x11d2, 0x83, 0x9a, 0x0, 0xc0, 0x4f, 0xd9, 0x18, 0xd0);

    /// <summary>
    ///  Gets the CLSID for the common file save dialog.
    /// </summary>
    public static Guid FileSaveDialog { get; } = new(0xc0b4e2f3, 0xba21, 0x4773, 0x8d, 0xba, 0x33, 0x5e, 0xc9, 0x46, 0xeb, 0x8b);

    /// <summary>
    ///  Gets the CLSID for the common file open dialog.
    /// </summary>
    public static Guid FileOpenDialog { get; } = new(0xdc1c5a9c, 0xe88a, 0x4dde, 0xa5, 0xa1, 0x60, 0xf8, 0x2a, 0x20, 0xae, 0xf7);

    /// <summary>
    ///  Gets the CLSID for the standard global interface table.
    /// </summary>
    public static Guid StdGlobalInterfaceTable { get; } = new(0x00000323, 0x0000, 0x0000, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

    /// <summary>
    ///  Gets the CLSID for the standard component categories manager.
    /// </summary>
    public static Guid StdComponentCategoriesManager { get; } = new(0x0002e005, 0x0000, 0x0000, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

    /// <summary>
    ///  Gets the CLSID for the Windows Media Player component.
    /// </summary>
    public static Guid WindowsMediaPlayer { get; } = new(0x6bf52a52, 0x394a, 0x11d3, 0xb1, 0x53, 0x00, 0xc0, 0x4f, 0x79, 0xfa, 0xa6);
}
