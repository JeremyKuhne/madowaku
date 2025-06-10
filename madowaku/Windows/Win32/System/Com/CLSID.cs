// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

namespace Windows.Win32.System.Com;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
///  Represents CLSIDs for various COM components.
/// </summary>
public static class CLSID
{
    // 42843719-DB4C-46C2-8E7C-64F1816EFD5B
    public static Guid SetupConfiguration2 { get; } = new(0x42843719, 0xdb4c, 0x46c2, 0x8e, 0x7c, 0x64, 0xf1, 0x81, 0x6e, 0xfd, 0x5b);

    // 177F0C4A-1CD3-4DE7-A32C-71DBBB9FA36D
    public static Guid SetupConfiguration { get; } = new(0x177f0c4a, 0x1cd3, 0x4de7, 0xa3, 0x2c, 0x71, 0xdb, 0xbb, 0x9f, 0xa3, 0x6d);

    // 00bb2763-6a77-11d0-a535-00c04fd7d062
    public static Guid AutoComplete { get; } = new(0x00bb2763, 0x6a77, 0x11d0, 0xa5, 0x35, 0x00, 0xc0, 0x4f, 0xd7, 0xd0, 0x62);

    // 4657278a-411b-11d2-839a-00c04fd918d0
    public static Guid DragDropHelper { get; } = new(0x4657278a, 0x411b, 0x11d2, 0x83, 0x9a, 0x0, 0xc0, 0x4f, 0xd9, 0x18, 0xd0);

    // c0b4e2f3-ba21-4773-8dba-335ec946eb8b
    public static Guid FileSaveDialog { get; } = new(0xc0b4e2f3, 0xba21, 0x4773, 0x8d, 0xba, 0x33, 0x5e, 0xc9, 0x46, 0xeb, 0x8b);

    // dc1c5a9c-e88a-4dde-a5a1-60f82a20aef7
    public static Guid FileOpenDialog { get; } = new(0xdc1c5a9c, 0xe88a, 0x4dde, 0xa5, 0xa1, 0x60, 0xf8, 0x2a, 0x20, 0xae, 0xf7);

    // 00000323-0000-0000-c000-000000000046
    public static Guid StdGlobalInterfaceTable { get; } = new(0x00000323, 0x0000, 0x0000, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

    // 0002e005-0000-0000-c000-000000000046
    public static Guid StdComponentCategoriesManager { get; } = new(0x0002e005, 0x0000, 0x0000, 0xc0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);

    // 6bf52a52-394a-11d3-b153-00c04f79faa6
    public static Guid WindowsMediaPlayer { get; } = new(0x6bf52a52, 0x394a, 0x11d3, 0xb1, 0x53, 0x00, 0xc0, 0x4f, 0x79, 0xfa, 0xa6);
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
