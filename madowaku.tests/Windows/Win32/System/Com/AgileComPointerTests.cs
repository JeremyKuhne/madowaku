// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using Windows.Win32.Foundation;

namespace Windows.Win32.System.Com;

public class AgileComPointerTests
{
    [Fact]
    public unsafe void Constructor_RegistersInterface_AndGetInterfaceReturnsScope()
    {
        using ComClassFactory factory = new(CLSID.StdGlobalInterfaceTable);
        using ComScope<IUnknown> source = factory.CreateInstance<IUnknown>();

        AgileComPointer<IUnknown> agile = new(source.Pointer, takeOwnership: false);
        try
        {
            using ComScope<IUnknown> scope = agile.GetInterface();
            Assert.False(scope.IsNull);
        }
        finally
        {
            agile.Dispose();
        }
    }

    [Fact]
    public unsafe void TryGetInterface_ReturnsSuccess()
    {
        using ComClassFactory factory = new(CLSID.StdGlobalInterfaceTable);
        using ComScope<IUnknown> source = factory.CreateInstance<IUnknown>();

        AgileComPointer<IUnknown> agile = new(source.Pointer, takeOwnership: false);
        try
        {
            using ComScope<IUnknown> scope = agile.TryGetInterface(out HRESULT hr);
            Assert.True(hr.Succeeded);
            Assert.False(scope.IsNull);
        }
        finally
        {
            agile.Dispose();
        }
    }

    [Fact]
    public unsafe void Equals_SamePointer_ReturnsTrue()
    {
        using ComClassFactory factory = new(CLSID.StdGlobalInterfaceTable);
        using ComScope<IUnknown> source = factory.CreateInstance<IUnknown>();
        IUnknown* ptr = source.Pointer;

        AgileComPointer<IUnknown> agile = new(ptr, takeOwnership: false);
        try
        {
            Assert.True(agile.Equals(ptr));
        }
        finally
        {
            agile.Dispose();
        }
    }
}
