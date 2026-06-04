// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

namespace Windows.Win32.System.Com;

[TestClass]
public class CLSIDTests
{
    [TestMethod]
    public void StdGlobalInterfaceTable_HasExpectedGuid()
    {
        CLSID.StdGlobalInterfaceTable.Should().Be(new Guid("00000323-0000-0000-c000-000000000046"));
    }

    [TestMethod]
    public void FileOpenDialog_HasExpectedGuid()
    {
        CLSID.FileOpenDialog.Should().Be(new Guid("dc1c5a9c-e88a-4dde-a5a1-60f82a20aef7"));
    }

    [TestMethod]
    public void FileSaveDialog_HasExpectedGuid()
    {
        CLSID.FileSaveDialog.Should().Be(new Guid("c0b4e2f3-ba21-4773-8dba-335ec946eb8b"));
    }

    [TestMethod]
    public void AutoComplete_HasExpectedGuid()
    {
        CLSID.AutoComplete.Should().Be(new Guid("00bb2763-6a77-11d0-a535-00c04fd7d062"));
    }

    [TestMethod]
    public void DragDropHelper_HasExpectedGuid()
    {
        CLSID.DragDropHelper.Should().Be(new Guid("4657278a-411b-11d2-839a-00c04fd918d0"));
    }

    [TestMethod]
    public void SetupConfiguration_HasExpectedGuid()
    {
        CLSID.SetupConfiguration.Should().Be(new Guid("177f0c4a-1cd3-4de7-a32c-71dbbb9fa36d"));
    }

    [TestMethod]
    public void SetupConfiguration2_HasExpectedGuid()
    {
        CLSID.SetupConfiguration2.Should().Be(new Guid("42843719-db4c-46c2-8e7c-64f1816efd5b"));
    }

    [TestMethod]
    public void StdComponentCategoriesManager_HasExpectedGuid()
    {
        CLSID.StdComponentCategoriesManager.Should().Be(new Guid("0002e005-0000-0000-c000-000000000046"));
    }

    [TestMethod]
    public void WindowsMediaPlayer_HasExpectedGuid()
    {
        CLSID.WindowsMediaPlayer.Should().Be(new Guid("6bf52a52-394a-11d3-b153-00c04f79faa6"));
    }
}
