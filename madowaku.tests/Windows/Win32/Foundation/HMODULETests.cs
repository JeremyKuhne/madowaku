// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

namespace Windows.Win32.Foundation;

public class HMODULETests
{
    [Fact]
    public void GetLaunchingExecutable_ReturnsNonNullModule()
    {
        HMODULE module = HMODULE.GetLaunchingExecutable();
        Assert.False(module.IsNull);
    }

    [Fact]
    public void FromName_KnownSystemDll_ReturnsLoadedModule()
    {
        // kernel32 is always loaded in any Windows process.
        HMODULE module = HMODULE.FromName("kernel32.dll");
        Assert.False(module.IsNull);
    }

    [Fact]
    public void FromName_UnknownModule_ReturnsNullHandle()
    {
        HMODULE module = HMODULE.FromName("definitely-not-a-real-module.dll");
        Assert.True(module.IsNull);
    }

    [Fact]
    public void LoadModule_KnownSystemDll_ReturnsModule()
    {
        HMODULE module = HMODULE.LoadModule("shell32.dll");
        Assert.False(module.IsNull);
    }

    [Fact]
    public void LoadModule_NonexistentPath_Throws()
    {
        Assert.ThrowsAny<Exception>(() => HMODULE.LoadModule("madowaku-no-such-file.dll"));
    }

    [Fact]
    public void GetProcAddress_KnownExport_ReturnsNonNullAddress()
    {
        HMODULE module = HMODULE.FromName("kernel32.dll");
        FARPROC proc = module.GetProcAddress("GetCurrentProcessId");
        Assert.False(proc.IsNull);
    }

    [Fact]
    public void GetProcAddress_UnknownExport_Throws()
    {
        HMODULE module = HMODULE.FromName("kernel32.dll");
        Assert.ThrowsAny<Exception>(() => module.GetProcAddress("MadowakuNoSuchExport"));
    }

    [Fact]
    public void GetDllVersion_Shell32_ReturnsVersion()
    {
        HMODULE module = HMODULE.LoadModule("shell32.dll");
        Version version = module.GetDllVersion();
        Assert.True(version.Major >= 5);
    }

    [Fact]
    public unsafe void FromAddress_AddressInsideKernel32_ReturnsKernel32()
    {
        HMODULE kernel32 = HMODULE.FromName("kernel32.dll");
        FARPROC proc = kernel32.GetProcAddress("GetCurrentProcessId");
        HMODULE byAddress = HMODULE.FromAddress(proc.Value);
        Assert.Equal(kernel32, byAddress);
    }
}
