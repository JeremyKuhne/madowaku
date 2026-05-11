// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

namespace Windows.Win32.System.Com;

public class CLSIDTests
{
    [Fact]
    public void StdGlobalInterfaceTable_HasExpectedGuid()
    {
        Assert.Equal(new Guid("00000323-0000-0000-c000-000000000046"), CLSID.StdGlobalInterfaceTable);
    }

    [Fact]
    public void FileOpenDialog_HasExpectedGuid()
    {
        Assert.Equal(new Guid("dc1c5a9c-e88a-4dde-a5a1-60f82a20aef7"), CLSID.FileOpenDialog);
    }

    [Fact]
    public void FileSaveDialog_HasExpectedGuid()
    {
        Assert.Equal(new Guid("c0b4e2f3-ba21-4773-8dba-335ec946eb8b"), CLSID.FileSaveDialog);
    }

    [Fact]
    public void AutoComplete_HasExpectedGuid()
    {
        Assert.Equal(new Guid("00bb2763-6a77-11d0-a535-00c04fd7d062"), CLSID.AutoComplete);
    }

    [Fact]
    public void DragDropHelper_HasExpectedGuid()
    {
        Assert.Equal(new Guid("4657278a-411b-11d2-839a-00c04fd918d0"), CLSID.DragDropHelper);
    }

    [Fact]
    public void SetupConfiguration_HasExpectedGuid()
    {
        Assert.Equal(new Guid("177f0c4a-1cd3-4de7-a32c-71dbbb9fa36d"), CLSID.SetupConfiguration);
    }

    [Fact]
    public void SetupConfiguration2_HasExpectedGuid()
    {
        Assert.Equal(new Guid("42843719-db4c-46c2-8e7c-64f1816efd5b"), CLSID.SetupConfiguration2);
    }

    [Fact]
    public void StdComponentCategoriesManager_HasExpectedGuid()
    {
        Assert.Equal(new Guid("0002e005-0000-0000-c000-000000000046"), CLSID.StdComponentCategoriesManager);
    }

    [Fact]
    public void WindowsMediaPlayer_HasExpectedGuid()
    {
        Assert.Equal(new Guid("6bf52a52-394a-11d3-b153-00c04f79faa6"), CLSID.WindowsMediaPlayer);
    }
}
