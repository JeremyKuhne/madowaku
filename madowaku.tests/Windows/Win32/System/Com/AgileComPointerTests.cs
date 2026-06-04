// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using Windows.Win32.Foundation;

namespace Windows.Win32.System.Com;

[TestClass]
public class AgileComPointerTests
{
    [TestMethod]
    public unsafe void Constructor_RegistersInterface_AndGetInterfaceReturnsScope()
    {
        using ComClassFactory factory = new(CLSID.StdGlobalInterfaceTable);
        using ComScope<IUnknown> source = factory.CreateInstance<IUnknown>();

        AgileComPointer<IUnknown> agile = new(source.Pointer, takeOwnership: false);
        try
        {
            using ComScope<IUnknown> scope = agile.GetInterface();
            scope.IsNull.Should().BeFalse();
        }
        finally
        {
            agile.Dispose();
        }
    }

    [TestMethod]
    public unsafe void TryGetInterface_ReturnsSuccess()
    {
        using ComClassFactory factory = new(CLSID.StdGlobalInterfaceTable);
        using ComScope<IUnknown> source = factory.CreateInstance<IUnknown>();

        AgileComPointer<IUnknown> agile = new(source.Pointer, takeOwnership: false);
        try
        {
            using ComScope<IUnknown> scope = agile.TryGetInterface(out HRESULT hr);
            hr.Succeeded.Should().BeTrue();
            scope.IsNull.Should().BeFalse();
        }
        finally
        {
            agile.Dispose();
        }
    }

    [TestMethod]
    public unsafe void Equals_SamePointer_ReturnsTrue()
    {
        using ComClassFactory factory = new(CLSID.StdGlobalInterfaceTable);
        using ComScope<IUnknown> source = factory.CreateInstance<IUnknown>();
        IUnknown* ptr = source.Pointer;

        AgileComPointer<IUnknown> agile = new(ptr, takeOwnership: false);
        try
        {
            agile.Equals(ptr).Should().BeTrue();
        }
        finally
        {
            agile.Dispose();
        }
    }

    [TestMethod]
    public unsafe void GetInterface_Generic_RequeriesAsSameInterface_Succeeds()
    {
        using ComClassFactory factory = new(CLSID.StdGlobalInterfaceTable);
        using ComScope<IUnknown> source = factory.CreateInstance<IUnknown>();

        AgileComPointer<IUnknown> agile = new(source.Pointer, takeOwnership: false);
        try
        {
            using ComScope<IUnknown> scope = agile.GetInterface<IUnknown>();
            scope.IsNull.Should().BeFalse();
        }
        finally
        {
            agile.Dispose();
        }
    }

    [TestMethod]
    public unsafe void TryGetInterface_Generic_RequeriesAsSameInterface_Succeeds()
    {
        using ComClassFactory factory = new(CLSID.StdGlobalInterfaceTable);
        using ComScope<IUnknown> source = factory.CreateInstance<IUnknown>();

        AgileComPointer<IUnknown> agile = new(source.Pointer, takeOwnership: false);
        try
        {
            using ComScope<IUnknown> scope = agile.TryGetInterface<IUnknown>(out HRESULT hr);
            hr.Succeeded.Should().BeTrue();
            scope.IsNull.Should().BeFalse();
        }
        finally
        {
            agile.Dispose();
        }
    }
}
