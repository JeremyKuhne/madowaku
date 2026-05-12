// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using Windows.Win32.Foundation;

namespace Windows.Win32.System.Com;

public class ComClassFactoryTests
{
    [Fact]
    public unsafe void Constructor_RegisteredClsid_CreateInstance_ReturnsInterface()
    {
        // StdGlobalInterfaceTable is always registered.
        using ComClassFactory factory = new(CLSID.StdGlobalInterfaceTable);
        using ComScope<IUnknown> unknown = factory.CreateInstance<IUnknown>();
        Assert.False(unknown.IsNull);
    }

    [Fact]
    public unsafe void TryCreateInstance_ReturnsSuccess()
    {
        using ComClassFactory factory = new(CLSID.StdGlobalInterfaceTable);
        using ComScope<IUnknown> unknown = factory.TryCreateInstance<IUnknown>(out HRESULT hr);
        Assert.True(hr.Succeeded);
        Assert.False(unknown.IsNull);
    }

    [Fact]
    public unsafe void ComScope_QueryInterface_ToIUnknown_Succeeds()
    {
        using ComClassFactory factory = new(CLSID.StdGlobalInterfaceTable);
        using ComScope<IUnknown> unknown = factory.CreateInstance<IUnknown>();
        using ComScope<IUnknown> requeried = unknown.QueryInterface<IUnknown>();
        Assert.False(requeried.IsNull);
    }

    [Fact]
    public unsafe void ComScope_TryQueryInterface_ToIUnknown_Succeeds()
    {
        using ComClassFactory factory = new(CLSID.StdGlobalInterfaceTable);
        using ComScope<IUnknown> unknown = factory.CreateInstance<IUnknown>();
        using ComScope<IUnknown> requeried = unknown.TryQueryInterface<IUnknown>(out HRESULT hr);
        Assert.True(hr.Succeeded);
        Assert.False(requeried.IsNull);
    }

    [Fact]
    public void Constructor_FilePath_NonexistentDll_Throws()
    {
        Assert.ThrowsAny<Exception>(
            () => new ComClassFactory("madowaku-no-such-dll.dll", CLSID.FileOpenDialog));
    }

    [Fact]
    public unsafe void Constructor_Hmodule_KnownExport_ExposesClassId()
    {
        // Get the module that already provides StdGlobalInterfaceTable's exports.
        // ole32.dll exports DllGetClassObject and the StdGlobalInterfaceTable CLSID.
        HMODULE ole32 = HMODULE.FromName("ole32.dll");
        Assert.False(ole32.IsNull);

        using ComClassFactory factory = new(ole32, CLSID.StdGlobalInterfaceTable);
        Assert.Equal(CLSID.StdGlobalInterfaceTable, factory.ClassId);

        using ComScope<IUnknown> unknown = factory.CreateInstance<IUnknown>();
        Assert.False(unknown.IsNull);
    }
}
