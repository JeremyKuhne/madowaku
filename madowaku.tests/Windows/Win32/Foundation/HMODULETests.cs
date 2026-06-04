// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using System.ComponentModel;

namespace Windows.Win32.Foundation;

[TestClass]
public class HMODULETests
{
    [TestMethod]
    public void GetLaunchingExecutable_ReturnsNonNullModule()
    {
        HMODULE module = HMODULE.GetLaunchingExecutable();
        module.IsNull.Should().BeFalse();
    }

    [TestMethod]
    public void FromName_KnownSystemDll_ReturnsLoadedModule()
    {
        // kernel32 is always loaded in any Windows process.
        HMODULE module = HMODULE.FromName("kernel32.dll");
        module.IsNull.Should().BeFalse();
    }

    [TestMethod]
    public void FromName_UnknownModule_ReturnsNullHandle()
    {
        HMODULE module = HMODULE.FromName("definitely-not-a-real-module.dll");
        module.IsNull.Should().BeTrue();
    }

    [TestMethod]
    public void LoadModule_KnownSystemDll_ReturnsModule()
    {
        HMODULE module = HMODULE.LoadModule("shell32.dll");
        try
        {
            module.IsNull.Should().BeFalse();
        }
        finally
        {
            PInvokeMadowaku.FreeLibrary(module);
        }
    }

    [TestMethod]
    public void LoadModule_NonexistentPath_ThrowsWin32Exception()
    {
        Win32Exception ex = FluentActions.Invoking(() => HMODULE.LoadModule("madowaku-no-such-file.dll"))
            .Should().Throw<Win32Exception>().Which;

        // ERROR_MOD_NOT_FOUND = 126, ERROR_FILE_NOT_FOUND = 2, ERROR_PATH_NOT_FOUND = 3
        ex.NativeErrorCode.Should().BeOneOf(126, 2, 3);
    }

    [TestMethod]
    public void GetProcAddress_KnownExport_ReturnsNonNullAddress()
    {
        HMODULE module = HMODULE.FromName("kernel32.dll");
        FARPROC proc = module.GetProcAddress("GetCurrentProcessId");
        proc.IsNull.Should().BeFalse();
    }

    [TestMethod]
    public void GetProcAddress_UnknownExport_ThrowsWin32Exception()
    {
        HMODULE module = HMODULE.FromName("kernel32.dll");
        Win32Exception ex = FluentActions.Invoking(() => module.GetProcAddress("MadowakuNoSuchExport"))
            .Should().Throw<Win32Exception>().Which;

        // ERROR_PROC_NOT_FOUND = 127
        ex.NativeErrorCode.Should().Be(127);
    }

    [TestMethod]
    public void GetDllVersion_Shell32_ReturnsVersion()
    {
        HMODULE module = HMODULE.LoadModule("shell32.dll");
        try
        {
            Version version = module.GetDllVersion();
            version.Major.Should().BeGreaterThanOrEqualTo(5);
        }
        finally
        {
            PInvokeMadowaku.FreeLibrary(module);
        }
    }

    [TestMethod]
    public unsafe void FromAddress_AddressInsideKernel32_ReturnsKernel32()
    {
        HMODULE kernel32 = HMODULE.FromName("kernel32.dll");
        FARPROC proc = kernel32.GetProcAddress("GetCurrentProcessId");
        HMODULE byAddress = HMODULE.FromAddress(proc.Value);
        byAddress.Should().Be(kernel32);
    }
}
