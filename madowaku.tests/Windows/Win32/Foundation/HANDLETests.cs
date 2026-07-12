// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

namespace Windows.Win32.Foundation;

[TestClass]
public class HANDLETests
{
    [TestMethod]
    public unsafe void Dispose_ValidHandle_ClosesAndClearsHandle()
    {
        string path = Path.GetTempFileName();
        FileStream stream = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete);

        try
        {
            HANDLE handle = (HANDLE)(void*)stream.SafeFileHandle.DangerousGetHandle();
            stream.SafeFileHandle.SetHandleAsInvalid();

            handle.Dispose();
            handle.IsNull.Should().BeTrue();

            handle.Dispose();
        }
        finally
        {
            stream.Dispose();
            File.Delete(path);
        }
    }

    [TestMethod]
    public unsafe void Dispose_InvalidHandleValue_ClearsHandle()
    {
        HANDLE handle = (HANDLE)(void*)(-1);

        handle.Dispose();

        handle.IsNull.Should().BeTrue();
    }
}