// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using Windows.Win32.Foundation;

namespace Madowaku.Io;

[TestClass]
public class FileExistsExceptionTests
{
    [TestMethod]
    public void Constructor_FileExists_SetsHResultForError()
    {
        FileExistsException exception = new(WIN32_ERROR.ERROR_FILE_EXISTS);
        exception.HResult.Should().Be((int)WIN32_ERROR.ERROR_FILE_EXISTS.ToHRESULT());
    }

    [TestMethod]
    public void Constructor_AlreadyExists_SetsHResultForError()
    {
        FileExistsException exception = new(WIN32_ERROR.ERROR_ALREADY_EXISTS);
        exception.HResult.Should().Be((int)WIN32_ERROR.ERROR_ALREADY_EXISTS.ToHRESULT());
    }

    [TestMethod]
    public void Constructor_NullMessage_UsesErrorToString()
    {
        FileExistsException exception = new(WIN32_ERROR.ERROR_FILE_EXISTS);
        exception.Message.Should().Be(WIN32_ERROR.ERROR_FILE_EXISTS.ErrorToString());
    }

    [TestMethod]
    public void Constructor_WithMessage_UsesGivenMessage()
    {
        FileExistsException exception = new(WIN32_ERROR.ERROR_FILE_EXISTS, "file already there");
        exception.Message.Should().Be("file already there");
    }
}
