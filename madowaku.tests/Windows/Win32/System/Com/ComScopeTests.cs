// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

namespace Windows.Win32.System.Com;

public class ComScopeTests
{
    [Fact]
    public unsafe void Constructor_NullPointer_IsNullTrue()
    {
        using ComScope<IUnknown> scope = new((IUnknown*)null);
        Assert.True(scope.IsNull);
        Assert.True(scope.Pointer is null);
    }

    [Fact]
    public unsafe void Constructor_VoidNull_IsNullTrue()
    {
        using ComScope<IUnknown> scope = new((void*)null);
        Assert.True(scope.IsNull);
    }

    [Fact]
    public unsafe void ImplicitOperator_TStar_NullPointer_ReturnsNull()
    {
        using ComScope<IUnknown> scope = new((IUnknown*)null);
        IUnknown* p = scope;
        Assert.True(p is null);
    }

    [Fact]
    public unsafe void ImplicitOperator_VoidStar_NullPointer_ReturnsNull()
    {
        using ComScope<IUnknown> scope = new((IUnknown*)null);
        void* p = scope;
        Assert.True(p is null);
    }

    [Fact]
    public unsafe void ImplicitOperator_Nint_NullPointer_ReturnsZero()
    {
        using ComScope<IUnknown> scope = new((IUnknown*)null);
        nint p = scope;
        Assert.Equal(0, p);
    }

    [Fact]
    public unsafe void ImplicitOperator_TStarStar_NonNull()
    {
        using ComScope<IUnknown> scope = new((IUnknown*)null);
        IUnknown** pp = scope;
        Assert.False(pp is null);
    }

    [Fact]
    public unsafe void Dispose_NullPointer_DoesNotThrow()
    {
        ComScope<IUnknown> scope = new((IUnknown*)null);
        scope.Dispose();
    }
}
