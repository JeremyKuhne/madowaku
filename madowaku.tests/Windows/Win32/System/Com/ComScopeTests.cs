// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

namespace Windows.Win32.System.Com;

[TestClass]
public class ComScopeTests
{
    [TestMethod]
    public unsafe void Constructor_NullPointer_IsNullTrue()
    {
        using ComScope<IUnknown> scope = new((IUnknown*)null);
        scope.IsNull.Should().BeTrue();
        (scope.Pointer is null).Should().BeTrue();
    }

    [TestMethod]
    public unsafe void Constructor_VoidNull_IsNullTrue()
    {
        using ComScope<IUnknown> scope = new((void*)null);
        scope.IsNull.Should().BeTrue();
    }

    [TestMethod]
    public unsafe void ImplicitOperator_TStar_NullPointer_ReturnsNull()
    {
        using ComScope<IUnknown> scope = new((IUnknown*)null);
        IUnknown* p = scope;
        (p is null).Should().BeTrue();
    }

    [TestMethod]
    public unsafe void ImplicitOperator_VoidStar_NullPointer_ReturnsNull()
    {
        using ComScope<IUnknown> scope = new((IUnknown*)null);
        void* p = scope;
        (p is null).Should().BeTrue();
    }

    [TestMethod]
    public unsafe void ImplicitOperator_Nint_NullPointer_ReturnsZero()
    {
        using ComScope<IUnknown> scope = new((IUnknown*)null);
        nint p = scope;
        p.Should().Be(0);
    }

    [TestMethod]
    public unsafe void ImplicitOperator_TStarStar_NonNull()
    {
        using ComScope<IUnknown> scope = new((IUnknown*)null);
        IUnknown** pp = scope;
        (pp is null).Should().BeFalse();
    }

    [TestMethod]
    public unsafe void Dispose_NullPointer_DoesNotThrow()
    {
        ComScope<IUnknown> scope = new((IUnknown*)null);
        scope.Dispose();
    }
}
