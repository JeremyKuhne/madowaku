// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using System.ComponentModel;
using Windows.Win32.Foundation;

namespace Windows.Win32.System.Com;

[TestClass]
public class ComClassFactoryTests
{
    [TestMethod]
    public unsafe void Constructor_RegisteredClsid_CreateInstance_ReturnsInterface()
    {
        // StdGlobalInterfaceTable is always registered.
        using ComClassFactory factory = new(CLSID.StdGlobalInterfaceTable);
        using ComScope<IUnknown> unknown = factory.CreateInstance<IUnknown>();
        unknown.IsNull.Should().BeFalse();
    }

    [TestMethod]
    public unsafe void TryCreateInstance_ReturnsSuccess()
    {
        using ComClassFactory factory = new(CLSID.StdGlobalInterfaceTable);
        using ComScope<IUnknown> unknown = factory.TryCreateInstance<IUnknown>(out HRESULT hr);
        hr.Succeeded.Should().BeTrue();
        unknown.IsNull.Should().BeFalse();
    }

    [TestMethod]
    public unsafe void ComScope_QueryInterface_ToIUnknown_Succeeds()
    {
        using ComClassFactory factory = new(CLSID.StdGlobalInterfaceTable);
        using ComScope<IUnknown> unknown = factory.CreateInstance<IUnknown>();
        using ComScope<IUnknown> requeried = unknown.QueryInterface<IUnknown>();
        requeried.IsNull.Should().BeFalse();
    }

    [TestMethod]
    public unsafe void ComScope_TryQueryInterface_ToIUnknown_Succeeds()
    {
        using ComClassFactory factory = new(CLSID.StdGlobalInterfaceTable);
        using ComScope<IUnknown> unknown = factory.CreateInstance<IUnknown>();
        using ComScope<IUnknown> requeried = unknown.TryQueryInterface<IUnknown>(out HRESULT hr);
        hr.Succeeded.Should().BeTrue();
        requeried.IsNull.Should().BeFalse();
    }

    [TestMethod]
    public void Constructor_FilePath_NonexistentDll_ThrowsWin32Exception()
    {
        Win32Exception ex = FluentActions.Invoking(
            () => new ComClassFactory("madowaku-no-such-dll.dll", CLSID.FileOpenDialog))
            .Should().Throw<Win32Exception>().Which;

        // ERROR_MOD_NOT_FOUND = 126, ERROR_FILE_NOT_FOUND = 2, ERROR_PATH_NOT_FOUND = 3
        ex.NativeErrorCode.Should().BeOneOf(126, 2, 3);
    }

    [TestMethod]
    public unsafe void Constructor_Hmodule_KnownExport_ExposesClassId()
    {
        // Explicitly load ole32.dll so the test isn't order-/environment-dependent.
        // ole32 exports DllGetClassObject and contains the StdGlobalInterfaceTable CLSID.
        HMODULE ole32 = HMODULE.LoadModule("ole32.dll");
        try
        {
            using ComClassFactory factory = new(ole32, CLSID.StdGlobalInterfaceTable);
            factory.ClassId.Should().Be(CLSID.StdGlobalInterfaceTable);

            using ComScope<IUnknown> unknown = factory.CreateInstance<IUnknown>();
            unknown.IsNull.Should().BeFalse();
        }
        finally
        {
            PInvoke.FreeLibrary(ole32);
        }
    }
}
