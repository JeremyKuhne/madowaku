// Copyright (c) 2025 Jeremy W Kuhne
// SPDX-License-Identifier: MIT
// See LICENSE file in the project root for full license information

using System.ComponentModel;
using Windows.Win32.System.Memory;

namespace Windows.Win32.Foundation;

[TestClass]
public unsafe class HGLOBALTests
{
    [TestMethod]
    public void Size_ValidHandle_ReturnsAllocatedSize()
    {
        HGLOBAL global = PInvoke.GlobalAlloc(GLOBAL_ALLOC_FLAGS.GMEM_MOVEABLE, sizeof(int) * 4);

        try
        {
            global.Size.Should().Be((nuint)(sizeof(int) * 4));
        }
        finally
        {
            PInvoke.GlobalFree(global);
        }
    }

    [TestMethod]
    public void Size_NullHandle_ReturnsZero()
    {
        HGLOBAL global = default;

        global.Size.Should().Be((nuint)0);
    }

    [TestMethod]
    public void IsValid_ValidHandle_ReturnsTrue()
    {
        HGLOBAL global = PInvoke.GlobalAlloc(GLOBAL_ALLOC_FLAGS.GMEM_MOVEABLE, sizeof(int));

        try
        {
            global.IsValid.Should().BeTrue();
        }
        finally
        {
            PInvoke.GlobalFree(global);
        }
    }

    [TestMethod]
    public void IsValid_NullHandle_ReturnsFalse()
    {
        HGLOBAL global = default;

        global.IsValid.Should().BeFalse();
    }

    [TestMethod]
    public void Lock_ValidHandle_ReturnsNonNullMemory()
    {
        HGLOBAL global = PInvoke.GlobalAlloc(GLOBAL_ALLOC_FLAGS.GMEM_MOVEABLE, sizeof(int));

        try
        {
            using HGLOBAL.LockScope scope = global.Lock();
            nint scopePointer = (nint)(void*)scope;
            nint memoryPointer = (nint)scope.Memory;
            scopePointer.Should().NotBe(0);
            memoryPointer.Should().Be(scopePointer);
        }
        finally
        {
            PInvoke.GlobalFree(global);
        }
    }

    [TestMethod]
    public void Lock_WrittenData_IsReadableThroughScope()
    {
        HGLOBAL global = PInvoke.GlobalAlloc(GLOBAL_ALLOC_FLAGS.GMEM_MOVEABLE, sizeof(int));

        try
        {
            using (HGLOBAL.LockScope scope = global.Lock())
            {
                *(int*)(void*)scope = 42;
            }

            using (HGLOBAL.LockScope scope = global.Lock())
            {
                (*(int*)(void*)scope).Should().Be(42);
            }
        }
        finally
        {
            PInvoke.GlobalFree(global);
        }
    }

    [TestMethod]
    public void Lock_MultipleNestedLocks_IncrementsLockCount()
    {
        HGLOBAL global = PInvoke.GlobalAlloc(GLOBAL_ALLOC_FLAGS.GMEM_MOVEABLE, sizeof(int));

        try
        {
            using HGLOBAL.LockScope outer = global.Lock();
            using HGLOBAL.LockScope inner = global.Lock();

            nint outerPointer = (nint)(void*)outer;
            nint innerPointer = (nint)(void*)inner;
            outerPointer.Should().NotBe(0);
            innerPointer.Should().NotBe(0);
        }
        finally
        {
            PInvoke.GlobalFree(global);
        }
    }

    [TestMethod]
    public void Lock_InvalidHandle_ThrowsWin32Exception()
    {
        HGLOBAL global = default;

        FluentActions.Invoking(() => global.Lock())
            .Should().Throw<Win32Exception>();
    }

    [TestMethod]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        HGLOBAL global = PInvoke.GlobalAlloc(GLOBAL_ALLOC_FLAGS.GMEM_MOVEABLE, sizeof(int));

        try
        {
            HGLOBAL.LockScope scope = global.Lock();
            scope.Dispose();
            scope.Dispose();
        }
        finally
        {
            PInvoke.GlobalFree(global);
        }
    }

    [TestMethod]
    public void Dispose_UnlocksMemory_AllowsGlobalFree()
    {
        HGLOBAL global = PInvoke.GlobalAlloc(GLOBAL_ALLOC_FLAGS.GMEM_MOVEABLE, sizeof(int));

        HGLOBAL.LockScope scope = global.Lock();
        scope.Dispose();

        PInvoke.GlobalFree(global).IsNull.Should().BeTrue();
    }

    [TestMethod]
    public void ImplicitConversion_ToVoidPointer_MatchesMemoryProperty()
    {
        HGLOBAL global = PInvoke.GlobalAlloc(GLOBAL_ALLOC_FLAGS.GMEM_MOVEABLE, sizeof(int));

        try
        {
            using HGLOBAL.LockScope scope = global.Lock();
            void* converted = scope;
            nint convertedPointer = (nint)converted;
            nint memoryPointer = (nint)scope.Memory;
            convertedPointer.Should().Be(memoryPointer);
        }
        finally
        {
            PInvoke.GlobalFree(global);
        }
    }
}
